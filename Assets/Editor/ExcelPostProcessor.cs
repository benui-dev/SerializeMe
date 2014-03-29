using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class ExcelPostProcessor : AssetPostprocessor
{
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (var str in importedAssets)
		{
			if (str.EndsWith(".xls"))
			{
				ExcelParser.CreateScriptableObjectFromExcelFile<GameData>(
					 str
					,Application.dataPath + "/Resources/" + Path.GetFileName(str)
				);
			}
		}

		foreach (var str in deletedAssets)
		{
			Debug.Log("Deleted Asset: " + str);
			// TODO Delete correct ScriptableObject
		}

	}
}
