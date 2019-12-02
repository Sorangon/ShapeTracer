using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShapeTracer.Shapes.Tools {
	[ShapeTool("Draw", "Add or insert points on the shape", "Assets/ShapeTracer/Editor/Skin/Textures/T_Draw_Icon.png", 10)]
	public class DrawPointsTool : ShapeEditorTool {

		public override void Init() {
			Editor.enabledSelection = false;
		}

		public override void Process() {
			Handles.BeginGUI();
			Handles.color = Color.yellow;
			Vector2 mousePos = Event.current.mousePosition;

			//Snap
			if (Event.current.control == true) {
				mousePos = Editor.SnapToGrid(mousePos);
			}

			if (Editor.Asset.shape.closeShape == true && Editor.Asset.shape.PointCount > 1) {
				Vector2 lastPoint = Editor.PointSpaceToWindowSpace(Editor.Asset.shape.GetVerticePosition(Editor.Asset.shape.PointCount - 2));
				Vector2 firstPoint = Editor.PointSpaceToWindowSpace(Editor.Asset.shape.GetVerticePosition(0));

				Handles.DrawLine(lastPoint, mousePos);
				Handles.DrawLine(firstPoint, mousePos);
			}
			else if (Editor.Asset.shape.closeShape == false && Editor.Asset.shape.PointCount > 0) {
				Vector2 fromPoint = Editor.PointSpaceToWindowSpace(Editor.Asset.shape.GetVerticePosition(Editor.Asset.shape.PointCount - 1));
				Handles.DrawLine(fromPoint, mousePos);
			}

			Handles.DotHandleCap(0, mousePos, Quaternion.identity, 5.0f, EventType.Repaint);

			if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && Editor.IsIntoWorkSpace(mousePos)) {
				Undo.RecordObject(Editor.Asset, "Add Point");
				EditorUtility.SetDirty(Editor.Asset);
				Vector2 verticePos = Editor.WindowSpaceToPointSpace(mousePos);
				float lastVerticeU = Editor.Asset.shape.GetVertexU(Editor.Asset.shape.PointCount - 1);
				float verticeU = lastVerticeU + (1 - lastVerticeU) / 2;
				Editor.Asset.shape.AddVertice(new Shape.Vertice(verticePos, verticeU));
				Event.current.Use();
				Editor.Repaint();
			}


			Editor.Repaint();

			Handles.color = Color.white;
			Handles.EndGUI();
		}
	}
}