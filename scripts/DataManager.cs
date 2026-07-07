using Godot;
using System;

public partial class DataManager : Node
{
	public ConfigFile config = new ConfigFile();

	public int Iron    { get => (int)config.GetValue("counts", "iron",    0); set => config.SetValue("counts", "iron",    value); }
	public int Gold    { get => (int)config.GetValue("counts", "gold",    0); set => config.SetValue("counts", "gold",    value); }
	public int Diamond { get => (int)config.GetValue("counts", "diamond", 0); set => config.SetValue("counts", "diamond", value); }
	public int Emerald { get => (int)config.GetValue("counts", "emerald", 0); set => config.SetValue("counts", "emerald", value); }

	public override void _Ready()
	{
		if (config.Load("user://gamedata.cfg") != Error.Ok)
			Save();
	}

	public void Add(FallItem.FallItemType type, int amount = 1)
	{
		switch (type)
		{
			case FallItem.FallItemType.IRON:    Iron    += amount; break;
			case FallItem.FallItemType.GOLD:    Gold    += amount; break;
			case FallItem.FallItemType.DIAMOND: Diamond += amount; break;
			case FallItem.FallItemType.EMERALD: Emerald += amount; break;
		}
		Save();
	}

	public void Save() => config.Save("user://gamedata.cfg");
}
