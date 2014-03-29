using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Reflection;


public class ExcelParser : MonoBehaviour
{
	// Manual reimporting
	[MenuItem("Excel Importer/Import XLS")]
	static void Import()
	{
		CreateScriptableObjectFromExcelFile<GameData>(
			Application.dataPath + "/ExcelData/GameData.xls",
			"Assets/Resources/GameData.asset"
		);
	}


	public static void CreateScriptableObjectFromExcelFile<T>(string excelPath, string assetPath) where T : ScriptableObject
	{
		// Dummy scriptable object
		T obj = ScriptableObject.CreateInstance<T>();
		obj.name = typeof(T).ToString();

		AssetDatabase.CreateAsset(obj, assetPath);

		Debug.Log("Parsing " + excelPath + " into " + assetPath);

		// Using reflection find all the fields in our main container class
		// These will be our sheets
		FieldInfo[] sheetFields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		if (sheetFields.Length == 0) Debug.LogError("Could not find any fields in class " + typeof(T).ToString());

		foreach (var sheetFieldInfo in sheetFields)
		{
			Debug.Log("Sheet name: "+sheetFieldInfo.ToString());

			object[] attrs = sheetFieldInfo.GetCustomAttributes(true);
			int found = 0;
			if (attrs.Length == 0) Debug.LogError("Could not find any attributes in class " + typeof(T).ToString());
			foreach (var att in attrs)
			{
				// Depending on the attributes, we might have to use reflection to work out the column names
				// We use reflection to work out the required datatype
				ExcelSheetAttribute e = att as ExcelSheetAttribute;
				if (e != null)
				{
					found += 1;
					Debug.Log(sheetFieldInfo.ToString());
					Debug.Log(sheetFieldInfo.GetType().ToString());

					// Get the type of the list
					Debug.Log(sheetFieldInfo.FieldType.GetGenericArguments().Length);
					Type listType = sheetFieldInfo.FieldType.GetGenericArguments()[0];

					Debug.Log(listType.ToString());

					// Use reflection to call the correct generic method
					MethodInfo method = typeof(ExcelParser).GetMethod("CreateListFromSheet");
					MethodInfo generic = method.MakeGenericMethod(listType);
					sheetFieldInfo.SetValue(obj, generic.Invoke(null, new object[]{ obj, excelPath, e.sheetName }));
				}
			}
			if (found == 0)
				Debug.LogError("Could not find any ExcelSheet attributes in class " + typeof(T).ToString());
		}

		AssetDatabase.SaveAssets();
	}


