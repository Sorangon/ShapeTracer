using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace ShapeTracer.Shapes {
	[System.Serializable]
	public struct Shape {
		#region Data

		[SerializeField] private Vertice[] _vertices;
		[SerializeField] private bool _closeShape;

		#endregion

		#region Sub Structures
		[System.Serializable]
		public struct Vertice {
			public Vector2 position;
			public float u;

			public Vertice(Vector2 position, float u) {
				this.position = position;
				this.u = u;
			}
		}

		#endregion

		#region Properties

		public bool closeShape {
			get {
				if (_vertices.Length < 2) {
					return false;
				}
				else {
					return _closeShape;
				}
			}
			set {
				//Change state
				if (value == true && _closeShape == false) {
					AddVertice(_vertices[0]);
				}
				else if (value == false && _closeShape == true) {
					RemoveVertice(PointCount - 1);
				}

				_closeShape = value;
			}
		}

		public static Shape defaultShape =>
				 new Shape(new Vertice[2] {
					new Vertice(new Vector2(-1, 0), 0f),
					new Vertice(new Vector2(1, 0), 1f)
				});

		public int PointCount => _vertices.Length;

		#endregion

		#region Contructors

		public Shape(Vertice[] vert) {
			_vertices = vert;
			_closeShape = false;
		}

		#endregion

		#region Points

		public void VerticePosition(int index, Vector2 pos) {
			if (_closeShape && (index == 0 || index == PointCount - 1)) {
				_vertices[0].position = pos;
				_vertices[PointCount - 1].position = pos;
			}
			else {
				_vertices[index].position = pos;
			}
		}

		public void SetVertexU(int index, float pos) {
			_vertices[index].u = pos;
		}

		public Vector2 GetVerticePosition(int index) {
			index = Mathf.Clamp(index, 0, PointCount);
			return _vertices[index].position;
		}

		public float GetVertexU(int index) {
			index = Mathf.Clamp(index, 0, PointCount);
			return _vertices[index].u;
		}

		/// <summary>
		/// Adds a point from the last one
		/// </summary>
		/// <param name="position"></param>
		public void AddVertice(Vertice vert) {
			AddVertice(PointCount - 1, vert);
		}

		/// <summary>
		/// Adds a point from an index
		/// </summary>
		/// <param name="fromIndex"></param>
		/// <param name="position"></param>
		public void AddVertice(int fromIndex, Vertice vert) {
			List<Vertice> pointList = _vertices.ToList();

			if (!_closeShape) {
				fromIndex++;
			}

			pointList.Insert(fromIndex, vert);

			_vertices = pointList.ToArray();
		}

		public void RemoveVertice(int index) {
			List<Vertice> pts = _vertices.ToList();
			pts.RemoveAt(index);
			_vertices = pts.ToArray();
		}

		#endregion
	}
}