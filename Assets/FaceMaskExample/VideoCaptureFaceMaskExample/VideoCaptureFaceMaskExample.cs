using DlibFaceLandmarkDetector;
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
    /// VideoCapture FaceMask Example
    /// </summary>
    [RequireComponent(typeof(VideoCaptureToMatHelper), typeof(TrackedMeshOverlay))]
    public class VideoCaptureFaceMaskExample : MonoBehaviour
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
        /// The video capture to mat helper.
        /// </summary>
        VideoCaptureToMatHelper sourceToMatHelper;

        /// <summary>
        /// VIDEO_FILENAME
        /// </summary>
        protected static readonly string VIDEO_FILENAME = "dance_mjpeg.mjpeg";

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

            sourceToMatHelper = gameObject.GetComponent<VideoCaptureToMatHelper>();

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

            if (string.IsNullOrEmpty(sourceToMatHelper.requestedVideoFilePath))
                sourceToMatHelper.requestedVideoFilePath = VIDEO_FILENAME;
            sourceToMatHelper.outputColorFormat = VideoCaptureToMatHelper.ColorFormat.RGB;
            sourceToMatHelper.Initialize();

            displayFaceRectsToggle.isOn = displayFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            enableNoiseFilterToggle.isOn = enableNoiseFilter;
            enableColorCorrectionToggle.isOn = enableColorCorrection;
            filterNonFrontalFacesToggle.isOn = filterNonFrontalFaces;
            displayDebugFacePointsToggle.isOn = displayDebugFacePoints;

        }

        /// <summary>
        /// Raises the video capture to mat helper initialized event.
        /// </summary>
        public void OnVideoCaptureToMatHelperInitialized()
        {
            Debug.Log("OnVideoCaptureToMatHelperInitialized");

            Mat rgbMat = sourceToMatHelper.GetMat();

            texture = new Texture2D(rgbMat.cols(), rgbMat.rows(), TextureFormat.RGB24, false);


            gameObject.transform.localScale = new Vector3(rgbMat.cols(), rgbMat.rows(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", rgbMat.width().ToString());
                fpsMonitor.Add("height", rgbMat.height().ToString());
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

            grayMat = new Mat(rgbMat.rows(), rgbMat.cols(), CvType.CV_8UC1);
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
        /// Raises the video capture to mat helper disposed event.
        /// </summary>
        public void OnVideoCaptureToMatHelperDisposed()
        {
            Debug.Log("OnVideoCaptureToMatHelperDisposed");

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
        /// Raises the video capture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnVideoCaptureToMatHelperErrorOccurred(VideoCaptureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnVideoCaptureToMatHelperErrorOccurred " + errorCode);

            if (fpsMonitor != null)
            {
                fpsMonitor.consoleText = "ErrorCode: " + errorCode;
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (sourceToMatHelper.IsPlaying() && sourceToMatHelper.DidUpdateThisFrame())
            {

                Mat rgbMat = sourceToMatHelper.GetMat();

                // detect faces.
                List<Rect> detectResult = new List<Rect>();
                if (useDlibFaceDetecter)
                {
                    OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbMat);
                    List<UnityEngine.Rect> result = faceLandmarkDetector.Detect();

                    foreach (var unityRect in result)
                    {
                        detectResult.Add(new Rect((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
                    }
                }
                else
                {
                    // convert image to greyscale.
                    Imgproc.cvtColor(rgbMat, grayMat, Imgproc.COLOR_RGB2GRAY);

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
                OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbMat);
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
                            opticalFlowFilterDict[tr.id].Process(rgbMat, points, points);
                            lowPassFilterDict[tr.id].Process(rgbMat, points, points);
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
                                CorrectFaceMaskColor(tr.id, faceMaskMat, rgbMat, faceLandmarkPointsInMask, landmarkPoints[i]);
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
                                CorrectFaceMaskColor(tr.id, rgbMat, rgbMat, landmarkPoints[0], landmarkPoints[i]);
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
                        OpenCVForUnityUtils.DrawFaceRect(rgbMat, rect, new Scalar(255, 0, 0, 255), 2);
                    }

                    for (int i = 0; i < trackedRects.Count; i++)
                    {
                        UnityEngine.Rect rect = new UnityEngine.Rect(trackedRects[i].x, trackedRects[i].y, trackedRects[i].width, trackedRects[i].height);
                        OpenCVForUnityUtils.DrawFaceRect(rgbMat, rect, new Scalar(255, 255, 0, 255), 2);
                        //Imgproc.putText (rgbMat, " " + frontalFaceChecker.GetFrontalFaceAngles (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                        //Imgproc.putText (rgbMat, " " + frontalFaceChecker.GetFrontalFaceRate (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                }

                // draw face points.
                if (displayDebugFacePoints)
                {
                    for (int i = 0; i < landmarkPoints.Count; i++)
                    {
                        OpenCVForUnityUtils.DrawFaceLandmark(rgbMat, landmarkPoints[i], new Scalar(0, 255, 0, 255), 2);
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

                    float scale = (rgbMat.width() / 4f) / faceMaskMat.width();
                    float tx = rgbMat.width() - faceMaskMat.width() * scale;
                    float ty = 0.0f;
                    Mat trans = new Mat(2, 3, CvType.CV_32F);//1.0, 0.0, tx, 0.0, 1.0, ty);
                    trans.put(0, 0, scale);
                    trans.put(0, 1, 0.0f);
                    trans.put(0, 2, tx);
                    trans.put(1, 0, 0.0f);
                    trans.put(1, 1, scale);
                    trans.put(1, 2, ty);

                    Imgproc.warpAffine(faceMaskMat, rgbMat, trans, rgbMat.size(), Imgproc.INTER_LINEAR, Core.BORDER_TRANSPARENT, new Scalar(0));

                    if (displayFaceRects || displayDebugFacePointsToggle)
                        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(faceMaskTexture, faceMaskMat);
                }

                //Imgproc.putText (rgbMat, "W:" + rgbMat.width () + " H:" + rgbMat.height () + " SO:" + Screen.orientation, new Point (5, rgbMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

                OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(rgbMat, texture);
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

            if (tm == null) return;

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

            if (tm == null) return;

            tm.sharedMaterial.SetTexture(shader_LUTTexID, LUTTex);
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (sourceToMatHelper != null)
                sourceToMatHelper.Dispose();

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
            sourceToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            sourceToMatHelper.Pause();
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
            faceMaskMat = new Mat(faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC3);
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

            /*
            DumpFaceRect (faceRectInMask);
            DumpLandMarkPoints (faceLandmarkPointsInMask);
            */

            /*
            if (maskData.name == "Panda")
            {
                UnityEngine.Rect faceRect;
                List<Vector2> landmarkPoints;
                CreatePandaMaskData(out faceRect, out landmarkPoints);
                SetFaceMaskData(maskData, faceRect, landmarkPoints);
            }
            else if (maskData.name == "Anime")
            {
                UnityEngine.Rect faceRect;
                List<Vector2> landmarkPoints;
                CreateAnimeMaskData(out faceRect, out landmarkPoints);
                SetFaceMaskData(maskData, faceRect, landmarkPoints);
            }
            */
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
                    Imgproc.cvtColor(mat, grayMat, Imgproc.COLOR_RGB2GRAY);
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

        /*
        private void DumpFaceRect(UnityEngine.Rect faceRect)
        {
            Debug.Log("== DumpFaceRect ==");
            Debug.Log("new Rect(" + faceRect.x + ", " + faceRect.y + ", " + faceRect.width + ", " + faceRect.height + ");");
            Debug.Log("==================");
        }

        private void DumpLandMarkPoints(List<Vector2> landmarkPoints)
        {
            Debug.Log("== DumpLandMarkPoints ==");
            string str = "";
            for (int i = 0; i < landmarkPoints.Count; i++)
            {
                str = str + "new Vector2(" + landmarkPoints[i].x + ", " + landmarkPoints[i].y + ")";
                if (i < landmarkPoints.Count - 1)
                {
                    str = str + "," + "\n";
                }
            }
            Debug.Log(str);
            Debug.Log("==================");
        }

        private void SetFaceMaskData(FaceMaskData data, UnityEngine.Rect faceRect, List<Vector2> landmarkPoints)
        {
            data.faceRect = faceRect;
            data.landmarkPoints = landmarkPoints;
        }

        private void CreatePandaMaskData(out UnityEngine.Rect faceRect, out List<Vector2> landmarkPoints)
        {
            faceRect = new UnityEngine.Rect(17, 64, 261, 205);

            landmarkPoints = new List<Vector2>() {
        new Vector2 (31, 136),
        new Vector2 (23, 169),
        new Vector2 (26, 195),
        new Vector2 (35, 216),
        new Vector2 (53, 236),
        new Vector2 (71, 251),
        new Vector2 (96, 257),
        new Vector2 (132, 259),
        new Vector2 (143, 263),
        //9
        new Vector2 (165, 258),
        new Vector2 (198, 255),
        new Vector2 (222, 242),
        new Vector2 (235, 231),
        new Vector2 (248, 215),
        new Vector2 (260, 195),
        new Vector2 (272, 171),
        new Vector2 (264, 135),
        //17
        new Vector2 (45, 115),
        new Vector2 (70, 94),
        new Vector2 (97, 89),
        new Vector2 (116, 90),
        new Vector2 (135, 105),
        new Vector2 (157, 104),
        new Vector2 (176, 90),
        new Vector2 (198, 86),
        new Vector2 (223, 90),
        new Vector2 (248, 110),
        //27
        new Vector2 (148, 134),
        new Vector2 (147, 152),
        new Vector2 (145, 174),
        new Vector2 (144, 192),
        new Vector2 (117, 205),
        new Vector2 (128, 213),
        new Vector2 (143, 216),
        new Vector2 (160, 216),
        new Vector2 (174, 206),
        //36
        new Vector2 (96, 138),
        new Vector2 (101, 131),
        new Vector2 (111, 132),
        new Vector2 (114, 140),
        new Vector2 (109, 146),
        new Vector2 (100, 146),
        new Vector2 (180, 138),
        new Vector2 (186, 130),
        new Vector2 (195, 131),
        new Vector2 (199, 137),
        new Vector2 (195, 143),
        new Vector2 (185, 143),
        //48
        new Vector2 (109, 235),
        new Vector2 (118, 231),
        new Vector2 (129, 228),
        new Vector2 (143, 225),
        new Vector2 (156, 227),
        new Vector2 (174, 232),
        new Vector2 (181, 234),
        new Vector2 (173, 241),
        new Vector2 (156, 245),
        new Vector2 (143, 245),
        new Vector2 (130, 244),
        new Vector2 (117, 239),
        new Vector2 (114, 235),
        new Vector2 (130, 232),
        new Vector2 (142, 232),
        new Vector2 (157, 233),
        new Vector2 (175, 236),
        new Vector2 (155, 237),
        new Vector2 (143, 238),
        new Vector2 (130, 237)
            };
        }

        private void CreateAnimeMaskData(out UnityEngine.Rect faceRect, out List<Vector2> landmarkPoints)
        {
            faceRect = new UnityEngine.Rect(56, 85, 190, 196);

            landmarkPoints = new List<Vector2>() {
        new Vector2(62, 179),
        new Vector2(72, 209),
        new Vector2(75, 223),
        new Vector2(81, 236),
        new Vector2(90, 244),
        new Vector2(101, 251),
        new Vector2(116, 258),
        new Vector2(129, 262),
        new Vector2(142, 268),
        new Vector2(160, 265),
        new Vector2(184, 260),
        new Vector2(202, 253),
        new Vector2(210, 247),
        new Vector2(217, 239),
        new Vector2(222, 229),
        new Vector2(225, 222),
        new Vector2(243, 191),
        //17
        new Vector2(68, 136),
        new Vector2(86, 128),
        new Vector2(104, 126),
        new Vector2(122, 131),
        new Vector2(134, 141),
        new Vector2(177, 143),
        new Vector2(191, 135),
        new Vector2(209, 132),
        new Vector2(227, 136),
        new Vector2(239, 143),
        //27
        new Vector2(153, 163),
        new Vector2(150, 190),
        new Vector2(149, 201),
        new Vector2(148, 212),
        new Vector2(138, 217),
        new Vector2(141, 219),
        new Vector2(149, 221),
        new Vector2(152, 220),
        new Vector2(155, 217),
        //36
        new Vector2(70, 182),
        new Vector2(85, 165),
        new Vector2(114, 168),
        new Vector2(122, 192),
        new Vector2(113, 211),
        new Vector2(82, 209),
        new Vector2(177, 196),
        new Vector2(189, 174),
        new Vector2(220, 175),
        new Vector2(234, 192),
        new Vector2(215, 220),
        new Vector2(184, 217),
        //48
        new Vector2(132, 249),
        new Vector2(134, 249),
        new Vector2(139, 250),
        new Vector2(144, 251),
        new Vector2(148, 251),
        new Vector2(153, 250),
        new Vector2(155, 251),
        new Vector2(154, 253),
        new Vector2(149, 257),
        new Vector2(144, 257),
        new Vector2(138, 256),
        new Vector2(133, 252),
        new Vector2(133, 250),
        new Vector2(139, 252),
        new Vector2(144, 254),
        new Vector2(148, 253),
        new Vector2(153, 251),
        new Vector2(148, 254),
        new Vector2(144, 254),
        new Vector2(139, 253)
            };
        }
        */
    }
}
