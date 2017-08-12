using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace FaceMaskExample
{
    /// <summary>
    /// Face mask example.
    /// </summary>
    public class FaceMaskExample : MonoBehaviour
    {
        // Use this for initialization
        void Start ()
        {

        }

        // Update is called once per frame
        void Update ()
        {

        }

        public void OnShowLicenseButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowLicense");
            #else
            Application.LoadLevel ("ShowLicense");
            #endif
        }

        public void OnTexture2DFaceMaskExampleButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("Texture2DFaceMaskExample");
            #else
            Application.LoadLevel ("Texture2DFaceMaskExample");
            #endif
        }
        
        public void OnVideoCaptureFaceMaskExampleButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("VideoCaptureFaceMaskExample");
            #else
            Application.LoadLevel ("VideoCaptureFaceMaskExample");
            #endif
        }

        public void OnWebCamTextureFaceMaskExampleButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureFaceMaskExample");
            #else
            Application.LoadLevel ("WebCamTextureFaceMaskExample");
            #endif
        }

        public void OnWebCamTextureFaceMaskAdditionalExampleButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureFaceMaskAdditionalExample");
            #else
            Application.LoadLevel ("WebCamTextureFaceMaskAdditionalExample");
            #endif
        }
    }
}