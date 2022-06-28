using System;
using UnityEngine;

namespace FaceMaskExample
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class TrackedMesh : MonoBehaviour
    {
        public MeshFilter meshFilter
        {
            get { return _meshFilter; }
        }

        protected MeshFilter _meshFilter;

        public MeshRenderer meshRenderer
        {
            get { return _meshRenderer; }
        }

        protected MeshRenderer _meshRenderer;

        public MeshCollider meshCollider
        {
            get { return _meshCollider; }
        }

        protected MeshCollider _meshCollider;

        public int id
        {
            get { return _id; }
            set { _id = value; }
        }

        protected int _id = 0;

        public Material material
        {
            get { return _meshRenderer.material; }
        }

        public Material sharedMaterial
        {
            get { return _meshRenderer.sharedMaterial; }
        }

        void Awake()
        {
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshRenderer = this.GetComponent<MeshRenderer>();
            _meshCollider = this.GetComponent<MeshCollider>();

            if (_meshRenderer.material == null)
                throw new Exception("material does not exist.");

            _meshRenderer.sortingOrder = 32767;
        }

        void OnDestroy()
        {
            if (_meshFilter != null && _meshFilter.mesh != null)
            {
                DestroyImmediate(_meshFilter.mesh);
            }
            if (_meshRenderer != null && _meshRenderer.materials != null)
            {
                foreach (var m in _meshRenderer.materials)
                {
                    DestroyImmediate(m);
                }
            }
        }
    }
}