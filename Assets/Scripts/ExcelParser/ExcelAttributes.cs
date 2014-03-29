using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class ExcelSheetAttribute : Attribute
{
	public string sheetName;
	public ExcelSheetAttribute(string _sheetName)
	{
		sheetName = _sheetName;
	}
}

[AttributeUsage(AttributeTargets.Field)]
public class ExcelColumnAttribute : Attribute
{
	public int startCol;
	public int endCol;
	public int cols; // Column width
	public string name = null;
	public object defaultVal = null;
	// Assuming default val will never be null...
	public bool IsRequired { get { return defaultVal != null; } }

	public ExcelColumnAttribute() { }

	public ExcelColumnAttribute(string colName)
	{
		this.name = colName;
	}
}