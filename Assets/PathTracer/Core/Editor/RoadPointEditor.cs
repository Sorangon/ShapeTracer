using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEditor.SceneManagement;

namespace PathTracer
{
    public static class RoadPointEditor
    {
        #region Attributes

        private static int _selectedId = -1;
        private static PathTangentType _selectedTangent =  (PathTangentType)(-1);
        private static PathMeshGenerator _currentPath = null;
        private static bool _displayNormal = false;
        private static Tool _lastUsedTool = Tool.None;

        #region Accessors
        /// <summary> Current edited point id </summary>
        public static int selectedId { get { return _selectedId; } }
        public static bool displayNormal { get { return _displayNormal; } set { _displayNormal = value; } }

        #endregion
        #endregion

        #region Methods
        #region LifeCycle

        /// <summary>
        /// Initialize the editor
        /// </summary>
        /// <param name="path"></param>
        public static void Init(PathMeshGenerator path)
        {
            _selectedId = -1;
            _currentPath = path;
        }

        /// <summary>
        /// Draws the point editor on GUI
        /// </summary>
        public static void EditPointGUI(int index)
        {
            GUILayout.Space(10.0f);
            GUILayout.BeginVertical("Box");
            GUILayout.Space(3.0f);

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField("Editing point " + index, labelStyle);

            EditorGUI.BeginChangeCheck();
            Vector3 newPointPos = EditorGUILayout.Vector3Field("Position", _currentPath.pathData[index].position);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_currentPath, "Move Point");
                EditorUtility.SetDirty(_currentPath);
                _currentPath.pathData[index].position = newPointPos;
            }

            GUILayout.Space(3.0f);
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            float newNormalAngle = EditorGUILayout.Slider("Normal Angle", _currentPath.pathData[index].normalAngle, -180, 180);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_currentPath, "Rotate Normal");
                EditorUtility.SetDirty(_currentPath);
                _currentPath.pathData[index].normalAngle = newNormalAngle;
            }

            EditorGUI.BeginDisabledGroup(_currentPath.pathData[index].normalAngle == 0);
            if (GUILayout.Button("Reset"))
            {
                Undo.RecordObject(_currentPath, "Reset Normal Rotation");
                EditorUtility.SetDirty(_currentPath);
                _currentPath.pathData[index].normalAngle = 0;
            }


            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4.0f);
            GUILayout.EndVertical();
            GUILayout.Space(15.0f);
        }

        /// <summary>
        /// Updates the scene editor
        /// </summary>
        public static void OnSceneUpdate(SceneView view)
        {
            Handles.color = Color.blue;

            for (int i = 0; i < _currentPath.pathData.Length; i++)
            {
                DrawPoint(i, view);
            }

            if (_selectedId >= 0 && Tools.current != Tool.None) //Locks the tool to none if a point is selected
            {
                Tools.current = Tool.None;
            }

            _currentPath.UpdateRoad();
        }

        public static void Disable()
        {
            _selectedId = -1;
            DeselectPoint();
        }


        #region Private

        private static void DrawPoint(int index, SceneView view)
        {
            Vector3 handlePosition = _currentPath.transform.position + _currentPath.pathData[index].position;

            //Calculate the camera distance to have a constant handle size on screen
            float cameraDistance = (view.camera.transform.position - handlePosition).magnitude;

            if (_displayNormal)
            {
                Handles.DrawLine(handlePosition, handlePosition + _currentPath.pathData[index].normal * 2.0f);
            }

            if (index >= 1)
            {
                Handles.color = Color.green;
            }

            if (Handles.Button(handlePosition, Quaternion.identity, 0.04f * cameraDistance, 0.05f * cameraDistance, Handles.SphereHandleCap))
            {
                SelectPoint(index);

                if(_lastUsedTool == Tool.None)
                {
                    _lastUsedTool = Tools.current;
                }
            }

            if (_selectedId == index)
            {
                DrawSelectedPointHandles(index, handlePosition, cameraDistance);
            }
        }

        private static void DrawSelectedPointHandles(int selectedId, Vector3 handlePos, float cameraDistance)
        {
            Handles.color = Color.yellow;

            //Draw tangents handles
            for(int i = 0; i < 2; i++)
            {
                Vector3 tangentPos = _currentPath.transform.position + _currentPath.pathData[selectedId].GetObjectSpaceTangent((PathTangentType)i);
                Handles.DrawAAPolyLine(6,tangentPos, handlePos);

                if (Handles.Button(tangentPos, Quaternion.identity, 0.03f * cameraDistance, 0.04f * cameraDistance, Handles.SphereHandleCap))
                {
                    _selectedTangent = (PathTangentType)i;
                }
            }

            if (_selectedTangent < 0) //If no tangent is selected
            {
                //Moves the point
                _currentPath.pathData[selectedId].position = MovePoint(handlePos); 
            }
            else
            {
                PathTangentType oppositeTangent = (PathTangentType)(1 - ((int)_selectedTangent));
                handlePos = _currentPath.transform.position;

                //Edits the position of the selected tangent
                Vector3 newPos = MovePoint(_currentPath.pathData[selectedId].GetObjectSpaceTangent(_selectedTangent) + handlePos);
                _currentPath.pathData[selectedId].SetObjectSpaceTangent(_selectedTangent, newPos);

                //Mirrors the position of the opposite tangent
                Vector3 mirrorTangentDir = -_currentPath.pathData[selectedId].GetPointSpaceTangent(_selectedTangent).normalized;
                float mirrorTangentLength =  _currentPath.pathData[selectedId].GetPointSpaceTangent(oppositeTangent).magnitude;

                _currentPath.pathData[selectedId].SetPointSpaceTangent(oppositeTangent, mirrorTangentDir * mirrorTangentLength);
            }          
        }

        private static Vector3 MovePoint(Vector3 handlePos)
        {
            EditorGUI.BeginChangeCheck();
            handlePos = Handles.DoPositionHandle(handlePos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_currentPath, "Move Point");
                EditorUtility.SetDirty(_currentPath);
            }

            return handlePos - _currentPath.transform.position;
        }


        private static void SelectPoint(int index)
        {
            _selectedId = index;
            _selectedTangent = (PathTangentType)(-1); //Resets selected tangent index
        }

        private static void DeselectPoint()
        {
            Tools.current = _lastUsedTool;
        }

        #endregion
        #endregion
        #endregion
    }
}