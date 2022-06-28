using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;

namespace FaceMaskExample
{
    public class AlphaMaskTextureCreater
    {
        /// <summary>
        /// Creates an alpha mask texture.
        /// </summary>
        /// <returns>An alpha mask texture.</returns>
        /// <param name="width">The texture width.</param>
        /// <param name="height">The texture height.</param>
        /// <param name="baseArea">The base area.(An array of points in UV coordinate system)</param>
        /// <param name="exclusionAreas">Exclusion areas.(An array of points in UV coordinate system)</param>
        public static Texture2D CreateAlphaMaskTexture(float width, float height, Vector2[] baseArea, params Vector2[][] exclusionAreas)
        {
            Mat baseAreaMaskMat = new Mat((int)height, (int)width, CvType.CV_8UC4);
            baseAreaMaskMat.setTo(new Scalar(0, 0, 0, 255));
            Point[] baseAreaPoints = new Point[baseArea.Length];
            for (int i = 0; i < baseArea.Length; i++)
            {
                baseAreaPoints[i] = new Point(baseArea[i].x * width, height - baseArea[i].y * height);
            }
            Imgproc.fillConvexPoly(baseAreaMaskMat, new MatOfPoint(baseAreaPoints), Scalar.all(255), Imgproc.LINE_AA, 0);
            //Imgproc.erode(baseAreaMaskMat, baseAreaMaskMat, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size (width * 0.01, height * 0.01)), new Point(-1, -1), 1, Core.BORDER_CONSTANT, new Scalar(0, 0, 0, 255));
            Imgproc.blur(baseAreaMaskMat, baseAreaMaskMat, new Size(width * 0.03, height * 0.03));


            Mat exclusionAreaMaskMat = new Mat((int)height, (int)width, CvType.CV_8UC4);
            exclusionAreaMaskMat.setTo(new Scalar(0, 0, 0, 255));
            foreach (Vector2[] exclusionArea in exclusionAreas)
            {
                Point[] points = new Point[exclusionArea.Length];
                for (int i = 0; i < exclusionArea.Length; i++)
                {
                    points[i] = new Point(exclusionArea[i].x * width, height - exclusionArea[i].y * height);
                }
                Imgproc.fillConvexPoly(exclusionAreaMaskMat, new MatOfPoint(points), Scalar.all(255), Imgproc.LINE_AA, 0);
            }
            //Imgproc.dilate(exclusionAreaMaskMat, exclusionAreaMaskMat, Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size (width * 0.002, height * 0.002)), new Point(-1, -1), 1, Core.BORDER_CONSTANT, new Scalar(0));
            Imgproc.blur(exclusionAreaMaskMat, exclusionAreaMaskMat, new Size(width * 0.01, height * 0.01), new Point(-1, -1), Core.BORDER_CONSTANT);


            Mat maskMat = new Mat((int)height, (int)width, CvType.CV_8UC4);
            Core.bitwise_xor(baseAreaMaskMat, exclusionAreaMaskMat, maskMat);

            Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            Utils.matToTexture2D(maskMat, texture);

            maskMat.Dispose();
            baseAreaMaskMat.Dispose();
            exclusionAreaMaskMat.Dispose();

            return texture;
        }
    }
}