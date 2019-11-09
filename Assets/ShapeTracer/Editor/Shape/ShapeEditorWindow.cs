using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ShapeTracer.Shapes.Tools;

namespace ShapeTracer.Shapes {
	public class ShapeEditorWindow : EditorWindow {

		#region Properties

		/// <summary>
		/// Does the selection is enabled, reset to true on tool change
		/// </summary>
		public bool enabledSelection = true;

		public ShapeAsset Asset => _asset;

		public int SelectedId {
			get {
				if (_selectedId > _asset.shape.PointCount - 1) {
					return -1; //reset
				}
				else {
					return _selectedId;
				}
			}
			set { _selectedId = value; }
		}

		public static bool IsActive => _isActive;

		#region Private

		private Texture2D _backgroundTexture {
			get {
				if (_bTex == null) {
					_bTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
					_bTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
					_bTex.Apply();
				}

				return _bTex;
			}
		}

		#endregion

		#endregion

		#region Current state
		private ShapeAsset _asset = null;

		private ShapeEditorTool[] _tools = { };
		private ShapeEditorTool _currentTool = null;

		private static bool _isActive = false;

		private float pixelsPerUnits = 200.0f;

		private Vector2 spaceCenter = Vector2.zero;

		private int _selectedId = -1;

		private Texture2D _bTex;
		private GUISkin _guiSkin;
		#endregion

		#region Constants
		
		private const float UTILITY_BUTTON_HEIGHT = 30.0f;
		private const float UTILITY_BUTTON_WIDTH = 80.0f;

		private const float TOOL_BUTTON_SIZE = 50.0f;

		private const float SHAPE_UTILITY_PANNEL_WIDTH = 250.0f;

		private const float ZOOM_SENSIBILITY = 0.022f;
		private const float BASE_GRID_RESOLUTION = 10.0f;

		private const float BOX_AREA_OFFSET = 8.0f;

		public const int NULL_POINT = -1;

		#endregion


		#region Init/Disable

		private void OnEnable() {
			_isActive = true;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			Reset();
			FindShapeEditorTools();

			//Load skin
			_guiSkin = (GUISkin)AssetDatabase.
				LoadAssetAtPath("Assets/ShapeTracer/Editor/Skin/GUISkins/GUIS_ShapeEditor.guiskin", (typeof(GUISkin)));
		}

		public static void Edit(ShapeAsset asset) {
			ShapeEditorWindow current = GetWindow<ShapeEditorWindow>("Shape Editor");
			current._asset = asset;
			current.Show();
			current.Reset();
		}


		private void FindShapeEditorTools() {
			SortedList<int, ShapeEditorTool> tools = new SortedList<int, ShapeEditorTool>();
			foreach (Type type in Assembly.GetAssembly(typeof(ShapeEditorTool)).GetTypes().Where(mType => mType.IsClass && !mType.IsAbstract &&
			 mType.IsSubclassOf(typeof(ShapeEditorTool)))) {
				GUIContent content = new GUIContent();
				ShapeEditorTool tool = (ShapeEditorTool)Activator.CreateInstance(type, this);
				int order = 10000;

				//Get tool identity
				var toolIdentity = (ShapeToolAttribute)Attribute.GetCustomAttribute(tool.GetType(), typeof(ShapeToolAttribute));
				if (toolIdentity != null) {
					content.text = toolIdentity.Name;
					content.tooltip = toolIdentity.Tooltip;
					Texture tex = (Texture)AssetDatabase.LoadAssetAtPath(toolIdentity.IconPath, typeof(Texture));
					if(tex != null) {
						content.image = tex;
					}
					order = toolIdentity.Order;
				}
				else {
					content.text = tool.GetType().Name;
				}
				tool.SetEditor(this); //Set the owner editor
				tool.content = content;
				tools.Add(order, tool);
			}

			_tools = new ShapeEditorTool[tools.Count];
			for (int i = 0; i < tools.Count; i++) {
				_tools[i] = tools.ElementAt(i).Value;
			}

			if(_tools.Length > 0) {
				_currentTool = _tools[0];
			}
		}


		private void Reset() {
			spaceCenter = new Vector2(position.width / 2, position.height / 2);
			SelectedId = -1;
			pixelsPerUnits = 200.0f;
		}

