﻿using DlibFaceLandmarkDetector;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.RectangleTrack;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rect = OpenCVForUnity.CoreModule.Rect;

namespace FaceMaskExample
{
    /// <summary>
    /// WebCamTexture FaceMask Example
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper), typeof(TrackedMeshOverlay))]
    public class WebCamTextureFaceMaskExample : MonoBehaviour
    {
        [HeaderAttribute("FaceMaskData")]

        /// <summary>
        /// The face mask data list.
        /// </summary>
        public List<FaceMaskData> faceMaskDatas;

        [HeaderAttribute("Option")]

        /// <summary>
        /// Determines if use dlib face detector.
        /// </summary>
        public bool useDlibFaceDetecter = false;

        /// <summary>
        /// The use dlib face detecter toggle.
        /// </summary>
        public Toggle useDlibFaceDetecterToggle;

        /// <summary>
        /// Determines if enables noise filter.
        /// </summary>
        public bool enableNoiseFilter = true;

        /// <summary>
        /// The enable noise filter toggle.
        /// </summary>
        public Toggle enableNoiseFilterToggle;

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
        /// The gray mat.
        /// </summary>
        Mat grayMat;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The cascade.
        /// </summary>
        CascadeClassifier cascade;

        /// <summary>
        /// The detection based tracker.
        /// </summary>
        RectangleTracker rectangleTracker;

        /// <summary>
        /// The web cam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The mean points filter dictionary.
        /// </summary>
        Dictionary<int, LowPassPointsFilter> lowPassFilterDict;

        /// <summary>
        /// The optical flow points filter dictionary.
        /// </summary>
        Dictionary<int, OFPointsFilter> opticalFlowFilterDict;

        /// <summary>
        /// The face mask color corrector.
        /// </summary>
        FaceMaskColorCorrector faceMaskColorCorrector;

        /// <summary>
        /// The frontal face checker.
        /// </summary>
        FrontalFaceChecker frontalFaceChecker;

        /// <summary>
        /// The mesh overlay.
        /// </summary>
        TrackedMeshOverlay meshOverlay;

        /// <summary>
        /// The Shader.PropertyToID for "_Fade".
        /// </summary>
        int shader_FadeID;

        /// <summary>
        /// The Shader.PropertyToID for "_ColorCorrection".
        /// </summary>
        int shader_ColorCorrectionID;

        /// <summary>
        /// The Shader.PropertyToID for "_LUTTex".
        /// </summary>
        int shader_LUTTexID;

        /// <summary>
        /// The face mask texture.
        /// </summary>
        Texture2D faceMaskTexture;

        /// <summary>
        /// The face mask mat.
        /// </summary>
        Mat faceMaskMat;

        /// <summary>
        /// The index number of face mask data.
        /// </summary>
        int faceMaskDataIndex = 0;

        /// <summary>
        /// The detected face rect in mask mat.
        /// </summary>
        UnityEngine.Rect faceRectInMask;

        /// <summary>
        /// The detected face landmark points in mask mat.
        /// </summary>
        List<Vector2> faceLandmarkPointsInMask;

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

#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        // Use this for initialization
        void Start()
        {
            fpsMonitor = GetComponent<FpsMonitor>();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

#if UNITY_WEBGL
            getFilePath_Coroutine = GetFilePath();
            StartCoroutine(getFilePath_Coroutine);
#else
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.UnityUtils.Utils.getFilePath("haarcascade_frontalface_alt.xml");
            sp_human_face_68_dat_filepath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath("sp_human_face_68.dat");
            Run();
#endif
        }

#if UNITY_WEBGL
        private IEnumerator GetFilePath()
        {
            var getFilePathAsync_0_Coroutine = OpenCVForUnity.UnityUtils.Utils.getFilePathAsync("haarcascade_frontalface_alt.xml", (result) =>
            {
                haarcascade_frontalface_alt_xml_filepath = result;
            });
            yield return getFilePathAsync_0_Coroutine;

            var getFilePathAsync_1_Coroutine = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePathAsync("sp_human_face_68.dat", (result) =>
            {
                sp_human_face_68_dat_filepath = result;
            });
            yield return getFilePathAsync_1_Coroutine;

            getFilePath_Coroutine = null;

            Run();
        }
#endif

        private void Run()
        {
            meshOverlay = this.GetComponent<TrackedMeshOverlay>();

            shader_FadeID = Shader.PropertyToID("_Fade");
            shader_ColorCorrectionID = Shader.PropertyToID("_ColorCorrection");
            shader_LUTTexID = Shader.PropertyToID("_LUTTex");

            rectangleTracker = new RectangleTracker();

            faceLandmarkDetector = new FaceLandmarkDetector(sp_human_face_68_dat_filepath);

            lowPassFilterDict = new Dictionary<int, LowPassPointsFilter>();
            opticalFlowFilterDict = new Dictionary<int, OFPointsFilter>();

            faceMaskColorCorrector = new FaceMaskColorCorrector();

            webCamTextureToMatHelper.Initialize();

            displayFaceRectsToggle.isOn = displayFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            enableNoiseFilterToggle.isOn = enableNoiseFilter;
            enableColorCorrectionToggle.isOn = enableColorCorrection;
            filterNonFrontalFacesToggle.isOn = filterNonFrontalFaces;
            displayDebugFacePointsToggle.isOn = displayDebugFacePoints;
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);


            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", webCamTextureMat.width().ToString());
                fpsMonitor.Add("height", webCamTextureMat.height().ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }


            float width = gameObject.transform.localScale.x;
            float height = gameObject.transform.localScale.y;

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

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            grayMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC1);
            cascade = new CascadeClassifier(haarcascade_frontalface_alt_xml_filepath);
            //if (cascade.empty())
            //{
            //    Debug.LogError("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            //}

            frontalFaceChecker = new FrontalFaceChecker(width, height);

            meshOverlay.UpdateOverlayTransform(gameObject.transform);

            OnChangeFaceMaskButtonClick();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            grayMat.Dispose();

            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }

            rectangleTracker.Reset();
            meshOverlay.Reset();

            foreach (var key in lowPassFilterDict.Keys)
            {
                lowPassFilterDict[key].Dispose();
            }
            lowPassFilterDict.Clear();
            foreach (var key in opticalFlowFilterDict.Keys)
            {
                opticalFlowFilterDict[key].Dispose();
            }
            opticalFlowFilterDict.Clear();

            faceMaskColorCorrector.Reset();

            frontalFaceChecker.Dispose();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update()
        {

            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                // detect faces.
                List<Rect> detectResult = new List<Rect>();
                if (useDlibFaceDetecter)
                {
                    OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                    List<UnityEngine.Rect> result = faceLandmarkDetector.Detect();

                    foreach (var unityRect in result)
                    {
                        detectResult.Add(new Rect((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
                    }
                }
                else
                {
                    // convert image to greyscale.
                    Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

                    using (Mat equalizeHistMat = new Mat())
                    using (MatOfRect faces = new MatOfRect())
                    {
                        Imgproc.equalizeHist(grayMat, equalizeHistMat);

                        cascade.detectMultiScale(equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new Size(equalizeHistMat.cols() * 0.15, equalizeHistMat.cols() * 0.15), new Size());

                        detectResult = faces.toList();
                    }

                    // corrects the deviation of a detection result between OpenCV and Dlib.
                    foreach (Rect r in detectResult)
                    {
                        r.y += (int)(r.height * 0.1f);
                    }
                }


                // face tracking.
                rectangleTracker.UpdateTrackedObjects(detectResult);
                List<TrackedRect> trackedRects = new List<TrackedRect>();
                rectangleTracker.GetObjects(trackedRects, true);

                // create noise filter.
                foreach (var openCVRect in trackedRects)
                {
                    if (openCVRect.state == TrackedState.NEW)
                    {
                        if (!lowPassFilterDict.ContainsKey(openCVRect.id))
                            lowPassFilterDict.Add(openCVRect.id, new LowPassPointsFilter((int)faceLandmarkDetector.GetShapePredictorNumParts()));
                        if (!opticalFlowFilterDict.ContainsKey(openCVRect.id))
                            opticalFlowFilterDict.Add(openCVRect.id, new OFPointsFilter((int)faceLandmarkDetector.GetShapePredictorNumParts()));
                    }
                    else if (openCVRect.state == TrackedState.DELETED)
                    {
                        if (lowPassFilterDict.ContainsKey(openCVRect.id))
                        {
                            lowPassFilterDict[openCVRect.id].Dispose();
                            lowPassFilterDict.Remove(openCVRect.id);
                        }
                        if (opticalFlowFilterDict.ContainsKey(openCVRect.id))
                        {
                            opticalFlowFilterDict[openCVRect.id].Dispose();
                            opticalFlowFilterDict.Remove(openCVRect.id);
                        }
                    }
                }

                // create LUT texture.
                foreach (var openCVRect in trackedRects)
                {
                    if (openCVRect.state == TrackedState.NEW)
                    {
                        faceMaskColorCorrector.CreateLUTTex(openCVRect.id);
                    }
                    else if (openCVRect.state == TrackedState.DELETED)
                    {
                        faceMaskColorCorrector.DeleteLUTTex(openCVRect.id);
                    }
                }

                // detect face landmark points.
                OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                List<List<Vector2>> landmarkPoints = new List<List<Vector2>>();
                for (int i = 0; i < trackedRects.Count; i++)
                {
                    TrackedRect tr = trackedRects[i];
                    UnityEngine.Rect rect = new UnityEngine.Rect(tr.x, tr.y, tr.width, tr.height);

                    List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);

                    // apply noise filter.
                    if (enableNoiseFilter)
                    {
                        if (tr.state > TrackedState.NEW && tr.state < TrackedState.DELETED)
                        {
                            opticalFlowFilterDict[tr.id].Process(rgbaMat, points, points);
                            lowPassFilterDict[tr.id].Process(rgbaMat, points, points);
                        }
                    }

                    landmarkPoints.Add(points);
                }

                // face masking.
                if (faceMaskTexture != null && landmarkPoints.Count >= 1)
                { // Apply face masking between detected faces and a face mask image.

                    float maskImageWidth = faceMaskTexture.width;
                    float maskImageHeight = faceMaskTexture.height;

                    TrackedRect tr;

                    for (int i = 0; i < trackedRects.Count; i++)
                    {
                        tr = trackedRects[i];

                        if (tr.state == TrackedState.NEW)
                        {
                            meshOverlay.CreateObject(tr.id, faceMaskTexture);
                        }
                        if (tr.state < TrackedState.DELETED)
                        {
                            MaskFace(meshOverlay, tr, landmarkPoints[i], faceLandmarkPointsInMask, maskImageWidth, maskImageHeight);

                            if (enableColorCorrection)
                            {
                                CorrectFaceMaskColor(tr.id, faceMaskMat, rgbaMat, faceLandmarkPointsInMask, landmarkPoints[i]);
                            }
                        }
                        else if (tr.state == TrackedState.DELETED)
                        {
                            meshOverlay.DeleteObject(tr.id);
                        }
                    }
                }
                else if (landmarkPoints.Count >= 1)
                { // Apply face masking between detected faces.

                    float maskImageWidth = texture.width;
                    float maskImageHeight = texture.height;

                    TrackedRect tr;

                    for (int i = 0; i < trackedRects.Count; i++)
                    {
                        tr = trackedRects[i];

                        if (tr.state == TrackedState.NEW)
                        {
                            meshOverlay.CreateObject(tr.id, texture);
                        }
                        if (tr.state < TrackedState.DELETED)
                        {
                            MaskFace(meshOverlay, tr, landmarkPoints[i], landmarkPoints[0], maskImageWidth, maskImageHeight);

                            if (enableColorCorrection)
                            {
                                CorrectFaceMaskColor(tr.id, rgbaMat, rgbaMat, landmarkPoints[0], landmarkPoints[i]);
                            }
                        }
                        else if (tr.state == TrackedState.DELETED)
                        {
                            meshOverlay.DeleteObject(tr.id);
                        }
                    }
                }

                // draw face rects.
                if (displayFaceRects)
                {
                    for (int i = 0; i < detectResult.Count; i++)
                    {
                        UnityEngine.Rect rect = new UnityEngine.Rect(detectResult[i].x, detectResult[i].y, detectResult[i].width, detectResult[i].height);
                        OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect, new Scalar(255, 0, 0, 255), 2);
                    }

                    for (int i = 0; i < trackedRects.Count; i++)
                    {
                        UnityEngine.Rect rect = new UnityEngine.Rect(trackedRects[i].x, trackedRects[i].y, trackedRects[i].width, trackedRects[i].height);
                        OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect, new Scalar(255, 255, 0, 255), 2);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceChecker.GetFrontalFaceAngles (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceChecker.GetFrontalFaceRate (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                }

                // draw face points.
                if (displayDebugFacePoints)
                {
                    for (int i = 0; i < landmarkPoints.Count; i++)
                    {
                        OpenCVForUnityUtils.DrawFaceLandmark(rgbaMat, landmarkPoints[i], new Scalar(0, 255, 0, 255), 2);
                    }
                }


                // display face mask image.
                if (faceMaskTexture != null && faceMaskMat != null)
                {

                    if (displayFaceRects)
                    {
                        OpenCVForUnityUtils.DrawFaceRect(faceMaskMat, faceRectInMask, new Scalar(255, 0, 0, 255), 2);
                    }
                    if (displayDebugFacePoints)
                    {
                        OpenCVForUnityUtils.DrawFaceLandmark(faceMaskMat, faceLandmarkPointsInMask, new Scalar(0, 255, 0, 255), 2);
                    }

                    float scale = (rgbaMat.width() / 4f) / faceMaskMat.width();
                    float tx = rgbaMat.width() - faceMaskMat.width() * scale;
                    float ty = 0.0f;
                    Mat trans = new Mat(2, 3, CvType.CV_32F);//1.0, 0.0, tx, 0.0, 1.0, ty);
                    trans.put(0, 0, scale);
                    trans.put(0, 1, 0.0f);
                    trans.put(0, 2, tx);
                    trans.put(1, 0, 0.0f);
                    trans.put(1, 1, scale);
                    trans.put(1, 2, ty);

                    Imgproc.warpAffine(faceMaskMat, rgbaMat, trans, rgbaMat.size(), Imgproc.INTER_LINEAR, Core.BORDER_TRANSPARENT, new Scalar(0));

                    if (displayFaceRects || displayDebugFacePointsToggle)
                        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(faceMaskTexture, faceMaskMat);
                }

                //Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

                OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(rgbaMat, texture);
            }
        }

        private void MaskFace(TrackedMeshOverlay meshOverlay, TrackedRect tr, List<Vector2> landmarkPoints, List<Vector2> landmarkPointsInMaskImage, float maskImageWidth = 0, float maskImageHeight = 0)
        {
            float imageWidth = meshOverlay.width;
            float imageHeight = meshOverlay.height;

            if (maskImageWidth == 0)
                maskImageWidth = imageWidth;

            if (maskImageHeight == 0)
                maskImageHeight = imageHeight;

            TrackedMesh tm = meshOverlay.GetObjectById(tr.id);

            Vector3[] vertices = tm.meshFilter.mesh.vertices;
            if (vertices.Length == landmarkPoints.Count)
            {
                for (int j = 0; j < vertices.Length; j++)
                {
                    vertices[j].x = landmarkPoints[j].x / imageWidth - 0.5f;
                    vertices[j].y = 0.5f - landmarkPoints[j].y / imageHeight;
                }
            }
            Vector2[] uv = tm.meshFilter.mesh.uv;
            if (uv.Length == landmarkPointsInMaskImage.Count)
            {
                for (int jj = 0; jj < uv.Length; jj++)
                {
                    uv[jj].x = landmarkPointsInMaskImage[jj].x / maskImageWidth;
                    uv[jj].y = (maskImageHeight - landmarkPointsInMaskImage[jj].y) / maskImageHeight;
                }
            }
            meshOverlay.UpdateObject(tr.id, vertices, null, uv);

            if (tr.numFramesNotDetected > 3)
            {
                tm.sharedMaterial.SetFloat(shader_FadeID, 1f);
            }
            else if (tr.numFramesNotDetected > 0 && tr.numFramesNotDetected <= 3)
            {
                tm.sharedMaterial.SetFloat(shader_FadeID, 0.3f + (0.7f / 4f) * tr.numFramesNotDetected);
            }
            else
            {
                tm.sharedMaterial.SetFloat(shader_FadeID, 0.3f);
            }

            if (enableColorCorrection)
            {
                tm.sharedMaterial.SetFloat(shader_ColorCorrectionID, 1f);
            }
            else
            {
                tm.sharedMaterial.SetFloat(shader_ColorCorrectionID, 0f);
            }

            // filter non frontal faces.
            if (filterNonFrontalFaces && frontalFaceChecker.GetFrontalFaceRate(landmarkPoints) < frontalFaceRateLowerLimit)
            {
                tm.sharedMaterial.SetFloat(shader_FadeID, 1f);
            }
        }

        private void CorrectFaceMaskColor(int id, Mat src, Mat dst, List<Vector2> src_landmarkPoints, List<Vector2> dst_landmarkPoints)
        {
            Texture2D LUTTex = faceMaskColorCorrector.UpdateLUTTex(id, src, dst, src_landmarkPoints, dst_landmarkPoints);
            TrackedMesh tm = meshOverlay.GetObjectById(id);
            tm.sharedMaterial.SetTexture(shader_LUTTexID, LUTTex);
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose();

            if (cascade != null)
                cascade.Dispose();

            if (rectangleTracker != null)
                rectangleTracker.Dispose();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose();

            foreach (var key in lowPassFilterDict.Keys)
            {
                lowPassFilterDict[key].Dispose();
            }
            lowPassFilterDict.Clear();
            foreach (var key in opticalFlowFilterDict.Keys)
            {
                opticalFlowFilterDict[key].Dispose();
            }
            opticalFlowFilterDict.Clear();

            if (faceMaskColorCorrector != null)
                faceMaskColorCorrector.Dispose();

#if UNITY_WEBGL
            if (getFilePath_Coroutine != null)
            {
                StopCoroutine(getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose();
            }
#endif
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("FaceMaskExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            webCamTextureToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            webCamTextureToMatHelper.Pause();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing();
        }

        /// <summary>
        /// Raises the use Dlib face detector toggle value changed event.
        /// </summary>
        public void OnUseDlibFaceDetecterToggleValueChanged()
        {
            if (useDlibFaceDetecterToggle.isOn)
            {
                useDlibFaceDetecter = true;
            }
            else
            {
                useDlibFaceDetecter = false;
            }
        }

        /// <summary>
        /// Raises the enable noise filter toggle value changed event.
        /// </summary>
        public void OnEnableNoiseFilterToggleValueChanged()
        {
            if (enableNoiseFilterToggle.isOn)
            {
                enableNoiseFilter = true;
                foreach (var key in lowPassFilterDict.Keys)
                {
                    lowPassFilterDict[key].Reset();
                }
                foreach (var key in opticalFlowFilterDict.Keys)
                {
                    opticalFlowFilterDict[key].Reset();
                }
            }
            else
            {
                enableNoiseFilter = false;
            }
        }

        /// <summary>
        /// Raises the enable color correction toggle value changed event.
        /// </summary>
        public void OnEnableColorCorrectionToggleValueChanged()
        {
            if (enableColorCorrectionToggle.isOn)
            {
                enableColorCorrection = true;
            }
            else
            {
                enableColorCorrection = false;
            }
        }

        /// <summary>
        /// Raises the filter non frontal faces toggle value changed event.
        /// </summary>
        public void OnFilterNonFrontalFacesToggleValueChanged()
        {
            if (filterNonFrontalFacesToggle.isOn)
            {
                filterNonFrontalFaces = true;
            }
            else
            {
                filterNonFrontalFaces = false;
            }
        }

        /// <summary>
        /// Raises the display face rects toggle value changed event.
        /// </summary>
        public void OnDisplayFaceRectsToggleValueChanged()
        {
            if (displayFaceRectsToggle.isOn)
            {
                displayFaceRects = true;
            }
            else
            {
                displayFaceRects = false;
            }
        }

        /// <summary>
        /// Raises the display debug face points toggle value changed event.
        /// </summary>
        public void OnDisplayDebugFacePointsToggleValueChanged()
        {
            if (displayDebugFacePointsToggle.isOn)
            {
                displayDebugFacePoints = true;
            }
            else
            {
                displayDebugFacePoints = false;
            }
        }

        /// <summary>
        /// Raises the change face mask button click event.
        /// </summary>
        public void OnChangeFaceMaskButtonClick()
        {
            RemoveFaceMask();

            if (faceMaskDatas.Count == 0)
                return;

            FaceMaskData maskData = faceMaskDatas[faceMaskDataIndex];
            faceMaskDataIndex = (faceMaskDataIndex < faceMaskDatas.Count - 1) ? faceMaskDataIndex + 1 : 0;

            if (maskData == null)
            {
                Debug.LogError("maskData == null");
                return;
            }

            if (maskData.image == null)
            {
                Debug.LogError("image == null");
                return;
            }

            if (maskData.landmarkPoints.Count != 68)
            {
                Debug.LogError("landmarkPoints.Count != 68");
                return;
            }

            faceMaskTexture = maskData.image;
            faceMaskMat = new Mat(faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC4);
            OpenCVForUnity.UnityUtils.Utils.texture2DToMat(faceMaskTexture, faceMaskMat);

            if (maskData.isDynamicMode)
            {
                faceRectInMask = DetectFace(faceMaskMat);
                faceLandmarkPointsInMask = DetectFaceLandmarkPoints(faceMaskMat, faceRectInMask);

                maskData.faceRect = faceRectInMask;
                maskData.landmarkPoints = faceLandmarkPointsInMask;
            }
            else
            {
                faceRectInMask = maskData.faceRect;
                faceLandmarkPointsInMask = maskData.landmarkPoints;
            }

            if (faceRectInMask.width == 0 && faceRectInMask.height == 0)
            {
                RemoveFaceMask();
                Debug.LogError("A face could not be detected from the input image.");
            }

            enableColorCorrectionToggle.isOn = maskData.enableColorCorrection;
        }

        /// <summary>
        /// Raises the scan face mask button click event.
        /// </summary>
        public void OnScanFaceMaskButtonClick()
        {
            RemoveFaceMask();

            // Capture webcam frame.
            if (webCamTextureToMatHelper.IsPlaying())
            {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                faceRectInMask = DetectFace(rgbaMat);
                if (faceRectInMask.width == 0 && faceRectInMask.height == 0)
                {
                    Debug.Log("A face could not be detected from the input image.");
                    return;
                }

                Rect rect = new Rect((int)faceRectInMask.x, (int)faceRectInMask.y, (int)faceRectInMask.width, (int)faceRectInMask.height);
                rect.inflate(rect.x / 5, rect.y / 5);
                rect = rect.intersect(new Rect(0, 0, rgbaMat.width(), rgbaMat.height()));

                faceMaskTexture = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
                faceMaskMat = new Mat(rgbaMat, rect).clone();
                OpenCVForUnity.UnityUtils.Utils.matToTexture2D(faceMaskMat, faceMaskTexture);
                Debug.Log("faceMaskMat ToString " + faceMaskMat.ToString());

                faceRectInMask = DetectFace(faceMaskMat);
                faceLandmarkPointsInMask = DetectFaceLandmarkPoints(faceMaskMat, faceRectInMask);

                if (faceRectInMask.width == 0 && faceRectInMask.height == 0)
                {
                    RemoveFaceMask();
                    Debug.Log("A face could not be detected from the input image.");
                }
            }
        }

        /// <summary>
        /// Raises the remove face mask button click event.
        /// </summary>
        public void OnRemoveFaceMaskButtonClick()
        {
            RemoveFaceMask();
        }

        private void RemoveFaceMask()
        {
            faceMaskTexture = null;
            if (faceMaskMat != null)
            {
                faceMaskMat.Dispose();
                faceMaskMat = null;
            }

            rectangleTracker.Reset();
            meshOverlay.Reset();
        }

        private UnityEngine.Rect DetectFace(Mat mat)
        {
            if (useDlibFaceDetecter)
            {
                OpenCVForUnityUtils.SetImage(faceLandmarkDetector, mat);
                List<UnityEngine.Rect> result = faceLandmarkDetector.Detect();
                if (result.Count >= 1)
                    return result[0];
            }
            else
            {

                using (Mat grayMat = new Mat())
                using (Mat equalizeHistMat = new Mat())
                using (MatOfRect faces = new MatOfRect())
                {
                    // convert image to greyscale.
                    Imgproc.cvtColor(mat, grayMat, Imgproc.COLOR_RGBA2GRAY);
                    Imgproc.equalizeHist(grayMat, equalizeHistMat);

                    cascade.detectMultiScale(equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new Size(equalizeHistMat.cols() * 0.15, equalizeHistMat.cols() * 0.15), new Size());

                    List<Rect> faceList = faces.toList();
                    if (faceList.Count >= 1)
                    {
                        UnityEngine.Rect r = new UnityEngine.Rect(faceList[0].x, faceList[0].y, faceList[0].width, faceList[0].height);
                        // corrects the deviation of a detection result between OpenCV and Dlib.
                        r.y += (int)(r.height * 0.1f);
                        return r;
                    }
                }
            }
            return new UnityEngine.Rect();
        }

        private List<Vector2> DetectFaceLandmarkPoints(Mat mat, UnityEngine.Rect rect)
        {
            OpenCVForUnityUtils.SetImage(faceLandmarkDetector, mat);
            List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);

            return points;
        }
    }
}