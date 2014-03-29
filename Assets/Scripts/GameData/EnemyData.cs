using UnityEngine;
using System.Collections;

public class EnemyData : ScriptableObject {
	// Can specify explicit column name to look for in Excel sheet
	[ExcelColumn("Name")]
	[SerializeField]
	public string shortName;

	// Can set up default values using named parameters
	[ExcelColumn(defaultVal = 10)]
	[SerializeField]
	public int hp;

	[ExcelColumn]
	[SerializeField]
	public int strength;

	// TODO Multicolumn stuff
	[ExcelColumn]
	[SerializeField]
	public float spawnRate;

	//[ExcelColumn("Background", new int[]{255,255,255}, cols = 3)]

	public override string ToString()
	{
		return string.Format("{0}: {1} HP, STR +{2}, spawns {3}% of the time",
			shortName,
			hp,
			strength,
			spawnRate * 100f);
	}
}