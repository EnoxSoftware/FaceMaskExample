﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace FaceMaskExample
{
    /// <summary>
    /// Show License
    /// </summary>
    public class ShowLicense : MonoBehaviour
    {
        // Use this for initialization
        void Start ()
        {

        }

        // Update is called once per frame
        void Update ()
        {

        }

        public void OnBackButtonClick ()
        {
            SceneManager.LoadScene ("FaceMaskExample");
        }
    }
}
