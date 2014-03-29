using UnityEngine;
using System.Collections;

public class ItemData : ScriptableObject
{
	// Using a different name to not clash with ScriptableObject.name
	[ExcelColumn("Name")]
	[SerializeField]
	public string itemName;

	// Can set up default values using named parameters
	[ExcelColumn(defaultVal = 1)]
	[SerializeField]
	public int cost;

	[ExcelColumn]
	[SerializeField]
	public int maxStack;

	// Can still use properties
	public bool CanStack { get { return maxStack > 0; } }

	[ExcelColumn]
	[SerializeField]
	public float dropRate;

	public override string ToString()
	{
		return string.Format("{0}: [{1} gold] stacks {2} items, drops {3}% of the time",
			itemName,
			cost,
			maxStack,
			dropRate * 100f);
	}
}