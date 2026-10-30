using Geowar.scripts.data.player;
using Godot;

public partial class DataManager : Node
{
	private ConfigFile config = new();
	string path = "user://坠母.cfg";
	
	public PlayerData playerData = new();

	public static DataManager Instance { get; private set; }

	public override void _Ready()
	{
		if (config.Load(path) != Error.Ok)
			SaveSettings();
		Instance = this;
	}

	public int GetUpgradeLevel(string id) => (int)config.GetValue("upgrades", id, 0);

	public void SetUpgradeLevel(string id, int level)
	{
		if (HasSameValue("upgrades", id, level)) return;
		config.SetValue("upgrades", id, level);
		SaveSettings();
	}

	public int GetIntConfig(string id) => (int)config.GetValue("settings", id, 0);

	public void SetIntConfig(string id, int level)
	{
		if (HasSameValue("settings", id, level)) return;
		config.SetValue("settings", id, level);
		SaveSettings();
	}

	public bool GetBoolConfig(string id) => (bool)config.GetValue("settings", id, false);

	public void SetBoolConfig(string id, bool on)
	{
		if (HasSameValue("settings", id, on)) return;
		config.SetValue("settings", id, on);
		SaveSettings();
	}

	public float GetFloatConfig(string id) => (float)config.GetValue("settings", id, 0);

	public void SetFloatConfig(string id, float f)
	{
		if (HasSameValue("settings", id, f)) return;
		config.SetValue("settings", id, f);
		SaveSettings();
	}
	
	public string GetStringConfig(string id) => (string)config.GetValue("settings", id, "");

	public void SetStringConfig(string id, string s)
	{
		if (HasSameValue("settings", id, s)) return;
		config.SetValue("settings", id, s);
		SaveSettings();
	}

	/// <summary> 值未发生变化时不写盘, 避免初始化/重复赋值触发无意义的保存 </summary>
	private bool HasSameValue(string section, string id, Variant value)
	{
		if (!config.HasSectionKey(section, id)) return false;
		Variant old = config.GetValue(section, id);
		if (old.VariantType != value.VariantType) return false;
		return value.VariantType switch
		{
			Variant.Type.Int => old.AsInt64() == value.AsInt64(),
			Variant.Type.Float => Mathf.IsEqualApprox((float)old.AsDouble(), (float)value.AsDouble()),
			Variant.Type.Bool => old.AsBool() == value.AsBool(),
			Variant.Type.String => old.AsString() == value.AsString(),
			_ => false
		};
	}

	public bool Spend(FallItem.FallItemType type, int amount)
	{
		if (amount <= 0) return false;
		switch (type)
		{
			case FallItem.FallItemType.GOLD:
				if (playerData.GoldCurrency < amount) return false;
				playerData.SpendGold((uint)amount);
				break;
			case FallItem.FallItemType.DIAMOND:
				if (playerData.DiamondCurrency < amount) return false;
				playerData.SpendDiamond((uint)amount);
				break;
			case FallItem.FallItemType.EMERALD:
				if (playerData.EmeraldCurrency < amount) return false;
				playerData.SpendEmerald((uint)amount);
				break;
			default: return false;
		}
		playerData.SavePlayerData();
		return true;
	}

	public void Add(FallItem.FallItemType type, int amount = 1)
	{
		if (amount <= 0) return;
		switch (type)
		{
			case FallItem.FallItemType.GOLD: playerData.AddGold((uint)amount); break;
			case FallItem.FallItemType.DIAMOND: playerData.AddDiamond((uint)amount); break;
			case FallItem.FallItemType.EMERALD: playerData.AddEmerald((uint)amount); break;
		}
		playerData.SavePlayerData();
	}

	public void ClearAll()
	{
		config.Clear();
		playerData.ClearCurrency();
		TayTool.Log("数据","游戏数据已清空");
		SaveSettings();
	}

	private void SaveSettings()
	{
		config.Save(path);
		TayTool.Log("设置","游戏设置已保存");
	}

	public bool IsExistsSetting(string id, string sec = "settings")
	{
		return config.HasSectionKey(sec, id);
	}
}
