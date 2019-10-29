using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ShapeToolIdentityAttribute : Attribute
{
	private string _name = string.Empty;
	private string _tooltip = string.Empty;
	private string _iconPath = string.Empty;
	private int _order = 1000;

	public string name => _name;
	public string tooltip => _tooltip;
	public string iconPath => iconPath;
	public int order => _order;

	public ShapeToolIdentityAttribute(string name, int order = 1000)
	{
		_name = name;
		_order = order;
	}

	public ShapeToolIdentityAttribute(string name, string tooltip, int order = 1000)
	{
		_name = name;
		_tooltip = tooltip;
		_order = order;
	}

	public ShapeToolIdentityAttribute(string name, string tooltip, string iconPath, int order = 1000)
	{
		_name = name;
		_tooltip = tooltip;
		_iconPath = iconPath;
		_order = order;
	}
}
