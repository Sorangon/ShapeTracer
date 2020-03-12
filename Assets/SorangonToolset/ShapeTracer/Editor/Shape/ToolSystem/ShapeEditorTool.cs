using UnityEngine;

namespace SorangonToolset.ShapeTracer.Shapes.Tools {
	/// <summary>
	/// The base shape editor tool class
	/// Create a new tool inhertiting this class and add a ShapeToolIdentity to set a custom name, icon, order and more
	/// </summary>
	public abstract class ShapeEditorTool {
		#region Data
		public GUIContent content = null;
		#endregion

		#region Current
		private ShapeEditorWindow _editor;
		#endregion

		#region Parameters
		/// <summary>
		/// The current on which the took is processing 
		/// </summary>
		protected ShapeEditorWindow Editor => _editor;
		#endregion

		#region Initialize
		public void SetEditor(ShapeEditorWindow editor) {
			if(editor != null) {
				_editor = editor;
			}
			else {
				Debug.LogWarning("Cannot set the current editor to null, ensure to set a corect reference");
			}
		}
		#endregion

		#region Callbacks
		public virtual void Init() { }
		public abstract void Process();
		#endregion
	}
}
