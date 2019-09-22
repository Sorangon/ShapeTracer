using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ShapeTracer.Shapes;

namespace ShapeTracer.Path
{
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class ShapeTracerPath : MonoBehaviour
    {
        #region Attributes

        [SerializeField] private ShapeAsset _crossSectionAsset = null;
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

        public ShapeAsset shapeAsset { get { return _crossSectionAsset; } set { _crossSectionAsset = value; } }

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
            UpdatePath();
        }

        #endregion

        #region Public

        public void UpdatePath()
        {
            targetFilter.mesh = GeneratePathMesh(pathData);
        }

        #endregion

        #region Private

        private Mesh GeneratePathMesh(PathData path)
        {
            Shape section;

            if(_crossSectionAsset != null)
            {
                section = _crossSectionAsset.shape;
            }
            else
            {
                section = Shape.defaultShape;
            }

            int sectionVertexCount = section.pointCount;

            if(sectionVertexCount < 2)
            {
                //Debug.LogWarning("Path cannot being generated, minimum 2 points are required");
                return new Mesh();
            }

            int points = path.Length + (_loopTrack ? 1 : 0);
            Vector3[] vertices = new Vector3[sectionVertexCount * (points + (_subdivisions - 1) * (points - 1))];
            Vector2[] uvs = new Vector2[vertices.Length];

            int shapeClosure = 1;

            int[] triangles = new int[((points - 1) * _subdivisions) * (6 * (sectionVertexCount - shapeClosure))];

            //Debug.Log(vertices.Length);

            for(int p = 0, s = 0, t = 0; p < points - 1; p++)
            {
                for(int i = p > 0 ? 1 : 0; i <= _subdivisions; i++)
                {
                    //Calculates vertices
                    float curveT = (float)i / (float)_subdivisions;

                    Vector3 bezierPos = path.GetBezierPosition(p, curveT); //Gets the positionon the bezier curve at t
                    Vector3 derivative = path.GetDerivativeDirection(p, curveT).normalized; //Gets the derivative on the bézier curve at t

                    float angle = Mathf.Lerp(path[p].normalAngle, path[p + 1].normalAngle, curveT); //Gets the normal angle
                    Vector3 normal = Quaternion.AngleAxis(angle, Vector3.back) * path[p].normal; //Calculates the normal
                    normal = Quaternion.LookRotation(derivative) * normal; //Applys derivative rotation to the normal

                    Quaternion secRotation = Quaternion.LookRotation(derivative, normal);

                    //Debug.DrawRay(transform.position + path.GetBezierPosition(p, curveT), normal, Color.yellow);
                    
                    float uvRes = ((float)s * _uvResolution) / _subdivisions;

                    //Vertices and Uvs
                    for (int vert = 0; vert < sectionVertexCount; vert++)
                    {
                        vertices[s * sectionVertexCount + vert] = bezierPos + secRotation * ((Vector3)section.GetPointPosition(vert) * _widthMultiplier);
                        uvs[s * sectionVertexCount + vert] = new Vector2(vert, uvRes);
                    }
                    
                    //Draws triangle
                    if (s > 0)
                    {
                        int subdivIndex = (s - 1) * (sectionVertexCount);

                        for (int vert = 0; vert < sectionVertexCount - shapeClosure; vert++)
                        {
                            for (int tri = 0; tri < _trisDrawOrder.Length; tri++)
                            {
                                int targetVert = _trisDrawOrder[tri];

                                if (targetVert >= 2)
                                {
                                    targetVert += sectionVertexCount - 2;
                                }                             

                                if(vert != sectionVertexCount - 1 || (_trisDrawOrder[tri] == 0|| _trisDrawOrder[tri] == 2))
                                {
                                    targetVert = subdivIndex + targetVert + vert;
                                }
                                else if(vert == sectionVertexCount - 1 && (_trisDrawOrder[tri] == 1 || _trisDrawOrder[tri] == 3))
                                {
                                    targetVert = subdivIndex + targetVert - 1;
                                }

                                //if(vert == sectionVertexCount - 1) Debug.Log("Loop vertice to : " + (subdivIndex + targetVert) + " at order " + tri);
                                triangles[t] = targetVert;

                                t++;
                            }
                        }
                    }

                    s++;
                }
            }

            Mesh finalMesh = new Mesh();
            finalMesh.name = "Generated_Path_" + gameObject.GetInstanceID();

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