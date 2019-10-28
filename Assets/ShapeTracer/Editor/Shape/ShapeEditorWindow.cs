using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ShapeTracer.Shapes.Tools;

namespace ShapeTracer.Shapes
{
    public class ShapeEditorWindow : EditorWindow
    {
        #region Sub Classes

        private class Tool
        {
            public string _name = "Tool";
            public Action _behavior = null;
            public bool _executionConditions = true;
            public string _errorMessage = "Error : Cannot execute the tool behavior";
            //public KeyCode _shortCut; //TODO : shortcut support

            public Tool(string name, Action behavior)
            {
                _name = name;
                _behavior = behavior;
            }

            public Tool(string name, Action behavior, bool executionConditions, string errorMessage)
            {
                _name = name;
                _behavior = behavior;
                _executionConditions = executionConditions;
                _errorMessage = errorMessage;
            }
        }

        #endregion

        #region Attributes

        public ShapeAsset asset = null;

        /// <summary>
        /// The toolbox containing all the tools functions, bind tools in the RegisterTool() functions
        /// </summary>
        private Dictionary<string,Tool> _toolbox= new Dictionary<string, Tool>();
        private ShapeEditorTool[] _tools = { };
        private Tool _currentTool = null;
        private Tool _defaultTool = null;
        private bool _initializedTool = false;
        public bool showSelected = true;

        public bool selectionFlag = true;

        private static bool _isActive = false;

        private float pixelsPerUnits = 200.0f;

        private Vector2 spaceCenter = Vector2.zero;

        private int _selectedId = -1;
        public int selectedId
        {
            get
            {
                if(_selectedId > asset.shape.pointCount - 1)
                {
                    return -1; //reset
                }
                else
                {
                    return _selectedId;
                }
            }
            set { _selectedId = value; }
        }

        private Texture2D _bTex;
        private Texture2D _backgroundTexture
        {
            get
            {
                if(_bTex == null)
                {
                    _bTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _bTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
                    _bTex.Apply();
                }

                return _bTex;
            }
        }


        #region Accessors

        public static bool isActive { get { return _isActive; } }

        #endregion

        #endregion

        #region Constants

        private const float UTILITY_BUTTON_HEIGHT = 20.0f;
        private const float UTILITY_BUTTON_WIDTH = 80.0f;

        private const float TOOL_BUTTON_SIZE = 50.0f;

        private const float SHAPE_UTILITY_PANNEL_WIDTH = 250.0f;

        private const float ZOOM_SENSIBILITY = 0.022f;
        private const float BASE_GRID_RESOLUTION = 10.0f;

        private const float BOX_AREA_OFFSET = 8.0f;

        #endregion



        #region Init/Disable

        private void OnEnable()
        {
            _isActive = true;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Reset();
            InitToolbox();
            FindShapeEditorTools();
        }

        public static void Edit(ShapeAsset asset)
        {
            ShapeEditorWindow current = GetWindow<ShapeEditorWindow>("Shape Editor");
            current.asset = asset;
            current.Show();
            current.Reset();
        }


		private void FindShapeEditorTools()
		{
			List<ShapeEditorTool> tools = new List<ShapeEditorTool>();
			foreach(Type type in Assembly.GetAssembly(typeof(ShapeEditorTool)).GetTypes().Where(mType => mType.IsClass && !mType.IsAbstract &&
			mType.IsSubclassOf(typeof(ShapeEditorTool)))){
				tools.Add((ShapeEditorTool)Activator.CreateInstance(type));
			}

			_tools = tools.ToArray();
		}


		private void Reset()
        {
            spaceCenter = new Vector2(position.width / 2, position.height / 2);
            selectedId = -1;
            pixelsPerUnits = 200.0f;
        }
       
        private void OnDisable()
        {
            _isActive = false;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }


        #endregion

        #region GUI

