using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShapeTracer.Shapes.Tools
{
	[ShapeToolIdentity("Move Tool", "Move the selected point on the workspace")]
    public class MoveTool : ShapeEditorTool
    {
        public MoveTool()
        {
            _name = "Move Tool";
            _content.text = "Move";
            _content.tooltip = "Moves the selected point";
        }

        public override void Process(ShapeEditorWindow editor)
        {
            if (editor.selectedId < 0) return;

            /*editor.BeginWindows();
            GUI.Window(0, new Rect(editor.position.width - 200, editor.position.height - 100, 180, 80), DisplayPointPositionWindow, "Point Position");
            editor.EndWindows();*/

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
        /*private void DisplayPointPositionWindow()
        {
            editor.asset.shape.SetPointPosition(editor.selectedId,
                EditorGUILayout.Vector2Field("Point " + editor.selectedId, editor.asset.shape.GetPointPosition(editor.selectedId)));
        }*/
    }
}