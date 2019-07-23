using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RoadGenerator
{
    [CustomEditor(typeof(PathMeshGenerator))]
    public class RoadPathEditor : Editor
    {
        #region Attributes

        private PathMeshGenerator _currentPath = null;

        #endregion

        #region Methods
        #region LifeCycle

        private void OnEnable()
        {
            _currentPath = (PathMeshGenerator)target;
            RoadPointEditor.Init(_currentPath);
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            _currentPath.widthMultiplier = EditorGUILayout.Slider("Width",_currentPath.widthMultiplier, 0.01f, 5.0f);

            for (int i = 0; i < _currentPath.pathData.Length; i++)
            {
                if(RoadPointEditor.selectedId == i)
                {
                    _currentPath.pathData[i].position = EditorGUILayout.Vector3Field("Position", _currentPath.pathData[i].position);
                }
            }

            GUILayout.Space(20);

            if(GUILayout.Button("Add Point"))
            {
                _currentPath.pathData.AddPoint(_currentPath.transform.position);
            }

            if (GUILayout.Button("Remove Point") && _currentPath.pathData.Length > 1)
            {
                _currentPath.pathData.RemovePoint(RoadPointEditor.selectedId);
            }
        }

        private void OnSceneGUI()
        {
            RoadPointEditor.OnSceneUpdate();

            //Draws bezier curve
            for(int i = 0; i < _currentPath.pathData.Length - 1; i++)
            {
                Vector3 offset = _currentPath.transform.position;
                PathPoint p0 = _currentPath.pathData[i];
                PathPoint p1 = _currentPath.pathData[i + 1];
                Handles.DrawBezier(
                    p0.position + offset, 
                    p1.position + offset, 
                    p0.GetWorldSpaceTangent(PathTangentType.Out) + offset, 
                    p1.GetWorldSpaceTangent(PathTangentType.In) + offset, Color.white, null, 8f);
            }
        }

        private void OnDisable()
        {
            _currentPath = null;
        }


        #endregion
        #endregion
    }
}