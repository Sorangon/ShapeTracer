using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

        private static ShapeAsset _target = null;

        /// <summary>
        /// The toolbox containing all the tools functions, bind tools in the RegisterTool() functions
        /// </summary>
        private Dictionary<string,Tool> _toolbox= new Dictionary<string, Tool>();
        private Tool _currentTool = null;
        private Tool _defaultTool = null;
        private bool _showSelected = true;

        private static bool _isActive = false;

        private static float _pixelsPerUnit = 200.0f;
        private static Vector2 _center = Vector2.zero;

        private static int _selectedId = -1;
        private static int selectedId
        {
            get
            {
                if(_selectedId > _target.shape.pointCount - 1)
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
        }

        public static void Edit(ShapeAsset asset)
        {
            ShapeEditorWindow current = GetWindow<ShapeEditorWindow>("Shape Editor");
            _target = asset;
            current.Show();
            current.Reset();
        }

        private void Reset()
        {
            _center = new Vector2(position.width / 2, position.height / 2);
            selectedId = -1;
            _pixelsPerUnit = 200.0f;
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
            Rect windowRect = new Rect(0, 0, position.width, position.height);
            GUI.DrawTexture(windowRect, _backgroundTexture, ScaleMode.StretchToFill);

            float gridResolution = RemapToFirstPowerOfTen(_pixelsPerUnit);

            DrawGrid(gridResolution, 0.1f, false);
            DrawGrid(gridResolution * 10.0f, 0.2f, true);
            WindowNavigation();

            if (_target == null) return;


            ProcessTool();

            int pointsToDisplay = _target.shape.pointCount - (_target.shape.closeShape ? 1 : 0);
            for (int i = 0; i < pointsToDisplay; i++)
            {
                DisplayPoints(i);
            }

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
                , upPannelWidth
                , UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);
            DrawArea(modeUtilityPannelRect, false, DrawModeUtilityPannel);

            Rect shapeUtilityPannelRect = new Rect(upPannelWidth, 0,
                SHAPE_UTILITY_PANNEL_WIDTH,
                UTILITY_BUTTON_HEIGHT + BOX_AREA_OFFSET);

            DrawArea(shapeUtilityPannelRect, false, DrawShapeUtilityPannel);

            _showSelected = true;
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

        #region Window Navigation

        private void WindowNavigation()
        {
            Event e = Event.current;


            if (e.isScrollWheel) //Zoom
            {
                float zoom = _pixelsPerUnit * Event.current.delta.y;
                _pixelsPerUnit = _pixelsPerUnit - zoom * ZOOM_SENSIBILITY;
                Repaint();
                e.Use();
            }
            else if (e.button == 2 && e.type == EventType.MouseDrag) //Pan view
            {
                _center += e.delta;
                Repaint();
                e.Use();
            }
        }

        #endregion

        #region Points

        #region Display

        private void DisplayPoints(int index)
        {
            Handles.BeginGUI();

            Vector2 pointPos = PointSpaceToWindowSpace(_target.shape.GetPointPosition(index));

            if(!_showSelected)
            {
                Handles.color = Color.white;
                Handles.DotHandleCap(0, pointPos, Quaternion.identity, 2.0f, EventType.Repaint);
            }
            else
            {
                if (index == selectedId)
                {
                    Handles.color = Color.green;
                    Handles.DotHandleCap(0, pointPos, Quaternion.identity, 8.0f, EventType.Repaint);
                }
                else
                {
                    Handles.color = Color.white;
                    if (Handles.Button(pointPos, Quaternion.identity, 4f, 6f, Handles.DotHandleCap))
                    {
                        selectedId = index;
                        Repaint();
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

            for(int i = 0; i < _target.shape.pointCount; i++)
            {
                if (i > 0)
                {
                    Handles.DrawLine( PointSpaceToWindowSpace(_target.shape.GetPointPosition(i)),
                        PointSpaceToWindowSpace(_target.shape.GetPointPosition(i - 1)));
                }
            }

            Handles.EndGUI();
        }


        /// <summary>
        /// Displays the current edited point settings
        /// </summary>
        /// <param name="id"></param>
        private void DisplayPointPositionWindow(int id)
        {
            _target.shape.SetPointPosition(selectedId,
                EditorGUILayout.Vector2Field("Point " + selectedId, _target.shape.GetPointPosition(selectedId)));
        }

        #endregion

        #region Position

        private Vector2 PointSpaceToWindowSpace(Vector2 position)
        {
            position.y *= -1; //Invert the y axis
            return position * _pixelsPerUnit + _center;
        }

        private Vector2 WindowSpaceToPointSpace(Vector2 position)
        {
            position = (position - _center) / _pixelsPerUnit;
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
            _showSelected = false;
            Debug.Log("Add Point");
        }

        /// <summary>
        /// Region Moves the selected point
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
            Vector2 pointPos = PointSpaceToWindowSpace( _target.shape.GetPointPosition(_selectedId));

            EditorGUI.BeginChangeCheck();
            Handles.Slider2D(pointPos, Vector3.forward,
                Vector2.up, Vector2.right, 8.0f, Handles.DotHandleCap, Vector2.one * 100.0f); //Point drag handle

            Event e = Event.current;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_target, "Move Shape Point");
                EditorUtility.SetDirty(_target);

                Vector2 newPos = e.mousePosition;

                //Snap on ctrl old
                if (e.control == true)
                {
                    newPos = SnapToGrid(RemapToFirstPowerOfTen(_pixelsPerUnit), newPos);
                }

                newPos = WindowSpaceToPointSpace(newPos);
                _target.shape.SetPointPosition(_selectedId, newPos);
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
            if (!_showSelected) return;

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
                    Undo.RecordObject(_target, "Delete Point");
                    EditorUtility.SetDirty(_target);
                    Debug.Log("Point" + _selectedId.ToString() + "Deleted");
                    _target.shape.RemovePoint(selectedId);
                }

            }
        }

        #endregion

        #region Shape Utility

        private void DrawShapeUtilityPannel()
        {
            GUILayout.Space(BOX_AREA_OFFSET/2);

            _target.shape.closeShape =
                GUILayout.Toggle(_target.shape.closeShape, "Close Shape", "Button",
                GUILayout.Width(85), GUILayout.Height(UTILITY_BUTTON_HEIGHT));
        }

        #endregion

        #region Grid

        private void DrawGrid(float spacing, float opacity, bool displayUnits)
        {
            spacing *= BASE_GRID_RESOLUTION;

            int widthDivs = Mathf.CeilToInt(position.width / spacing);
            int heightDivs = Mathf.CeilToInt(position.height / spacing);

            Handles.BeginGUI();

            Color gridColor = Color.black;
            gridColor.a = opacity;

            Handles.color = gridColor;

            float xOffset = _center.x % spacing;
            float yOffset = _center.y % spacing;

            for(float h = yOffset; h < position.height; h += spacing)
            {
                Handles.DrawLine( new Vector2(0, h), new Vector2(position.width, h));
            }

            for (float v = xOffset; v < position.width; v += spacing)
            {
                Handles.DrawLine(new Vector2(v, 0), new Vector2(v, position.height));
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// Returns the snaped position of the input one
        /// </summary>
        /// <param name="spacing"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Vector2 SnapToGrid(float spacing, Vector2 pos)
        {
            spacing *= BASE_GRID_RESOLUTION;
            Vector2 snappedPos = new Vector2();

            snappedPos.x = Mathf.Floor(pos.x / spacing) * spacing;
            snappedPos.y = Mathf.Floor(pos.y / spacing) * spacing;

            snappedPos.x += _center.x % spacing;
            snappedPos.y += _center.y % spacing;

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

        #endregion
    }
}