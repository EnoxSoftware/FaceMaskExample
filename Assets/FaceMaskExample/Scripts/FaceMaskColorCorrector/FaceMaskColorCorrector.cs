using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using System;
using System.Collections.Generic;
using UnityEngine;
using Rect = OpenCVForUnity.CoreModule.Rect;

namespace FaceMaskExample
{
    public class FaceMaskColorCorrector
    {
        Mat src_mask;
        Mat dst_mask;
        Dictionary<int, Texture2D> LUTTexDict;

        Point[] src_facialContourPoints;
        Point[] dst_facialContourPoints;

        public FaceMaskColorCorrector()
        {
            LUTTexDict = new Dictionary<int, Texture2D>();

            src_facialContourPoints = new Point[9];
            for (int i = 0; i < src_facialContourPoints.Length; i++)
            {
                src_facialContourPoints[i] = new Point();
            }

            dst_facialContourPoints = new Point[9];
            for (int i = 0; i < dst_facialContourPoints.Length; i++)
            {
                dst_facialContourPoints[i] = new Point();
            }
        }

        public virtual void CreateLUTTex(int id)
        {
            if (!LUTTexDict.ContainsKey(id))
                LUTTexDict.Add(id, new Texture2D(256, 1, TextureFormat.RGB24, false));
        }

        public virtual Texture2D UpdateLUTTex(int id, Mat src, Mat dst, List<Vector2> src_landmarkPoints, List<Vector2> dst_landmarkPoints)
        {
            if (src_mask != null && (src.width() != src_mask.width() || src.height() != src_mask.height()))
            {
                src_mask.Dispose();
                src_mask = null;
            }
            src_mask = src_mask ?? new Mat(src.rows(), src.cols(), CvType.CV_8UC1, Scalar.all(0));

            if (dst_mask != null && (dst.width() != dst_mask.width() || dst.height() != dst_mask.height()))
            {
                dst_mask.Dispose();
                dst_mask = null;
            }
            dst_mask = dst_mask ?? new Mat(dst.rows(), dst.cols(), CvType.CV_8UC1, Scalar.all(0));

            // Get facial contour points.
            GetFacialContourPoints(src_landmarkPoints, src_facialContourPoints);
            GetFacialContourPoints(dst_landmarkPoints, dst_facialContourPoints);

            // Get facial contour rect.
            Rect src_facialContourRect = Imgproc.boundingRect(new MatOfPoint(src_facialContourPoints));
            Rect dst_facialContourRect = Imgproc.boundingRect(new MatOfPoint(dst_facialContourPoints));
            src_facialContourRect = src_facialContourRect.intersect(new Rect(0, 0, src.width(), src.height()));
            dst_facialContourRect = dst_facialContourRect.intersect(new Rect(0, 0, dst.width(), dst.height()));

            Mat src_ROI = new Mat(src, src_facialContourRect);
            Mat dst_ROI = new Mat(dst, dst_facialContourRect);
            Mat src_mask_ROI = new Mat(src_mask, src_facialContourRect);
            Mat dst_mask_ROI = new Mat(dst_mask, dst_facialContourRect);

            GetPointsInFrame(src_mask_ROI, src_facialContourPoints, src_facialContourPoints);
            GetPointsInFrame(dst_mask_ROI, dst_facialContourPoints, dst_facialContourPoints);

            src_mask_ROI.setTo(new Scalar(0));
            dst_mask_ROI.setTo(new Scalar(0));
            Imgproc.fillConvexPoly(src_mask_ROI, new MatOfPoint(src_facialContourPoints), new Scalar(255));
            Imgproc.fillConvexPoly(dst_mask_ROI, new MatOfPoint(dst_facialContourPoints), new Scalar(255));

            Texture2D LUTTex;
            if (LUTTexDict.ContainsKey(id))
            {
                LUTTex = LUTTexDict[id];
            }
            else
            {
                LUTTex = new Texture2D(256, 1, TextureFormat.RGB24, false);
                LUTTexDict.Add(id, LUTTex);
            }

            FaceMaskShaderUtils.CalculateLUT(src_ROI, dst_ROI, src_mask_ROI, dst_mask_ROI, LUTTex);

            return LUTTex;
        }

        public virtual void DeleteLUTTex(int id)
        {
            if (LUTTexDict.ContainsKey(id))
            {
                Texture2D.Destroy(LUTTexDict[id]);
                LUTTexDict.Remove(id);
            }
        }

        public virtual Texture2D GetLUTTex(int id)
        {
            if (LUTTexDict.ContainsKey(id))
            {
                return LUTTexDict[id];
            }
            return null;
        }

        protected virtual void GetFacialContourPoints(List<Vector2> landmark_points, Point[] dst_points)
        {
            if (landmark_points.Count < 9)
                throw new ArgumentException("Invalid landmark_points.");

            if (dst_points.Length != 9)
                throw new ArgumentException("Invalid points.");

            dst_points[0].x = landmark_points[0].x;
            dst_points[0].y = landmark_points[0].y;
            dst_points[1].x = landmark_points[3].x;
            dst_points[1].y = landmark_points[3].y;
            dst_points[2].x = landmark_points[5].x;
            dst_points[2].y = landmark_points[5].y;
            dst_points[3].x = landmark_points[8].x;
            dst_points[3].y = landmark_points[8].y;
            dst_points[4].x = landmark_points[11].x;
            dst_points[4].y = landmark_points[11].y;
            dst_points[5].x = landmark_points[13].x;
            dst_points[5].y = landmark_points[13].y;
            dst_points[6].x = landmark_points[16].x;
            dst_points[6].y = landmark_points[16].y;
            float nose_length_x = landmark_points[27].x - landmark_points[30].x;
            float nose_length_y = landmark_points[27].y - landmark_points[30].y;
            dst_points[7].x = landmark_points[26].x + nose_length_x;
            dst_points[7].y = landmark_points[26].y + nose_length_y;
            dst_points[8].x = landmark_points[17].x + nose_length_x;
            dst_points[8].y = landmark_points[17].y + nose_length_y;
        }

        protected virtual void GetPointsInFrame(Mat frame, Point[] points, Point[] dst_points)
        {
            if (points.Length != dst_points.Length)
                throw new ArgumentException("points.Length != dst_points.Length");

            Size wholesize = new Size();
            Point ofs = new Point();
            frame.locateROI(wholesize, ofs);

            for (int i = 0; i < points.Length; i++)
            {
                dst_points[i].x = points[i].x - ofs.x;
                dst_points[i].y = points[i].y - ofs.y;
            }
        }

        public virtual void Reset()
        {
            foreach (var key in LUTTexDict.Keys)
            {
                Texture2D.Destroy(LUTTexDict[key]);
            }
            LUTTexDict.Clear();
        }

        public virtual void Dispose()
        {
            if (src_mask != null)
            {
                src_mask.Dispose();
                src_mask = null;
            }

            if (dst_mask != null)
            {
                dst_mask.Dispose();
                dst_mask = null;
            }

            Reset();
        }
    }
}