using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FaceMaskExample
{
    /// <summary>
    /// Frontal face checker. (Calculates from points of face landmark which was detected by using Dlib Face Landmark Detector)
    /// v 1.0.1
    /// </summary>
    public class FrontalFaceChecker
    {
        float imageWidth;
        float imageHeight;
        Point[] landmarkPoints = new Point[7];
        Matrix4x4 transformationM = new Matrix4x4();
        MatOfPoint3f objectPoints;
        MatOfPoint2f imagePoints;
        Mat rvec;
        Mat tvec;
        Mat rotM;
        Mat camMatrix;
        MatOfDouble distCoeffs;
        Matrix4x4 invertYM;
        Matrix4x4 invertZM;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceSwapperExample.FrontalFaceChecker"/> class.
        /// </summary>
        /// <param name="width">Width of the image which was used in the face landmark detection.</param>
        /// <param name="height">Height of the image which was used in the face landmark detection.</param>
        public FrontalFaceChecker(float width, float height)
        {
            imageWidth = width;
            imageHeight = height;

            for (int i = 0; i < landmarkPoints.Length; i++)
            {
                landmarkPoints[i] = new Point(0, 0);
            }

            objectPoints = new MatOfPoint3f(
                new Point3(-34, 90, 83),//l eye (Interpupillary breadth)
                new Point3(34, 90, 83),//r eye (Interpupillary breadth)
                new Point3(0.0, 50, 120),//nose (Nose top)
                new Point3(-26, 15, 83),//l mouse (Mouth breadth)
                new Point3(26, 15, 83),//r mouse (Mouth breadth)
                new Point3(-79, 90, 0.0),//l ear (Bitragion breadth)
                new Point3(79, 90, 0.0)//r ear (Bitragion breadth)
            );

            imagePoints = new MatOfPoint2f();

            rvec = new Mat(3, 1, CvType.CV_64FC1);
            tvec = new Mat(3, 1, CvType.CV_64FC1);

            rotM = new Mat(3, 3, CvType.CV_64FC1);

            float max_d = Mathf.Max(imageHeight, imageWidth);
            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, max_d);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, imageWidth / 2.0f);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, max_d);
            camMatrix.put(1, 2, imageHeight / 2.0f);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);

            distCoeffs = new MatOfDouble(0, 0, 0, 0);

            invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
            invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
        }

        public void Dispose()
        {
            if (objectPoints != null && !objectPoints.IsDisposed)
                objectPoints.Dispose();

            if (imagePoints != null && !imagePoints.IsDisposed)
                imagePoints.Dispose();

            if (rvec != null && !rvec.IsDisposed)
                rvec.Dispose();

            if (tvec != null && !tvec.IsDisposed)
                tvec.Dispose();

            if (rotM != null && !rotM.IsDisposed)
                rotM.Dispose();

            if (camMatrix != null && !camMatrix.IsDisposed)
                camMatrix.Dispose();

            if (distCoeffs != null && !distCoeffs.IsDisposed)
                distCoeffs.Dispose();
        }

        /// <summary>
        /// Gets the frontal face angles.
        /// </summary>
        /// <returns>Frontal face angles.</returns>
        /// <param name="points">Points of face landmark which was detected with Dlib.</param>
        public Vector3 GetFrontalFaceAngles(List<Vector2> points)
        {
            if (points.Count < 68)
                throw new ArgumentException("Invalid face landmark points", "points");

            landmarkPoints[0].x = (points[38].x + points[41].x) / 2;
            landmarkPoints[0].y = (points[38].y + points[41].y) / 2;
            landmarkPoints[1].x = (points[43].x + points[46].x) / 2;
            landmarkPoints[1].y = (points[43].y + points[46].y) / 2;
            landmarkPoints[2].x = points[30].x;
            landmarkPoints[2].y = points[30].y;
            landmarkPoints[3].x = points[48].x;
            landmarkPoints[3].y = points[48].y;
            landmarkPoints[4].x = points[54].x;
            landmarkPoints[4].y = points[54].y;
            landmarkPoints[5].x = points[0].x;
            landmarkPoints[5].y = points[0].y;
            landmarkPoints[6].x = points[16].x;
            landmarkPoints[6].y = points[16].y;

            // Normalize points.
            Point centerOffset = landmarkPoints[2] - new Point(imageWidth / 2, imageHeight / 2);
            for (int i = 0; i < landmarkPoints.Length; i++)
            {
                landmarkPoints[i] = landmarkPoints[i] - centerOffset;
            }

            imagePoints.fromArray(landmarkPoints);

            Calib3d.solvePnP(objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);

            double tvec_z = tvec.get(2, 0)[0];

            //Debug.Log (rvec.dump());
            //Debug.Log (tvec.dump());

            if (!double.IsNaN(tvec_z))
            {
                Calib3d.Rodrigues(rvec, rotM);

                //Debug.Log (rotM.dump());

                transformationM.SetRow(0, new Vector4((float)rotM.get(0, 0)[0], (float)rotM.get(0, 1)[0], (float)rotM.get(0, 2)[0], (float)tvec.get(0, 0)[0]));
                transformationM.SetRow(1, new Vector4((float)rotM.get(1, 0)[0], (float)rotM.get(1, 1)[0], (float)rotM.get(1, 2)[0], (float)tvec.get(1, 0)[0]));
                transformationM.SetRow(2, new Vector4((float)rotM.get(2, 0)[0], (float)rotM.get(2, 1)[0], (float)rotM.get(2, 2)[0], (float)tvec.get(2, 0)[0]));
                transformationM.SetRow(3, new Vector4(0, 0, 0, 1));

                transformationM = invertYM * transformationM * invertZM;

                Vector3 angles = ExtractRotationFromMatrix(ref transformationM).eulerAngles;

                //Debug.Log ("angles " + angles.x + " " + angles.y + " " + angles.z);

                float rotationX = (angles.x > 180) ? angles.x - 360 : angles.x;
                float rotationY = (angles.y > 180) ? angles.y - 360 : angles.y;
                float rotationZ = (tvec_z >= 0) ? (angles.z > 180) ? angles.z - 360 : angles.z : 180 - angles.z;

                if (tvec_z < 0)
                {
                    rotationX = -rotationX;
                    rotationY = -rotationY;
                    rotationZ = -rotationZ;
                }

                return new Vector3(rotationX, rotationY, rotationZ);
            }
            else
            {
                return new Vector3(0, 0, 0);
            }
        }

        /// <summary>
        /// Gets the frontal face rate.
        /// </summary>
        /// <returns>Frontal face rate.(a value of 0 to 1)</returns>
        /// <param name="points">Points of face landmark which was detected with Dlib.</param>
        public float GetFrontalFaceRate(List<Vector2> points)
        {
            Vector3 angles = GetFrontalFaceAngles(points);

            //Debug.Log ("angles " + angles.x + " " + angles.y + " " + angles.z);

            float angle = Mathf.Max(Mathf.Abs(angles.x), Mathf.Abs(angles.y));
            float rate = (angle <= 90) ? angle / 90 : 1;

            //Debug.Log ("ratio " + (1.0f - rate));

            return 1.0f - rate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        private Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }
    }
}
