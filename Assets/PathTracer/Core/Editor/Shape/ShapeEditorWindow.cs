using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PathTracer.Shapes
{
    public class ShapeEditorWindow : EditorWindow
    {
        #region Attributes

        private static ShapeAsset _target = null;
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

        private const float TOP_BUTTON_HEIGHT = 35.0f;

        #endregion

        #region Init/Disable

        private void OnEnable()
        {
            _isActive = true;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public static void Edit(ShapeAsset asset)
        {
            ShapeEditorWindow current = GetWindow<ShapeEditorWindow>("Shape Editor");
            _target = asset;
            current.Show();
            _center = new Vector2(current.position.width / 2, current.position.height / 2);
            selectedId = -1;
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

            DrawGrid(_pixelsPerUnit / 10, 0.1f, false);
            DrawGrid(_pixelsPerUnit, 0.2f, true);
            WindowNavigation();

            if (_target == null) return;

            int pointsToDisplay = _target.shape.pointCount - (_target.shape.closeShape ? 1 : 0);
            for (int i = 0; i < pointsToDisplay; i++)
            {
                DisplayPoints(i);
            }

            DisplayEdges();

            //Point settings window
            if (selectedId >= 0)
            {
                BeginWindows();
                GUI.Window(0, new Rect(position.width - 200, position.height - 100, 180, 80), DisplayPointSettingsWindow, "Point Settings");
                EndWindows();
            }

            DrawToolPannel();
        }


        #endregion

        #region Window Navigation

        private void WindowNavigation()
        {
            Event e = Event.current;

            if (e.isScrollWheel) //Zoom
            {
                float zoom = Event.current.delta.y;
                _pixelsPerUnit = Mathf.Clamp(_pixelsPerUnit - zoom * 3, 10, 1000);
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

        #region Points GUI

        private void DisplayPoints(int index)
        {
            Handles.BeginGUI();

            Vector2 pointPos = PointSpaceToWindowSpace(_target.shape.GetPointPosition(index));

            if (index == selectedId)
            {
                Handles.color = Color.green;

                EditorGUI.BeginChangeCheck();
                Vector2 newPos = Handles.Slider2D(pointPos, Vector3.forward,
                    Vector2.up, Vector2.right, 8.0f, Handles.DotHandleCap, Vector2.one * 100.0f); //Point drag handle

                //newPos = new Vector2(Mathf.Round())

                Event e = Event.current;

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_target, "Move Shape Point");
                    EditorUtility.SetDirty(_target);

                    newPos = WindowSpaceToPointSpace(newPos);
                    _target.shape.SetPointPosition(index, newPos);
                }
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
        private void DisplayPointSettingsWindow(int id)
        {
            _target.shape.SetPointPosition(selectedId,
                EditorGUILayout.Vector2Field("Point " + selectedId, _target.shape.GetPointPosition(selectedId)));
        }

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

        #region ToolPannel

        /// <summary>
        /// Draws tools pannel
        /// </summary>
        private void DrawToolPannel()
        {
            Event e = Event.current;

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(10);
            if(GUILayout.Button("Draw Points", GUILayout.Width(85), GUILayout.Height(TOP_BUTTON_HEIGHT)))
            {
                AddPoint();
            }

            GUILayout.Space(10);

            _target.shape.closeShape =
                GUILayout.Toggle(_target.shape.closeShape, "Close Shape", "Button",
                GUILayout.Width(85), GUILayout.Height(TOP_BUTTON_HEIGHT));

            //if(GUILayout.Button())

            GUILayout.Space(20);

            if(selectedId >= 0 && _target.shape.pointCount > 2)
            {
                bool delete = false;

                if(e.keyCode == KeyCode.Delete && e.type == EventType.KeyDown)
                {
                    e.Use();
                    delete = true;         
                }

                if (GUILayout.Button("Remove Point", GUILayout.Width(95), GUILayout.Height(TOP_BUTTON_HEIGHT)))
                {
                    delete = true;
                }

                if (delete)
                {
                    Undo.RecordObject(_target, "Delete Point");
                    EditorUtility.SetDirty(_target);
                    RemovePoint();
                }

            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Tools
        /// <summary>
        /// Adds a point to the shape
        /// </summary>
        private void AddPoint()
        {
            Debug.Log("Add Point");
        }

        private void RemovePoint()
        {
            if (_target.shape.pointCount <= 2) return;

            int removeIndex = selectedId;

            Debug.Log("Delete Point : " + removeIndex);

            _target.shape.RemovePoint(removeIndex);
        }


        #endregion

        #region Grid

        private void DrawGrid(float spacing, float opacity, bool displayUnits)
        {
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
        #endregion

        #region Events

        private void OnUndoRedoPerformed()
        {
            Repaint();
        }

        #endregion
    }
}