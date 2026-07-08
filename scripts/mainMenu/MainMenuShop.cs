using Godot;
using System.Collections.Generic;

public partial class MainMenuShop : Node2D
{
	public enum ChoseItem { GOLD, DIAMOND, EMERALD, NONE }
	public ChoseItem currentChoseItem;

	private Line2D line2D;
	private AnimationPlayer animationPlayer;
	private DataManager dataManager;
	private GlobalAudioPlayer globalAudioPlayer;

	private Label _goldLabel, _diamondLabel, _emeraldLabel;

	private bool _isInShop;

	private List<SingleSellItem> _sellItems = new();
	private HFlowContainer _grid;
	private Dictionary<ChoseItem, List<ShopItem>> _catalog = new();

	public override void _Ready()
	{
		dataManager = GetNode<DataManager>("/root/DataManager");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
		line2D = GetNode<Line2D>("Line2D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_grid = GetNode<HFlowContainer>("HFlowContainer");

		_goldLabel    = GetNode<Label>("HBoxContainer/Label");
		_diamondLabel = GetNode<Label>("HBoxContainer/Label2");
		_emeraldLabel = GetNode<Label>("HBoxContainer/Label3");

		foreach (var child in _grid.GetChildren())
			if (child is SingleSellItem si)
				_sellItems.Add(si);

		BuildCatalog();
		SanitizeSaveData();
		RefreshDisplay();
		PopulateGrid();
	}

	// ==================== 商品目录 (6件) ====================

	private void BuildCatalog()
	{
		_catalog[ChoseItem.GOLD] = new List<ShopItem>
		{
			new() { Id = "point_count", Name = "骨质增生", Desc = "球更多了  ?\n 点数 +1",           Price = 5,  MaxLevel = 12 },
			new() { Id = "fire_rate",   Name = "碎裂加速", Desc = "射速 +1，按住射得更快",     Price = 3,  MaxLevel = 15 },
			new() { Id = "move_speed",  Name = "漂移",     Desc = "移动速度 +50，随便漂",    Price = 4,  MaxLevel = 10 },
		};

		_catalog[ChoseItem.DIAMOND] = new List<ShopItem>
		{
			new() { Id = "atk_hp",      Name = "骨刺淬毒", Desc = "子弹伤害 +1",                Price = 5,  MaxLevel = 10 },
		};

		_catalog[ChoseItem.EMERALD] = new List<ShopItem>
		{
			new() { Id = "max_hp",      Name = "骨骼硬化", Desc = "最大生命值 +30，没那么容易碎",        Price = 3,  MaxLevel = 15 },
			new() { Id = "lifesteal",   Name = "噬血骨刺", Desc = "击杀敌人回复最大生命值 2%",           Price = 10, MaxLevel = 5 },
		};
	}

	/// <summary> 读档时把超出 MaxLevel 的脏数据修正 </summary>
	private void SanitizeSaveData()
	{
		foreach (var (_, items) in _catalog)
			foreach (var item in items)
			{
				int stored = dataManager.GetUpgradeLevel(item.Id);
				if (stored > item.MaxLevel)
					dataManager.SetUpgradeLevel(item.Id, item.MaxLevel);
			}
	}

	// ==================== 货币兑换 ====================

	private void OnExchangeGoldToDiamond(int count)
	{
		if (dataManager.Gold < 10 * count)
		{
			GD.Print("货币不足!");
			return;
		}
		dataManager.Spend(FallItem.FallItemType.GOLD, 10 * count);
		dataManager.Add(FallItem.FallItemType.DIAMOND, 1 * count);
		RefreshDisplay();
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
	}

	private void OnExchangeDiamondToEmerald(int count)
	{
		if (dataManager.Diamond < 10 * count)
		{
			GD.Print("货币不足!");
			return;
		}
		dataManager.Spend(FallItem.FallItemType.DIAMOND, 10 * count);
		dataManager.Add(FallItem.FallItemType.EMERALD, 1 * count);
		RefreshDisplay();
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
	}

	// ==================== 网格填充 ====================

	private void PopulateGrid()
	{
		var items = _catalog.GetValueOrDefault(currentChoseItem);
		if (items == null) { HideAll(); return; }

		for (int i = 0; i < _sellItems.Count; i++)
		{
			if (i < items.Count)
			{
				_sellItems[i].Visible = true;
				int level = dataManager.GetUpgradeLevel(items[i].Id);
				_sellItems[i].Setup(items[i], level, OnBuyItem);
			}
			else
			{
				_sellItems[i].Visible = false;
			}
		}
	}

	private void HideAll() { foreach (var si in _sellItems) si.Visible = false; }

	// ==================== 购买 ====================

	private void OnBuyItem(SingleSellItem si)
	{
		if (si.CurrentLevel >= si.Item.MaxLevel) return;

		FallItem.FallItemType currency = currentChoseItem switch
		{
			ChoseItem.DIAMOND => FallItem.FallItemType.DIAMOND,
			ChoseItem.EMERALD => FallItem.FallItemType.EMERALD,
			_                 => FallItem.FallItemType.GOLD,
		};

		if (!dataManager.Spend(currency, si.Item.Price))
			return;

		int newLevel = dataManager.GetUpgradeLevel(si.Item.Id) + 1;
		dataManager.SetUpgradeLevel(si.Item.Id, newLevel);
		ApplyUpgrade(si.Item.Id, newLevel);

		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		si.CurrentLevel = newLevel;
		si.Refresh();
		RefreshDisplay();
	}

	// ==================== 升级写进 GameManager ====================

	private void ApplyUpgrade(string id, int level)
	{
		switch (id)
		{
			case "point_count": GameManager.Instance.pointCountUpgrade = level; break;
			case "fire_rate":   GameManager.Instance.fireRateUpgrade   = level; break;
			case "move_speed":  GameManager.Instance.moveSpeedUpgrade  = level; break;
			case "atk_hp":      GameManager.Instance.atkHpUpgrade      = level; break;
			case "max_hp":      GameManager.Instance.maxHpUpgrade      = level; break;
			case "lifesteal":   GameManager.Instance.lifestealUpgrade  = level; break;
		}
	}

	// ==================== UI ====================

	private void RefreshDisplay()
	{
		_goldLabel.Text    = dataManager.Gold.ToString();
		_diamondLabel.Text = dataManager.Diamond.ToString();
		_emeraldLabel.Text = dataManager.Emerald.ToString();
	}

	public override void _Process(double delta) => UpdateLineRotation();

	private void UpdateLineRotation()
	{
		float target = currentChoseItem switch
		{
			ChoseItem.GOLD    => 7f,
			ChoseItem.EMERALD => -7f,
			_                 => 0f,
		};
		line2D.RotationDegrees = Mathf.Lerp(line2D.RotationDegrees, target, 8f * (float)GetProcessDeltaTime());
	}

	private void OnChangeButtonPressed()
	{
		if (animationPlayer.IsPlaying()) return;
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		if (_isInShop)
		{
			animationPlayer.Play("exit_shop");
			((Node2D)GetNode("../setting")).Visible = true;
		}
		else
		{
			RefreshDisplay();
			animationPlayer.Play("enter_shop");
			((Node2D)GetNode("../setting")).Visible = false;
		}
		_isInShop = !_isInShop;
	}

	private void OnGoldButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		currentChoseItem = ChoseItem.GOLD;    
		PopulateGrid();
	}

	private void OnDiamondButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		currentChoseItem = ChoseItem.DIAMOND; 
		PopulateGrid();
	}

	private void OnEmeraldButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		currentChoseItem = ChoseItem.EMERALD; 
		PopulateGrid();
	}
}
