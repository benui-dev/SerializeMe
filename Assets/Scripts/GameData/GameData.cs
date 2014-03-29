using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameData : ScriptableObject
{
	[ExcelSheet("Enemies")]
	[SerializeField]
	public List<EnemyData> m_enemies;

	[ExcelSheet("Items")]
	[SerializeField]
	public List<ItemData> m_items;
}