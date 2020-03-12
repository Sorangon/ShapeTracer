using UnityEngine;
using UnityEditor;

namespace SorangonToolset.ShapeTracer.Shapes {
	/// <summary>
	/// The custom inspector of a shape asset
	/// </summary>
	[CustomEditor(typeof(ShapeAsset))]
	public class ShapeInspector : Editor {
		#region Attributes

		private static ShapeAsset _target = null;

		#endregion

		#region Methods

		private void OnEnable() {
			_target = (ShapeAsset)target;

			if (ShapeEditorWindow.IsActive) {
				ShapeEditorWindow.Edit(_target);
			}
		}

		public override void OnInspectorGUI() {
			if (GUILayout.Button("Open Shape Editor")) {
				ShapeEditorWindow.Edit(_target);
			}
		}

		#endregion
	}
}