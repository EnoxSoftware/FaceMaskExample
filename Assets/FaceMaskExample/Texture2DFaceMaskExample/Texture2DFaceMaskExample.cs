using DlibFaceLandmarkDetector;
using DlibFaceLandmarkDetector.UnityIntegration;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rect = OpenCVForUnity.CoreModule.Rect;

namespace FaceMaskExample
{
    /// <summary>
    /// Texture2D FaceMask Example
    /// </summary>
    [RequireComponent(typeof(TrackedMeshOverlay))]
    public class Texture2DFaceMaskExample : MonoBehaviour
    {
        /// <summary>
        /// Determines if use dlib face detector.
        /// </summary>
        public bool useDlibFaceDetecter = true;

        /// <summary>
        /// The use dlib face detecter toggle.
        /// </summary>
        public Toggle useDlibFaceDetecterToggle;

        /// <summary>
        /// Determines if enables color correction.
        /// </summary>
        public bool enableColorCorrection = true;

        /// <summary>
        /// The enable color correction toggle.
        /// </summary>
        public Toggle enableColorCorrectionToggle;

        /// <summary>
        /// Determines if filters non frontal faces.
        /// </summary>
        public bool filterNonFrontalFaces = false;

        /// <summary>
        /// The filter non frontal faces toggle.
        /// </summary>
        public Toggle filterNonFrontalFacesToggle;

        /// <summary>
        /// The frontal face rate lower limit.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float frontalFaceRateLowerLimit = 0.85f;

        /// <summary>
        /// Determines if displays face rects.
        /// </summary>
        public bool displayFaceRects = false;

        /// <summary>
        /// The toggle for switching face rects display state
        /// </summary>
        public Toggle displayFaceRectsToggle;

        /// <summary>
        /// Determines if displays debug face points.
        /// </summary>
        public bool displayDebugFacePoints = false;

        /// <summary>
        /// The toggle for switching debug face points display state.
        /// </summary>
        public Toggle displayDebugFacePointsToggle;

        /// <summary>
        /// The image texture.
        /// </summary>
        Texture2D imgTexture;

        /// <summary>
        /// The cascade.
        /// </summary>
        CascadeClassifier cascade;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The face mask color corrector.
        /// </summary>
        FaceMaskColorCorrector faceMaskColorCorrector;

        /// <summary>
        /// The mesh overlay.
        /// </summary>
        TrackedMeshOverlay meshOverlay;

        /// <summary>
        /// The haarcascade_frontalface_alt_xml_filepath.
        /// </summary>
        string haarcascade_frontalface_alt_xml_filepath;

        /// <summary>
        /// The sp_human_face_68_dat_filepath.
        /// </summary>
        string sp_human_face_68_dat_filepath;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        /// <summary>
        /// The CancellationTokenSource.
        /// </summary>
        CancellationTokenSource cts = new CancellationTokenSource();

        // Use this for initialization
        async void Start()
        {
            fpsMonitor = GetComponent<FpsMonitor>();

            // Asynchronously retrieves the readable file path from the StreamingAssets directory.
            if (fpsMonitor != null)
                fpsMonitor.ConsoleText = "Preparing file access...";

            haarcascade_frontalface_alt_xml_filepath = await OpenCVEnv.GetFilePathTaskAsync("DlibFaceLandmarkDetector/haarcascade_frontalface_alt.xml", cancellationToken: cts.Token);
            sp_human_face_68_dat_filepath = await OpenCVEnv.GetFilePathTaskAsync("DlibFaceLandmarkDetector/sp_human_face_68.dat", cancellationToken: cts.Token);

            if (fpsMonitor != null)
                fpsMonitor.ConsoleText = "";

            Run();
        }

        private void Run()
        {
            meshOverlay = this.GetComponent<TrackedMeshOverlay>();

            displayFaceRectsToggle.isOn = displayFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            enableColorCorrectionToggle.isOn = enableColorCorrection;
            filterNonFrontalFacesToggle.isOn = filterNonFrontalFaces;
            displayDebugFacePointsToggle.isOn = displayDebugFacePoints;

            if (imgTexture == null)
                imgTexture = Resources.Load("family") as Texture2D;

            // Adjust the scale of the game object to match the dimensions of the texture
            gameObject.transform.localScale = new Vector3(imgTexture.width, imgTexture.height, 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            // Adjust the orthographic size of the main Camera to fit the aspect ratio of the image
            float width = 0;
            float height = 0;
            width = gameObject.transform.localScale.x;
            height = gameObject.transform.localScale.y;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }


            meshOverlay.UpdateOverlayTransform(gameObject.transform);
            meshOverlay.Reset();


            Mat rgbaMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);

