using System.Collections.Generic;
using UnityEngine;

public class FaceMaskData : MonoBehaviour
{
    [SerializeField]
    private Texture2D _image;

    public Texture2D image
    {
        get { return this._image; }
        set { this._image = value; }
    }

    /// <summary>
    /// Determines if to use dynamically detected points.
    /// </summary>
    [TooltipAttribute("Determines if to use dynamically detected points.")]
    public bool isDynamicMode = true;

    /// <summary>
    /// Determines if to enable color correction.
    /// </summary>
    [TooltipAttribute("Determines if to enable color correction.")]
    public bool enableColorCorrection = true;

    [SerializeField]
    private Rect _faceRect = new Rect(78, 95, 151, 150);

    public Rect faceRect
    {
        get { return this._faceRect; }
        set { this._faceRect = value; }
    }

    [SerializeField]
    private List<Vector2> _landmarkPoints = new List<Vector2>() {
        new Vector2 (84, 148),
        new Vector2 (84, 167),
        new Vector2 (86, 187),
        new Vector2 (89, 206),
        new Vector2 (96, 224),
        new Vector2 (106, 240),
        new Vector2 (119, 253),
        new Vector2 (134, 264),
        new Vector2 (151, 266),
        new Vector2 (168, 264),
        new Vector2 (184, 254),
        new Vector2 (197, 241),
        new Vector2 (207, 226),
        new Vector2 (214, 209),
        new Vector2 (218, 190),
        new Vector2 (221, 170),
        new Vector2 (221, 150),
        new Vector2 (100, 125),
        new Vector2 (108, 117),
        new Vector2 (119, 114),
        new Vector2 (130, 116),
        new Vector2 (141, 120),
        new Vector2 (164, 120),
        new Vector2 (176, 116),
        new Vector2 (187, 115),
        new Vector2 (199, 118),
        new Vector2 (206, 126),
        new Vector2 (153, 133),
        new Vector2 (153, 144),
        new Vector2 (154, 156),
        new Vector2 (154, 168),
        new Vector2 (142, 181),
        new Vector2 (148, 182),
        new Vector2 (154, 184),
        new Vector2 (160, 182),
        new Vector2 (165, 181),
        new Vector2 (113, 134),
        new Vector2 (120, 127),
        new Vector2 (129, 127),
        new Vector2 (136, 136),
        new Vector2 (128, 137),
        new Vector2 (119, 137),
        new Vector2 (170, 137),
        new Vector2 (177, 128),
        new Vector2 (187, 128),
        new Vector2 (193, 136),
        new Vector2 (188, 139),
        new Vector2 (178, 138),
        new Vector2 (127, 215),
        new Vector2 (135, 204),
        new Vector2 (145, 199),
        new Vector2 (154, 201),
        new Vector2 (163, 199),
        new Vector2 (173, 205),
        new Vector2 (178, 218),
        new Vector2 (173, 225),
        new Vector2 (163, 229),
        new Vector2 (154, 230),
        new Vector2 (144, 228),
        new Vector2 (134, 224),
        new Vector2 (131, 215),
        new Vector2 (145, 206),
        new Vector2 (154, 207),
        new Vector2 (163, 207),
        new Vector2 (175, 217),
        new Vector2 (163, 219),
        new Vector2 (154, 220),
        new Vector2 (144, 218)
    };

    public List<Vector2> landmarkPoints
    {
        get { return this._landmarkPoints; }
        set { this._landmarkPoints = value; }
    }
}
