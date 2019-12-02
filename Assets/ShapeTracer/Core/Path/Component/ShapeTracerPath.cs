using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShapeTracer.Shapes;

namespace ShapeTracer.Path {
	[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
	public class ShapeTracerPath : MonoBehaviour {
		#region Data
		[SerializeField] private ShapeAsset _crossSectionAsset = null;
		[SerializeField] private PathData _pathData = new PathData();
		[SerializeField] private Vector2 _scale = Vector2.one;
		[SerializeField, Range(0.2f, 10.0f)] private float _uvResolution = 4.0f;
		[SerializeField] private int _subdivisions = 10;
		[SerializeField] private bool _loopCurve = false;
		#endregion

		#region Current
		private MeshRenderer _targetRenderer = null;
		private MeshFilter _targetFilter = null;
		#endregion

		#region Constants
		private static readonly int[] TRIS_DRAW_ORDER = new int[6] { 0, 2, 1, 1, 2, 3 };
		#endregion

		#region Accessors

		private MeshRenderer targetRenderer {
			get {
				if (_targetRenderer == null) {
					_targetRenderer = GetComponent<MeshRenderer>();
				}

				return _targetRenderer;
			}
		}

		private MeshFilter targetFilter {
			get {
				if (_targetFilter == null) {
					_targetFilter = GetComponent<MeshFilter>();
				}

				return _targetFilter;
			}
		}

		public PathData pathData {
			get { return _pathData; }
		}

		public ShapeAsset shapeAsset { get { return _crossSectionAsset; } set { _crossSectionAsset = value; } }

		public Vector2 scale {
			get { return _scale; }
			set { _scale = value; }
		}

		public int subdivisions {
			get { return _subdivisions; }
			set { _subdivisions = Mathf.Clamp(value, 1, value); }
		}

		public float uvResolution {
			get { return _uvResolution; }
			set { _uvResolution = Mathf.Clamp(value, 0.01f, value); }
		}

		public bool loopCurve { get { return _loopCurve; } set { _loopCurve = value; } }

		#endregion


		#region Unity Callbacks

		private void Awake() {
			UpdatePath();
		}


		#endregion

		#region Update

		public void UpdatePath() {
			targetFilter.mesh = GeneratePathMesh(pathData);
		}

		#endregion

		#region Generation

		private Mesh GeneratePathMesh(PathData path) {
			Shape section;

			if (_crossSectionAsset != null) {
				section = _crossSectionAsset.shape;
			}
			else {
				section = Shape.defaultShape;
			}

			int sectionVertexCount = section.PointCount;

			if (sectionVertexCount < 2) {
				//Debug.LogWarning("Path cannot being generated, minimum 2 points are required");
				return new Mesh();
			}

			int points = path.Length + (_loopCurve ? 1 : 0);
			Vector3[] vertices = new Vector3[sectionVertexCount * (points + (_subdivisions - 1) * (points - 1))];
			Vector2[] uvs = new Vector2[vertices.Length];

			int shapeClosure = 1;

			int[] triangles = new int[((points - 1) * _subdivisions) * (6 * (sectionVertexCount - shapeClosure))];

			for (int p = 0, s = 0, t = 0; p < points - 1; p++) {
				for (int i = p > 0 ? 1 : 0; i <= _subdivisions; i++) {
					//Calculates vertices
					float curveT = (float)i / (float)_subdivisions;

					Vector3 bezierPos = path.GetBezierPosition(p, curveT); //Gets the positionon the bezier curve at t
					Vector3 derivative = path.GetDerivativeDirection(p, curveT).normalized; //Gets the derivative on the bézier curve at t

					float blendT = path[p].blend.Evaluate(curveT);
					float angle = Mathf.Lerp(path[p].normalAngle, path[p + 1].normalAngle, blendT); //Gets the normal angle
					Vector3 normal = Quaternion.AngleAxis(angle, Vector3.back) * path[p].normal; //Calculates the normal
					normal = Quaternion.LookRotation(derivative) * normal; //Applys derivative rotation to the normal

					Quaternion secRotation = Quaternion.LookRotation(derivative, normal);
					Vector2 pointScale = Vector2.Lerp(path[p].scale, path[p + 1].scale, blendT);
					pointScale =  new Vector2(pointScale.x * _scale.x, pointScale.y * _scale.y);

					//Debug.DrawRay(transform.position + path.GetBezierPosition(p, curveT), normal, Color.yellow);

					float uvRes = ((float)s * _uvResolution) / _subdivisions;

					//Vertices and Uvs
					for (int vert = 0; vert < sectionVertexCount; vert++) {
						Vector2 pointPos = (Vector3)section.GetVerticePosition(vert);
						pointPos.x *= pointScale.x;
						pointPos.y *= pointScale.y;
						vertices[s * sectionVertexCount + vert] = bezierPos + secRotation * (pointPos);
						uvs[s * sectionVertexCount + vert] = new Vector2(vert, uvRes);
					}

					//Draws triangle
					if (s > 0) {
						int subdivIndex = (s - 1) * (sectionVertexCount);

						for (int vert = 0; vert < sectionVertexCount - shapeClosure; vert++) {
							for (int tri = 0; tri < TRIS_DRAW_ORDER.Length; tri++) {
								int targetVert = TRIS_DRAW_ORDER[tri];

								if (targetVert >= 2) {
									targetVert += sectionVertexCount - 2;
								}

								if (vert != sectionVertexCount - 1 || (TRIS_DRAW_ORDER[tri] == 0 || TRIS_DRAW_ORDER[tri] == 2)) {
									targetVert = subdivIndex + targetVert + vert;
								}
								else if (vert == sectionVertexCount - 1 && (TRIS_DRAW_ORDER[tri] == 1 || TRIS_DRAW_ORDER[tri] == 3)) {
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

	}
}