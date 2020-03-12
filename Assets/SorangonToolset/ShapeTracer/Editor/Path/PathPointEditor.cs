using UnityEngine;
using UnityEditor;

namespace SorangonToolset.ShapeTracer.Path {
	/// <summary>
	/// The custom editor functions to edit path points
	/// </summary>
	public static class PathPointEditor {
		#region Constants

		private const float POINT_HANDLE_SIZE = 0.3f;

		#endregion

		#region Current

		private static int _selectedId = -1;
		private static PathTangentType _selectedTangent = (PathTangentType)(-1);
		private static ShapeTracerPath _currentPath = null;
		private static bool _displayNormal = false;
		private static float _lastDeltaRotation = 0.0f;
		private static float _deltaTangentMove;

		#region Accessors
		/// <summary> Current edited point id </summary>
		public static int selectedId { get { return _selectedId; } }
		public static bool displayNormal { get { return _displayNormal; } set { _displayNormal = value; } }

		#endregion
		#endregion

		#region Updates

		/// <summary>
		/// Initialize the editor
		/// </summary>
		/// <param name="path"></param>
		public static void Init(ShapeTracerPath path) {
			_selectedId = -1;
			_currentPath = path;
			_lastDeltaRotation = 0.0f;
		}

		public static void Disable() {
			_selectedId = -1;
			DeselectPoint();
		}

		#endregion

		#region GUI Updates
		/// <summary>
		/// Draws the point editor on GUI
		/// </summary>
		public static void EditPointGUI(int index) {
			GUILayout.Space(10.0f);
			GUILayout.BeginVertical("HelpBox");
			GUILayout.Space(3.0f);

			GUIStyle labelStyle = new GUIStyle();
			labelStyle.fontStyle = FontStyle.Bold;

			EditorGUILayout.LabelField("Editing point " + index, labelStyle);

			EditorGUI.BeginChangeCheck();
			Vector3 newPointPos = EditorGUILayout.Vector3Field("Position", _currentPath.pathData[index].position);
			Vector2 scale = EditorGUILayout.Vector2Field("Scale", _currentPath.pathData[index].scale);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_currentPath, "Set Point Transform");
				EditorUtility.SetDirty(_currentPath);
				_currentPath.pathData[index].position = newPointPos;
				_currentPath.pathData[index].scale = scale;
			}