		private void OnDisable() {
			_isActive = false;
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
		}


		#endregion

		#region GUI

		private void OnGUI() {
			GUI.skin = _guiSkin; //Set gui skin

			Rect windowRect = new Rect(0, 0, position.width, position.height);
			GUI.DrawTexture(windowRect, _backgroundTexture, ScaleMode.StretchToFill);

			float gridResolution = RemapToFirstPowerOfTen(pixelsPerUnits);

			DrawGrid(gridResolution, 0.1f, false, Color.black);
			DrawGrid(gridResolution * 10.0f, 0.2f, true, Color.black);
			DrawGrid(1000f, 0.2f, false, Color.white);
			WindowNavigation();

			if (_asset == null) return;

			//Process current selected tool
			if (_currentTool != null) {
				_currentTool.Process();
			}

			//Manage point display and controls
			int pointsToDisplay = _asset.shape.PointCount - (_asset.shape.closeShape ? 1 : 0);
			if (enabledSelection) {
				SelectPoints();
			}

			DisplayPoints();
			DisplayEdges();

			//Display pannels
			Color lastBaseColor = GUI.color;
			GUI.color = new Color(0.5f, 0.5f, 0.5f);

			GUI.Box(new Rect(0, 0,
				TOOL_BUTTON_SIZE + BOX_AREA_OFFSET, UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET), new GUIContent());

			GUI.color = lastBaseColor;

			Rect toolPannelRect = new Rect(0, UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET
				, TOOL_BUTTON_SIZE + BOX_AREA_OFFSET
				, position.height - UTILITY_BUTTON_HEIGHT - BOX_AREA_OFFSET);
			DrawArea(toolPannelRect, true, DrawToolPannel);

			float upPannelWidth = Mathf.Clamp(position.width - SHAPE_UTILITY_PANNEL_WIDTH,
				260.0f, Mathf.Infinity);

			Rect modeUtilityPannelRect = new Rect(TOOL_BUTTON_SIZE + BOX_AREA_OFFSET, 0
				, upPannelWidth - TOOL_BUTTON_SIZE - BOX_AREA_OFFSET
				, UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);
			DrawArea(modeUtilityPannelRect, false, DrawModeUtilityPannel);

			Rect shapeUtilityPannelRect = new Rect(upPannelWidth, 0,
				SHAPE_UTILITY_PANNEL_WIDTH,
				UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);

			DrawArea(shapeUtilityPannelRect, false, DrawShapeUtilityPannel);

			//Check inputs
			CheckShortuts();
		}


		private void DrawArea(Rect rect, bool isVertical, Action drawFunction) {
			Rect areaRect = rect;

			areaRect.height -= BOX_AREA_OFFSET;
			areaRect.y += BOX_AREA_OFFSET / 2;
			areaRect.width -= BOX_AREA_OFFSET;
			areaRect.x += BOX_AREA_OFFSET / 2;

			GUI.Box(rect, new GUIContent());
			GUILayout.BeginArea(areaRect);

			if (isVertical) {
				EditorGUILayout.BeginVertical();
			}
			else {
				EditorGUILayout.BeginHorizontal();
			}

			drawFunction.Invoke();


			if (isVertical) {
				EditorGUILayout.EndVertical();
			}
			else {
				EditorGUILayout.EndHorizontal();
			}


			GUILayout.EndArea();
		}

		#endregion

		#region Shortcuts
		private void CheckShortuts() {
			Event e = Event.current;

			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) {
				SetTool(null);
				Repaint();
			}
		}

		#endregion

		#region Window Navigation

		private void WindowNavigation() {
			Event e = Event.current;

			if (e.isScrollWheel) //Zoom
			{
				//Gets the zoom amount
				float delta = e.delta.y;
				float zoom = pixelsPerUnits * delta * ZOOM_SENSIBILITY;
				Vector2 zoomDirection = GetWorkspaceMousePosition();
				pixelsPerUnits = pixelsPerUnits - zoom;

				//Add an offset from the center to zoom on the cursor position
				zoomDirection = (GetWorkspaceMousePosition() - zoomDirection) * pixelsPerUnits;
				zoomDirection.y *= -1;
				spaceCenter += zoomDirection;
				Repaint();
				e.Use();
			}
			else if (e.button == 2 && e.type == EventType.MouseDrag) //Pan view
			{
				spaceCenter += e.delta;
				Repaint();
				e.Use();
			}
			else if (e.keyCode == KeyCode.F && e.type == EventType.KeyDown) {
				spaceCenter = new Vector2(position.width, position.height) / 2;
				Repaint();
				e.Use();
			}
		}

