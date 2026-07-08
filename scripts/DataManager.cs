using Godot;
using System;

public partial class DataManager : Node
{
	public ConfigFile config = new ConfigFile();
	string path = "user://坠母.cfg";

	public int Gold    { get => (int)config.GetValue("counts", "gold",    0); set => config.SetValue("counts", "gold",    value); }
	public int Diamond { get => (int)config.GetValue("counts", "diamond", 0); set => config.SetValue("counts", "diamond", value); }
	public int Emerald { get => (int)config.GetValue("counts", "emerald", 0); set => config.SetValue("counts", "emerald", value); }

	public override void _Ready()
	{
		if (config.Load(path) != Error.Ok)
			Save();
	}

	public int GetUpgradeLevel(string id) => (int)config.GetValue("upgrades", id, 0);
	public void SetUpgradeLevel(string id, int level) { config.SetValue("upgrades", id, level); Save(); }

	public bool Spend(FallItem.FallItemType type, int amount)
	{
		switch (type)
		{
			case FallItem.FallItemType.GOLD:    if (Gold    < amount) return false; Gold    -= amount; break;
			case FallItem.FallItemType.DIAMOND: if (Diamond < amount) return false; Diamond -= amount; break;
			case FallItem.FallItemType.EMERALD: if (Emerald < amount) return false; Emerald -= amount; break;
			default: return false;
		}
		Save();
		return true;
	}

	public void Add(FallItem.FallItemType type, int amount = 1)
	{
		switch (type)
		{
			case FallItem.FallItemType.GOLD:    Gold    += amount; break;
			case FallItem.FallItemType.DIAMOND: Diamond += amount; break;
			case FallItem.FallItemType.EMERALD: Emerald += amount; break;
		}
		Save();
	}

	public void ClearData()
	{
		config.Clear();
		Save();
	}

	public void Save() => config.Save(path);
}
