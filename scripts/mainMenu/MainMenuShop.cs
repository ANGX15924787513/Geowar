using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using static Player;

public partial class MainMenuShop : Control
{
	public enum ItemEnum { GOLD, DIAMOND, EMERALD, NONE }
	public ItemEnum currentItemEnum;

	private Line2D line2D;
	private AnimationPlayer animationPlayer;
	private DataManager dataManager;
	private GlobalAudioPlayer globalAudioPlayer;

	private Label _goldLabel, _diamondLabel, _emeraldLabel;

	private bool _isInShop;

	private List<SingleSellItem> _sellItems = new();
	private HFlowContainer _grid;
	private Dictionary<ItemEnum, List<ShopItem>> _catalog = new();

	[Export] private int maxMoneyChar = 9999;
	
	[Export] private PackedScene singleSellItemScene;
	
	// ==================== 商品目录 ====================

	private void BuildCatalog()
	{
		_catalog[ItemEnum.GOLD] = new List<ShopItem>
		{
			new() { Id = "point_count", Name = "骨质增生", Desc = "球更多了  ?\n点数 +1",           Price = 20,  MaxLevel = 7 ,addTo = PlayerTypeEnum.POINT},
			new() { Id = "line_count", Name = "延展", Desc = "线更多了  \n线数 +1",           Price = 30,  MaxLevel = 10 ,addTo =  PlayerTypeEnum.LINE},
			new() { Id = "fire_rate",   Name = "碎裂加速", Desc = "射速 +1",     Price = 5,  MaxLevel = 5},
			new() { Id = "move_speed",  Name = "漂移",     Desc = "移动速度 +20\n随便漂",    Price = 3,  MaxLevel = 10 },
		};

		_catalog[ItemEnum.DIAMOND] = new List<ShopItem>
		{
			new() { Id = "atk_hp",      Name = "骨刺淬毒", Desc = "子弹伤害 +1",                Price = 5,  MaxLevel = 10 },
			new() { Id = "line",      Name = "线", Desc = "可以使用线了(你变成刺了)",                Price = 20,  MaxLevel = 1 ,isSpecial = true},
			new() { Id = "edge_atk",      Name = "磨刀石", Desc = "点可以磨死敌人了",                Price = 10,  MaxLevel = 1 ,addTo = PlayerTypeEnum.POINT},
			new() { Id = "through",      Name = "一箭穿心", Desc = "线穿透 +2",                Price = 3,  MaxLevel = 5 ,addTo = PlayerTypeEnum.LINE},
		};

		_catalog[ItemEnum.EMERALD] = new List<ShopItem>
		{
			new() { Id = "max_hp",      Name = "骨骼硬化", Desc = "点最大生命值 +10\n线最大生命值 +5\n更块(三声)了",        Price = 3,  MaxLevel = 15 },
			new() { Id = "lifesteal",   Name = "噬血骨刺", Desc = "击杀敌人回复 1生命值",           Price = 5, MaxLevel = 2 },
			new() { Id = "camera_scale",   Name = "上帝视角", Desc = "扩大视野 10%",           Price = 2, MaxLevel = 6 }
		};
	}

