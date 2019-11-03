using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ShapeTracer.Shapes;

namespace ShapeTracer.Path {
	[CustomEditor(typeof(ShapeTracerPath))]
	public class PathEditor : Editor {
		#region Attributes

		private ShapeTracerPath _currentPath = null;
		private bool _autoGenerateRoad = true;

		#endregion

		#region Events/Delegates
		#endregion


		#region Callbacks

		private void OnEnable() {
			_currentPath = (ShapeTracerPath)target;

			PathPointEditor.Init(_currentPath);
		}

		public override void OnInspectorGUI() {
			GUILayout.Space(5.0f);

			EditorGUILayout.BeginHorizontal();
			ShapeAsset shapeAsset = (ShapeAsset)EditorGUILayout.ObjectField(
			"Shape", _currentPath.shapeAsset, typeof(ShapeAsset), false);
			_currentPath.shapeAsset = shapeAsset;

			if (shapeAsset != null) {
				if (GUILayout.Button("Edit", GUILayout.Width(60))) {
					ShapeEditorWindow.Edit(shapeAsset);
				}
			}
			else {
				if (GUILayout.Button("Create", GUILayout.Width(60))) {
					ShapeAsset newShapeAsset = CreateShapeAsset();
					if (newShapeAsset != null) {
						_currentPath.shapeAsset = newShapeAsset;
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(15);

			EditorGUI.BeginChangeCheck();

			Vector2 scale = EditorGUILayout.Vector2Field("Scale", _currentPath.scale);
			int subdivisions = EditorGUILayout.IntField("Subdivisions", Mathf.Clamp(_currentPath.subdivisions, 1, 100));
			float uvResolution = EditorGUILayout.Slider("Uv resolution", _currentPath.uvResolution, 0.2f, 10.0f);
			bool loopTrack = EditorGUILayout.Toggle("Loop track", _currentPath.loopTrack);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_currentPath, "Modify Path Settings");
				EditorUtility.SetDirty(_currentPath);
				_currentPath.scale = scale;
				_currentPath.subdivisions = subdivisions;
				_currentPath.uvResolution = uvResolution;
				_currentPath.loopTrack = loopTrack;
			}

			GUILayout.Space(20);

			for (int i = 0; i < _currentPath.pathData.Length; i++) {
				if (PathPointEditor.selectedId == i) {
					PathPointEditor.EditPointGUI(i);
				}
			}

			if (GUILayout.Button("Add Point")) {
				Undo.RecordObject(_currentPath, "Add point");
				EditorUtility.SetDirty(_currentPath);
				_currentPath.pathData.AddPoint(_currentPath.transform.position);
				SceneView.RepaintAll();
			}

			if (GUILayout.Button("Remove Point") && _currentPath.pathData.Length > 1) {
				Undo.RecordObject(_currentPath, "Remove point");
				EditorUtility.SetDirty(_currentPath);
				_currentPath.pathData.RemovePoint(PathPointEditor.selectedId);
				SceneView.RepaintAll();
			}

			GUILayout.Space(20);

			_autoGenerateRoad = EditorGUILayout.Toggle("Auto Generate", _autoGenerateRoad);

			if (!_autoGenerateRoad && GUILayout.Button("Generate Road")) {
				_currentPath.UpdatePath();
				SceneView.RepaintAll();
			}
		}

		private void OnSceneGUI() {
			if (_autoGenerateRoad == true) {
				PathPointEditor.OnSceneUpdate(SceneView.lastActiveSceneView);
			}

			//Draws bezier curve

			DrawBezierCurve();
		}

		private void OnDisable() {
			_currentPath = null;
			PathPointEditor.Disable();
		}


		#endregion

		#region Bezier Curve

		private void DrawBezierCurve() {
			int curves = _currentPath.pathData.Length - (_currentPath.loopTrack ? 0 : 1);
			Matrix4x4 transform = Matrix4x4.TRS(_currentPath.transform.position, _currentPath.transform.rotation, _currentPath.transform.lossyScale);
			for (int i = 0; i < curves; i++) {
				PathPoint p0 = _currentPath.pathData[i];
				PathPoint p1;

				if (i < _currentPath.pathData.Length - 1) {
					p1 = _currentPath.pathData[i + 1];
				}
				else {
					p1 = _currentPath.pathData[0];
				}

				Handles.DrawBezier(
					transform.MultiplyPoint3x4(p0.position),
					transform.MultiplyPoint3x4(p1.position),
					transform.MultiplyPoint3x4(p0.GetObjectSpaceTangent(PathTangentType.Out)),
					transform.MultiplyPoint3x4(p1.GetObjectSpaceTangent(PathTangentType.In)), Color.green, null, 5f);
			}
		}

		#endregion

		#region Asset Utility

		private ShapeAsset CreateShapeAsset() {
			string extension = ".asset";
			string path = EditorUtility.SaveFilePanel("Create New Shape Asset", Application.dataPath, "New Shape" + extension, "asset");

			if (path.Length == 0) {
				return null;
			}


			int filePos = path.LastIndexOf(extension);
			int namePos = path.LastIndexOf('/') + 1;
			string name = path.Substring(namePos, filePos - namePos);

			path = path.Substring(path.LastIndexOf("Assets"));

			ShapeAsset newShape = ShapeAsset.CreateInstance(typeof(ShapeAsset)) as ShapeAsset;
			newShape.name = name;
			AssetDatabase.CreateAsset(newShape, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = newShape;

			return newShape;
		}

		#endregion

	}
}