	public static List<T> CreateListFromSheet<T>(ScriptableObject parentObj, string filename, string sheetname) where T : ScriptableObject
	{
		Debug.Log(string.Format(
			"Creating a list of type {0} from file {1} and sheet {2}", typeof(T).ToString(), filename, sheetname));

		List<ColumnSpec> specs = new List<ColumnSpec>();

		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		if (fields.Length == 0)
			Debug.LogError("Could not find any fields in class " + typeof(T).ToString());
		foreach (var fieldInfo in fields)
		{
			Debug.Log(fieldInfo.ToString());
			//System.Attribute[] attrs = propInfo.GetCustomAttributes(true);
			object[] attrs = fieldInfo.GetCustomAttributes(true);
			if (attrs.Length == 0)
				Debug.LogError("Could not find any attributes in class " + typeof(T).ToString());
			int found = 0;
			foreach (var att in attrs)
			{
				ExcelColumnAttribute e = att as ExcelColumnAttribute;
				if (e != null)
				{
					found += 1;
					Debug.Log(att.ToString());
					ColumnSpec spec = new ColumnSpec(
						fieldInfo.FieldType,
						e.startCol,
						e.endCol,
						fieldInfo.Name,
						e.name != null ? e.name : fieldInfo.Name
					);
					specs.Add(spec);
				}
			}
			if (found == 0)
				Debug.LogError("Could not find any ExcelColumn attributes in class " + typeof(T).ToString());

		}

		// Now we have a column spec, we can go through the excel file and extract a bunch of objects

		DataTable dt = GetDataTableFromExcel(filename, sheetname);


		// Check that header has all of our expected fields
		foreach (ColumnSpec spec in specs) {
			DataColumn found = null;
			string colName = FormatColumnName(spec.columnName);
			foreach (DataColumn col in dt.Columns)
			{
				if (FormatColumnName(col.ColumnName) == colName)
				{
					found = col;
					break;
				}
			}

			if (found == null)
			{
				Debug.LogError(string.Format(
					"Spec error: File {0}, Sheet {1}: Couldn't find column named {2} (munged to {3})",
					filename, sheetname, spec.columnName, colName));
			}
			else
			{
				spec.startCol = found.Ordinal;
				spec.endCol   = found.Ordinal;
			}
		}



		// Create objects!
		var dataObjects = new List<T>();
		for (int r = 0; r < dt.Rows.Count; r++)
		{
			// All of the objects within our ScriptableObject must be ScriptableObjects themselves
			// because of Unity's serialization and inheritance bug
			T rowObj = (T) ScriptableObject.CreateInstance(typeof(T).Name);
			foreach (ColumnSpec spec in specs)
			{
				for (int c = spec.startCol; c <= spec.endCol; c++)
				{
					Debug.Log(dt.Rows[r][c]);
					Debug.Log(dt.Rows[r][c].GetType().ToString());
					if (spec.isReq)
					{
						if (string.IsNullOrEmpty(dt.Rows[r][c].ToString()))
						{
							Debug.LogError(string.Format(
								"Spec error: File {0}, Sheet {1}, Row {2}, Column {3} (#{4}): Cannot be empty.",
								filename, sheetname, spec.startCol, r, spec.fieldName, c + 1));
						}
					}

					FieldInfo rowObjFieldInfo = typeof(T).GetField(spec.fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

					if (rowObj == null) Debug.Log("RowObj null");
					if (rowObjFieldInfo == null) Debug.Log("rowObjFieldInfo null for "+spec.fieldName + " for type "+typeof(T).ToString());

					// Automagically try to cast to the target datatype
					rowObjFieldInfo.SetValue(rowObj, Convert.ChangeType(dt.Rows[r][c], rowObjFieldInfo.FieldType));
				}
			}
			dataObjects.Add(rowObj);

			// To save this child ScriptableObject asset, we have to add it to the parent one
			AssetDatabase.AddObjectToAsset(rowObj, parentObj);

			// Hide these child assets so the container is the only selectable one
			rowObj.hideFlags = HideFlags.HideAndDontSave;
		}


		if (dataObjects.Count == 0)
		{
			Debug.LogError("Didn't load anything");
		}

		return dataObjects;
	}


	// Convert an excel sheet into a datatable
	// Multiple calls to this will repeatedly open connections but optimise later
	static DataTable GetDataTableFromExcel(string filename, string sheet)
	{
		// Must be saved as excel 2003 workbook, not 2007, mono issue really
		OdbcConnection conn = new OdbcConnection("Driver={Microsoft Excel Driver (*.xls)}; DriverId=790; Dbq=" + filename + ";");
		OdbcCommand cmd = new OdbcCommand(string.Format("SELECT * FROM [{0}$]", sheet), conn);
		DataTable dt = new DataTable(sheet);
		conn.Open();
		OdbcDataReader data = cmd.ExecuteReader();
		dt.Load(data);
		data.Close();
		conn.Close();
		return dt;
	}


	// Munge column and field names so they match more easily
	static string FormatColumnName(string colName)
	{
		return colName.Replace(" ", "").ToLower();
	}


	// For debugging, convert number to Excel's A-Z, AA-AZ style
	static string NumToColumnName(int colNum)
	{
		int dividend = colNum + 1;
		string columnName = String.Empty;
		int modulo;

		while (dividend > 0)
		{
			modulo = (dividend - 1) % 26;
			columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
			dividend = (int)((dividend - modulo) / 26);
		}

		return columnName;
	}
}



// Attribute fields are converted into temporary column specs
public class ColumnSpec
{
	public int startCol;
	public int endCol;
	public string fieldName;
	public string columnName;
	public Type dataType;
	public object defaultVal;
	public bool isReq;

	public ColumnSpec(Type dt, int start, int end, string fieldName, string columnName) {
		this.dataType = dt;
		this.startCol = start;
		this.endCol = end;
		this.isReq = true;
		this.fieldName = fieldName;
		this.columnName = columnName;
	}
}



