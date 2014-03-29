# SerializeMe

Create ScriptableObjects from Excel, with minimal markup.


## Usage

```C#
public class GameData : ScriptableObject
{
	// Sheet to convert into ScriptableObjects
	[ExcelSheet("Enemies")]
	[SerializeField]
	public List<EnemyData> m_enemies;
}

public class EnemyData : ScriptableObject
{
	// Fill with data from column with "HP" as header
	[ExcelColumn]
	[SerializeField]
	public int hp;

	// Can specify explicit column name to look for
	[ExcelColumn("Name")]
	[SerializeField]
	public string shortName;
}
```


## Caveats

Currently uses a Windows-only DLL. Could be easily changed to read data from
CSV as well as Excel

You should treat this as a proof-of-concept rather than a finished product.
Take what it starts and go nuts with it.


## Still to Do

* CSV support
* Multiple columns
* An easy way of performing data conversions:
  e.g. convert string columns to boolean, merging three float columns to one
  Vector3


## License

[CC Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/)
