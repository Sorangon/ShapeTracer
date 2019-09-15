using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PathTracer.CrossSectionUtility
{
    public class CrossSectionEditorWindow : EditorWindow
    {
        #region Attributes

        private static CrossSectionAsset _target = null;
        private static bool _isActive = false;
        private static float _pixelsPerUnit = 200.0f;
        private static Vector2 _center = Vector2.zero;
        private static int _selectedId = -1;

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

        #region Methods
        #region GUICycle

        private void OnEnable()
        {
            _isActive = true;
        }

        public static void Edit(CrossSectionAsset asset)
        {
            CrossSectionEditorWindow current = GetWindow<CrossSectionEditorWindow>("Cross Section Editor");
            _target = asset;
            current.Show();
            _center = new Vector2(current.position.width / 2, current.position.height / 2);
            _selectedId = -1;
        }

        private void OnGUI()
        {
            Rect windowRect = new Rect(0, 0, position.width, position.height);
            GUI.DrawTexture(windowRect, _backgroundTexture, ScaleMode.StretchToFill);

            DrawGrid(_pixelsPerUnit/10, 0.1f, false);
            DrawGrid(_pixelsPerUnit, 0.2f, true);

            Event e = Event.current;

            if (_target == null) return;

            if(e.isScrollWheel) //Zoom
            {
                float zoom = Event.current.delta.y;
                _pixelsPerUnit = Mathf.Clamp(_pixelsPerUnit - zoom * 3,10, 1000);
                Repaint();
                e.Use();
            }
            else if(e.button == 2 && e.type == EventType.MouseDrag) //Pan view
            {
                _center += e.delta;
                Repaint();
                e.Use();
            }

            for(int i = 0; i < _target.crossSection.pointCount; i++)
            {
                DisplayPoints(i);
            }

            //Point settings window
            if(_selectedId >= 0)
            {
                BeginWindows();
                GUI.Window(0, new Rect(position.width - 200, position.height - 100, 180, 80), DisplayPointSettingsWindow, "Point Settings");
                EndWindows();
            }

            DrawToolPannel();
        }

        private void OnDisable()
        {
            _isActive = false;
        }


        #endregion
        #region Points GUI

        private void DisplayPoints(int index)
        {
            Handles.BeginGUI();

            Vector2 pointPos = new Vector2(_target.crossSection.points[index].x, -_target.crossSection.points[index].y);
            pointPos = pointPos * _pixelsPerUnit + _center;

            if (index == _selectedId)
            {
                Handles.color = Color.green;

                EditorGUI.BeginChangeCheck();
                Vector2 newPos = Handles.Slider2D(pointPos, Vector3.forward,
                    Vector2.up, Vector2.right, 12.0f, Handles.CircleHandleCap, Vector2.one * 100.0f); //Point drag handle

                //newPos = new Vector2(Mathf.Round())

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_target, "Move Cross Section Point");
                    EditorUtility.SetDirty(_target);

                    newPos.y = newPos.y * -1 + position.height; //Invert the y to match with GUI
                    _target.crossSection.points[index] = (newPos - _center) / _pixelsPerUnit;
                }
            }
            else
            {
                Handles.color = Color.white;
                if (Handles.Button(pointPos, Quaternion.identity, 4f, 6f, Handles.DotHandleCap))
                {
                    _selectedId = index;
                    Repaint();
                }
            }

            Handles.color = Color.white;

            if (index > 0)
            {
                Vector2 previousPoint = new Vector2(_target.crossSection.points[index - 1].x, -_target.crossSection.points[index - 1].y);
                Handles.DrawLine(pointPos, previousPoint * _pixelsPerUnit + _center);
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// Displays the current edited point settings
        /// </summary>
        /// <param name="id"></param>
        private void DisplayPointSettingsWindow(int id)
        {
            _target.crossSection.points[_selectedId] = 
                EditorGUILayout.Vector2Field("Point " + _selectedId, _target.crossSection.points[_selectedId]);
        }

        #endregion
        #region ToolPannel

        /// <summary>
        /// Draws tools pannel
        /// </summary>
        private void DrawToolPannel()
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(10);
            if(GUILayout.Button("Add Point", GUILayout.Width(75), GUILayout.Height(30f)))
            {
                AddPoint();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Remove Point", GUILayout.Width(95), GUILayout.Height(30f)))
            {
                RemovePoint();
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion
        #region Tools
        /// <summary>
        /// Adds a point to the cross section
        /// </summary>
        private void AddPoint()
        {
            Debug.Log("Add Point");
        }

        private void RemovePoint()
        {
            Debug.Log("Remove Point");
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
        #endregion
    }
}