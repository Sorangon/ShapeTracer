using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeTracer.Shapes.Tools {
	public static class ShapeEditorSelection {

		#region Constants
		public const float SELECTION_DISTANCE = 20.0f;
		public static int NONE => ShapeEditorWindow.NULL_POINT;
		#endregion

		#region Point selection

		/// <summary>
		/// Returns the closest point from a position, returns null point if the position is too far
		/// </summary>
		/// <param name="position"></param>
		/// <param name="editor"></param>
		/// <param name="maximumDistance"></param>
		/// <returns></returns>
		public static int GetClosestPointIndex(Vector2 position , ShapeEditorWindow editor, float maximumDistance = SELECTION_DISTANCE) {
			float closestDistance = Mathf.Infinity;
			int closestId = 0;

			//Convert the position to the point space dimensions
			Vector2 pointSpacePos = editor.WindowSpaceToPointSpace(position);

			//Check all point distances and keep the closest one
			for (int i = 0; i < editor.Asset.shape.PointCount; i++) {
				float distance = Vector2.Distance(pointSpacePos, editor.Asset.shape.GetPointPosition(i));
				if(distance < closestDistance) {
					closestId = i;
					closestDistance = distance;
				}
			}
			
			//Convert the point position to window space to calculate distance relatively to the screen
			Vector2 selectedPointScreenPos = editor.Asset.shape.GetPointPosition(closestId);
			selectedPointScreenPos = editor.PointSpaceToWindowSpace(selectedPointScreenPos);

			//Returns the point if this one is close enough
			if (Vector2.Distance(selectedPointScreenPos, position) <= maximumDistance) {
				return closestId;
			}
			return NONE;
		}

		#endregion
	}
}
