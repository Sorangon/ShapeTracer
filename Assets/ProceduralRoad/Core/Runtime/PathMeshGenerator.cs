using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoadGenerator
{
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class PathMeshGenerator : MonoBehaviour
    {
        #region Attributes

        [SerializeField] private PathData _pathData = new PathData();
        [SerializeField, Range(0.5f, 2.0f)] private float _widthMultiplier = 1.0f;
        private Mesh _currentMesh = null;
        private MeshRenderer _targetRenderer = null;
        private MeshFilter _targetFilter = null;

        private MeshRenderer targetRenderer
        {
            get
            {
                if (_targetRenderer == null)
                {
                    _targetRenderer = GetComponent<MeshRenderer>();
                }

                return _targetRenderer;
            }
        }

        private MeshFilter targetFilter
        {
            get
            {
                if (_targetFilter == null)
                {
                    _targetFilter = GetComponent<MeshFilter>();
                }

                return _targetFilter;
            }
        }

        public PathData pathData
        {
            get { return _pathData; }
        }

        public float widthMultiplier
        {
            get { return _widthMultiplier; }
            set { _widthMultiplier = Mathf.Clamp(value, 0.01f, value); }
        }

        #endregion


        #region Methods
        #region Lifecycle

        private void Update()
        {
            targetFilter.mesh = GenerateRoadMesh(pathData);
        }

        #endregion

        #region Public

        public void UpdateRoad()
        {
            targetFilter.mesh = GenerateRoadMesh(pathData);
        }

        #endregion

        #region Private

        private Mesh GenerateRoadMesh(PathData path)
        {
            Vector3[] vertices = new Vector3[path.Length * 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[(path.Length - 1) * 6];

            for(int p = 0, t = 0; p < path.Length; p++)
            {
                //Set vertices
                Vector3 side = Vector3.Cross(path[p].normal, path[p].GetLocalSpaceTangent(PathTangentType.Out).normalized) * _widthMultiplier;
                vertices[p * 2] = path[p].position - side;
                vertices[p * 2 + 1] = path[p].position + side;

                //uvs
                uvs[p * 2] = new Vector2(0, (float)p / (float)path.Length * 4);
                uvs[p * 2 + 1] = new Vector2(1, (float)p / (float)path.Length * 4 );

                //Draws triangle
                if(p > 0)
                {
                    int vertIndex = (p - 1) * 2;

                    triangles[t] = vertIndex;
                    triangles[t + 1] = vertIndex + 2;
                    triangles[t + 2] = vertIndex + 1;
                    triangles[t + 3] = vertIndex + 1;
                    triangles[t + 4] = vertIndex + 2;
                    triangles[t + 5] = vertIndex + 3;
                    t += 6;
                }

            }

            Mesh finalMesh = new Mesh();
            finalMesh.name = "Generated_Road_" + gameObject.GetInstanceID();

            finalMesh.vertices = vertices;
            finalMesh.uv = uvs;
            finalMesh.triangles = triangles;

            finalMesh.RecalculateNormals();
            finalMesh.RecalculateBounds();

            return finalMesh;
        }

        #endregion

        #endregion

    }
}