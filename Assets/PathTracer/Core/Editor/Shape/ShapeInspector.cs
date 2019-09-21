using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace PathTracer.Shapes
{
    [CustomEditor(typeof(ShapeAsset))]
    public class ShapeInspector : Editor
    {
        #region Attributes

        private static ShapeAsset _target = null;

        #endregion

        #region Methods

        private void OnEnable()
        {
            _target = (ShapeAsset)target;

            if (ShapeEditorWindow.isActive)
            {
                ShapeEditorWindow.Edit(_target);
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Shape Editor"))
            {
                ShapeEditorWindow.Edit(_target);
            }
        }

        /*[OnOpenAssetAttribute(1)]
        public static bool OpenAsset(int instanceID, int line)
        {
            CrossSectionAsset asset = EditorUtility.InstanceIDToObject(instanceID) as CrossSectionAsset;
            CrossSectionEditorWindow.Edit(asset);
            return false;
        }*/

        #endregion
    }
}