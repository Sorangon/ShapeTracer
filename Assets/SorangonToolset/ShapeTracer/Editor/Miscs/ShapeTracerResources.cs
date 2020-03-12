using UnityEngine;
using UnityEditor;

/// <summary>
/// Contains all the required datas for Shape Tracer Editor
/// </summary>
internal class ShapeTracerResources : ScriptableObject {
	#region Data
	[SerializeField] private Material _defaultMaterial = null;
	[SerializeField] private GUISkin _shapeEditorGUISkin = null;
	#endregion

	#region Current
	private static ShapeTracerResources _target;
	#endregion

	#region Properties
	public static Material DefaultMaterial => Target._defaultMaterial;
	public static GUISkin ShapeEditorGUISkin => Target._shapeEditorGUISkin;

	private static ShapeTracerResources Target {
		get {
			if(_target == null) {
				string[] guid = AssetDatabase.FindAssets("t:ShapeTracerResources");
				
				if(guid.Length > 0) {
					_target = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid[0]), typeof(ShapeTracerResources)) as ShapeTracerResources;
				}
				else {
					Debug.LogError("Missing Shape Tracer ressource file");
				}
			}

			return _target;
		}
	}
	#endregion
}
