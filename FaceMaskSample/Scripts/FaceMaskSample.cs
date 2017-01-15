using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace FaceMaskSample
{
    /// <summary>
    /// Face mask sample.
    /// </summary>
    public class FaceMaskSample : MonoBehaviour
    {

        // Use this for initialization
        void Start ()
        {

        }

        // Update is called once per frame
        void Update ()
        {

        }

        public void OnShowLicenseButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowLicense");
            #else
            Application.LoadLevel ("ShowLicense");
            #endif
        }

        public void OnTexture2DFaceMaskSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("Texture2DFaceMaskSample");
            #else
            Application.LoadLevel ("Texture2DFaceMaskSample");
            #endif
        }
        
        public void OnVideoCaptureFaceMaskSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("VideoCaptureFaceMaskSample");
            #else
            Application.LoadLevel ("VideoCaptureFaceMaskSample");
            #endif
        }

        public void OnWebCamTextureFaceMaskSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureFaceMaskSample");
            #else
            Application.LoadLevel ("WebCamTextureFaceMaskSample");
            #endif
        }
    }
}