        private void OnGUI()
        {
            if(Event.current.button == 0 && Event.current.type == EventType.MouseUp && IsIntoWorkSpace(Event.current.mousePosition))
            {
                selectionFlag = false;
            }

            Rect windowRect = new Rect(0, 0, position.width, position.height);
            GUI.DrawTexture(windowRect, _backgroundTexture, ScaleMode.StretchToFill);

            float gridResolution = RemapToFirstPowerOfTen(pixelsPerUnits);

            DrawGrid(gridResolution, 0.1f, false, Color.black);
            DrawGrid(gridResolution * 10.0f, 0.2f, true, Color.black);
			DrawGrid(1000f, 0.2f, false, Color.white);
            WindowNavigation();

            if (asset == null) return;


            ProcessTool();

            int pointsToDisplay = asset.shape.pointCount - (asset.shape.closeShape ? 1 : 0);

            DisplayPoints();

            DisplayEdges();


            Color lastBaseColor = GUI.color;
            GUI.color = new Color(0.5f, 0.5f, 0.5f);

            GUI.Box(new Rect(0, 0,
                TOOL_BUTTON_SIZE + BOX_AREA_OFFSET, UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET), new GUIContent());

            GUI.color = lastBaseColor;

            Rect toolPannelRect = new Rect(0, UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET
                , TOOL_BUTTON_SIZE + BOX_AREA_OFFSET
                , position.height - UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);
            DrawArea(toolPannelRect, true, DrawToolPannel);

            float upPannelWidth = Mathf.Clamp(position.width - SHAPE_UTILITY_PANNEL_WIDTH,
                260.0f,Mathf.Infinity);

			Rect modeUtilityPannelRect = new Rect(TOOL_BUTTON_SIZE + BOX_AREA_OFFSET, 0
				, upPannelWidth - TOOL_BUTTON_SIZE - BOX_AREA_OFFSET
				, UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);
            DrawArea(modeUtilityPannelRect, false, DrawModeUtilityPannel);

            Rect shapeUtilityPannelRect = new Rect(upPannelWidth, 0,
                SHAPE_UTILITY_PANNEL_WIDTH,
                UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);

            DrawArea(shapeUtilityPannelRect, false, DrawShapeUtilityPannel);
            CheckShortuts();

            showSelected = true;

            if (selectionFlag == false && _selectedId >= 0)
            {
                _selectedId = -1;
                Repaint();
            }
        }


        private void DrawArea(Rect rect, bool isVertical, Action drawFunction)
        {
            Rect areaRect = rect;

            areaRect.height -= BOX_AREA_OFFSET;
            areaRect.y += BOX_AREA_OFFSET / 2;
            areaRect.width -= BOX_AREA_OFFSET;
            areaRect.x += BOX_AREA_OFFSET / 2;

            GUI.Box(rect, new GUIContent());
            GUILayout.BeginArea(areaRect);

            if (isVertical)
            {
                EditorGUILayout.BeginVertical();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
            }

            drawFunction.Invoke();


            if (isVertical)
            {
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.EndHorizontal();
            }


            GUILayout.EndArea();
        }

        #endregion

        #region Shortcuts
        private void CheckShortuts()
        {
            Event e = Event.current;

            if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                _currentTool = _defaultTool;
            }
        }

        #endregion

        #region Window Navigation

