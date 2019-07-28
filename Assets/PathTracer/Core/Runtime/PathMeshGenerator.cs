using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathTracer.CrossSectionUtility;

namespace PathTracer
{
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class PathMeshGenerator : MonoBehaviour
    {
        #region Attributes

        [SerializeField] private CrossSectionAsset _crossSectionAsset = null;
        [SerializeField] private PathData _pathData = new PathData();
        [SerializeField, Range(0.5f, 2.0f)] private float _widthMultiplier = 1.0f;
        [SerializeField, Range(0.2f, 10.0f)] private float _uvResolution = 1.0f;
        [SerializeField] private int _subdivisions = 5;
        [SerializeField] private bool _loopTrack = false;

        private MeshRenderer _targetRenderer = null;
        private MeshFilter _targetFilter = null;

        private static readonly int[] _trisDrawOrder = new int[6] { 0, 2, 1, 1, 2, 3 };

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

        public CrossSectionAsset crossSection { get { return _crossSectionAsset; } set { _crossSectionAsset = value; } }

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
            CrossSection section;

            if(_crossSectionAsset != null)
            {
                section = _crossSectionAsset.crossSection;
            }
            else
            {
                section = CrossSection.defaultSection;
            }

            int points = path.Length + (_loopTrack ? 1 : 0);
            Vector3[] vertices = new Vector3[2 * (points + (_subdivisions - 1) * (points - 1))];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[((points - 1) * _subdivisions) * 6];

            //Debug.Log(vertices.Length);

            for(int p = 0, v = 0; p < points - 1; p++)
            {
                for(int i = p > 0 ? 1 : 0; i <= _subdivisions; i++)
                {
                    //Calculates vertices
                    float curveT = (float)i / (float)_subdivisions;

                    Vector3 bezierPos = path.GetBezierPosition(p, curveT); //Gets the positionon the bezier curve at t
                    Vector3 derivative = path.GetDerivativeDirection(p, curveT).normalized; //Gets the derivative on the bézier curve at t

                    float angle = Mathf.Lerp(path[p].normalAngle, path[p + 1].normalAngle, curveT); //Gets the normal angle
                    Vector3 normal = Quaternion.AngleAxis(angle, Vector3.back) * path[p].normal; //Calculates the normal
                    normal = Quaternion.LookRotation(derivative) * normal; //Applys derivative rotion to the normal

                    Quaternion rotation = Quaternion.LookRotation(derivative, normal);

                    //Debug.DrawRay(transform.position + path.GetBezierPosition(p, curveT), normal, Color.yellow);
                    
                    vertices[v * 2] = bezierPos + rotation * ((Vector3)section.points[0] * _widthMultiplier);
                    vertices[v * 2 + 1] = bezierPos + rotation * ((Vector3)section.points[1] * _widthMultiplier);

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