            OpenCVMatUtils.Texture2DToMat(imgTexture, rgbaMat);
            Debug.Log("rgbaMat ToString " + rgbaMat.ToString());

            if (faceLandmarkDetector == null)
                faceLandmarkDetector = new FaceLandmarkDetector(sp_human_face_68_dat_filepath);

            faceMaskColorCorrector = faceMaskColorCorrector ?? new FaceMaskColorCorrector();
            FrontalFaceChecker frontalFaceChecker = new FrontalFaceChecker(width, height);

            // detect faces.
            List<Rect> detectResult = new List<Rect>();
            if (useDlibFaceDetecter)
            {
                DlibOpenCVUtils.SetImage(faceLandmarkDetector, rgbaMat);
                List<UnityEngine.Rect> result = faceLandmarkDetector.Detect();

                foreach (var unityRect in result)
                {
                    detectResult.Add(new Rect((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
                }
            }
            else
            {
                if (cascade == null)
                    cascade = new CascadeClassifier(haarcascade_frontalface_alt_xml_filepath);
                //if (cascade.empty())
                //{
                //    Debug.LogError("cascade file is not loaded. Please copy from “DlibFaceLandmarkDetector/StreamingAssets/DlibFaceLandmarkDetector/” to “Assets/StreamingAssets/DlibFaceLandmarkDetector/” folder. ");
                //}

                // convert image to greyscale.
                Mat gray = new Mat();
                Imgproc.cvtColor(rgbaMat, gray, Imgproc.COLOR_RGBA2GRAY);

                MatOfRect faces = new MatOfRect();
                Imgproc.equalizeHist(gray, gray);
                cascade.detectMultiScale(gray, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new Size(gray.cols() * 0.05, gray.cols() * 0.05), new Size());
                //Debug.Log ("faces " + faces.dump ());

                detectResult = faces.toList();

                // corrects the deviation of a detection result between OpenCV and Dlib.
                foreach (Rect r in detectResult)
                {
                    r.y += (int)(r.height * 0.1f);
                }

                gray.Dispose();
            }

            // detect face landmark points.
            DlibOpenCVUtils.SetImage(faceLandmarkDetector, rgbaMat);
            List<List<Vector2>> landmarkPoints = new List<List<Vector2>>();
            foreach (var openCVRect in detectResult)
            {
                UnityEngine.Rect rect = new UnityEngine.Rect(openCVRect.x, openCVRect.y, openCVRect.width, openCVRect.height);

                Debug.Log("face : " + rect);
                //DlibOpenCVUtils.DrawFaceRect(imgMat, rect, new Scalar(255, 0, 0, 255), 2);

                List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);
                //DlibOpenCVUtils.DrawFaceLandmark(imgMat, points, new Scalar(0, 255, 0, 255), 2);
                landmarkPoints.Add(points);
            }

            // mask faces.
            int[] face_nums = new int[landmarkPoints.Count];
            for (int i = 0; i < face_nums.Length; i++)
            {
                face_nums[i] = i;
            }
            face_nums = face_nums.OrderBy(i => System.Guid.NewGuid()).ToArray();

            float imageWidth = meshOverlay.width;
            float imageHeight = meshOverlay.height;
            float maskImageWidth = imgTexture.width;
            float maskImageHeight = imgTexture.height;

            TrackedMesh tm;
            for (int i = 0; i < face_nums.Length; i++)
            {

                meshOverlay.CreateObject(i, imgTexture);
                tm = meshOverlay.GetObjectById(i);

                Vector3[] vertices = tm.meshFilter.mesh.vertices;
                if (vertices.Length == landmarkPoints[face_nums[i]].Count)
                {
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        vertices[j].x = landmarkPoints[face_nums[i]][j].x / imageWidth - 0.5f;
                        vertices[j].y = 0.5f - landmarkPoints[face_nums[i]][j].y / imageHeight;
                    }
                }
                Vector2[] uv = tm.meshFilter.mesh.uv;
                if (uv.Length == landmarkPoints[face_nums[0]].Count)
                {
                    for (int jj = 0; jj < uv.Length; jj++)
                    {
                        uv[jj].x = landmarkPoints[face_nums[0]][jj].x / maskImageWidth;
                        uv[jj].y = (maskImageHeight - landmarkPoints[face_nums[0]][jj].y) / maskImageHeight;
                    }
                }
                meshOverlay.UpdateObject(i, vertices, null, uv);

                if (enableColorCorrection)
                {
                    faceMaskColorCorrector.CreateLUTTex(i);
                    Texture2D LUTTex = faceMaskColorCorrector.UpdateLUTTex(i, rgbaMat, rgbaMat, landmarkPoints[face_nums[0]], landmarkPoints[face_nums[i]]);
                    tm.sharedMaterial.SetTexture("_LUTTex", LUTTex);
                    tm.sharedMaterial.SetFloat("_ColorCorrection", 1f);
                }
                else
                {
                    tm.sharedMaterial.SetFloat("_ColorCorrection", 0f);
                }

                // filter non frontal faces.
                if (filterNonFrontalFaces && frontalFaceChecker.GetFrontalFaceRate(landmarkPoints[i]) < frontalFaceRateLowerLimit)
                {
                    tm.sharedMaterial.SetFloat("_Fade", 1f);
                }
                else
                {
                    tm.sharedMaterial.SetFloat("_Fade", 0.3f);
                }
            }

            // draw face rects.
            if (displayFaceRects)
            {
                int ann = face_nums[0];
                UnityEngine.Rect rect_ann = new UnityEngine.Rect(detectResult[ann].x, detectResult[ann].y, detectResult[ann].width, detectResult[ann].height);
                DlibOpenCVUtils.DrawFaceRect(rgbaMat, rect_ann, new Scalar(255, 255, 0, 255), 2);

                int bob = 0;
                for (int i = 1; i < face_nums.Length; i++)
                {
                    bob = face_nums[i];
                    UnityEngine.Rect rect_bob = new UnityEngine.Rect(detectResult[bob].x, detectResult[bob].y, detectResult[bob].width, detectResult[bob].height);
                    DlibOpenCVUtils.DrawFaceRect(rgbaMat, rect_bob, new Scalar(255, 0, 0, 255), 2);
                }
            }

            // draw face points.
            if (displayDebugFacePoints)
            {
                for (int i = 0; i < landmarkPoints.Count; i++)
                {
                    DlibOpenCVUtils.DrawFaceLandmark(rgbaMat, landmarkPoints[i], new Scalar(0, 255, 0, 255), 2);
                }
            }


            Texture2D texture = new Texture2D(rgbaMat.cols(), rgbaMat.rows(), TextureFormat.RGBA32, false);
            OpenCVMatUtils.MatToTexture2D(rgbaMat, texture);

            // Set the Texture2D as the main texture of the Renderer component attached to the game object
            gameObject.transform.GetComponent<Renderer>().material.mainTexture = texture;

            frontalFaceChecker.Dispose();
            rgbaMat.Dispose();
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (faceMaskColorCorrector != null)
                faceMaskColorCorrector.Dispose();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose();

            if (cascade != null)
                cascade.Dispose();

            if (cts != null)
                cts.Dispose();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("FaceMaskExample");
        }

