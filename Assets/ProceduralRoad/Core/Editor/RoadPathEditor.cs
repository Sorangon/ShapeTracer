using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RoadGenerator
{
    [CustomEditor(typeof(PathMeshGenerator))]
    public class RoadPathEditor : Editor
    {
        #region Attributes

        private PathMeshGenerator _currentPath = null;
        private bool _autoGenerateRoad = true;

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

            GUILayout.Space(5.0f);

            _currentPath.widthMultiplier = EditorGUILayout.Slider("Width",_currentPath.widthMultiplier, 0.01f, 5.0f);
            _currentPath.subdivisions = EditorGUILayout.IntSlider("Subdivisions",_currentPath.subdivisions, 1, 20);
            _currentPath.uvResolution = EditorGUILayout.Slider("Uv resolution",_currentPath.uvResolution, 0.2f, 10.0f);
            _currentPath.loopTrack = EditorGUILayout.Toggle("Loop track",_currentPath.loopTrack);

            for (int i = 0; i < _currentPath.pathData.Length; i++)
            {
                if(RoadPointEditor.selectedId == i)
                {
                    GUILayout.Space(10.0f);

                    GUIStyle labelStyle = new GUIStyle();
                    labelStyle.fontStyle = FontStyle.Bold;

                    EditorGUILayout.LabelField("Edit point " + i, labelStyle);
                    _currentPath.pathData[i].position = EditorGUILayout.Vector3Field("Position", _currentPath.pathData[i].position);
                }
            }

            GUILayout.Space(20);

            _autoGenerateRoad = EditorGUILayout.Toggle("Auto Generate",_autoGenerateRoad);

            GUILayout.Space(20);

            if(GUILayout.Button("Add Point"))
            {
                _currentPath.pathData.AddPoint(_currentPath.transform.position);
            }

            if (GUILayout.Button("Remove Point") && _currentPath.pathData.Length > 1)
            {
                _currentPath.pathData.RemovePoint(RoadPointEditor.selectedId);
            }

            if (GUILayout.Button("Generate Road"))
            {
                _currentPath.UpdateRoad();
            }
        }

        private void OnSceneGUI()
        {
            if (_autoGenerateRoad == true)
            {           
                RoadPointEditor.OnSceneUpdate(SceneView.lastActiveSceneView);
            }

            //Draws bezier curve

            int curves = _currentPath.pathData.Length - (_currentPath.loopTrack ? 0 : 1);
            for (int i = 0; i < curves; i++)
            {
                Vector3 offset = _currentPath.transform.position;
                PathPoint p0 = _currentPath.pathData[i];
                PathPoint p1;

                if (i < _currentPath.pathData.Length - 1)
                {
                   p1 = _currentPath.pathData[i + 1];
                }
                else
                {
                   p1 = _currentPath.pathData[0];
                }


                Handles.DrawBezier(
                    p0.position + offset, 
                    p1.position + offset, 
                    p0.GetWorldSpaceTangent(PathTangentType.Out) + offset, 
                    p1.GetWorldSpaceTangent(PathTangentType.In) + offset, Color.green, null, 5f);
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