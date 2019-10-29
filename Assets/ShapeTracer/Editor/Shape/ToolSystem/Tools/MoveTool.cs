using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShapeTracer.Shapes.Tools
{
	[ShapeToolIdentity("Move", "Move the selected point on the workspace", 0)]
    public class MoveTool : ShapeEditorTool
    {
		#region Current State

		private ShapeEditorWindow _editor = null;

		#endregion

		public override void Init(ShapeEditorWindow editor)
		{
			_editor = editor;
		}

		public override void Process(ShapeEditorWindow editor)
        {
            if (editor.selectedId < 0) return;

			editor.BeginWindows();
			GUI.Window(0, new Rect(editor.position.width - 200, editor.position.height - 100, 180, 80), DisplayPointPositionWindow, "Point Position");
			editor.EndWindows();

			Handles.color = Color.green;

            Vector2 pointPos = editor.PointSpaceToWindowSpace(editor.asset.shape.GetPointPosition(editor.selectedId));

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDrag)
            {
                Undo.RecordObject(editor.asset, "Move Shape Point");
                EditorUtility.SetDirty(editor.asset);

                editor.selectionFlag = true;

                Event e = Event.current;
                Vector2 newPos = e.mousePosition;

                //Snap on ctrl old
                if (e.control == true)
                {
                    newPos = editor.SnapToGrid(newPos);
                }

                newPos = editor.WindowSpaceToPointSpace(newPos);
                editor.asset.shape.SetPointPosition(editor.selectedId, newPos);

                editor.Repaint();
            }
        }

		/// <summary>
		/// Displays the current edited point settings
		/// </summary>
		/// <param name="id"></param>
		private void DisplayPointPositionWindow(int windowId)
		{
			_editor.asset.shape.SetPointPosition(_editor.selectedId,
				EditorGUILayout.Vector2Field("Point " + _editor.selectedId, _editor.asset.shape.GetPointPosition(_editor.selectedId)));
		}
	}
}