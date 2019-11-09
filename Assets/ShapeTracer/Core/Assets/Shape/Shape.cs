using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace ShapeTracer.Shapes {
	[System.Serializable]
	public struct Shape {
		#region Data

		[SerializeField] private Vector2[] _points;
		[SerializeField] private bool _closeShape;

		#endregion

		#region Sub Structures
		[System.Serializable]
		public struct Vertice {
			public Vector2 position;
			public int index;
		}

		[System.Serializable]
		public struct Edge {
			public int from;
			public int to;
		}

		#endregion

		#region Properties


		public bool closeShape {
			get {
				if (_points.Length < 2) {
					return false;
				}
				else {
					return _closeShape;
				}
			}
			set {
				//Change state
				if (value == true && _closeShape == false) {
					AddPoint(_points[0]);
				}
				else if (value == false && _closeShape == true) {
					RemovePoint(PointCount - 1);
				}

				_closeShape = value;
			}
		}

		public static Shape defaultShape {
			get {
				Shape shape = new Shape(new Vector2[2] { new Vector2(-1, 0), new Vector2(1, 0) });
				return shape;
			}
		}

		public int PointCount => _points.Length;

		#endregion

		#region Contructors

		public Shape(Vector2[] points) {
			_points = points;
			_closeShape = false;
		}


		#endregion



		#region Points

		public void SetPointPosition(int index, Vector2 pos) {
			if (_closeShape && (index == 0 || index == PointCount - 1)) {
				_points[0] = pos;
				_points[PointCount - 1] = pos;
			}
			else {
				_points[index] = pos;
			}
		}

		public Vector2 GetPointPosition(int index) {
			index = Mathf.Clamp(index, 0, PointCount);
			return _points[index];
		}

		/// <summary>
		/// Adds a point from the last one
		/// </summary>
		/// <param name="position"></param>
		public void AddPoint(Vector2 position) {
			AddPoint(PointCount - 1, position);
		}

		/// <summary>
		/// Adds a point from an index
		/// </summary>
		/// <param name="fromIndex"></param>
		/// <param name="position"></param>
		public void AddPoint(int fromIndex, Vector2 position) {
			List<Vector2> pointList = _points.ToList();

			if (!_closeShape) {
				fromIndex++;
			}

			pointList.Insert(fromIndex, position);

			_points = pointList.ToArray();
		}

		public void RemovePoint(int index) {
			List<Vector2> pts = _points.ToList();
			pts.RemoveAt(index);
			_points = pts.ToArray();
		}

		#endregion
	}
}