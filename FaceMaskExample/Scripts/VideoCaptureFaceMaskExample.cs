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
    /// Face Mask from videoCapture example.
    /// </summary>
    public class VideoCaptureFaceMaskExample : MonoBehaviour
    {
        /// <summary>
        /// The width of the frame.
        /// </summary>
        private double frameWidth = 320;

        /// <summary>
        /// The height of the frame.
        /// </summary>
        private double frameHeight = 240;

        /// <summary>
        /// The capture.
        /// </summary>
        VideoCapture capture;

        /// <summary>
        /// The rgb mat.
        /// </summary>
        Mat rgbMat;

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
        public bool useDlibFaceDetecter = true;

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
            frontalFaceRateLowerLimit;

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

        /// <summary>
        /// The couple_avi_filepath.
        /// </summary>
        private string couple_avi_filepath;

        // Use this for initialization
        void Start ()
        {
            WebGLFileUploadManager.SetImageEncodeSetting (true);
            WebGLFileUploadManager.SetAllowedFileName ("\\.(png|jpe?g|gif)$");
            WebGLFileUploadManager.SetImageShrinkingSize (640, 480);
            WebGLFileUploadManager.FileUploadEventHandler += fileUploadHandler;

            capture = new VideoCapture ();

            #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(getFilePathCoroutine());
            #else
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath ("haarcascade_frontalface_alt.xml");
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("shape_predictor_68_face_landmarks.dat");
            couple_avi_filepath = OpenCVForUnity.Utils.getFilePath ("dance.avi");
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
            var getFilePathAsync_2_Coroutine = StartCoroutine (OpenCVForUnity.Utils.getFilePathAsync ("dance.avi", (result) => {
                couple_avi_filepath = result;
            }));

            yield return getFilePathAsync_0_Coroutine;
            yield return getFilePathAsync_1_Coroutine;
            yield return getFilePathAsync_2_Coroutine;

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

            rgbMat = new Mat ();

            capture.open (couple_avi_filepath);

            if (capture.isOpened ()) {
                Debug.Log ("capture.isOpened() true");
            } else {
                Debug.Log ("capture.isOpened() false");
            }


            Debug.Log ("CAP_PROP_FORMAT: " + capture.get (Videoio.CAP_PROP_FORMAT));
            Debug.Log ("CV_CAP_PROP_PREVIEW_FORMAT: " + capture.get (Videoio.CV_CAP_PROP_PREVIEW_FORMAT));
            Debug.Log ("CAP_PROP_POS_MSEC: " + capture.get (Videoio.CAP_PROP_POS_MSEC));
            Debug.Log ("CAP_PROP_POS_FRAMES: " + capture.get (Videoio.CAP_PROP_POS_FRAMES));
            Debug.Log ("CAP_PROP_POS_AVI_RATIO: " + capture.get (Videoio.CAP_PROP_POS_AVI_RATIO));
            Debug.Log ("CAP_PROP_FRAME_COUNT: " + capture.get (Videoio.CAP_PROP_FRAME_COUNT));
            Debug.Log ("CAP_PROP_FPS: " + capture.get (Videoio.CAP_PROP_FPS));
            Debug.Log ("CAP_PROP_FRAME_WIDTH: " + capture.get (Videoio.CAP_PROP_FRAME_WIDTH));
            Debug.Log ("CAP_PROP_FRAME_HEIGHT: " + capture.get (Videoio.CAP_PROP_FRAME_HEIGHT));


            texture = new Texture2D ((int)(frameWidth), (int)(frameHeight), TextureFormat.RGBA32, false);
            gameObject.transform.localScale = new Vector3 ((float)frameWidth, (float)frameHeight, 1);
            float widthScale = (float)Screen.width / (float)frameWidth;
            float heightScale = (float)Screen.height / (float)frameHeight;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = ((float)frameWidth * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = (float)frameHeight / 2;
            }

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            meshOverlay.UpdateOverlayTransform (gameObject.transform);


            grayMat = new Mat ((int)frameHeight, (int)frameWidth, CvType.CV_8UC1);
            cascade = new CascadeClassifier (haarcascade_frontalface_alt_xml_filepath);
            if (cascade.empty ()) {
                Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
            }

            isShowingFaceRectsToggle.isOn = isShowingFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            isFilteringNonFrontalFacesToggle.isOn = isFilteringNonFrontalFaces;
            isShowingDebugFacePointsToggle.isOn = isShowingDebugFacePoints;

            OnChangeFaceMaskButton ();
        }

        // Update is called once per frame
        void Update ()
        {
            //Loop play
            if (capture.get (Videoio.CAP_PROP_POS_FRAMES) >= capture.get (Videoio.CAP_PROP_FRAME_COUNT))
                capture.set (Videoio.CAP_PROP_POS_FRAMES, 0);

            //error PlayerLoop called recursively! on iOS.reccomend WebCamTexture.
            if (capture.grab ()) {

                capture.retrieve (rgbMat, 0);

                Imgproc.cvtColor (rgbMat, rgbMat, Imgproc.COLOR_BGR2RGB);
                //Debug.Log ("Mat toString " + rgbMat.ToString ());


                //face detection.
                List<OpenCVForUnity.Rect> detectResult = new List<OpenCVForUnity.Rect> ();
                if (useDlibFaceDetecter) {
                    OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbMat);
                    List<UnityEngine.Rect> result = faceLandmarkDetector.Detect ();

                    foreach (var unityRect in result) {
                        detectResult.Add (new OpenCVForUnity.Rect ((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
                    }
                } else {
                    // convert image to greyscale.
                    Imgproc.cvtColor (rgbMat, grayMat, Imgproc.COLOR_RGB2GRAY);

                    using (Mat equalizeHistMat = new Mat ())
                    using (MatOfRect faces = new MatOfRect ()) {
                        Imgproc.equalizeHist (grayMat, equalizeHistMat);

                        cascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size (equalizeHistMat.cols () * 0.15, equalizeHistMat.cols () * 0.15), new Size ());

                        detectResult = faces.toList ();

                        // Adjust to Dilb's result.
                        foreach (OpenCVForUnity.Rect r in detectResult) {
                            r.y += (int)(r.height * 0.1f);
                        }
                    }
                }


                // face traking.
                rectangleTracker.UpdateTrackedObjects (detectResult);
                List<TrackedRect> trackedRects = new List<TrackedRect> ();
                rectangleTracker.GetObjects (trackedRects, true);
                
                // detect face landmark.
                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbMat);
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
                     
                    float imageWidth = meshOverlay.Width;
                    float imageHeight = meshOverlay.Height; 
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
                                    vertices [j].x = landmarkPoints [i] [j].x / imageWidth - 0.5f;
                                    vertices [j].y = 0.5f - landmarkPoints [i] [j].y / imageHeight;
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

                    float imageWidth = meshOverlay.Width;
                    float imageHeight = meshOverlay.Height; 
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
                                    vertices [j].x = landmarkPoints[i][j].x / imageWidth - 0.5f;
                                    vertices [j].y = 0.5f - landmarkPoints[i][j].y / imageHeight;
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
                        OpenCVForUnityUtils.DrawFaceRect (rgbMat, rect, new Scalar (255, 0, 0, 255), 2);
                    }
                    
                    for (int i = 0; i < trackedRects.Count; i++) {
                        UnityEngine.Rect rect = new UnityEngine.Rect (trackedRects [i].x, trackedRects [i].y, trackedRects [i].width, trackedRects [i].height);
                        OpenCVForUnityUtils.DrawFaceRect (rgbMat, rect, new Scalar (255, 255, 0, 255), 2);
                        //Imgproc.putText (rgbMat, " " + frontalFaceParam.getAngleOfFrontalFace (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                        //Imgproc.putText (rgbMat, " " + frontalFaceParam.getFrontalFaceRate (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                }
                
                // draw face points.
                if (isShowingDebugFacePoints) {
                    for (int i = 0; i < landmarkPoints.Count; i++) {
                        OpenCVForUnityUtils.DrawFaceLandmark (rgbMat, landmarkPoints [i], new Scalar (0, 255, 0, 255), 2);
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
                    
                    float scale = (rgbMat.width () / 4f) / faceMaskMat.width ();
                    float tx = rgbMat.width () - faceMaskMat.width () * scale;
                    float ty = 0.0f;
                    Mat trans = new Mat (2, 3, CvType.CV_32F);//1.0, 0.0, tx, 0.0, 1.0, ty);
                    trans.put (0, 0, scale);
                    trans.put (0, 1, 0.0f);
                    trans.put (0, 2, tx);
                    trans.put (1, 0, 0.0f);
                    trans.put (1, 1, scale);
                    trans.put (1, 2, ty);
                    
                    Imgproc.warpAffine (faceMaskMat, rgbMat, trans, rgbMat.size (), Imgproc.INTER_LINEAR, Core.BORDER_TRANSPARENT, new Scalar (0));
                }
                    
                Imgproc.putText (rgbMat, "W:" + rgbMat.width () + " H:" + rgbMat.height () + " SO:" + Screen.orientation, new Point (5, rgbMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255), 1, Imgproc.LINE_AA, false);

                OpenCVForUnity.Utils.matToTexture2D (rgbMat, texture);
            }
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDestroy ()
        {
            WebGLFileUploadManager.FileUploadEventHandler -= fileUploadHandler;
            WebGLFileUploadManager.Dispose ();

            capture.release ();

            if (rgbMat != null)
                rgbMat.Dispose ();
            if (grayMat != null)
                grayMat.Dispose ();

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
            SceneManager.LoadScene ("FaceMaskExample");
            #else
            Application.LoadLevel ("FaceMaskExample");
            #endif
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
            
            ExampleMaskData maskData = ExampleDataSet.GetData();
            
            faceMaskTexture = Resources.Load (maskData.FileName) as Texture2D;
            faceMaskMat = new Mat (faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC3);
            OpenCVForUnity.Utils.texture2DToMat (faceMaskTexture, faceMaskMat);
            Debug.Log ("faceMaskMat ToString " + faceMaskMat.ToString ());

            if(maskData.LandmarkPoints != null){
                faceRectInMask = maskData.FaceRect;
                faceLandmarkPointsInMask = maskData.LandmarkPoints;
            }else{
                faceRectInMask = detectFace (faceMaskMat);
                faceLandmarkPointsInMask = detectFaceLandmarkPoints (faceMaskMat, faceRectInMask);
            }
            
            ExampleDataSet.Next();
            
            if (faceRectInMask.width == 0 && faceRectInMask.height == 0){
                removeFaceMask ();
                Debug.Log ("A face could not be detected from the input image.");
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
                faceMaskMat = new Mat (faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC3);
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
                    Imgproc.cvtColor (mat, grayMat, Imgproc.COLOR_RGB2GRAY);
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
    }
}