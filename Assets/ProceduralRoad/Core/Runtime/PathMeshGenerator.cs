﻿using System.Collections;
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
        [SerializeField, Range(0.2f, 10.0f)] private float _uvResolution = 1.0f;
        [SerializeField] private int _subdivisions = 5;
        [SerializeField] private bool _loopTrack = false;

        private Mesh _currentMesh = null;
        private MeshRenderer _targetRenderer = null;
        private MeshFilter _targetFilter = null;

        private int[] _trisDrawOrder = new int[6] { 0, 2, 1, 1, 2, 3 };

        #region Accessors

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

        public int subdivisions
        {
            get { return _subdivisions; }
            set { _subdivisions = Mathf.Clamp(value, 1, value); }
        }

        public float uvResolution
        {
            get { return _uvResolution; }
            set { _uvResolution = Mathf.Clamp(value, 0.01f, value); }
        }

        public bool loopTrack { get { return _loopTrack; } set { _loopTrack = value; }}

        #endregion
        #endregion


        #region Methods
        #region Lifecycle

        private void Awake()
        {
            UpdateRoad();
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
            Vector3[] vertices = new Vector3[2 * (path.Length + (_subdivisions - 1) * (path.Length - 1))];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[((path.Length - 1) * _subdivisions) * 6];

            //Debug.Log(vertices.Length);

            for(int p = 0, v = 0; p < path.Length - 1; p++)
            {
                for(int i = p > 0 ? 1 : 0; i <= _subdivisions; i++)
                {
                    //Set vertices
                    float curveT = (float)i / (float)_subdivisions;

                    Vector3 side = Vector3.Cross(path[p].normal, path.GetDerivativeDirection(p, curveT)).normalized * _widthMultiplier;
                    Vector3 bezierPos = path.GetBezierPosition(p, curveT);
                    vertices[v * 2] = bezierPos - side;
                    vertices[v * 2 + 1] = bezierPos + side;

                    //Debug.DrawRay(transform.position + vertices[(v) * 2], Vector3.up, Color.red);
                    //Debug.DrawRay(transform.position + vertices[(v) * 2 + 1], Vector3.up, Color.red);

                    //Uvs
                    float uvRes = ((float)v * _uvResolution)/_subdivisions;
                    uvs[v * 2] = new Vector2(0, uvRes);
                    uvs[v * 2 + 1] = new Vector2(1, uvRes);

                    //Debug.Log(p + i);

                    //Draws triangle
                    if (v > 0)
                    {
                        int vertIndex = (v - 1) * 2;

                        for (int tri = 0; tri < _trisDrawOrder.Length; tri++)
                        {
                            triangles[((v - 1) * 6) + tri] = vertIndex + _trisDrawOrder[tri];
                        }
                    }

                    v++;
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