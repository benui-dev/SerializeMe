using UnityEngine;
using System.Collections;

public class DataTester : MonoBehaviour {

	GameData m_gameData;
	void Awake()
	{
		GameData prefab = Resources.Load<GameData>("GameData");
		m_gameData = (GameData) ScriptableObject.Instantiate(prefab);
	}

	void OnGUI() {
		GUI.Label(new Rect(20, 20, 300, 50), "Enemies");
		for (int i = 0; i < m_gameData.m_enemies.Count; i++)
		{
			EnemyData enemy = m_gameData.m_enemies[i];
			GUI.Label(new Rect(40, 60 + i * 60, 300, 50), enemy.ToString());
		}

		GUI.Label(new Rect(420, 20, 300, 50), "Items");
		for (int i = 0; i < m_gameData.m_items.Count; i++)
		{
			ItemData item = m_gameData.m_items[i];
			GUI.Label(new Rect(440, 60 + i * 60, 300, 50), item.ToString());
		}
	}
}
