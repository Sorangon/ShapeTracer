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

	public string name => _name;
	public string tooltip => _tooltip;
	public string iconPath => iconPath;

	public ShapeToolIdentityAttribute(string name)
	{
		_name = name;
	}

	public ShapeToolIdentityAttribute(string name, string tooltip)
	{
		_name = name;
		_tooltip = tooltip;
	}

	public ShapeToolIdentityAttribute(string name, string tooltip, string iconPath)
	{
		_name = name;
		_tooltip = tooltip;
		_iconPath = iconPath;
	}
}
