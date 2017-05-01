using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DlibFaceLandmarkDetector;
using OpenCVForUnity;
using WebGLFileUploader;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace FaceMaskExample
{
    /// <summary>
    /// Texture2D face mask example.
    /// </summary>
    public class Texture2DFaceMaskExample : MonoBehaviour
    {

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
        /// The is upload image button.
        /// </summary>
        public Button uploadImageButton;

        /// <summary>
        /// The mesh overlay.
        /// </summary>
        private TrackedMeshOverlay meshOverlay;

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

            #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(getFilePathCoroutine());
            #else
            haarcascade_frontalface_alt_xml_filepath = OpenCVForUnity.Utils.getFilePath ("haarcascade_frontalface_alt.xml");
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("shape_predictor_68_face_landmarks.dat");
            Run ();
            #endif
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator getFilePathCoroutine ()
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
            uploadImageButton.interactable = true;
        }
        #endif

        private void Run ()
        {
            meshOverlay = this.GetComponent<TrackedMeshOverlay> ();

            isShowingFaceRectsToggle.isOn = isShowingFaceRects;
            useDlibFaceDetecterToggle.isOn = useDlibFaceDetecter;
            isFilteringNonFrontalFacesToggle.isOn = isFilteringNonFrontalFaces;
            isShowingDebugFacePointsToggle.isOn = isShowingDebugFacePoints;

            if (imgTexture == null)
                imgTexture = Resources.Load ("family") as Texture2D;

            gameObject.transform.localScale = new Vector3 (imgTexture.width, imgTexture.height, 1);
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            meshOverlay.UpdateOverlayTransform (gameObject.transform);
            meshOverlay.Reset ();


            float width = 0;
            float height = 0;
            width = gameObject.transform.localScale.x;
            height = gameObject.transform.localScale.y;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            Mat rgbaMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC4);

            OpenCVForUnity.Utils.texture2DToMat (imgTexture, rgbaMat);
            Debug.Log ("rgbaMat ToString " + rgbaMat.ToString ());

            if (faceLandmarkDetector == null)
                faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);

            FrontalFaceParam frontalFaceParam = new FrontalFaceParam ();


            // face detection.
            List<OpenCVForUnity.Rect> detectResult = new List<OpenCVForUnity.Rect> ();
            if (useDlibFaceDetecter) {
                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbaMat);
                List<UnityEngine.Rect> result = faceLandmarkDetector.Detect ();
                
                foreach (var unityRect in result) {
                    detectResult.Add (new OpenCVForUnity.Rect ((int)unityRect.x, (int)unityRect.y, (int)unityRect.width, (int)unityRect.height));
                }
            } else {
                if (cascade == null)
                    cascade = new CascadeClassifier (haarcascade_frontalface_alt_xml_filepath);
//                if (cascade.empty ()) {
//                    Debug.LogError ("cascade file is not loaded.Please copy from “FaceTrackerExample/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
//                }
                
                // convert image to greyscale.
                Mat gray = new Mat ();
                Imgproc.cvtColor (rgbaMat, gray, Imgproc.COLOR_RGBA2GRAY);
                
                // detect Faces.
                MatOfRect faces = new MatOfRect ();
                Imgproc.equalizeHist (gray, gray);
                cascade.detectMultiScale (gray, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new OpenCVForUnity.Size (gray.cols () * 0.05, gray.cols () * 0.05), new Size ());
                //Debug.Log ("faces " + faces.dump ());
                
                detectResult = faces.toList ();

                // Adjust to Dilb's result.
                foreach (OpenCVForUnity.Rect r in detectResult) {
                    r.y += (int)(r.height * 0.1f);
                }
                
                gray.Dispose ();
            }
            
            // detect face landmark.
            OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbaMat);
            List<List<Vector2>> landmarkPoints = new List<List<Vector2>> ();
            foreach (var openCVRect in detectResult) {
                UnityEngine.Rect rect = new UnityEngine.Rect (openCVRect.x, openCVRect.y, openCVRect.width, openCVRect.height);
                
                Debug.Log ("face : " + rect);
                //OpenCVForUnityUtils.DrawFaceRect(imgMat, rect, new Scalar(255, 0, 0, 255), 2);
                
                List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);
                //OpenCVForUnityUtils.DrawFaceLandmark(imgMat, points, new Scalar(0, 255, 0, 255), 2);
                landmarkPoints.Add (points);
            }

            // mask faces.
            int[] face_nums = new int[landmarkPoints.Count];
            for (int i = 0; i < face_nums.Length; i++) {
                face_nums [i] = i;
            }
            face_nums = face_nums.OrderBy (i => System.Guid.NewGuid ()).ToArray ();

            float imageWidth = meshOverlay.Width;
            float imageHeight = meshOverlay.Height; 
            float maskImageWidth = imgTexture.width;
            float maskImageHeight = imgTexture.height;

            TrackedMesh tm;
            for (int i = 0; i < face_nums.Length; i++) {

                meshOverlay.CreateObject (i, imgTexture);
                tm = meshOverlay.GetObjectById (i);
                    
                Vector3[] vertices = tm.MeshFilter.mesh.vertices;
                if (vertices.Length == landmarkPoints [face_nums [i]].Count) {
                    for (int j = 0; j < vertices.Length; j++) {
                        vertices [j].x = landmarkPoints [face_nums [i]] [j].x / imageWidth - 0.5f;
                        vertices [j].y = 0.5f - landmarkPoints [face_nums [i]] [j].y / imageHeight;
                    }
                }
                Vector2[] uv = tm.MeshFilter.mesh.uv;
                if (uv.Length == landmarkPoints [face_nums [0]].Count) {
                    for (int jj = 0; jj < uv.Length; jj++) {
                        uv [jj].x = landmarkPoints [face_nums [0]] [jj].x / maskImageWidth;
                        uv [jj].y = (maskImageHeight - landmarkPoints [face_nums [0]] [jj].y) / maskImageHeight;
                    }
                }
                meshOverlay.UpdateObject (i, vertices, null, uv);
                    
                // filter nonfrontalface.
                if (isFilteringNonFrontalFaces && frontalFaceParam.getFrontalFaceRate (landmarkPoints [i]) < frontalFaceRateLowerLimit) {
                    tm.Material.SetFloat ("_Fade", 1f);
                } else {
                    tm.Material.SetFloat ("_Fade", 0.3f);
                }
            }
            
            // draw face rects.
            if (isShowingFaceRects) {
                int ann = face_nums[0]; 
                UnityEngine.Rect rect_ann = new UnityEngine.Rect (detectResult [ann].x, detectResult [ann].y, detectResult [ann].width, detectResult [ann].height);
                OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect_ann, new Scalar (255, 255, 0, 255), 2);

                int bob = 0;
                for (int i = 1; i < face_nums.Length; i ++) {
                    bob = face_nums [i];
                    UnityEngine.Rect rect_bob = new UnityEngine.Rect (detectResult [bob].x, detectResult [bob].y, detectResult [bob].width, detectResult [bob].height);
                    OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect_bob, new Scalar (255, 0, 0, 255), 2);
                }
            }

            // draw face points.
            if (isShowingDebugFacePoints) {
                for (int i = 0; i < landmarkPoints.Count; i++) {
                    OpenCVForUnityUtils.DrawFaceLandmark (rgbaMat, landmarkPoints [i], new Scalar (0, 255, 0, 255), 2);
                }
            }


            Texture2D texture = new Texture2D (rgbaMat.cols (), rgbaMat.rows (), TextureFormat.RGBA32, false);
            OpenCVForUnity.Utils.matToTexture2D (rgbaMat, texture);
            gameObject.transform.GetComponent<Renderer> ().material.mainTexture = texture;

            frontalFaceParam.Dispose ();
            rgbaMat.Dispose ();
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            WebGLFileUploadManager.FileUploadEventHandler -= fileUploadHandler;
            WebGLFileUploadManager.Dispose ();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose ();

            if (cascade != null)
                cascade.Dispose ();
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
        /// Raises the shuffle button event.
        /// </summary>
        public void OnShuffleButton ()
        {
            if (imgTexture != null)
                Run ();
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

            if (imgTexture != null)
                Run ();
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

            if (imgTexture != null)
                Run ();
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

            if (imgTexture != null)
                Run ();
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

            if (imgTexture != null)
                Run ();
        }

        /// <summary>
        /// Raises the upload image button event.
        /// </summary>
        public void OnUploadImageButton ()
        {
            WebGLFileUploadManager.PopupDialog (null, "Select image file (.png|.jpg|.gif)");
        }

        private void fileUploadHandler (UploadedFileInfo[] result)
        {
            
            if (result.Length == 0) {
                Debug.Log ("File upload Error!");
                return;
            }
            
            foreach (UploadedFileInfo file in result) {
                if (file.isSuccess) {
                    Debug.Log ("file.filePath: " + file.filePath + " exists:" + File.Exists (file.filePath));

                    imgTexture = new Texture2D (2, 2);
                    byte[] byteArray = File.ReadAllBytes (file.filePath);
                    imgTexture.LoadImage (byteArray);
                    
                    break;
                }
            }
            Run ();
        }
    }
}