        /// <summary>
        /// Raises the shuffle button click event.
        /// </summary>
        public void OnShuffleButtonClick()
        {
            if (imgTexture != null)
                Run();
        }

        /// <summary>
        /// Raises the use Dlib face detector toggle value changed event.
        /// </summary>
        public void OnUseDlibFaceDetecterToggleValueChanged()
        {
            useDlibFaceDetecter = useDlibFaceDetecterToggle.isOn;

            if (imgTexture != null)
                Run();
        }

        /// <summary>
        /// Raises the enable color correction toggle value changed event.
        /// </summary>
        public void OnEnableColorCorrectionToggleValueChanged()
        {
            enableColorCorrection = enableColorCorrectionToggle.isOn;

            if (imgTexture != null)
                Run();
        }

        /// <summary>
        /// Raises the filter non frontal faces toggle value changed event.
        /// </summary>
        public void OnFilterNonFrontalFacesToggleValueChanged()
        {
            filterNonFrontalFaces = filterNonFrontalFacesToggle.isOn;

            if (imgTexture != null)
                Run();
        }

        /// <summary>
        /// Raises the display face rects toggle value changed event.
        /// </summary>
        public void OnDisplayFaceRectsToggleValueChanged()
        {
            displayFaceRects = displayFaceRectsToggle.isOn;

            if (imgTexture != null)
                Run();
        }

        /// <summary>
        /// Raises the display debug face points toggle value changed event.
        /// </summary>
        public void OnDisplayDebugFacePointsToggleValueChanged()
        {
            displayDebugFacePoints = displayDebugFacePointsToggle.isOn;

            if (imgTexture != null)
                Run();
        }
    }
}