		private Vector2 GetScreenSpaceMousePosition() {
			Vector2 mousePos = Event.current.mousePosition;
			mousePos.y = position.height - mousePos.y;
			return mousePos;
		}

		private Vector2 GetWorkspaceMousePosition() {
			Vector2 pos = GetScreenSpaceMousePosition();
			return WindowSpaceToPointSpace(Event.current.mousePosition);
		}

		#endregion

		#region Points

		#region Display

		private void DisplayPoints() {
			Handles.BeginGUI();
			for (int i = 0; i < _asset.shape.PointCount; i++) {
				Vector2 pointPos = PointSpaceToWindowSpace(_asset.shape.GetPointPosition(i));
				if (_selectedId == i && enabledSelection) {
					Handles.color = Color.green;
				}
				else {
					Handles.color = Color.white;
				}
				Handles.DotHandleCap(0, pointPos, Quaternion.identity, 4.0f, EventType.Repaint);
			}
			Handles.EndGUI();
		}

		/// <summary>
		/// Displays the edges of all points
		/// </summary>
		private void DisplayEdges() {
			Handles.BeginGUI();

			Handles.color = Color.white;

			for (int i = 0; i < _asset.shape.PointCount; i++) {
				if (i > 0) {
					if (_asset.shape.closeShape && i == _asset.shape.PointCount - 1 && _asset.shape.PointCount > 1) {
						Handles.color = Color.grey;
					}

					Handles.DrawLine(PointSpaceToWindowSpace(_asset.shape.GetPointPosition(i)),
						PointSpaceToWindowSpace(_asset.shape.GetPointPosition(i - 1)));
				}
			}
			Handles.color = Color.white;

			Handles.EndGUI();
		}


		/// <summary>
		/// Displays the current edited point settings
		/// </summary>
		/// <param name="id"></param>
		private void DisplayPointPositionWindow(int id) {
			_asset.shape.SetPointPosition(SelectedId,
				EditorGUILayout.Vector2Field("Point " + SelectedId, _asset.shape.GetPointPosition(SelectedId)));
		}

		#endregion

		#region Position

		public Vector2 PointSpaceToWindowSpace(Vector2 position) {
			position.y *= -1; //Invert the y axis
			return position * pixelsPerUnits + spaceCenter;
		}

		public Vector2 WindowSpaceToPointSpace(Vector2 position) {
			position = (position - spaceCenter) / pixelsPerUnits;
			position.y *= -1;
			return position;
		}

		#endregion

		#region Selection

