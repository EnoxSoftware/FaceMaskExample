using System;
using System.Collections.Generic;
using UnityEngine;

namespace FaceMaskExample
{
    public class TrackedMeshOverlay : MonoBehaviour
    {
        public int interval = 1;
        public int poolSize = 10;

        [SerializeField]
        protected GameObject _baseObject;

        public GameObject baseObject
        {
            get
            {
                return _baseObject;
            }
            set
            {
                _baseObject = value;
                SetBaseObject(_baseObject);
            }
        }

        public float width
        {
            get
            {
                return targetWidth;
            }
        }

        public float height
        {
            get
            {
                return targetHeight;
            }
        }

        protected Transform targetTransform;
        protected float targetWidth = 0;
        protected float targetHeight = 0;
        protected Transform overlayTransform;
        protected ObjectPool objectPool;
        protected Dictionary<int, TrackedMesh> showingObjects = new Dictionary<int, TrackedMesh>();

        void Awake()
        {
            Initialize("TrackedMeshOverlay");
        }

        void OnDestroy()
        {
            overlayTransform = null;
            targetTransform = null;
            targetWidth = 0;
            targetHeight = 0;
            showingObjects.Clear();
            if (objectPool != null)
            {
                Destroy(objectPool.gameObject);
                objectPool = null;
            }
        }

        protected virtual GameObject GetPoolObject(Transform parent)
        {
            if (objectPool == null)
                return null;

            GameObject newObj = objectPool.GetInstance(parent);
            if (newObj != null)
            {
                newObj.transform.SetParent(parent, false);
                return newObj;
            }
            else
            {
                return null;
            }
        }

        protected virtual void Initialize(String name)
        {
            GameObject obj = new GameObject(name);
            overlayTransform = obj.transform;
            overlayTransform.parent = gameObject.transform.parent;

            if (_baseObject != null)
                SetBaseObject(_baseObject);
        }

        protected virtual void SetBaseObject(GameObject obj)
        {
            if (obj.GetComponent<TrackedMesh>() == null)
            {
                Debug.LogWarning("This gameObject is not TrackedMesh.");
                return;
            }

            if (objectPool != null)
            {
                Destroy(objectPool);
            }

            objectPool = overlayTransform.gameObject.AddComponent<ObjectPool>();
            objectPool.prefab = obj;
            objectPool.maxCount = poolSize;
            objectPool.prepareCount = (int)poolSize / 2;
            objectPool.Interval = interval;
        }

        public virtual void UpdateOverlayTransform(Transform targetTransform)
        {
            if (targetTransform == null)
            {
                this.targetTransform = null;
                return;
            }

            targetWidth = targetTransform.localScale.x;
            targetHeight = targetTransform.localScale.y;
            this.targetTransform = targetTransform;
            overlayTransform.localPosition = targetTransform.localPosition;
            overlayTransform.localRotation = targetTransform.localRotation;
            overlayTransform.localScale = targetTransform.localScale;
        }

        public virtual TrackedMesh GetObjectById(int id)
        {
            if (showingObjects.ContainsKey(id))
            {
                return showingObjects[id];
            }
            return null;
        }

        public virtual TrackedMesh CreateObject(int id, Texture2D tex = null)
        {
            if (_baseObject == null)
                Debug.LogError("The baseObject does not exist.");

            if (!showingObjects.ContainsKey(id))
            {
                GameObject obj = GetPoolObject(overlayTransform);
                if (obj == null)
                    return null;
                TrackedMesh tm = obj.GetComponent<TrackedMesh>();
                if (tm != null)
                {
                    tm.id = id;
                    tm.transform.localPosition = Vector3.zero;
                    tm.transform.localRotation = new Quaternion();
                    tm.transform.localScale = Vector3.one;
                    if (tex != null)
                    {
                        Renderer tmRenderer = tm.transform.GetComponent<Renderer>();
                        tmRenderer.sharedMaterial.SetTexture("_MainTex", tex);
                    }
                    showingObjects.Add(id, tm);
                }
                return tm;
            }
            else
            {
                return null;
            }
        }

        public virtual void UpdateObject(int id, Vector3[] vertices, int[] triangles = null, Vector2[] uv = null, Vector2[] uv2 = null)
        {
            if (showingObjects.ContainsKey(id))
            {
                TrackedMesh tm = showingObjects[id];

                if (vertices.Length != tm.meshFilter.mesh.vertices.Length)
                    Debug.LogError("The number of vertices does not match.");
                tm.meshFilter.mesh.vertices = vertices;

                if (triangles != null)
                {
                    tm.meshFilter.mesh.triangles = triangles;
                }
                if (uv != null)
                {
                    tm.meshFilter.mesh.uv = uv;
                }

                if (uv2 != null)
                {
                    tm.meshFilter.mesh.uv2 = uv2;
                }

                tm.meshFilter.mesh.RecalculateBounds();
                tm.meshFilter.mesh.RecalculateNormals();
            }
        }

        public virtual void DeleteObject(int id)
        {
            if (showingObjects.ContainsKey(id))
            {
                if (showingObjects[id] != null)
                    showingObjects[id].gameObject.SetActive(false);
                showingObjects.Remove(id);
            }
        }

        public virtual void Reset()
        {
            foreach (int key in showingObjects.Keys)
            {
                if (showingObjects[key] != null)
                    showingObjects[key].gameObject.SetActive(false);
            }

            showingObjects.Clear();
        }
    }
}