using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ShapeTracer.Shapes.Tools
{
    public abstract class ShapeEditorTool
    {
        #region Attributes

        protected string _name = "Tool";
        protected GUIContent _content = new GUIContent();
        public GUIContent content { get { return _content; } }

        #endregion

        #region Methods
        public virtual void Init(ShapeEditorWindow editor) { }
        public abstract void Process(ShapeEditorWindow editor);

        #endregion
    }
}
