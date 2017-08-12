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
    /// VideoCapture face mask example.
    /// </summary>
    [RequireComponent (typeof(TrackedMeshOverlay))]
    public class VideoCaptureFaceMaskExample : MonoBehaviour
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
        public float frontalFaceRateLowerLimit;

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
        /// The width of the frame.
        /// </summary>
        double frameWidth = 320;
        
        /// <summary>
        /// The height of the frame.
        /// </summary>
        double frameHeight = 240;
        
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
        /// The shape_predictor_68_face_landmarks_dat_filepath.
        /// </summary>
        string shape_predictor_68_face_landmarks_dat_filepath;

        /// <summary>
        /// The couple_avi_filepath.
        /// </summary>
        string couple_avi_filepath;

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

            capture = new VideoCapture ();

            #if UNITY_WEBGL && !UNITY_EDITOR
            var getFilePath_Coroutine = GetFilePath ();
            coroutines.Push (getFilePath_Coroutine);
            StartCoroutine (getFilePath_Coroutine);
            #else
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath ("haarcascade_frontalface_alt.xml");
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("shape_predictor_68_face_landmarks.dat");
            couple_avi_filepath = OpenCVForUnity.Utils.getFilePath ("dance.avi");
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

            var getFilePathAsync_1_Coroutine = DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shape_predictor_68_face_landmarks.dat", (result) => {
                shape_predictor_68_face_landmarks_dat_filepath = result;
            });
            coroutines.Push (getFilePathAsync_1_Coroutine);
            yield return StartCoroutine (getFilePathAsync_1_Coroutine);

            var getFilePathAsync_2_Coroutine = OpenCVForUnity.Utils.getFilePathAsync ("dance.avi", (result) => {
                couple_avi_filepath = result;
            });
            coroutines.Push (getFilePathAsync_2_Coroutine);
            yield return StartCoroutine (getFilePathAsync_2_Coroutine);

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

            frontalFaceChecker = new FrontalFaceChecker((float)frameWidth, (float)frameHeight);

            faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);


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
//            if (cascade.empty ()) {
//                Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
//            }

            displayFaceRectsToggle.isOn = displayFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            filterNonFrontalFacesToggle.isOn = filterNonFrontalFaces;
            displayDebugFacePointsToggle.isOn = displayDebugFacePoints;

            OnChangeFaceMaskButtonClick ();
        }

        // Update is called once per frame
        void Update ()
        {
            // loop play.
            if (capture.get (Videoio.CAP_PROP_POS_FRAMES) >= capture.get (Videoio.CAP_PROP_FRAME_COUNT))
                capture.set (Videoio.CAP_PROP_POS_FRAMES, 0);

            if (capture.grab ()) {

                capture.retrieve (rgbMat, 0);

                Imgproc.cvtColor (rgbMat, rgbMat, Imgproc.COLOR_BGR2RGB);
                //Debug.Log ("Mat toString " + rgbMat.ToString ());


                // detect faces.
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

                        // adjust to Dilb's result.
                        foreach (OpenCVForUnity.Rect r in detectResult) {
                            r.y += (int)(r.height * 0.1f);
                        }
                    }
                }


                // face traking.
                rectangleTracker.UpdateTrackedObjects (detectResult);
                List<TrackedRect> trackedRects = new List<TrackedRect> ();
                rectangleTracker.GetObjects (trackedRects, true);
                
                // detect face landmark points.
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

                            // filter non frontal faces.
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
                        OpenCVForUnityUtils.DrawFaceRect (rgbMat, rect, new Scalar (255, 0, 0, 255), 2);
                    }
                    
                    for (int i = 0; i < trackedRects.Count; i++) {
                        UnityEngine.Rect rect = new UnityEngine.Rect (trackedRects [i].x, trackedRects [i].y, trackedRects [i].width, trackedRects [i].height);
                        OpenCVForUnityUtils.DrawFaceRect (rgbMat, rect, new Scalar (255, 255, 0, 255), 2);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceChecker.GetFrontalFaceAngles (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                        //Imgproc.putText (rgbaMat, " " + frontalFaceChecker.GetFrontalFaceRate (landmarkPoints [i]), new Point (rect.xMin, rect.yMin - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                    }
                }
                
                // draw face points.
                if (displayDebugFacePoints) {
                    for (int i = 0; i < landmarkPoints.Count; i++) {
                        OpenCVForUnityUtils.DrawFaceLandmark (rgbMat, landmarkPoints [i], new Scalar (0, 255, 0, 255), 2);
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
            WebGLFileUploadManager.onFileUploaded -= OnFileUploaded;
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

            if (frontalFaceChecker != null)
                frontalFaceChecker.Dispose ();

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
            faceMaskMat = new Mat (faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC3);
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
                faceMaskMat = new Mat (faceMaskTexture.height, faceMaskTexture.width, CvType.CV_8UC3);
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
                    Imgproc.cvtColor (mat, grayMat, Imgproc.COLOR_RGB2GRAY);
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