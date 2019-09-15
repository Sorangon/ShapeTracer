using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace PathTracer.CrossSectionUtility
{
    [CustomEditor(typeof(CrossSectionAsset))]
    public class CrossSectionInspector : Editor
    {
        #region Attributes

        private static CrossSectionAsset _target = null;

        #endregion

        #region Methods

        private void OnEnable()
        {
            _target = (CrossSectionAsset)target;

            if (CrossSectionEditorWindow.isActive)
            {
                CrossSectionEditorWindow.Edit(_target);
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Cross Section Editor"))
            {
                CrossSectionEditorWindow.Edit(_target);
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