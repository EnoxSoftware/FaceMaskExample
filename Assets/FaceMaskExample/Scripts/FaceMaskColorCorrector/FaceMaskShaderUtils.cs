using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UtilsModule;
using System;
using UnityEngine;

namespace FaceMaskExample
{
    public class FaceMaskShaderUtils
    {
        // Match histograms of 'src' to that of 'dst', according to both masks.
        public static void CalculateLUT(Mat src, Mat dst, Mat src_mask, Mat dst_mask, Texture2D LUTTex)
        {
            if (src.channels() < 3)
                throw new ArgumentException("src.channels() < 3");

            if (dst.channels() < 3)
                throw new ArgumentException("dst.channels() < 3");

            if (src_mask.channels() != 1)
                throw new ArgumentException("src_mask.channels() != 1");

            if (dst_mask.channels() != 1)
                throw new ArgumentException("dst_mask.channels() != 1");

            if (src_mask != null && src.total() != src_mask.total())
                throw new ArgumentException("src.total() != src_mask.total()");

            if (dst_mask != null && dst.total() != dst_mask.total())
                throw new ArgumentException("dst.total() != dst_mask.total()");

            if (LUTTex.width != 256 || LUTTex.height != 1 || LUTTex.format != TextureFormat.RGB24)
                throw new ArgumentException("Invalid LUTTex.");

            byte[] LUT = new byte[3 * 256];
            double[][] src_hist = new double[3][];
            for (int i = 0; i < src_hist.Length; i++)
            {
                src_hist[i] = new double[256];
            }
            double[][] dst_hist = new double[3][];
            for (int i = 0; i < dst_hist.Length; i++)
            {
                dst_hist[i] = new double[256];
            }
            double[][] src_cdf = new double[3][];
            for (int i = 0; i < src_cdf.Length; i++)
            {
                src_cdf[i] = new double[256];
            }
            double[][] dst_cdf = new double[3][];
            for (int i = 0; i < dst_cdf.Length; i++)
            {
                dst_cdf[i] = new double[256];
            }

            double[] src_histMax = new double[3];
            double[] dst_histMax = new double[3];

            byte[] src_mask_byte = null;
            byte[] dst_mask_byte = null;
            if (src_mask != null)
            {
                src_mask_byte = new byte[src_mask.total() * src_mask.channels()];
                MatUtils.copyFromMat<byte>(src_mask, src_mask_byte);
            }
            if (dst_mask != null)
            {
                dst_mask_byte = new byte[dst_mask.total() * dst_mask.channels()];
                MatUtils.copyFromMat<byte>(dst_mask, dst_mask_byte);
            }

            byte[] src_byte = new byte[src.total() * src.channels()];
            MatUtils.copyFromMat<byte>(src, src_byte);
            byte[] dst_byte = new byte[dst.total() * dst.channels()];
            MatUtils.copyFromMat<byte>(dst, dst_byte);

            int pixel_i = 0;
            int channels = src.channels();
            int total = (int)src.total();
            if (src_mask_byte != null)
            {
                for (int i = 0; i < total; i++)
                {
                    if (src_mask_byte[i] != 0)
                    {
                        byte c = src_byte[pixel_i];
                        src_hist[0][c]++;
                        if (src_hist[0][c] > src_histMax[0])
                            src_histMax[0] = src_hist[0][c];

                        c = src_byte[pixel_i + 1];
                        src_hist[1][c]++;
                        if (src_hist[1][c] > src_histMax[1])
                            src_histMax[1] = src_hist[1][c];

                        c = src_byte[pixel_i + 2];
                        src_hist[2][c]++;
                        if (src_hist[2][c] > src_histMax[2])
                            src_histMax[2] = src_hist[2][c];
                    }

                    // Advance to next pixel
                    pixel_i += channels;
                }
            }
            else
            {
                for (int i = 0; i < total; i++)
                {
                    byte c = src_byte[pixel_i];
                    src_hist[0][c]++;
                    if (src_hist[0][c] > src_histMax[0])
                        src_histMax[0] = src_hist[0][c];

                    c = src_byte[pixel_i + 1];
                    src_hist[1][c]++;
                    if (src_hist[1][c] > src_histMax[1])
                        src_histMax[1] = src_hist[1][c];

                    c = src_byte[pixel_i + 2];
                    src_hist[2][c]++;
                    if (src_hist[2][c] > src_histMax[2])
                        src_histMax[2] = src_hist[2][c];

                    // Advance to next pixel
                    pixel_i += channels;
                }
            }

            pixel_i = 0;
            channels = dst.channels();
            total = (int)dst.total();
            if (dst_mask_byte != null)
            {
                for (int i = 0; i < total; i++)
                {
                    if (dst_mask_byte[i] != 0)
                    {
                        byte c = dst_byte[pixel_i];
                        dst_hist[0][c]++;
                        if (dst_hist[0][c] > dst_histMax[0])
                            dst_histMax[0] = dst_hist[0][c];

                        c = dst_byte[pixel_i + 1];
                        dst_hist[1][c]++;
                        if (dst_hist[1][c] > dst_histMax[1])
                            dst_histMax[1] = dst_hist[1][c];

                        c = dst_byte[pixel_i + 2];
                        dst_hist[2][c]++;
                        if (dst_hist[2][c] > dst_histMax[2])
                            dst_histMax[2] = dst_hist[2][c];
                    }
                    // Advance to next pixel
                    pixel_i += channels;
                }
            }
            else
            {
                for (int i = 0; i < total; i++)
                {
                    byte c = dst_byte[pixel_i];
                    dst_hist[0][c]++;
                    if (dst_hist[0][c] > dst_histMax[0])
                        dst_histMax[0] = dst_hist[0][c];

                    c = dst_byte[pixel_i + 1];
                    dst_hist[1][c]++;
                    if (dst_hist[1][c] > dst_histMax[1])
                        dst_histMax[1] = dst_hist[1][c];

                    c = dst_byte[pixel_i + 2];
                    dst_hist[2][c]++;
                    if (dst_hist[2][c] > dst_histMax[2])
                        dst_histMax[2] = dst_hist[2][c];

                    // Advance to next pixel
                    pixel_i += channels;
                }
            }

            //normalize hist
            for (int i = 0; i < 256; i++)
            {
                src_hist[0][i] /= src_histMax[0];
                src_hist[1][i] /= src_histMax[1];
                src_hist[2][i] /= src_histMax[2];

                dst_hist[0][i] /= dst_histMax[0];
                dst_hist[1][i] /= dst_histMax[1];
                dst_hist[2][i] /= dst_histMax[2];
            }

            // Calc cumulative distribution function (CDF) 
            src_cdf[0][0] = src_hist[0][0];
            src_cdf[1][0] = src_hist[1][0];
            src_cdf[2][0] = src_hist[2][0];
            dst_cdf[0][0] = dst_hist[0][0];
            dst_cdf[1][0] = dst_hist[1][0];
            dst_cdf[2][0] = dst_hist[2][0];
            for (int i = 1; i < 256; i++)
            {
                src_cdf[0][i] = src_cdf[0][i - 1] + src_hist[0][i];
                src_cdf[1][i] = src_cdf[1][i - 1] + src_hist[1][i];
                src_cdf[2][i] = src_cdf[2][i - 1] + src_hist[2][i];

                dst_cdf[0][i] = dst_cdf[0][i - 1] + dst_hist[0][i];
                dst_cdf[1][i] = dst_cdf[1][i - 1] + dst_hist[1][i];
                dst_cdf[2][i] = dst_cdf[2][i - 1] + dst_hist[2][i];
            }

            // Normalize CDF
            for (int i = 0; i < 256; i++)
            {
                src_cdf[0][i] /= src_cdf[0][255];
                src_cdf[1][i] /= src_cdf[1][255];
                src_cdf[2][i] /= src_cdf[2][255];

                dst_cdf[0][i] /= dst_cdf[0][255];
                dst_cdf[1][i] /= dst_cdf[1][255];
                dst_cdf[2][i] /= dst_cdf[2][255];
            }

            // Create lookup table
            const double HISTMATCH_EPSILON = 0.000001f;
            for (int i = 0; i < 3; i++)
            {
                int last = 0;
                for (int j = 0; j < 256; j++)
                {
                    double F1j = src_cdf[i][j];

                    for (int k = last; k < 256; k++)
                    {
                        double F2k = dst_cdf[i][k];
                        if (Math.Abs(F2k - F1j) < HISTMATCH_EPSILON || F2k > F1j)
                        {
                            LUT[(j * 3) + i] = (byte)k;
                            last = k;
                            break;
                        }
                    }
                }
            }

            LUTTex.LoadRawTextureData(LUT);
            LUTTex.Apply(false);
        }
    }
}