			GUILayout.Space(3.0f);
			EditorGUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
			float newNormalAngle = EditorGUILayout.FloatField("Roll", _currentPath.pathData[index].normalAngle);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_currentPath, "Rotate Normal");
				EditorUtility.SetDirty(_currentPath);
				_currentPath.pathData[index].normalAngle = newNormalAngle;
			}

			EditorGUI.BeginDisabledGroup(_currentPath.pathData[index].normalAngle == 0);
			if (GUILayout.Button("Reset")) {
				Undo.RecordObject(_currentPath, "Reset Normal Rotation");
				EditorUtility.SetDirty(_currentPath);
				_currentPath.pathData[index].normalAngle = 0;
			}

			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();
			AnimationCurve curve = EditorGUILayout.CurveField("Blend", _currentPath.pathData[index].blend);
			bool isEmpty = EditorGUILayout.Toggle("Empty", _currentPath.pathData[index].isEmpty);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_currentPath, "Toggled Empty Point");
				EditorUtility.SetDirty(_currentPath);
				_currentPath.pathData[index].blend = curve;
				_currentPath.pathData[index].isEmpty = isEmpty;
			}

			GUILayout.Space(4.0f);
			GUILayout.EndVertical();
			GUILayout.Space(15.0f);
		}

		#endregion

		#region Scene Updates
		/// <summary>
		/// Updates the scene editor
		/// </summary>
		public static void OnSceneUpdate(SceneView view) {
			Handles.color = Color.blue;

			for (int i = 0; i < _currentPath.pathData.Length; i++) {
				DrawPoint(i, view);
			}

			//Reset selection if press escape
			if (Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyDown) {
				_selectedId = -1;
				_selectedTangent = (PathTangentType)(-1);
				DeselectPoint();
				Event.current.Use();
			}

			_currentPath.UpdatePath();
		}

		#endregion

		#region Draw Points

		private static void DrawPoint(int index, SceneView view) {
			Vector3 handlePosition = CurveSpaceToWorldSpace(_currentPath.pathData[index].position); //Multiply by transform matrix

			if (_displayNormal) {
				Handles.DrawLine(handlePosition, handlePosition + _currentPath.pathData[index].normal * 2.0f);
			}

			if (index >= 1) {
				Handles.color = Color.green;
			}

			float handleSize = HandleUtility.GetHandleSize(handlePosition) * 0.5f;


			if (Handles.Button(handlePosition, Quaternion.identity, POINT_HANDLE_SIZE * handleSize, POINT_HANDLE_SIZE * 1.1f * handleSize, Handles.SphereHandleCap)) {
				SelectPoint(index);
				if (Tools.hidden == false) {
					Tools.hidden = true;
				}
			}

			if (_selectedId == index) {
				DrawSelectedPointHandles(index, handlePosition, handleSize);
			}
		}

		private static void DrawSelectedPointHandles(int selectedId, Vector3 handlePos, float handleSize) {
			PathPoint point = _currentPath.pathData[selectedId];
			Handles.color = Color.yellow;

			if (Tools.current != Tool.Rotate) {
				//Draw tangents handles
				for (int i = 0; i < 2; i++) {
					Vector3 tangentPos = CurveSpaceToWorldSpace(point.GetObjectSpaceTangent((PathTangentType)i));
					Handles.DrawAAPolyLine(6, tangentPos, handlePos);

					if (Handles.Button(tangentPos, Quaternion.identity, POINT_HANDLE_SIZE * handleSize, POINT_HANDLE_SIZE * 1.1f * handleSize, Handles.SphereHandleCap)) {
						_selectedTangent = (PathTangentType)i;
					}
				}
			}

			if (_selectedTangent < 0) //If no tangent is selected
			{
				switch (Tools.current) {
					case Tool.Rotate:
						//Rotate the point roll and tangents
						SetPointRotation(_currentPath.pathData[selectedId]);
						break;

					case Tool.Scale:
						//Scale the point
						SetPointScale(handlePos, _currentPath.pathData[selectedId]);
						break;

					default:
						//Moves curve the point
						point.position = MovePoint(handlePos);
						break;
				}
			}
			else {
				//Moves the curve tangent point
				PathTangentType oppositeTangent = (PathTangentType)(1 - ((int)_selectedTangent));

				//Edits the position of the selected tangent
				Vector3 tangentPos = CurveSpaceToWorldSpace(point.GetObjectSpaceTangent((PathTangentType)_selectedTangent));
				Vector3 newPos = MovePoint(tangentPos);

				bool mirrorScale = !Event.current.alt;

				_currentPath.pathData[selectedId].SetObjectSpaceTangent(_selectedTangent, newPos, true, mirrorScale);
			}
		}

		#endregion

		#region Point Movement
		private static Vector3 MovePoint(Vector3 handlePos) {
			EditorGUI.BeginChangeCheck();
			handlePos = Handles.DoPositionHandle(handlePos, Quaternion.identity);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_currentPath, "Move Point");
				EditorUtility.SetDirty(_currentPath);
			}

			return WorldSpaceToCurveSpace(handlePos);
		}


		private static void SetPointRotation(PathPoint point) {
			EditorGUI.BeginChangeCheck();
			Vector3 tangentDir = _currentPath.transform.rotation * point.GetPointSpaceTangent(PathTangentType.Out);
			Quaternion pivotRotation = Quaternion.LookRotation(tangentDir);
			Quaternion roll = Quaternion.AngleAxis(point.normalAngle, Vector3.back);
			Vector3 pointWorldPos = CurveSpaceToWorldSpace(point.position);
			float handleSize = HandleUtility.GetHandleSize(pointWorldPos);
			Quaternion newRot = Handles.Disc(pivotRotation * roll,
				pointWorldPos, tangentDir, handleSize, false, 0.0f);

			//Line rotation
			Quaternion lineRot = newRot * Quaternion.Inverse(pivotRotation) * Quaternion.LookRotation(tangentDir, _currentPath.transform.up);
			Handles.DrawLine(pointWorldPos + lineRot * Vector3.left * handleSize, pointWorldPos + lineRot * Vector3.right * handleSize);


			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_currentPath, "Rotate Point");
				EditorUtility.SetDirty(_currentPath);
				float deltaRot = -((point.normalAngle % 360) + newRot.eulerAngles.z);

				/*if (_lastDeltaRotation * (deltaRot) < 0.0f && Mathf.Abs(deltaRot) > 180.0f)
                {
                    Debug.Log(deltaRot);
                    if(deltaRot > 0)
                    {
                        deltaRot -= 360.0f;
                    }
                    else
                    {
                        deltaRot += 360.0f;
                    }

                    Debug.Log("Loop");
                }*/

				//Debug.Log("Delta Rotation : " + (-(point.normalAngle + newRot.eulerAngles.z)));
				//float 
				point.normalAngle += deltaRot;
				_lastDeltaRotation = deltaRot;
			}
		}

		private static void SetPointScale(Vector3 handlePos, PathPoint point) {
			Vector3 tangentDir = point.GetPointSpaceTangent(PathTangentType.Out);
			Quaternion pivotRotation = Quaternion.LookRotation(tangentDir);
			Quaternion roll = Quaternion.AngleAxis(point.normalAngle, Vector3.back);
			float handleSize = HandleUtility.GetHandleSize(handlePos);
			EditorGUI.BeginChangeCheck();
			Vector2 scale = Handles.ScaleHandle(point.scale, handlePos, _currentPath.transform.rotation * pivotRotation * roll, handleSize);

			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(_currentPath, "Scale Path Point");
				EditorUtility.SetDirty(_currentPath);
				point.scale = scale;
			}
		}

		#endregion

		#region Coord Space

		private static Vector3 CurveSpaceToWorldSpace(Vector3 position) {
			Matrix4x4 transform = Matrix4x4.TRS(_currentPath.transform.position, _currentPath.transform.rotation, _currentPath.transform.lossyScale);

			position = transform.MultiplyPoint3x4(position);

			return position;
		}

		private static Vector3 WorldSpaceToCurveSpace(Vector3 position) {
			Matrix4x4 transform = Matrix4x4.TRS(_currentPath.transform.position, _currentPath.transform.rotation, _currentPath.transform.lossyScale);

			position = transform.inverse.MultiplyPoint3x4(position);
			return position;
		}

		#endregion

		#region Selection
		private static void SelectPoint(int index) {
			_selectedId = index;
			_selectedTangent = (PathTangentType)(-1); //Resets selected tangent index
		}

		private static void DeselectPoint() {
			Tools.hidden = false;
		}

		#endregion
	}
}