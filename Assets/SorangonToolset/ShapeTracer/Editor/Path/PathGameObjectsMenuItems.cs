using UnityEngine;
using UnityEditor;

namespace SorangonToolset.ShapeTracer.Path {
	/// <summary>
	/// Menu items for functions related to Shape Tracer
	/// </summary>
	public class PathGameObjectsMenuItems {
		/// <summary>
		/// Create a shape traver path on the scene
		/// </summary>
		/// <param name="menuCommand"></param>
		[MenuItem("GameObject/3D Object/Shape Tracer Path", priority = 10, validate = false)]
		static void CreateShapeTracerPath(MenuCommand menuCommand) {
			GameObject go = new GameObject("Shape Tracer Path");
			go.AddComponent(typeof(ShapeTracerPath));

			if (menuCommand != null) {
				GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			}

			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView != null) {
				go.transform.position = sceneView.pivot;
			}

			Material defaultMaterial = ShapeTracerResources.DefaultMaterial;
			go.GetComponent<Renderer>().sharedMaterial = defaultMaterial;

			Undo.RegisterCreatedObjectUndo(go, "Created " + go.name);
			Selection.activeObject = go;
		}
	}
}

