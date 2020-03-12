using UnityEngine;
using System;

namespace SorangonToolset.ShapeTracer.Shapes.Tools {
	/// <summary>
	/// Use this attribute to setup a custom name, icon, order and tooltip  to your Shape Editor tools
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ShapeToolAttribute : Attribute {
		private string _name = string.Empty;
		private string _tooltip = string.Empty;
		private string _iconPath = string.Empty;
		private int _order = 1000;
		private KeyCode _shortcut = KeyCode.None;

		public string Name => _name;
		public string Tooltip => _tooltip;
		public string IconPath => _iconPath;
		public int Order => _order;
		public KeyCode Shortcut => _shortcut;

		public ShapeToolAttribute(string name, int order = 1000) {
			_name = name;
			_order = order;
		}

		public ShapeToolAttribute(string name, string tooltip, int order = 1000) {
			_name = name;
			_tooltip = tooltip;
			_order = order;
		}

		public ShapeToolAttribute(string name, string tooltip, string iconPath, int order = 1000) {
			_name = name;
			_tooltip = tooltip;
			_iconPath = iconPath;
			_order = order;
		}

		public ShapeToolAttribute(string name, string tooltip, string iconPath, KeyCode shortcut, int order = 1000) {
			_name = name;
			_tooltip = tooltip;
			_iconPath = iconPath;
			_order = order;
			_shortcut = shortcut;
		}
	}
}
