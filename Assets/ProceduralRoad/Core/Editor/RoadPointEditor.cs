using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEditor.SceneManagement;

namespace RoadGenerator
{
    public class RoadPointEditor : MonoBehaviour
    {
        #region Attributes

        private static int _selectedId = -1;
        private static PathTangentType _selectedTangent =  (PathTangentType)(-1);
        private static PathMeshGenerator _currentPath = null;

        #region Accessors
        /// <summary> Current edited point id </summary>
        public static int selectedId { get { return _selectedId; } }

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
        /// Updates the point editor
        /// </summary>
        public static void OnSceneUpdate(SceneView view)
        {
            Handles.color = Color.blue;

            for (int i = 0; i < _currentPath.pathData.Length; i++)
            {
                DrawPoint(i, view);
            }

            _currentPath.UpdateRoad();
        }


        #region Private

        private static void DrawPoint(int index, SceneView view)
        {
            Vector3 handlePosition = _currentPath.transform.position + _currentPath.pathData[index].position;

            if (index >= 1)
            {
                Handles.color = Color.green;
            }

            //Calculate the camera distance to have a constant handle size on screen
            float cameraDistance = (view.camera.transform.position - handlePosition).magnitude;

            if (Handles.Button(handlePosition, Quaternion.identity, 0.04f * cameraDistance, 0.05f * cameraDistance, Handles.SphereHandleCap))
            {
                SelectPoint(index);
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
            handlePos = Handles.DoPositionHandle(handlePos, Quaternion.identity);
            return handlePos - _currentPath.transform.position;    
        }


        private static void SelectPoint(int index)
        {
            _selectedId = index;
            _selectedTangent = (PathTangentType)(-1); //Resets selected tangent index
        }

        #endregion
        #endregion
        #endregion
    }
}