using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace ShapeTracer.Shapes.Tools
{
    public abstract class ShapeEditorTool
    {
		#region Data
		public GUIContent content = null;
		#endregion


		#region Methods
		public virtual void Init(ShapeEditorWindow editor) { }
        public abstract void Process(ShapeEditorWindow editor);
        #endregion
    }
}
