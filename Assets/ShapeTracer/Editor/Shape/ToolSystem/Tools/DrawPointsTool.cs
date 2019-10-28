using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShapeTracer.Shapes.Tools
{
	[ShapeToolIdentity("Draw Points", "Add or insert points on the shape")]
    public class DrawPointsTool : ShapeEditorTool
    {
        public DrawPointsTool()
        {
            _name = "Draw Point Tool";
            _content.text = "Draw\nPoints";
            _content.tooltip = "Draw a point at the left click position";
        }

        public override void Process(ShapeEditorWindow editor)
        {
            editor.showSelected = false;


            Handles.BeginGUI();
            Handles.color = Color.yellow;

            Vector2 mousePos = Event.current.mousePosition;

            //Snap
            if (Event.current.control == true)
            {
                mousePos = editor.SnapToGrid(mousePos);
            }

            if (editor.asset.shape.closeShape == true && editor.asset.shape.pointCount > 1)
            {
                Vector2 lastPoint = editor.PointSpaceToWindowSpace(editor.asset.shape.GetPointPosition(editor.asset.shape.pointCount - 2));
                Vector2 firstPoint = editor.PointSpaceToWindowSpace(editor.asset.shape.GetPointPosition(0));

                Handles.DrawLine(lastPoint, mousePos);
                Handles.DrawLine(firstPoint, mousePos);
            }
            else if (editor.asset.shape.closeShape == false && editor.asset.shape.pointCount > 0)
            {
                Vector2 fromPoint = editor.PointSpaceToWindowSpace(editor.asset.shape.GetPointPosition(editor.asset.shape.pointCount - 1));
                Handles.DrawLine(fromPoint, mousePos);
            }

            Handles.DotHandleCap(0, mousePos, Quaternion.identity, 5.0f, EventType.Repaint);

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && editor.IsIntoWorkSpace(mousePos))
            {
                Undo.RecordObject(editor.asset, "Add Point");
                EditorUtility.SetDirty(editor.asset);
                editor.asset.shape.AddPoint(editor.WindowSpaceToPointSpace(mousePos));
                Event.current.Use();
                editor.Repaint();
            }


            editor.Repaint();

            Handles.color = Color.white;
            Handles.EndGUI();
        }
    }
}