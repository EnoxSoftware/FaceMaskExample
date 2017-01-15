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

namespace FaceMaskSample
{
    /// <summary>
    /// WebCamTexture face mask sample.
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper), typeof(TrackedMeshOverlay))]
    public class WebCamTextureFaceMaskSample : MonoBehaviour
    {
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
        /// The frontal face parameter.
        /// </summary>
        FrontalFaceParam frontalFaceParam;

        /// <summary>
        /// The is showing face rects.
        /// </summary>
        public bool isShowingFaceRects = false;

        /// <summary>
        /// The is showing face rects toggle.
        /// </summary>
        public Toggle isShowingFaceRectsToggle;

        /// <summary>
        /// The use Dlib face detector flag.
        /// </summary>
        public bool useDlibFaceDetecter = false;

        /// <summary>
        /// The use dlib face detecter toggle.
        /// </summary>
        public Toggle useDlibFaceDetecterToggle;

        /// <summary>
        /// The is filtering non frontal faces.
        /// </summary>
        public bool isFilteringNonFrontalFaces;

        /// <summary>
        /// The is filtering non frontal faces toggle.
        /// </summary>
        public Toggle isFilteringNonFrontalFacesToggle;

        /// <summary>
        /// The frontal face rate lower limit.
        /// </summary>
        [Range (0.0f, 1.0f)]
        public float
            frontalFaceRateLowerLimit = 0.85f;

        /// <summary>
        /// The is showing debug face points.
        /// </summary>
        public bool isShowingDebugFacePoints = false;

        /// <summary>
        /// The is showing debug face points toggle.
        /// </summary>
        public Toggle isShowingDebugFacePointsToggle;

        /// <summary>
        /// The is upload face mask button.
        /// </summary>
        public Button uploadFaceMaskButton;
        
        /// <summary>
        /// The mesh overlay.
        /// </summary>
        private TrackedMeshOverlay meshOverlay;

        /// <summary>
        /// The Shader.PropertyToID for "_Fade".
        /// </summary>
        private int shader_FadeID;
        
        /// <summary>
        /// The face mask texture.
        /// </summary>
        private Texture2D faceMaskTexture;
        
        /// <summary>
        /// The face mask mat.
        /// </summary>
        private Mat faceMaskMat;

        /// <summary>
        /// The detected face rect in mask mat.
        /// </summary>
        private UnityEngine.Rect faceRectInMask;

        /// <summary>
        /// The detected face landmark points in mask mat.
        /// </summary>
        private List<Vector2> faceLandmarkPointsInMask;

        /// <summary>
        /// The haarcascade_frontalface_alt_xml_filepath.
        /// </summary>
        private string haarcascade_frontalface_alt_xml_filepath;

        /// <summary>
        /// The shape_predictor_68_face_landmarks_dat_filepath.
        /// </summary>
        private string shape_predictor_68_face_landmarks_dat_filepath;


        // Use this for initialization
        void Start ()
        {
            WebGLFileUploadManager.SetImageEncodeSetting (true);
            WebGLFileUploadManager.SetAllowedFileName ("\\.(png|jpe?g|gif)$");
            WebGLFileUploadManager.SetImageShrinkingSize (640, 480);
            WebGLFileUploadManager.FileUploadEventHandler += fileUploadHandler;

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

            #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(getFilePathCoroutine());
            #else
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath ("haarcascade_frontalface_alt.xml");
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("shape_predictor_68_face_landmarks.dat");
            Run ();
            #endif
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator getFilePathCoroutine()
        {
            var getFilePathAsync_0_Coroutine = StartCoroutine (OpenCVForUnity.Utils.getFilePathAsync ("haarcascade_frontalface_alt.xml", (result) => {
                haarcascade_frontalface_alt_xml_filepath = result;
            }));
            var getFilePathAsync_1_Coroutine = StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shape_predictor_68_face_landmarks.dat", (result) => {
                shape_predictor_68_face_landmarks_dat_filepath = result;
            }));

            yield return getFilePathAsync_0_Coroutine;
            yield return getFilePathAsync_1_Coroutine;

            Run ();
            uploadFaceMaskButton.interactable = true;
        }
        #endif

        private void Run ()
        {
            meshOverlay = this.GetComponent<TrackedMeshOverlay> ();
            shader_FadeID = Shader.PropertyToID("_Fade");

            rectangleTracker = new RectangleTracker ();

            faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);

            frontalFaceParam = new FrontalFaceParam ();

            webCamTextureToMatHelper.Init ();

            isShowingFaceRectsToggle.isOn = isShowingFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            isFilteringNonFrontalFacesToggle.isOn = isFilteringNonFrontalFaces;
            isShowingDebugFacePointsToggle.isOn = isShowingDebugFacePoints;
        }

        /// <summary>
        /// Raises the web cam texture to mat helper inited event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInited ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInited");

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
            if (cascade.empty ()) {
                Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerSample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }

            meshOverlay.UpdateOverlayTransform ();

            OnChangeFaceMaskButton ();
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

                    // Adjust to Dilb's result.
                    foreach (OpenCVForUnity.Rect r in detectResult) {
                        r.y += (int)(r.height * 0.1f);
                    }
                }

                // face traking.
                rectangleTracker.UpdateTrackedObjects (detectResult);
                List<TrackedRect> trackedRects = new List<TrackedRect> ();
                rectangleTracker.GetObjects (trackedRects, true);
                
                // detect face landmark.
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

                    float offsetX = meshOverlay.Width / 2f;
                    float offsetY = meshOverlay.Height / 2f; 
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

                            Vector3[] vertices = tm.MeshFilter.mesh.vertices;
                            if (vertices.Length == landmarkPoints [i].Count) {
                                for (int j = 0; j < vertices.Length; j++) {
                                    vertices [j].x = landmarkPoints [i] [j].x - offsetX;
                                    vertices [j].y = offsetY - landmarkPoints [i] [j].y;
                                }
                            }
                            Vector2[] uv = tm.MeshFilter.mesh.uv;
                            if (uv.Length == faceLandmarkPointsInMask.Count) {
                                for (int jj = 0; jj < uv.Length; jj++) {
                                    uv [jj].x = faceLandmarkPointsInMask [jj].x / maskImageWidth;
                                    uv [jj].y = (maskImageHeight - faceLandmarkPointsInMask [jj].y) / maskImageHeight;
                                }
                            }
                            meshOverlay.UpdateObject (tr.id, vertices, null, uv);

                            if (tr.numFramesNotDetected > 3) {
                                tm.Material.SetFloat (shader_FadeID, 1f);
                            }else if (tr.numFramesNotDetected > 0 && tr.numFramesNotDetected <= 3) {
                                tm.Material.SetFloat (shader_FadeID, 0.3f + (0.7f/4f) * tr.numFramesNotDetected);
                            } else {
                                tm.Material.SetFloat (shader_FadeID, 0.3f);
                            }
                            
                            // filter nonfrontalface.
                            if (isFilteringNonFrontalFaces && frontalFaceParam.getFrontalFaceRate (landmarkPoints [i]) < frontalFaceRateLowerLimit) {
                                tm.Material.SetFloat (shader_FadeID, 1f);
                            }

                        } else if (tr.state == TrackedState.DELETED) {
                            meshOverlay.DeleteObject (tr.id);
                        }
                    }
                } else if (landmarkPoints.Count >= 1) {

                    float offsetX = meshOverlay.Width / 2f;
                    float offsetY = meshOverlay.Height / 2f; 
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
                            
                            Vector3[] vertices = tm.MeshFilter.mesh.vertices;
                            if (vertices.Length == landmarkPoints [i].Count) {
                                for (int j = 0; j < vertices.Length; j++) {
                                    vertices [j].x = landmarkPoints[i][j].x - offsetX;
                                    vertices [j].y = offsetY - landmarkPoints[i][j].y;
                                }
                            }
                            Vector2[] uv = tm.MeshFilter.mesh.uv;
                            if (uv.Length == landmarkPoints [0].Count) {
                                for (int jj = 0; jj < uv.Length; jj++) {
                                    uv [jj].x = landmarkPoints[0][jj].x / maskImageWidth;
                                    uv [jj].y = (maskImageHeight - landmarkPoints[0][jj].y) / maskImageHeight;
                                }
                            }
                            meshOverlay.UpdateObject (tr.id, vertices, null, uv);

                            if (tr.numFramesNotDetected > 3) {
                                tm.Material.SetFloat (shader_FadeID, 1f);
                            }else if (tr.numFramesNotDetected > 0 && tr.numFramesNotDetected <= 3) {
                                tm.Material.SetFloat (shader_FadeID, 0.3f + (0.7f/4f) * tr.numFramesNotDetected);
                            } else {
                                tm.Material.SetFloat (shader_FadeID, 0.3f);
                            }
                            
                            // filter nonfrontalface.
                            if (isFilteringNonFrontalFaces && frontalFaceParam.getFrontalFaceRate (landmarkPoints [i]) < frontalFaceRateLowerLimit) {
                                tm.Material.SetFloat (shader_FadeID, 1f);
                            }
                            
                        } else if (tr.state == TrackedState.DELETED) {
                            meshOverlay.DeleteObject (tr.id);
                        }
                    }
                }

                // draw face rects.
                if (isShowingFaceRects) {
                    for (int i = 0; i < detectResult.Count; i++) {
                        UnityEngine.Rect rect = new UnityEngine.Rect (detectResult [i].x, detectResult [i].y, detectResult [i].width, detectResult [i].height);
                        OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect, new Scalar (255, 0, 0, 255), 2);
                    }

                    for (int i = 0; i < trackedRects.Count; i++) {
                        UnityEngine.Rect rect = new UnityEngine.Rect (trackedRects [i].x, trackedRects [i].y, trackedRects [i].width, trackedRects [i].height);
                        OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect, new Scalar (255, 255, 0, 255), 2);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceParam.getAngleOfFrontalFace (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceParam.getFrontalFaceRate (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                }

                // draw face points.
                if (isShowingDebugFacePoints) {
                    for (int i = 0; i < landmarkPoints.Count; i++) {
                        OpenCVForUnityUtils.DrawFaceLandmark (rgbaMat, landmarkPoints [i], new Scalar (0, 255, 0, 255), 2);
                    }
                }


                // display face mask image.
                if (faceMaskTexture != null && faceMaskMat != null) {

                    if (isShowingFaceRects) {
                        OpenCVForUnityUtils.DrawFaceRect (faceMaskMat, faceRectInMask, new Scalar (255, 0, 0, 255), 2);
                    }
                    if (isShowingDebugFacePoints) {
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
        /// Raises the disable event.
        /// </summary>
        void OnDisable ()
        {
            WebGLFileUploadManager.FileUploadEventHandler -= fileUploadHandler;
            WebGLFileUploadManager.Dispose ();

            webCamTextureToMatHelper.Dispose ();

            if (cascade != null)
                cascade.Dispose ();

            if (rectangleTracker != null)
                rectangleTracker.Dispose ();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose ();

            if (frontalFaceParam != null)
                frontalFaceParam.Dispose ();
        }

        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("FaceMaskSample");
            #else
            Application.LoadLevel ("FaceMaskSample");
            #endif
        }

        /// <summary>
        /// Raises the play button event.
        /// </summary>
        public void OnPlayButton ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button event.
        /// </summary>
        public void OnPauseButton ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the change camera button event.
        /// </summary>
        public void OnChangeCameraButton ()
        {
            webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
        }

        /// <summary>
        /// Raises the is showing face rects toggle event.
        /// </summary>
        public void OnIsShowingFaceRectsToggle ()
        {
            if (isShowingFaceRectsToggle.isOn) {
                isShowingFaceRects = true;
            } else {
                isShowingFaceRects = false;
            }
        }

        /// <summary>
        /// Raises the use Dlib face detector toggle event.
        /// </summary>
        public void OnUseDlibFaceDetecterToggle ()
        {
            if (useDlibFaceDetecterToggle.isOn) {
                useDlibFaceDetecter = true;
            } else {
                useDlibFaceDetecter = false;
            }
        }

        /// <summary>
        /// Raises the is filtering non frontal faces toggle event.
        /// </summary>
        public void OnIsFilteringNonFrontalFacesToggle ()
        {
            if (isFilteringNonFrontalFacesToggle.isOn) {
                isFilteringNonFrontalFaces = true;
            } else {
                isFilteringNonFrontalFaces = false;
            }
        }

        /// <summary>
        /// Raises the is showing debug face points toggle event.
        /// </summary>
        public void OnIsShowingDebugFacePointsToggle ()
        {
            if (isShowingDebugFacePointsToggle.isOn) {
                isShowingDebugFacePoints = true;
            } else {
                isShowingDebugFacePoints = false;
            }
        }

        /// <summary>
        /// Raises the set face mask button event.
        /// </summary>
        public void OnChangeFaceMaskButton ()
        {
            removeFaceMask ();

            SampleMaskData maskData = SampleDataSet.GetData();

            faceMaskTexture = Resources.Load (maskData.FileName) as Texture2D;
            faceMaskMat = new Mat (faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC4);
            OpenCVForUnity.Utils.texture2DToMat (faceMaskTexture, faceMaskMat);
            Debug.Log ("faceMaskMat ToString " + faceMaskMat.ToString ());

            if(maskData.LandmarkPoints != null){
                faceRectInMask = maskData.FaceRect;
                faceLandmarkPointsInMask = maskData.LandmarkPoints;
            }else{
                faceRectInMask = detectFace (faceMaskMat);
                faceLandmarkPointsInMask = detectFaceLandmarkPoints (faceMaskMat, faceRectInMask);
            }

            SampleDataSet.Next();

            if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                removeFaceMask ();
                Debug.Log ("A face could not be detected from the input image.");
            }

            //dumpRect(faceRectInMask);
            //dumpVector2(faceLandmarkPointsInMask);
            //dumpVector3(faceLandmarkPointsInMask);
            //MeshFilter mf = createFaceMesh(faceMaskTexture.width, faceMaskTexture.height);
            //ObjExporter.MeshToFile(mf, "Assets/FaceMaskSample/Resources/FaceMesh.obj");
        }

        /// <summary>
        /// Raises the scan face mask button event.
        /// </summary>
        public void OnScanFaceMaskButton ()
        {
            removeFaceMask ();

            // Capture webcam frame.
            if (webCamTextureToMatHelper.IsPlaying ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                faceRectInMask = detectFace (rgbaMat);
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

                faceRectInMask = detectFace (faceMaskMat);
                faceLandmarkPointsInMask = detectFaceLandmarkPoints (faceMaskMat, faceRectInMask);

                if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                    removeFaceMask ();
                    Debug.Log ("A face could not be detected from the input image.");
                }
            }
        }
        
        /// <summary>
        /// Raises the upload face mask button event.
        /// </summary>
        public void OnUploadFaceMaskButton ()
        {
            WebGLFileUploadManager.PopupDialog (null, "Select frontal face image file (.png|.jpg|.gif)");
        }
        
        /// <summary>
        /// Raises the remove face mask button event.
        /// </summary>
        public void OnRemoveFaceMaskButton ()
        {
            removeFaceMask ();
        }

        private void removeFaceMask ()
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
        /// Files the upload handler.
        /// </summary>
        /// <param name="result">Result.</param>
        private void fileUploadHandler (UploadedFileInfo[] result)
        {
            if (result.Length == 0) {
                Debug.Log ("File upload Error!");
                return;
            }

            removeFaceMask ();
            
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
                faceRectInMask = detectFace (faceMaskMat);
                faceLandmarkPointsInMask = detectFaceLandmarkPoints (faceMaskMat, faceRectInMask);
                
                if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                    removeFaceMask ();
                    Debug.Log ("A face could not be detected from the input image.");
                }
            }
        }
        
        private UnityEngine.Rect detectFace (Mat mat)
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
                        // Adjust to Dilb's result.
                        r.y += (int)(r.height * 0.1f);
                        return r;
                    }
                }
            }
            return new UnityEngine.Rect ();
        }

        private List<Vector2> detectFaceLandmarkPoints (Mat mat, UnityEngine.Rect rect)
        {
            OpenCVForUnityUtils.SetImage (faceLandmarkDetector, mat);
            List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);

            return points;
        }

        /*
        private void dumpRect(UnityEngine.Rect rect){
            
            string r = "new Rect("  + rect.x + ", "  + rect.y + ", "  + rect.width + ", " + rect.height + ")";
            Debug.Log ("dumpRect:" + "\n" + r);
        }

        private void dumpVector2(List<Vector2> points){
            
            string p = "";
            int i = 0;
            foreach (var item in points) {
                p += "new Vector2(" + "" + item.x + ", " + item.y + "),\n";
                i++;
            }
            Debug.Log ("dumpMeshVector2:" + "\n" + p);
        }

        private void dumpVector3(List<Vector2> points){

            string p = "";
            int i = 0;
            foreach (var item in points) {
                //p += ", " + i + ":" + item;
                p += "new Vector3(" + "" + item.x + ", " + item.y + "),\n";
                i++;
            }
            Debug.Log ("dumpMeshVector3:" + "\n" + p);
        }

        private MeshFilter createFaceMesh (float textureWidth, float textureHeight)
        {
            GameObject newObj = new GameObject("FaceMesh");
            MeshFilter meshFilter = newObj.AddComponent<MeshFilter>();
            newObj.AddComponent<MeshCollider>();
            MeshRenderer meshRenderer = newObj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Hide/FadeShader"));

            //vertices
            Vector3[] vertices = new Vector3[68]{
                new Vector3(63, 170),
                new Vector3(65, 190),
                new Vector3(69, 211),
                new Vector3(74, 231),
                new Vector3(83, 250),
                new Vector3(95, 267),
                new Vector3(110, 279),
                new Vector3(126, 288),
                new Vector3(145, 289),
                new Vector3(164, 285),
                new Vector3(180, 273),
                new Vector3(193, 256),
                new Vector3(202, 236),
                new Vector3(207, 214),
                new Vector3(210, 193),
                new Vector3(210, 171),
                new Vector3(207, 149),
                new Vector3(70, 159),
                new Vector3(76, 147),
                new Vector3(90, 145),
                new Vector3(103, 147),
                new Vector3(118, 152),
                new Vector3(138, 149),
                new Vector3(151, 140),
                new Vector3(167, 133),
                new Vector3(183, 132),
                new Vector3(194, 142),
                new Vector3(129, 163),
                new Vector3(130, 178),
                new Vector3(132, 192),
                new Vector3(133, 207),
                new Vector3(121, 217),
                new Vector3(128, 220),
                new Vector3(137, 222),
                new Vector3(145, 218),
                new Vector3(152, 213),
                new Vector3(86, 167),
                new Vector3(93, 161),
                new Vector3(104, 160),
                new Vector3(112, 167),
                new Vector3(104, 171),
                new Vector3(93, 171),
                new Vector3(151, 162),
                new Vector3(159, 153),
                new Vector3(170, 150),
                new Vector3(179, 155),
                new Vector3(172, 161),
                new Vector3(161, 163),
                new Vector3(114, 248),
                new Vector3(123, 243),
                new Vector3(131, 240),
                new Vector3(139, 240),
                new Vector3(145, 237),
                new Vector3(156, 237),
                new Vector3(166, 240),
                new Vector3(159, 248),
                new Vector3(149, 252),
                new Vector3(142, 254),
                new Vector3(134, 254),
                new Vector3(124, 253),
                new Vector3(119, 248),
                new Vector3(132, 245),
                new Vector3(139, 245),
                new Vector3(146, 243),
                new Vector3(162, 241),
                new Vector3(147, 244),
                new Vector3(140, 246),
                new Vector3(133, 247)
            };
            Vector3[] vertices2 = (Vector3[])vertices.Clone();
            for (int j = 0; j < vertices2.Length; j++) {
                vertices2 [j].x = vertices2 [j].x - textureWidth/2;
                vertices2 [j].y = textureHeight/2 - vertices2 [j].y;
            }
            //Flip X axis
            for (int j = 0; j < vertices2.Length; j++) {
                vertices2 [j].x = -vertices2 [j].x;
            }
            meshFilter.mesh.vertices = vertices2;


            //triangles
            //int[] triangles = new int[327]{
            int[] triangles = new int[309]{
                //Around the right eye 21
                0,36,1,
                1,36,41,
                1,41,31,
                41,40,31,
                40,29,31,
                40,39,29,
                39,28,29,
                39,27,28,
                39,21,27,
                38,21,39,
                20,21,38,
                37,20,38,
                37,19,20,
                18,19,37,
                18,37,36,
                17,18,36,
                0,17,36,

                36,37,41,
                37,40,41,
                37,38,40,
                38,39,40,

                //Around the left eye 21
                45,16,15,
                46,45,15,
                46,15,35,
                47,46,35,
                29,47,35,
                42,47,29,
                28,42,29,
                27,42,28,
                27,22,42,
                22,43,42,
                22,23,43,
                23,44,43,
                23,24,44,
                24,25,44,
                44,25,45,
                25,26,45,
                45,26,16,

                44,45,46,
                47,44,46,
                43,44,47,
                42,43,47,

                //Eyebrows, nose and cheeks 13
                20,23,21,
                21,23,22,
                21,22,27,
                29,30,31,
                29,35,30,
                30,32,31,
                30,33,32,
                30,34,33,
                30,35,34,
                1,31,2,
                2,31,3,
                35,15,14,
                35,14,13,

                //mouth 48
                33,51,50,
                32,33,50,
                31,32,50,
                31,50,49,
                31,49,48,
                3,31,48,
                3,48,4,
                4,48,5,
                48,59,5,
                5,59,6,
                59,58,6,
                58,7,6,
                58,57,7,
                57,8,7,
                57,9,8,
                57,56,9,
                56,10,9,
                56,55,10,
                55,11,10,
                55,54,11,
                54,12,11,
                54,13,12,
                35,13,54,
                35,54,53,
                35,53,52,
                34,35,52,
                33,34,52,
                33,52,51,

                48,49,60,
                48,60,59,
                49,50,61,
                49,61,60,
                60,67,59,
                59,67,58,
                50,51,61,
                51,62,61,
                67,66,58,
                66,57,58,
                51,52,63,
                51,63,62,
                66,65,56,
                66,56,57,
                52,53,63,
                53,64,63,
                65,64,55,
                65,55,56,
                53,54,64,
                64,54,55

                //inner mouth 6
                //60,61,67,
                //61,62,67,
                //62,66,67,
                //62,63,65,
                //62,65,66,
                //63,64,65,
            };
            //Flip X axis
            for (int j = 0; j < triangles.Length; j=j+3) {
                int a = triangles [j+1];
                int b = triangles [j+2];
                triangles [j+1] = b;
                triangles [j+2] = a;
            }
            meshFilter.mesh.triangles = triangles;


            //uv
            Vector2[] uv = new Vector2[68];
            for (int j = 0; j < uv.Length; j++) {
                uv [j].x = vertices[j].x / textureWidth;
                uv [j].y = (textureHeight - vertices[j].y) / textureHeight;
            }
            meshFilter.mesh.uv = uv;

            meshFilter.mesh.RecalculateBounds ();
            meshFilter.mesh.RecalculateNormals ();


//            string v = "";
//            foreach (var item in vertices) {
//                v += "," + item;
//            }
//            Debug.Log ("vertices: " + v);
//
//            string t = "";
//            foreach (var item in triangles) {
//                t += "," + item;
//            }
//            Debug.Log ("triangles: " + t);
//            
//            string u = "";
//            foreach (var item in uv) {
//                u += "," + item;
//            }
//            Debug.Log ("uv: " + u);


            return meshFilter;
        }
        */
    }
}