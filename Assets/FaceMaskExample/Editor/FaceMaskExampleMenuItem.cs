#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FaceMaskExample
{
    public class FaceMaskExampleMenuItem : MonoBehaviour
    {
        /// <summary>
        /// Create face mask tracked mesh prefab.
        /// </summary>
        [MenuItem("Tools/Face Mask Example/Create Face Mask Prefab")]
        private static void CreateFaceMaskPrefab()
        {
            float width = 512f;
            float height = 512f;
            string basePath = "Assets/FaceMaskExample/FaceMaskPrefab/";

            GameObject newObj = new GameObject("FaceMaskTrackedMesh");

            //Add MeshFilter Component.
            MeshFilter meshFilter = newObj.AddComponent<MeshFilter>();

            // Create Mesh.
            meshFilter.mesh = new Mesh();
            Mesh mesh = meshFilter.sharedMesh;
            mesh.name = "DlibFaceLandmark68Mesh";

            // Mesh_vertices
            Vector3[] vertices = new Vector3[68] {
                new Vector3 (117, 250),
                new Vector3 (124, 291),
                new Vector3 (131, 330),
                new Vector3 (136, 369),
                new Vector3 (146, 404),
                new Vector3 (167, 433),
                new Vector3 (192, 459),
                new Vector3 (222, 480),
                new Vector3 (261, 485),
                new Vector3 (300, 478),

                new Vector3 (328, 453),
                new Vector3 (353, 425),
                new Vector3 (372, 394),
                new Vector3 (383, 357),
                new Vector3 (387, 319),
                new Vector3 (392, 281),
                new Vector3 (397, 241),
                new Vector3 (135, 224),
                new Vector3 (151, 205),
                new Vector3 (175, 196),

                new Vector3 (202, 192),
                //22^23
                new Vector3 (228, 205), //new Vector3(228, 197), y+10
                new Vector3 (281, 205), //new Vector3(281, 197), y+10

                new Vector3 (308, 191),
                new Vector3 (334, 195),
                new Vector3 (358, 205),
                new Vector3 (374, 222),
                new Vector3 (255, 228),
                new Vector3 (256, 254),
                new Vector3 (257, 279),

                new Vector3 (258, 306),
                new Vector3 (230, 322),
                new Vector3 (243, 325),
                new Vector3 (259, 329),
                new Vector3 (274, 324),
                new Vector3 (288, 320),
                new Vector3 (168, 237),
                new Vector3 (182, 229),
                new Vector3 (200, 229),
                new Vector3 (214, 239),

                new Vector3 (199, 241),
                new Vector3 (182, 242),
                new Vector3 (296, 238),
                new Vector3 (310, 228),
                new Vector3 (327, 228),
                new Vector3 (341, 235),
                new Vector3 (328, 240),
                new Vector3 (311, 240),
                //49
                new Vector3 (198, 372), //new Vector3(198, 367), y+5

                new Vector3 (219, 360),

                new Vector3 (242, 357),
                new Vector3 (261, 360),
                new Vector3 (280, 357),
                new Vector3 (301, 357),
                //55
                new Vector3 (322, 367), //new Vector3(322, 362), y+5

                new Vector3 (303, 387),
                new Vector3 (282, 396),
                new Vector3 (261, 398),
                new Vector3 (241, 396),
                new Vector3 (218, 388),
                //61^65
                new Vector3 (205, 373), //new Vector3(205, 368), y+5
                new Vector3 (242, 371), //new Vector3(242, 366), y+5
                new Vector3 (261, 373), //new Vector3(261, 368), y+5
                new Vector3 (280, 370), //new Vector3(280, 365), y+5
                new Vector3 (314, 368), //new Vector3(314, 363) y+5

                new Vector3 (281, 382),
                new Vector3 (261, 384),
                new Vector3 (242, 383)
            };

            Vector3[] vertices2 = (Vector3[])vertices.Clone();
            for (int j = 0; j < vertices2.Length; j++)
            {
                vertices2[j].x = (vertices2[j].x - width / 2f) / width;
                vertices2[j].y = (height / 2f - vertices2[j].y) / height;
            }
            mesh.vertices = vertices2;

            // Mesh_triangles
            int[] triangles = new int[327] {
                // Around the right eye 21
                0, 36, 1,
                1, 36, 41,
                1, 41, 31,
                41, 40, 31,
                40, 29, 31,
                40, 39, 29,
                39, 28, 29,
                39, 27, 28,
                39, 21, 27,
                38, 21, 39,
                20, 21, 38,
                37, 20, 38,
                37, 19, 20,
                18, 19, 37,
                18, 37, 36,
                17, 18, 36,
                0, 17, 36,

                // (inner right eye 4)
                36, 37, 41,
                37, 40, 41,
                37, 38, 40,
                38, 39, 40,
                
                // Around the left eye 21
                45, 16, 15,
                46, 45, 15,
                46, 15, 35,
                47, 46, 35,
                29, 47, 35,
                42, 47, 29,
                28, 42, 29,
                27, 42, 28,
                27, 22, 42,
                22, 43, 42,
                22, 23, 43,
                23, 44, 43,
                23, 24, 44,
                24, 25, 44,
                44, 25, 45,
                25, 26, 45,
                45, 26, 16,

                // (inner left eye 4)
                44, 45, 46,
                47, 44, 46,
                43, 44, 47,
                42, 43, 47,
                
                // Eyebrows, nose and cheeks 13
                20, 23, 21,
                21, 23, 22,
                21, 22, 27,
                29, 30, 31,
                29, 35, 30,
                30, 32, 31,
                30, 33, 32,
                30, 34, 33,
                30, 35, 34,
                1, 31, 2,
                2, 31, 3,
                35, 15, 14,
                35, 14, 13,
                
                // mouth 48
                33, 51, 50,
                32, 33, 50,
                31, 32, 50,
                31, 50, 49,
                31, 49, 48,
                3, 31, 48,
                3, 48, 4,
                4, 48, 5,
                48, 59, 5,
                5, 59, 6,
                59, 58, 6,
                58, 7, 6,
                58, 57, 7,
                57, 8, 7,
                57, 9, 8,
                57, 56, 9,
                56, 10, 9,
                56, 55, 10,
                55, 11, 10,
                55, 54, 11,
                54, 12, 11,
                54, 13, 12,
                35, 13, 54,
                35, 54, 53,
                35, 53, 52,
                34, 35, 52,
                33, 34, 52,
                33, 52, 51,

                48, 49, 60,
                48, 60, 59,
                49, 50, 61,
                49, 61, 60,
                60, 67, 59,
                59, 67, 58,
                50, 51, 61,
                51, 62, 61,
                67, 66, 58,
                66, 57, 58,
                51, 52, 63,
                51, 63, 62,
                66, 65, 56,
                66, 56, 57,
                52, 53, 63,
                53, 64, 63,
                65, 64, 55,
                65, 55, 56,
                53, 54, 64,
                64, 54, 55,
                
                // inner mouth 6
                60, 61, 67,
                61, 62, 67,
                62, 66, 67,
                62, 63, 65,
                62, 65, 66,
                63, 64, 65
            };
            mesh.triangles = triangles;

            // Mesh_uv
            Vector2[] uv = new Vector2[68];
            for (int j = 0; j < uv.Length; j++)
            {
                uv[j].x = vertices[j].x / width;
                uv[j].y = (height - vertices[j].y) / height;
            }
            mesh.uv = uv;
            mesh.uv2 = (Vector2[])uv.Clone();

            mesh.RecalculateNormals();

            // Add Collider Component.
            MeshCollider meshCollider = newObj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = CreatePrimitiveQuadMesh();

            // Add Renderer Component.
            MeshRenderer meshRenderer = newObj.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Hide/FaceMaskShader"));

            // Create alpha mask texture.
            Vector2[] facialContourUVPoints = new Vector2[] {
                uv [0],
                uv [1],
                uv [2],
                uv [3],
                uv [4],
                uv [5],
                uv [6],
                uv [7],
                uv [8],
                uv [9],
                uv [10],
                uv [11],
                uv [12],
                uv [13],
                uv [14],
                uv [15],
                uv [16],
                uv [26],
                uv [25],
                uv [24],
                uv [23],
                uv [20],
                uv [19],
                uv [18],
                uv [17]
            };

            /*
            Vector2[] rightEyeContourUVPoints = new Vector2[]{
                uv[36],
                uv[37],
                uv[38],
                uv[39],
                uv[40],
                uv[41]
            };

            Vector2[] leftEyeContourUVPoints = new Vector2[]{
                uv[42],
                uv[43],
                uv[44],
                uv[45],
                uv[46],
                uv[47]
            };
            */

            Vector2[] mouthContourUVPoints = new Vector2[] {
                uv [60],
                uv [61],
                uv [62],
                uv [63],
                uv [64],
                uv [65],
                uv [66],
                uv [67]
            };

            Texture2D alphaMaskTexture = AlphaMaskTextureCreater.CreateAlphaMaskTexture(width, height, facialContourUVPoints, /*rightEyeContourUVPoints, leftEyeContourUVPoints,*/mouthContourUVPoints);
            string alphaMaskTexturePath = basePath + "FaceMaskAlphaMask.png";
            byte[] pngData = alphaMaskTexture.EncodeToPNG();

            if (CreateWithoutFolder(basePath))
            {
                File.WriteAllBytes(alphaMaskTexturePath, pngData);
                AssetDatabase.ImportAsset(alphaMaskTexturePath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();

                Debug.Log("Create asset \"" + basePath + "FaceMaskAlphaMask.png\"");
            }

            TextureImporter importer = TextureImporter.GetAtPath(alphaMaskTexturePath) as TextureImporter;
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 1024;
            //importer.textureFormat = TextureImporterFormat.RGBA16;
            EditorUtility.SetDirty(importer);
            AssetDatabase.ImportAsset(alphaMaskTexturePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();

            GameObject.DestroyImmediate(alphaMaskTexture);
            alphaMaskTexture = AssetDatabase.LoadAssetAtPath(alphaMaskTexturePath, typeof(Texture2D)) as Texture2D;
            material.SetTexture("_MaskTex", alphaMaskTexture);
            meshRenderer.material = material;

            // Add TracedMesh Compornent.
            newObj.AddComponent<TrackedMesh>();

            // Save FaceMask Assets.
            if (CreateWithoutFolder(basePath))
            {
                AssetDatabase.CreateAsset(material, basePath + "FaceMaskMaterial.mat");
                AssetDatabase.CreateAsset(mesh, basePath + "DlibFaceLandmark68Mesh.asset");
                AssetDatabase.SaveAssets();

                string prefab_path = basePath + "FaceMaskTrackedMesh.prefab";

#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAsset(newObj, prefab_path);
#else
                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(prefab_path, typeof(UnityEngine.Object));
                if (prefab == null)
                {
                    PrefabUtility.CreatePrefab(prefab_path, newObj);
                }
                else
                {
                    PrefabUtility.ReplacePrefab(newObj, prefab);
                }
#endif

                AssetDatabase.SaveAssets();

                Debug.Log("Create asset \"" + basePath + "FaceMaskMaterial.mat\"");
                Debug.Log("Create asset \"" + basePath + "DlibFaceLandmark68Mesh.asset\"");
                Debug.Log("Create asset \"" + basePath + "FaceMaskTrackedMesh.prefab\"");
            }

            GameObject.DestroyImmediate(newObj);
        }

        private static Mesh CreatePrimitiveQuadMesh()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(gameObject);

            return mesh;
        }

        private static bool CreateWithoutFolder(string filename)
        {
            string directory = Path.GetDirectoryName(filename);

            if (Directory.Exists(directory + "/") == true)
                return true;

            string[] values = directory.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string checkFolder = string.Empty;
            foreach (var folder in values)
            {
                string baseFolder = checkFolder;
                if (!string.IsNullOrEmpty(checkFolder))
                {
                    baseFolder = Path.GetDirectoryName(checkFolder);
                }
                checkFolder += folder;
                if (Directory.Exists(checkFolder + "/") != true)
                {
                    AssetDatabase.CreateFolder(baseFolder, folder);
                }
                checkFolder += "/";
            }
            return true;
        }
    }
}
#endif