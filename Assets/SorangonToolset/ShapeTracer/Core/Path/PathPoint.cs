using UnityEngine;

namespace SorangonToolset.ShapeTracer {
	/// <summary>
	/// A path waypoint, contains a position, a normal and tangents
	/// </summary>
	[System.Serializable]
	public class PathPoint {
		#region Data
		[SerializeField] private Vector3 _position = Vector3.zero;
		[SerializeField] private Vector3 _normal = Vector3.up;
		[SerializeField] private Vector3 _inTangent = Vector3.forward;
		[SerializeField] private Vector3 _outTangent = Vector3.back;
		[SerializeField] private float _normalAngle = 0.0f;
		[SerializeField] private bool _isEmpty = false;
		[SerializeField] private Vector2 _scale = Vector2.one;
		[SerializeField]
		private AnimationCurve _blend = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

		#endregion

		#region Properties
		/// <summary> The position of the point in world space </summary>
		public Vector3 position {
			get { return _position; }
			set { _position = value; }
		}


		public Vector2 scale {
			get { return _scale; }
			set { _scale = value; }
		}

		/// <summary> The normal of the point in world space </summary>
		public Vector3 normal {
			get { return _normal; }
			set { _normal = value; }
		}

		/// <summary> Does disable the mesh generation on this point </summary>
		public bool isEmpty {
			get { return _isEmpty; }
			set { _isEmpty = value; }
		}

		public AnimationCurve blend {
			get { return _blend; }
			set { _blend = value; }
		}


		/*public Vector3 binormal
        {
            get { return _binormal; }
            set { _binormal = value; }
        }*/

		public float normalAngle {
			get { return _normalAngle; }
			set {
				if (value > 180) {
					value -= 360;
				}
				else if (value < -180) {
					value += 360;
				}


				_normalAngle = value;
			}
		}

		#endregion

		#region Constructor

		public PathPoint(Vector3 position, Vector3 normal) {
			_position = position;
			_normal = normal;

			Vector3 tangentOffset = new Vector3(1.0f, 0, 0.5f);

			_inTangent = -tangentOffset;
			_outTangent = tangentOffset;
		}

		public PathPoint(Vector3 position, Vector3 normal, Vector3 inTangent, Vector3 outTangent) {
			_position = position;
			_normal = normal;
			_inTangent = inTangent;
			_outTangent = outTangent;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Return the position of the tangent relatively to his point
		/// </summary>
		/// <param name="tangent"></param>
		public Vector3 GetPointSpaceTangent(PathTangentType tangent) {
			if (tangent == PathTangentType.In) {
				return _inTangent;
			}
			else if (tangent == PathTangentType.Out) {
				return _outTangent;
			}
			else {
				return Vector3.zero;
			}
		}

		/// <summary>
		/// Set the position of the tangent relatively to his point
		/// </summary>
		/// <param name="tangent"></param>
		/// <param name="newPos"></param>
		/// <param name="mirrorTangents"></param>
		/// <param name="mirrorScale"></param>
		public void SetPointSpaceTangent(PathTangentType tangent, Vector3 newPos, bool mirrorTangents, bool mirrorScale) {
			if (tangent == PathTangentType.In) {
				Vector3 deltaDirection = newPos - _inTangent;
				float deltaScale = deltaDirection.magnitude * Vector3.Dot(deltaDirection.normalized, _inTangent.normalized);

				_inTangent = newPos;
				if (mirrorTangents) {
					_outTangent = -_inTangent.normalized * _outTangent.magnitude;
				}

				if (mirrorScale) {
					_outTangent += _outTangent.normalized * deltaScale;
				}

			}
			else if (tangent == PathTangentType.Out) {
				Vector3 deltaDirection = newPos - _outTangent;
				float deltaScale = deltaDirection.magnitude * Vector3.Dot(deltaDirection.normalized, _outTangent.normalized);

				_outTangent = newPos;
				if (mirrorTangents) {
					_inTangent = -_outTangent.normalized * _inTangent.magnitude;
				}

				if (mirrorScale) {
					_inTangent += _inTangent.normalized * deltaScale;
				}
			}
		}

		/// <summary>
		/// Return the position of the tangent relatively to the object 
		/// </summary>
		/// <param name="tangent"></param>
		public Vector3 GetObjectSpaceTangent(PathTangentType tangent) {
			return GetPointSpaceTangent(tangent) + _position;
		}

		/// <summary>
		/// Return the position of the tangent relatively to the object 
		/// </summary>
		/// <param name="tangent"></param>
		/// <param name="newPos"></param>
		/// <param name="mirrorTangents"></param>
		/// <param name="mirrorScale"></param>
		public void SetObjectSpaceTangent(PathTangentType tangent, Vector3 newPos, bool mirrorTangents, bool mirrorScale) {
			SetPointSpaceTangent(tangent, newPos - _position, mirrorTangents, mirrorScale);
		}

		#endregion
	}
}