		/// <summary>
		/// Checks points selection
		/// </summary>
		private void SelectPoints() {
			if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && IsIntoWorkSpace(Event.current.mousePosition)) {
				_selectedId = ShapeEditorSelection.GetClosestPointIndex(Event.current.mousePosition, this);
				Repaint();
				Event.current.Use();
			}
		}

		#endregion

		#endregion

		#region Toolbox

		#region Tool Pannel
		/// <summary>
		/// Draws the tool buttons pannel
		/// </summary>
		/// <param name="tool"></param>
		private void DrawToolButton(ShapeEditorTool tool) {
			Color lastColor = GUI.backgroundColor;
			Color buttonColor = (tool == _currentTool ? new Color(0.4f, 1.0f, 1.0f) : Color.white);

			GUI.backgroundColor = buttonColor;

			if (GUILayout.Button(tool.content, GUILayout.Width(TOOL_BUTTON_SIZE), GUILayout.Height(TOOL_BUTTON_SIZE))) {
				SetTool(tool);
			}

			GUI.backgroundColor = lastColor;
		}


		/// <summary>
		/// Draws tools pannel
		/// </summary>
		private void DrawToolPannel() {
			foreach (ShapeEditorTool tool in _tools) {
				DrawToolButton(tool);
			}
		}

		/// <summary>
		/// Set the current tool, disable if equal to the parameter
		/// </summary>
		/// <param name="tool"></param>
		private void SetTool(ShapeEditorTool tool) {
			if(tool != _currentTool) {
				_currentTool = tool;
			}
			else {
				_currentTool = null;
			}
			enabledSelection = true; //By default enable the selection on change tool

			if(_currentTool != null) {
				_currentTool.Init(); //Initialize the tool
			}
			else {
				if(_tools.Length > 0) {
					_currentTool = _tools[0];
				}
			}
		}

		#endregion

		#endregion

		#region Current Tool Utility

		private void DrawModeUtilityPannel() {
			Event e = Event.current;
			RemovePointUtility(e);
		}

		private void RemovePointUtility(Event e) {
			if (!enabledSelection) return;

			GUILayout.Space(BOX_AREA_OFFSET / 2);

			if (SelectedId >= 0) {
				bool delete = false;

				if (e.keyCode == KeyCode.Delete && e.type == EventType.KeyDown) {
					e.Use();
					delete = true;
				}

				if (GUILayout.Button("Remove Point", GUILayout.Width(95), GUILayout.Height(UTILITY_BUTTON_HEIGHT))) {
					delete = true;
				}

				if (delete == true) {
					Undo.RecordObject(_asset, "Delete Point");
					EditorUtility.SetDirty(_asset);
					_asset.shape.RemovePoint(SelectedId);
				}

			}
		}

		#endregion

		#region Shape Utility

		private void DrawShapeUtilityPannel() {
			GUILayout.Space(BOX_AREA_OFFSET / 2);

			_asset.shape.closeShape =
				GUILayout.Toggle(_asset.shape.closeShape, "Close Shape", "Button",
				GUILayout.Width(85), GUILayout.Height(UTILITY_BUTTON_HEIGHT));
		}

		#endregion

		#region Grid

		private void DrawGrid(float spacing, float opacity, bool displayUnits, Color color) {
			spacing *= BASE_GRID_RESOLUTION;

			int widthDivs = Mathf.CeilToInt(position.width / spacing);
			int heightDivs = Mathf.CeilToInt(position.height / spacing);

			Handles.BeginGUI();

			Color backupColor = Handles.color;

			Color gridColor = color;
			gridColor.a = opacity;

			Handles.color = gridColor;

			float xOffset = spaceCenter.x % spacing;
			float yOffset = spaceCenter.y % spacing;

			for (float h = yOffset; h < position.height; h += spacing) {
				Handles.DrawLine(new Vector2(0, h), new Vector2(position.width, h));
			}

			for (float v = xOffset; v < position.width; v += spacing) {
				Handles.DrawLine(new Vector2(v, 0), new Vector2(v, position.height));
			}

			Handles.color = backupColor;

			Handles.EndGUI();
		}

		/// <summary>
		/// Returns the snaped position of the input one
		/// </summary>
		/// <param name="spacing"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public Vector2 SnapToGrid(Vector2 pos) {
			float spacing = RemapToFirstPowerOfTen(pixelsPerUnits) * BASE_GRID_RESOLUTION;
			Vector2 snappedPos = new Vector2();

			snappedPos.x = Mathf.Floor(pos.x / spacing) * spacing;
			snappedPos.y = Mathf.Floor(pos.y / spacing) * spacing;

			snappedPos.x += spaceCenter.x % spacing;
			snappedPos.y += spaceCenter.y % spacing;

			return snappedPos;
		}

		#endregion

		#region Events

		private void OnUndoRedoPerformed() {
			Repaint();
		}

		#endregion

		#region Utils



		private static float RemapToFirstPowerOfTen(float value) {
			while (value > 10.0f || value < 1.0f) {
				if (value > 10.0f) {
					value /= 10.0f;
				}
				else if (value < 1.0f) {
					value *= 10.0f;
				}
			}

			return value;
		}

		public bool IsIntoWorkSpace(Vector2 pos) {
			Rect workSpace = new Rect(TOOL_BUTTON_SIZE + BOX_AREA_OFFSET,
				UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET,
				position.width - TOOL_BUTTON_SIZE + BOX_AREA_OFFSET,
				position.height - UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);

			return workSpace.Contains(pos);
		}

		#endregion
	}
}