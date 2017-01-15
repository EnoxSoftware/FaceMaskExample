using System.Collections;
using System.IO;
using UnityEngine;
using WebGLFileUploader;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace WebGLFileUploaderSample
{
    /// <summary>
    /// File Upload sample.
    /// </summary>
    public class FileUploadSample : MonoBehaviour
    {

        // Use this for initialization
        void Start ()
        {

            Debug.Log("WebGLFileUploadManager.getOS: " + WebGLFileUploadManager.getOS);
            Debug.Log("WebGLFileUploadManager.isMOBILE: " + WebGLFileUploadManager.IsMOBILE);
            Debug.Log("WebGLFileUploadManager.getUserAgent: " + WebGLFileUploadManager.GetUserAgent);

            WebGLFileUploadManager.SetDebug(true);
            if ( 
                #if UNITY_WEBGL && !UNITY_EDITOR 
                    WebGLFileUploadManager.IsMOBILE 
                #else
                    Application.isMobilePlatform
                #endif
            ) {
                if(!WebGLFileUploadManager.IsInitialized) WebGLFileUploadManager.InitFileUploader (false);
                WebGLFileUploadManager.SetDescription("Select image files (.png|.jpg|.gif)");

            }else{
                if(!WebGLFileUploadManager.IsInitialized) WebGLFileUploadManager.InitFileUploader (true);
                WebGLFileUploadManager.SetDescription("Drop image files (.png|.jpg|.gif) here");
            }
            WebGLFileUploadManager.SetImageEncodeSetting(true);
            WebGLFileUploadManager.SetAllowedFileName("\\.(png|jpe?g|gif)$");
            WebGLFileUploadManager.SetImageShrinkingSize(1280 ,960);
            WebGLFileUploadManager.FileUploadEventHandler += fileUploadHandler;
        }

        // Update is called once per frame
        void Update ()
        {

        }

        void OnDestroy ()
        {
            WebGLFileUploadManager.FileUploadEventHandler -= fileUploadHandler;
            WebGLFileUploadManager.Dispose();
        }

        private void fileUploadHandler(UploadedFileInfo[] result){

            if(result.Length == 0) {
                Debug.Log("File upload Error!");
            }else{
                Debug.Log("File upload success! (result.Length: " + result.Length + ")");
            }

            foreach(UploadedFileInfo file in result){
                if(file.isSuccess){
                    Debug.Log("file.filePath: " + file.filePath + " exists:" + File.Exists(file.filePath));

                    Texture2D texture = new Texture2D (2, 2);
                    byte[] byteArray = File.ReadAllBytes (file.filePath);
                    texture.LoadImage (byteArray);
                    gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

                    Debug.Log("File.ReadAllBytes:byte[].Length: " + byteArray.Length);

                    break;
                }
            }
        }

        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebGLFileUploaderSample");
            #else
            Application.LoadLevel ("WebGLFileUploaderSample");
            #endif
        }

        public void OnButtonOverlayToggleButton ()
        {
            WebGLFileUploadManager.InitFileUploader(false, !WebGLFileUploadManager.IsOverlay);
        }

        public void OnDropOverlayToggleButton ()
        {
            WebGLFileUploadManager.InitFileUploader(true, !WebGLFileUploadManager.IsOverlay);
        }

        public void OnPopupDialogButton ()
        {
            WebGLFileUploadManager.PopupDialog(null, "Select image files (.png|.jpg|.gif)");
        }

    }
}