        private void WindowNavigation()
        {
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
            }else if (e.keyCode == KeyCode.F && e.type == EventType.KeyDown)
			{
				spaceCenter = new Vector2(position.width, position.height) / 2;
				Repaint();
				e.Use();
			}
        }

		private Vector2 GetScreenSpaceMousePosition()
		{
			Vector2 mousePos = Event.current.mousePosition;
			mousePos.y = position.height - mousePos.y;
			return mousePos;
		}

		private Vector2 GetWorkspaceMousePosition()
		{
			Vector2 pos = GetScreenSpaceMousePosition();
			return WindowSpaceToPointSpace(Event.current.mousePosition);
		}

        #endregion

        #region Points

        #region Display

        private void DisplayPoints()
        {
            Event e = Event.current;

            Handles.BeginGUI();

            for (int i = 0; i < asset.shape.pointCount; i++)
            {
                Vector2 pointPos = PointSpaceToWindowSpace(asset.shape.GetPointPosition(i));

                if (!showSelected)
                {
                    Handles.color = Color.white;
                    Handles.DotHandleCap(0, pointPos, Quaternion.identity, 2.0f, EventType.Repaint);
                }
                else
                {
                    Handles.color = Color.white;
                    if (i == selectedId)
                    {
                        Handles.color = Color.green;
                        if (Handles.Button(pointPos, Quaternion.identity, 6f, 8f, Handles.DotHandleCap))
                        {
                            selectionFlag = true;
                            selectedId = i;
                            Repaint();
                        }
                    }
                    else
                    {
                        if (Handles.Button(pointPos, Quaternion.identity, 4f , 6f , Handles.DotHandleCap))
                        {
                            selectionFlag = true;
                            selectedId = i;
                            Repaint();
                        }
                    }
                }
            }


            Handles.EndGUI();
        }

        /// <summary>
        /// Displays the edges of all points
        /// </summary>
        private void DisplayEdges()
        {
            Handles.BeginGUI();

            Handles.color = Color.white;

            for(int i = 0; i < asset.shape.pointCount; i++)
            {
                if (i > 0)
                {
                    if(asset.shape.closeShape && i == asset.shape.pointCount - 1 && asset.shape.pointCount > 1)
                    {
                        Handles.color = Color.grey;
                    }

                    Handles.DrawLine( PointSpaceToWindowSpace(asset.shape.GetPointPosition(i)),
                        PointSpaceToWindowSpace(asset.shape.GetPointPosition(i - 1)));
                }
            }
            Handles.color = Color.white;

            Handles.EndGUI();
        }


        /// <summary>
        /// Displays the current edited point settings
        /// </summary>
        /// <param name="id"></param>
        private void DisplayPointPositionWindow(int id)
        {
            asset.shape.SetPointPosition(selectedId,
                EditorGUILayout.Vector2Field("Point " + selectedId, asset.shape.GetPointPosition(selectedId)));
        }

        #endregion

        #region Position

        public Vector2 PointSpaceToWindowSpace(Vector2 position)
        {
            position.y *= -1; //Invert the y axis
            return position * pixelsPerUnits + spaceCenter;
        }

        public Vector2 WindowSpaceToPointSpace(Vector2 position)
        {
            position = (position - spaceCenter) / pixelsPerUnits;
            position.y *= -1;
            return position;
        }

        #endregion

        #endregion

        #region Toolbox

        #region Init Tools

        /// <summary>
        /// Register all the tools function to the toolbx dictionnary
        /// </summary>
        private void InitToolbox()
        {
            _defaultTool = new Tool("Move", MoveSelectedPoint);
            _toolbox.Add("MovePoint", _defaultTool);

            _toolbox.Add("DrawPoints", new Tool("Draw\nPoints", DrawPoints));

            _currentTool = _defaultTool;
        }

        #endregion

        #region Tool Pannel

        private void DrawToolButton(Tool tool, bool additionnalConditions = true)
        {
            Color lastColor = GUI.backgroundColor;
            Color buttonColor = (tool == _currentTool ? new Color(0.4f, 0.4f, 0.4f) : Color.white);

            GUI.backgroundColor = buttonColor;

            if (GUILayout.Button(tool._name, GUILayout.Width(TOOL_BUTTON_SIZE), GUILayout.Height(TOOL_BUTTON_SIZE)))
            {
                if(_currentTool != tool)
                {
                    _currentTool = tool;
                }
                else
                {
                    _currentTool = _defaultTool; //Toggle off
                }

                _initializedTool = false;
            }

            GUI.backgroundColor = lastColor;
        }


        /// <summary>
        /// Draws tools pannel
        /// </summary>
        private void DrawToolPannel()
        {
            //DrawToolButton("Draw\nPoints", DrawPoints);

            foreach(KeyValuePair<string, Tool> tool in _toolbox)
            {
                DrawToolButton(tool.Value);
            }
        }

        #endregion

        #region Tools

        /// <summary>
        /// Adds a point to the shape
        /// </summary>
        private void DrawPoints()
        {
            showSelected = false;

            if(_initializedTool == false)
            {
                _selectedId = -1; //Reset Selection
            }


            Handles.BeginGUI();
            Handles.color = Color.yellow;

            Vector2 mousePos = Event.current.mousePosition;

            //Snap
            if (Event.current.control == true)
            {
                mousePos = SnapToGrid( mousePos);
            }

            if (asset.shape.closeShape == true && asset.shape.pointCount > 1)
            {
                Vector2 lastPoint = PointSpaceToWindowSpace(asset.shape.GetPointPosition(asset.shape.pointCount - 2));
                Vector2 firstPoint = PointSpaceToWindowSpace(asset.shape.GetPointPosition(0));

                Handles.DrawLine(lastPoint, mousePos);
                Handles.DrawLine(firstPoint, mousePos);
            }
            else if(asset.shape.closeShape == false && asset.shape.pointCount > 0)
            {
                Vector2 fromPoint = PointSpaceToWindowSpace(asset.shape.GetPointPosition(asset.shape.pointCount - 1));
                Handles.DrawLine(fromPoint, mousePos);
            }

            Handles.DotHandleCap(0, mousePos, Quaternion.identity, 5.0f, EventType.Repaint);

            if(Event.current.button == 0 && Event.current.type == EventType.MouseDown && IsIntoWorkSpace(mousePos))
            {
                Undo.RecordObject(asset, "Add Point");
                EditorUtility.SetDirty(asset);
                asset.shape.AddPoint(WindowSpaceToPointSpace(mousePos));
                Event.current.Use();
                Repaint();
            }
                

            Repaint();

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        /// Moves the selected point
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pointPos"></param>
        private void MoveSelectedPoint()
        {
            if (selectedId < 0) return;

            BeginWindows();
            GUI.Window(0, new Rect(position.width - 200, position.height - 100, 180, 80), DisplayPointPositionWindow, "Point Position");
            EndWindows();

            Handles.color = Color.green;

            Vector2 pointPos = PointSpaceToWindowSpace( asset.shape.GetPointPosition(_selectedId));

            if(Event.current.button == 0 && Event.current.type == EventType.MouseDrag)
            {
                Undo.RecordObject(asset, "Move Shape Point");
                EditorUtility.SetDirty(asset);

                selectionFlag = true;

                Event e = Event.current;
                Vector2 newPos = e.mousePosition;

                //Snap on ctrl old
                if (e.control == true)
                {
                    newPos = SnapToGrid( newPos);
                }

                newPos = WindowSpaceToPointSpace(newPos);
                asset.shape.SetPointPosition(_selectedId, newPos);

                Repaint();
            }
        }

        #endregion

        #region Tool process

        private void ProcessTool()
        {
            if(_currentTool != null)
            {
                _currentTool._behavior.Invoke();
            }
            else
            {
                if(_defaultTool != null)
                    _defaultTool._behavior.Invoke();
            }

            if(_initializedTool == false)
            {
                _initializedTool = true;
            }
        }

        #endregion

        #endregion

        #region Current Tool Utility

        private void DrawModeUtilityPannel()
        {
            Event e = Event.current;
            RemovePointUtility(e);
        }

        private void RemovePointUtility(Event e)
        {
            if (!showSelected) return;

            GUILayout.Space(BOX_AREA_OFFSET/2);

            if (selectedId >= 0)
            {
                bool delete = false;

                if (e.keyCode == KeyCode.Delete && e.type == EventType.KeyDown)
                {
                    e.Use();
                    delete = true;
                }

                if (GUILayout.Button("Remove Point", GUILayout.Width(95), GUILayout.Height(UTILITY_BUTTON_HEIGHT)))
                {
                    delete = true;
                }

                if (delete == true)
                {
                    Undo.RecordObject(asset, "Delete Point");
                    EditorUtility.SetDirty(asset);
                    asset.shape.RemovePoint(selectedId);
                }

            }
        }

        #endregion

        #region Shape Utility

        private void DrawShapeUtilityPannel()
        {
            GUILayout.Space(BOX_AREA_OFFSET/2);

            asset.shape.closeShape =
                GUILayout.Toggle(asset.shape.closeShape, "Close Shape", "Button",
                GUILayout.Width(85), GUILayout.Height(UTILITY_BUTTON_HEIGHT));
        }

        #endregion

        #region Grid

        private void DrawGrid(float spacing, float opacity, bool displayUnits, Color color)
        {
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

            for(float h = yOffset; h < position.height; h += spacing)
            {
                Handles.DrawLine( new Vector2(0, h), new Vector2(position.width, h));
            }

            for (float v = xOffset; v < position.width; v += spacing)
            {
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
        public Vector2 SnapToGrid(Vector2 pos)
        {
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

        private void OnUndoRedoPerformed()
        {
            Repaint();
        }

        #endregion

        #region Utils



        private static float RemapToFirstPowerOfTen(float value)
        {
            while (value > 10.0f || value < 1.0f)
            {
                if (value > 10.0f)
                {
                    value /= 10.0f;
                }
                else if (value < 1.0f)
                {
                    value *= 10.0f;
                }
            }

            return value;
        }

        public bool IsIntoWorkSpace(Vector2 pos)
        {
            Rect workSpace = new Rect(TOOL_BUTTON_SIZE + BOX_AREA_OFFSET,
                UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET,
                position.width - TOOL_BUTTON_SIZE + BOX_AREA_OFFSET,
                position.height - UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);

            return workSpace.Contains(pos);
        }

        #endregion
    }
}