	public override void _Ready()
	{
		dataManager = GetNode<DataManager>("/root/DataManager");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
		line2D = GetNode<Line2D>("Line2D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_grid = GetNode<HFlowContainer>("HFlowContainer");

		_goldLabel    = GetNode<Label>("Q币显示/Label");
		_diamondLabel = GetNode<Label>("Q币显示/Label2");
		_emeraldLabel = GetNode<Label>("Q币显示/Label3");
		
		BuildCatalog();
		ClearGrid();
		SanitizeSaveData();
		RefreshDisplay();
		PopulateGrid();
		UpdateDifficulty();
	}

	private void ClearGrid()
	{
		foreach (var child in _grid.GetChildren())
			if (child is SingleSellItem si)
				_grid.RemoveChild(si);
		
		var maxC = _catalog.Select(_v => _v.Value.Count).Prepend(0).Max();

		for (var i = 0; i < maxC; i++)
		{
			_grid.AddChild(singleSellItemScene.Instantiate<SingleSellItem>());
		}

		foreach (var child in _grid.GetChildren())
			if (child is SingleSellItem si)
				_sellItems.Add(si);
	}

	public float UpdateDifficulty()
	{
		int total = 0;
		foreach (var a in _catalog)
		{
			foreach (var b in a.Value)
			{
				total += b.MaxLevel;
			}
		}

		int current = 0;
		foreach (var a in _catalog)
		{
			foreach (var b in a.Value)
			{
				current += dataManager.GetUpgradeLevel(b.Id);
			}
		}

		if (total == 0)
		{
			GD.PrintErr("难度计算错误:总升级为0");
			return 0;
		}
		GameManager.Instance.difficulty = (float)current / total;
		GD.Print($"当前难度为:{(int)(GameManager.Instance.difficulty * 100 - GameManager.Instance.difficulty * 10000 % 100 * 0.01)}%");
		return GameManager.Instance.difficulty;
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
		var items = _catalog.GetValueOrDefault(currentItemEnum);
		if (items == null) { HideAll(); return; }

		for (int i = 0; i < _sellItems.Count; i++)
		{
			if (i < items.Count)
			{
				_sellItems[i].Visible = true;
				int level = dataManager.GetUpgradeLevel(items[i].Id);
				_sellItems[i].Setup(items[i], level, OnBuyItem, OnSellItem);
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

		FallItem.FallItemType currency = currentItemEnum switch
		{
			ItemEnum.DIAMOND => FallItem.FallItemType.DIAMOND,
			ItemEnum.EMERALD => FallItem.FallItemType.EMERALD,
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
		UpdateDifficulty();
	}

	// ==================== 降级 / 卖出 ====================

	private void OnSellItem(SingleSellItem si)
	{
		if (si.CurrentLevel <= 0) return;

		// 退还一半价格
		FallItem.FallItemType currency = currentItemEnum switch
		{
			ItemEnum.DIAMOND => FallItem.FallItemType.DIAMOND,
			ItemEnum.EMERALD => FallItem.FallItemType.EMERALD,
			_                 => FallItem.FallItemType.GOLD,
		};
		int refund = Mathf.Max(1, si.Item.Price / 2);

		int newLevel = si.CurrentLevel - 1;
		dataManager.SetUpgradeLevel(si.Item.Id, newLevel);
		dataManager.Add(currency, refund);
		ApplyUpgrade(si.Item.Id, newLevel);

		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		si.CurrentLevel = newLevel;
		si.Refresh();
		RefreshDisplay();
		UpdateDifficulty();
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
		_goldLabel.Text    = dataManager.Gold > maxMoneyChar ? $"{maxMoneyChar}+" : dataManager.Gold.ToString();
		_diamondLabel.Text = dataManager.Diamond > maxMoneyChar ? $"{maxMoneyChar}+" : dataManager.Diamond.ToString();
		_emeraldLabel.Text = dataManager.Emerald >  maxMoneyChar ? $"{maxMoneyChar}+" : dataManager.Emerald.ToString();
	}

	public override void _Process(double delta) => UpdateLineRotation();

	private void UpdateLineRotation()
	{
		float target = currentItemEnum switch
		{
			ItemEnum.GOLD    => 7f,
			ItemEnum.EMERALD => -7f,
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
			((CanvasItem)GetNode("../setting")).Visible = true;
		}
		else
		{
			RefreshDisplay();
			animationPlayer.Play("enter_shop");
			((CanvasItem)GetNode("../setting")).Visible = false;
		}
		_isInShop = !_isInShop;
	}

	private void OnGoldButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		currentItemEnum = ItemEnum.GOLD;    
		PopulateGrid();
	}

	private void OnDiamondButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		currentItemEnum = ItemEnum.DIAMOND; 
		PopulateGrid();
	}

	private void OnEmeraldButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		currentItemEnum = ItemEnum.EMERALD; 
		PopulateGrid();
	}
}
