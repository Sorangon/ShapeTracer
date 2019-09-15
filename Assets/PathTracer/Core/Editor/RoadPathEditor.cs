using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathTracer.CrossSectionUtility;

namespace PathTracer
{
    [CustomEditor(typeof(PathMeshGenerator))]
    public class RoadPathEditor : Editor
    {
        #region Attributes

        private PathMeshGenerator _currentPath = null;
        private bool _autoGenerateRoad = true;

        #endregion

        #region Events/Delegates
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
            GUILayout.Space(5.0f);

            EditorGUILayout.BeginHorizontal();
            CrossSectionAsset crossSection = (CrossSectionAsset)EditorGUILayout.ObjectField(
            "Cross Section", _currentPath.crossSection, typeof(CrossSectionAsset), false);
            _currentPath.crossSection = crossSection;

            if (GUILayout.Button("Edit", GUILayout.Width(60)) && crossSection != null)
            {
                CrossSectionEditorWindow.Edit(crossSection);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            EditorGUI.BeginChangeCheck();

            float widthMultiplier = Mathf.Clamp(EditorGUILayout.FloatField("Width",_currentPath.widthMultiplier),0.01f,1000);
            int subdivisions = EditorGUILayout.IntSlider("Subdivisions",_currentPath.subdivisions, 1, 25);
            float uvResolution = EditorGUILayout.Slider("Uv resolution",_currentPath.uvResolution, 0.2f, 10.0f);
            bool loopTrack = EditorGUILayout.Toggle("Loop track",_currentPath.loopTrack);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_currentPath, "Modify Path Settings");
                EditorUtility.SetDirty(_currentPath);
                _currentPath.widthMultiplier = widthMultiplier;
                _currentPath.subdivisions = subdivisions;
                _currentPath.uvResolution = uvResolution;
                _currentPath.loopTrack = loopTrack;
            }

            GUILayout.Space(20);

            for (int i = 0; i < _currentPath.pathData.Length; i++)
            {
                if (RoadPointEditor.selectedId == i)
                {
                    RoadPointEditor.EditPointGUI(i);
                }
            }

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(_currentPath, "Add point");
                EditorUtility.SetDirty(_currentPath);
                _currentPath.pathData.AddPoint(_currentPath.transform.position);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Remove Point") && _currentPath.pathData.Length > 1)
            {
                Undo.RecordObject(_currentPath, "Remove point");
                EditorUtility.SetDirty(_currentPath);
                _currentPath.pathData.RemovePoint(RoadPointEditor.selectedId);
                SceneView.RepaintAll();
            }

            GUILayout.Space(20);

            _autoGenerateRoad = EditorGUILayout.Toggle("Auto Generate",_autoGenerateRoad);

            if (!_autoGenerateRoad && GUILayout.Button("Generate Road"))
            {
                _currentPath.UpdateRoad();
                SceneView.RepaintAll();
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
                    p0.GetObjectSpaceTangent(PathTangentType.Out) + offset, 
                    p1.GetObjectSpaceTangent(PathTangentType.In) + offset, Color.green, null, 5f);
            }
        }

        private void OnDisable()
        {
            _currentPath = null;
            RoadPointEditor.Disable();
        }


        #endregion

        #region Private

        #endregion

        #endregion
    }
}