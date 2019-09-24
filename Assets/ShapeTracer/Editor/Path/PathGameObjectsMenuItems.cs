using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShapeTracer.Path
{
    public class PathGameObjectsMenuItems
    {
        [MenuItem("GameObject/3D Object/Shape Tracer Path", priority = 10, validate = false)]
        static void CreateShapeTracerPath(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Shape Tracer Path");
            go.AddComponent(typeof(ShapeTracerPath));

            if (menuCommand != null)
            {
                GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if(sceneView != null)
            {
                go.transform.position = sceneView.pivot;
            }

            Material defaultMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/ShapeTracer/DefaultContent/Materials/M_Road.mat",
                typeof(Material));

            go.GetComponent<Renderer>().material = defaultMaterial;
    
            Undo.RegisterCreatedObjectUndo(go, "Created " + go.name);
            Selection.activeObject = go;
        }
    }
}

