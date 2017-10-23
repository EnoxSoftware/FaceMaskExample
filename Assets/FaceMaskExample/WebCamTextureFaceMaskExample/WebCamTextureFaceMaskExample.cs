using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DlibFaceLandmarkDetector;
using OpenCVForUnity;
using OpenCVForUnity.RectangleTrack;
using WebGLFileUploader;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace FaceMaskExample
{
    /// <summary>
    /// WebCamTexture FaceMask Example
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper), typeof(TrackedMeshOverlay))]
    public class WebCamTextureFaceMaskExample : MonoBehaviour
    {
        /// <summary>
        /// Determines if use dlib face detector.
        /// </summary>
        public bool useDlibFaceDetecter = false;

        /// <summary>
        /// The use dlib face detecter toggle.
        /// </summary>
        public Toggle useDlibFaceDetecterToggle;

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
        [Range (0.0f, 1.0f)]
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
        /// The upload face mask button.
        /// </summary>
        public Button uploadFaceMaskButton;

        /// <summary>
        /// The colors.
        /// </summary>
        Color32[] colors;
        
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
        /// The web cam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;
        
        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;
        
        /// <summary>
        /// The detection based tracker.
        /// </summary>
        RectangleTracker rectangleTracker;
        
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
        /// The face mask texture.
        /// </summary>
        Texture2D faceMaskTexture;
        
        /// <summary>
        /// The face mask mat.
        /// </summary>
        Mat faceMaskMat;

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

        #if UNITY_WEBGL && !UNITY_EDITOR
        Stack<IEnumerator> coroutines = new Stack<IEnumerator> ();
        #endif

        // Use this for initialization
        void Start ()
        {
            WebGLFileUploadManager.SetImageEncodeSetting (true);
            WebGLFileUploadManager.SetAllowedFileName ("\\.(png|jpe?g|gif)$");
            WebGLFileUploadManager.SetImageShrinkingSize (640, 480);
            WebGLFileUploadManager.onFileUploaded += OnFileUploaded;

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

            #if UNITY_WEBGL && !UNITY_EDITOR
            var getFilePath_Coroutine = GetFilePath ();
            coroutines.Push (getFilePath_Coroutine);
            StartCoroutine (getFilePath_Coroutine);
            #else
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath ("haarcascade_frontalface_alt.xml");
            sp_human_face_68_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("sp_human_face_68.dat");
            Run ();
            #endif
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator GetFilePath ()
        {
            var getFilePathAsync_0_Coroutine = OpenCVForUnity.Utils.getFilePathAsync ("haarcascade_frontalface_alt.xml", (result) => {
                haarcascade_frontalface_alt_xml_filepath = result;
            });
            coroutines.Push (getFilePathAsync_0_Coroutine);
            yield return StartCoroutine (getFilePathAsync_0_Coroutine);

            var getFilePathAsync_1_Coroutine = DlibFaceLandmarkDetector.Utils.getFilePathAsync ("sp_human_face_68.dat", (result) => {
                sp_human_face_68_dat_filepath = result;
            });
            coroutines.Push (getFilePathAsync_1_Coroutine);
            yield return StartCoroutine (getFilePathAsync_1_Coroutine);

            coroutines.Clear ();

            Run ();
            uploadFaceMaskButton.interactable = true;
        }
        #endif

        private void Run ()
        {
            meshOverlay = this.GetComponent<TrackedMeshOverlay> ();

            shader_FadeID = Shader.PropertyToID("_Fade");

            rectangleTracker = new RectangleTracker ();

            faceLandmarkDetector = new FaceLandmarkDetector (sp_human_face_68_dat_filepath);

            webCamTextureToMatHelper.Initialize ();

            displayFaceRectsToggle.isOn = displayFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            filterNonFrontalFacesToggle.isOn = filterNonFrontalFaces;
            displayDebugFacePointsToggle.isOn = displayDebugFacePoints;
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            colors = new Color32[webCamTextureMat.cols () * webCamTextureMat.rows ()];
            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);


            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            float width = gameObject.transform.localScale.x;
            float height = gameObject.transform.localScale.y;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            grayMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
            cascade = new CascadeClassifier (haarcascade_frontalface_alt_xml_filepath);
//            if (cascade.empty ()) {
//                Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
//            }

            frontalFaceChecker = new FrontalFaceChecker (width, height);

            meshOverlay.UpdateOverlayTransform (gameObject.transform);

            OnChangeFaceMaskButtonClick ();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");

            grayMat.Dispose ();

            rectangleTracker.Reset ();
            meshOverlay.Reset ();

            frontalFaceChecker.Dispose ();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update ()
        {

            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                // detect faces.
                List<OpenCVForUnity.Rect> detectResult = new List<OpenCVForUnity.Rect> ();
                if (useDlibFaceDetecter) {
                    OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbaMat);
                    List<UnityEngine.Rect> result = faceLandmarkDetector.Detect ();

                    foreach (var unityRect in result) {
                        detectResult.Add (new OpenCVForUnity.Rect ((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
                    }
                } else {
                    // convert image to greyscale.
                    Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

                    using (Mat equalizeHistMat = new Mat ())
                    using (MatOfRect faces = new MatOfRect ()) {
                        Imgproc.equalizeHist (grayMat, equalizeHistMat);

                        cascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size (equalizeHistMat.cols () * 0.15, equalizeHistMat.cols () * 0.15), new Size ());

                        detectResult = faces.toList ();
                    }

                    // adjust to Dilb's result.
                    foreach (OpenCVForUnity.Rect r in detectResult) {
                        r.y += (int)(r.height * 0.1f);
                    }
                }

                // face traking.
                rectangleTracker.UpdateTrackedObjects (detectResult);
                List<TrackedRect> trackedRects = new List<TrackedRect> ();
                rectangleTracker.GetObjects (trackedRects, true);
                
                // detect face landmark points.
                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbaMat);
                List<List<Vector2>> landmarkPoints = new List<List<Vector2>> ();
                for (int i = 0; i < trackedRects.Count; i++) {
                    TrackedRect tr = trackedRects [i];
                    UnityEngine.Rect rect = new UnityEngine.Rect (tr.x, tr.y, tr.width, tr.height);

                    List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);
                    landmarkPoints.Add (points);
                }

                // face masking.
                if (faceMaskTexture != null && landmarkPoints.Count >= 1) {
                    OpenCVForUnity.Utils.texture2DToMat (faceMaskTexture, faceMaskMat);

                    float imageWidth = meshOverlay.width;
                    float imageHeight = meshOverlay.height; 
                    float maskImageWidth = faceMaskTexture.width;
                    float maskImageHeight = faceMaskTexture.height;

                    TrackedRect tr;
                    TrackedMesh tm;
                    for (int i = 0; i < trackedRects.Count; i++) {
                        tr = trackedRects [i];

                        if (tr.state == TrackedState.NEW) {
                            meshOverlay.CreateObject (tr.id, faceMaskTexture);
                        }
                        if (tr.state < TrackedState.DELETED) {
                            tm = meshOverlay.GetObjectById (tr.id);

                            Vector3[] vertices = tm.meshFilter.mesh.vertices;
                            if (vertices.Length == landmarkPoints [i].Count) {
                                for (int j = 0; j < vertices.Length; j++) {
                                    vertices [j].x = landmarkPoints [i] [j].x / imageWidth - 0.5f;
                                    vertices [j].y = 0.5f - landmarkPoints [i] [j].y / imageHeight;
                                }
                            }
                            Vector2[] uv = tm.meshFilter.mesh.uv;
                            if (uv.Length == faceLandmarkPointsInMask.Count) {
                                for (int jj = 0; jj < uv.Length; jj++) {
                                    uv [jj].x = faceLandmarkPointsInMask [jj].x / maskImageWidth;
                                    uv [jj].y = (maskImageHeight - faceLandmarkPointsInMask [jj].y) / maskImageHeight;
                                }
                            }
                            meshOverlay.UpdateObject (tr.id, vertices, null, uv);

                            if (tr.numFramesNotDetected > 3) {
                                tm.material.SetFloat (shader_FadeID, 1f);
                            }else if (tr.numFramesNotDetected > 0 && tr.numFramesNotDetected <= 3) {
                                tm.material.SetFloat (shader_FadeID, 0.3f + (0.7f/4f) * tr.numFramesNotDetected);
                            } else {
                                tm.material.SetFloat (shader_FadeID, 0.3f);
                            }
                            
                            // filter non frontal faces.
                            if (filterNonFrontalFaces && frontalFaceChecker.GetFrontalFaceRate (landmarkPoints [i]) < frontalFaceRateLowerLimit) {
                                tm.material.SetFloat (shader_FadeID, 1f);
                            }

                        } else if (tr.state == TrackedState.DELETED) {
                            meshOverlay.DeleteObject (tr.id);
                        }
                    }
                } else if (landmarkPoints.Count >= 1) {

                    float imageWidth = meshOverlay.width;
                    float imageHeight = meshOverlay.height; 
                    float maskImageWidth = texture.width;
                    float maskImageHeight = texture.height;

                    TrackedRect tr;
                    TrackedMesh tm;
                    for (int i = 0; i < trackedRects.Count; i++) {
                        tr = trackedRects [i];
                        
                        if (tr.state == TrackedState.NEW) {
                            meshOverlay.CreateObject (tr.id, texture);
                        }
                        if (tr.state < TrackedState.DELETED) {
                            tm = meshOverlay.GetObjectById (tr.id);
                            
                            Vector3[] vertices = tm.meshFilter.mesh.vertices;
                            if (vertices.Length == landmarkPoints [i].Count) {
                                for (int j = 0; j < vertices.Length; j++) {
                                    vertices [j].x = landmarkPoints[i][j].x / imageWidth - 0.5f;
                                    vertices [j].y = 0.5f - landmarkPoints[i][j].y / imageHeight;
                                }
                            }
                            Vector2[] uv = tm.meshFilter.mesh.uv;
                            if (uv.Length == landmarkPoints [0].Count) {
                                for (int jj = 0; jj < uv.Length; jj++) {
                                    uv [jj].x = landmarkPoints[0][jj].x / maskImageWidth;
                                    uv [jj].y = (maskImageHeight - landmarkPoints[0][jj].y) / maskImageHeight;
                                }
                            }
                            meshOverlay.UpdateObject (tr.id, vertices, null, uv);

                            if (tr.numFramesNotDetected > 3) {
                                tm.material.SetFloat (shader_FadeID, 1f);
                            }else if (tr.numFramesNotDetected > 0 && tr.numFramesNotDetected <= 3) {
                                tm.material.SetFloat (shader_FadeID, 0.3f + (0.7f/4f) * tr.numFramesNotDetected);
                            } else {
                                tm.material.SetFloat (shader_FadeID, 0.3f);
                            }
                            
                            // filter nonsfrontal faces.
                            if (filterNonFrontalFaces && frontalFaceChecker.GetFrontalFaceRate (landmarkPoints [i]) < frontalFaceRateLowerLimit) {
                                tm.material.SetFloat (shader_FadeID, 1f);
                            }
                            
                        } else if (tr.state == TrackedState.DELETED) {
                            meshOverlay.DeleteObject (tr.id);
                        }
                    }
                }

                // draw face rects.
                if (displayFaceRects) {
                    for (int i = 0; i < detectResult.Count; i++) {
                        UnityEngine.Rect rect = new UnityEngine.Rect (detectResult [i].x, detectResult [i].y, detectResult [i].width, detectResult [i].height);
                        OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect, new Scalar (255, 0, 0, 255), 2);
                    }

                    for (int i = 0; i < trackedRects.Count; i++) {
                        UnityEngine.Rect rect = new UnityEngine.Rect (trackedRects [i].x, trackedRects [i].y, trackedRects [i].width, trackedRects [i].height);
                        OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect, new Scalar (255, 255, 0, 255), 2);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceChecker.GetFrontalFaceAngles (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceChecker.GetFrontalFaceRate (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                }

                // draw face points.
                if (displayDebugFacePoints) {
                    for (int i = 0; i < landmarkPoints.Count; i++) {
                        OpenCVForUnityUtils.DrawFaceLandmark (rgbaMat, landmarkPoints [i], new Scalar (0, 255, 0, 255), 2);
                    }
                }


                // display face mask image.
                if (faceMaskTexture != null && faceMaskMat != null) {

                    if (displayFaceRects) {
                        OpenCVForUnityUtils.DrawFaceRect (faceMaskMat, faceRectInMask, new Scalar (255, 0, 0, 255), 2);
                    }
                    if (displayDebugFacePoints) {
                        OpenCVForUnityUtils.DrawFaceLandmark (faceMaskMat, faceLandmarkPointsInMask, new Scalar (0, 255, 0, 255), 2);
                    }

                    float scale = (rgbaMat.width () / 4f) / faceMaskMat.width ();
                    float tx = rgbaMat.width () - faceMaskMat.width () * scale;
                    float ty = 0.0f;
                    Mat trans = new Mat (2, 3, CvType.CV_32F);//1.0, 0.0, tx, 0.0, 1.0, ty);
                    trans.put (0, 0, scale);
                    trans.put (0, 1, 0.0f);
                    trans.put (0, 2, tx);
                    trans.put (1, 0, 0.0f);
                    trans.put (1, 1, scale);
                    trans.put (1, 2, ty);
                    
                    Imgproc.warpAffine (faceMaskMat, rgbaMat, trans, rgbaMat.size (), Imgproc.INTER_LINEAR, Core.BORDER_TRANSPARENT, new Scalar (0));
                }

                Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

                OpenCVForUnity.Utils.matToTexture2D (rgbaMat, texture, colors);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            WebGLFileUploadManager.onFileUploaded -= OnFileUploaded;
            WebGLFileUploadManager.Dispose ();

            webCamTextureToMatHelper.Dispose ();

            if (cascade != null)
                cascade.Dispose ();

            if (rectangleTracker != null)
                rectangleTracker.Dispose ();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose ();

            #if UNITY_WEBGL && !UNITY_EDITOR
            foreach (var coroutine in coroutines) {
                StopCoroutine (coroutine);
                ((IDisposable)coroutine).Dispose ();
            }
            #endif
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("FaceMaskExample");
            #else
            Application.LoadLevel ("FaceMaskExample");
            #endif
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
            webCamTextureToMatHelper.Initialize (null, webCamTextureToMatHelper.requestedWidth, webCamTextureToMatHelper.requestedHeight, !webCamTextureToMatHelper.requestedIsFrontFacing);
        }

        /// <summary>
        /// Raises the use Dlib face detector toggle value changed event.
        /// </summary>
        public void OnUseDlibFaceDetecterToggleValueChanged ()
        {
            if (useDlibFaceDetecterToggle.isOn) {
                useDlibFaceDetecter = true;
            } else {
                useDlibFaceDetecter = false;
            }
        }

        /// <summary>
        /// Raises the filter non frontal faces toggle value changed event.
        /// </summary>
        public void OnFilterNonFrontalFacesToggleValueChanged ()
        {
            if (filterNonFrontalFacesToggle.isOn) {
                filterNonFrontalFaces = true;
            } else {
                filterNonFrontalFaces = false;
            }
        }
        
        /// <summary>
        /// Raises the display face rects toggle value changed event.
        /// </summary>
        public void OnDisplayFaceRectsToggleValueChanged ()
        {
            if (displayFaceRectsToggle.isOn) {
                displayFaceRects = true;
            } else {
                displayFaceRects = false;
            }
        }

        /// <summary>
        /// Raises the display debug face points toggle value changed event.
        /// </summary>
        public void OnDisplayDebugFacePointsToggleValueChanged ()
        {
            if (displayDebugFacePointsToggle.isOn) {
                displayDebugFacePoints = true;
            } else {
                displayDebugFacePoints = false;
            }
        }

        /// <summary>
        /// Raises the change face mask button click event.
        /// </summary>
        public void OnChangeFaceMaskButtonClick ()
        {
            RemoveFaceMask ();

            ExampleMaskData maskData = ExampleDataSet.GetData();

            faceMaskTexture = Resources.Load (maskData.fileName) as Texture2D;
            faceMaskMat = new Mat (faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC4);
            OpenCVForUnity.Utils.texture2DToMat (faceMaskTexture, faceMaskMat);
            Debug.Log ("faceMaskMat ToString " + faceMaskMat.ToString ());

            if(maskData.landmarkPoints != null){
                faceRectInMask = maskData.faceRect;
                faceLandmarkPointsInMask = maskData.landmarkPoints;
            }else{
                faceRectInMask = DetectFace (faceMaskMat);
                faceLandmarkPointsInMask = DetectFaceLandmarkPoints (faceMaskMat, faceRectInMask);
            }

            ExampleDataSet.Next();

            if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                RemoveFaceMask ();
                Debug.Log ("A face could not be detected from the input image.");
            }
        }

        /// <summary>
        /// Raises the scan face mask button click event.
        /// </summary>
        public void OnScanFaceMaskButtonClick ()
        {
            RemoveFaceMask ();

            // Capture webcam frame.
            if (webCamTextureToMatHelper.IsPlaying ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                faceRectInMask = DetectFace (rgbaMat);
                if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                    Debug.Log ("A face could not be detected from the input image.");
                    return;
                }

                OpenCVForUnity.Rect rect = new OpenCVForUnity.Rect((int)faceRectInMask.x, (int)faceRectInMask.y, (int)faceRectInMask.width, (int)faceRectInMask.height);
                rect.inflate(rect.x/5, rect.y/5);
                rect = rect.intersect(new OpenCVForUnity.Rect(0,0,rgbaMat.width(),rgbaMat.height()));

                faceMaskTexture = new Texture2D (rect.width, rect.height, TextureFormat.RGBA32, false);
                faceMaskMat = new Mat(rgbaMat, rect).clone ();
                OpenCVForUnity.Utils.matToTexture2D(faceMaskMat, faceMaskTexture);
                Debug.Log ("faceMaskMat ToString " + faceMaskMat.ToString ());

                faceRectInMask = DetectFace (faceMaskMat);
                faceLandmarkPointsInMask = DetectFaceLandmarkPoints (faceMaskMat, faceRectInMask);

                if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                    RemoveFaceMask ();
                    Debug.Log ("A face could not be detected from the input image.");
                }
            }
        }
        
        /// <summary>
        /// Raises the upload face mask button click event.
        /// </summary>
        public void OnUploadFaceMaskButtonClick ()
        {
            WebGLFileUploadManager.PopupDialog (null, "Select frontal face image file (.png|.jpg|.gif)");
        }
        
        /// <summary>
        /// Raises the remove face mask button click event.
        /// </summary>
        public void OnRemoveFaceMaskButtonClick ()
        {
            RemoveFaceMask ();
        }

        private void RemoveFaceMask ()
        {
            faceMaskTexture = null;
            if (faceMaskMat != null) {
                faceMaskMat.Dispose ();
                faceMaskMat = null;
            }
            rectangleTracker.Reset ();
            meshOverlay.Reset ();
        }

        /// <summary>
        /// Raises the file uploaded event.
        /// </summary>
        /// <param name="result">Uploaded file infos.</param>
        private void OnFileUploaded (UploadedFileInfo[] result)
        {
            if (result.Length == 0) {
                Debug.Log ("File upload Error!");
                return;
            }

            RemoveFaceMask ();
            
            foreach (UploadedFileInfo file in result) {
                if (file.isSuccess) {
                    Debug.Log ("file.filePath: " + file.filePath + " exists:" + File.Exists (file.filePath));
                    
                    faceMaskTexture = new Texture2D (2, 2);
                    byte[] byteArray = File.ReadAllBytes (file.filePath);
                    faceMaskTexture.LoadImage (byteArray);
                    
                    break;
                }
            }
            
            if (faceMaskTexture != null) {
                faceMaskMat = new Mat (faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC4);
                OpenCVForUnity.Utils.texture2DToMat (faceMaskTexture, faceMaskMat);
                Debug.Log ("faceMaskMat ToString " + faceMaskMat.ToString ());
                faceRectInMask = DetectFace (faceMaskMat);
                faceLandmarkPointsInMask = DetectFaceLandmarkPoints (faceMaskMat, faceRectInMask);
                
                if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                    RemoveFaceMask ();
                    Debug.Log ("A face could not be detected from the input image.");
                }
            }
        }
        
        private UnityEngine.Rect DetectFace (Mat mat)
        {
            if (useDlibFaceDetecter) {
                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, mat);
                List<UnityEngine.Rect> result = faceLandmarkDetector.Detect ();
                if (result.Count >= 1)
                    return result [0];
            } else {
                
                using (Mat grayMat = new Mat ())
                using (Mat equalizeHistMat = new Mat ())
                using (MatOfRect faces = new MatOfRect ()) {
                    // convert image to greyscale.
                    Imgproc.cvtColor (mat, grayMat, Imgproc.COLOR_RGBA2GRAY);
                    Imgproc.equalizeHist (grayMat, equalizeHistMat);
                    
                    cascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size (equalizeHistMat.cols () * 0.15, equalizeHistMat.cols () * 0.15), new Size ());

                    List<OpenCVForUnity.Rect> faceList = faces.toList ();
                    if (faceList.Count >= 1) {
                        UnityEngine.Rect r = new UnityEngine.Rect (faceList [0].x, faceList [0].y, faceList [0].width, faceList [0].height);
                        // adjust to Dilb's result.
                        r.y += (int)(r.height * 0.1f);
                        return r;
                    }
                }
            }
            return new UnityEngine.Rect ();
        }

        private List<Vector2> DetectFaceLandmarkPoints (Mat mat, UnityEngine.Rect rect)
        {
            OpenCVForUnityUtils.SetImage (faceLandmarkDetector, mat);
            List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);

            return points;
        }
    }
}