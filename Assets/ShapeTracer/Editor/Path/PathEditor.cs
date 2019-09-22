﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ShapeTracer.Shapes;

namespace ShapeTracer.Path
{
    [CustomEditor(typeof(ShapeTracerPath))]
    public class PathEditor : Editor
    {
        #region Attributes

        private ShapeTracerPath _currentPath = null;
        private bool _autoGenerateRoad = true;

        #endregion

        #region Events/Delegates
        #endregion

        #region Methods

        #region LifeCycle

        private void OnEnable()
        {
            _currentPath = (ShapeTracerPath)target;

            PathPointEditor.Init(_currentPath);
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(5.0f);

            EditorGUILayout.BeginHorizontal();
            ShapeAsset crossSection = (ShapeAsset)EditorGUILayout.ObjectField(
            "Shape", _currentPath.shapeAsset, typeof(ShapeAsset), false);
            _currentPath.shapeAsset = crossSection;

            if (GUILayout.Button("Edit", GUILayout.Width(60)) && crossSection != null)
            {
                ShapeEditorWindow.Edit(crossSection);
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
                if (PathPointEditor.selectedId == i)
                {
                    PathPointEditor.EditPointGUI(i);
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
                _currentPath.pathData.RemovePoint(PathPointEditor.selectedId);
                SceneView.RepaintAll();
            }

            GUILayout.Space(20);

            _autoGenerateRoad = EditorGUILayout.Toggle("Auto Generate",_autoGenerateRoad);

            if (!_autoGenerateRoad && GUILayout.Button("Generate Road"))
            {
                _currentPath.UpdatePath();
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            if (_autoGenerateRoad == true)
            {           
                PathPointEditor.OnSceneUpdate(SceneView.lastActiveSceneView);
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
            PathPointEditor.Disable();
        }


        #endregion

        #region Private

        #endregion

        #endregion
    }
}