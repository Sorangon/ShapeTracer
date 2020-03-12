using UnityEngine;
using UnityEditor;

namespace SorangonToolset.ShapeTracer.Shapes.Tools {
	/// <summary>
	/// Allow you to move the selected point on the Shape Editor Window
	/// </summary>
	[ShapeTool("Move", "Move the selected point on the workspace", "Assets/SorangonToolset/ShapeTracer/Editor/Skin/Textures/T_Move_Icon.png", 0)]
	public class MoveTool : ShapeEditorTool {
		public override void Process() {
			if (Editor.SelectedId < 0) return;

			Editor.BeginWindows();
			GUI.Window(0, new Rect(Editor.position.width - 200, Editor.position.height - 100, 180, 80), DisplayPointPositionWindow, "Point Position");
			Editor.EndWindows();

			Handles.color = Color.green;

			if (Event.current.button == 0 && Event.current.type == EventType.MouseDrag) {
				Undo.RecordObject(Editor.Asset, "Move Shape Point");
				EditorUtility.SetDirty(Editor.Asset);

				Event e = Event.current;
				Vector2 newPos = e.mousePosition;

				//Snap on ctrl old
				if (e.control == true) {
					newPos = Editor.SnapToGrid(newPos);
				}

				newPos = Editor.WindowSpaceToPointSpace(newPos);
				Editor.Asset.shape.VerticePosition(Editor.SelectedId, newPos);

				Editor.Repaint();
			}
		}

		/// <summary>
		/// Displays the current edited point settings
		/// </summary>
		/// <param name="id"></param>
		private void DisplayPointPositionWindow(int windowId) {
			Editor.Asset.shape.VerticePosition(Editor.SelectedId,
				EditorGUILayout.Vector2Field("Point " + Editor.SelectedId, Editor.Asset.shape.GetVerticePosition(Editor.SelectedId)));
		}
	}
}