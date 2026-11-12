using Godot;
using System.Collections.Generic;
using System.Linq;
using Geowar.scripts.data.player;
using static Player;

public partial class MainMenuShop : Control
{
	public static MainMenuShop Instance;
	public enum ItemEnum { GOLD, DIAMOND, EMERALD, NONE }
	public ItemEnum currentItemEnum;

	private Line2D line2D;
	private AnimationPlayer animationPlayer;

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
			new() { Id = "line_fire",   Name = "熔岩地板", Desc = "这地板有点烫?!\n线的尾迹可以灼烧敌人\n伤害+2",     Price = 50,  MaxLevel = 3,addTo = PlayerTypeEnum.LINE},
		};

		_catalog[ItemEnum.DIAMOND] = new List<ShopItem>
		{
			new() { Id = "atk_hp",      Name = "骨刺淬毒", Desc = "子弹伤害 +1",                Price = 5,  MaxLevel = 10 },
			new() { Id = "line",      Name = "线", Desc = "可以使用线了(你变成刺了)",                Price = 20,  MaxLevel = 1 ,isSpecial = true},
			new() { Id = "edge_atk",      Name = "磨刀石", Desc = "点可以磨死敌人了",                Price = 10,  MaxLevel = 1 ,addTo = PlayerTypeEnum.POINT},
			new() { Id = "through",      Name = "一箭穿心", Desc = "线穿透 +2",                Price = 3,  MaxLevel = 5 ,addTo = PlayerTypeEnum.LINE},
			new() { Id = "skill_tnt",      Name = "核弹支援", Desc = "随机呼叫火力支援! \n每90秒TNT轰炸全场",                Price = 15,  MaxLevel = 1 ,isSpecial = true},
			new() { Id = "skill_collect_all_coins",      Name = "旋风吸入", Desc = "这下爽了 \n每30秒收集所有金币",                Price = 10,  MaxLevel = 1 ,isSpecial = true},
		};

		_catalog[ItemEnum.EMERALD] = new List<ShopItem>
		{
			new() { Id = "max_hp",      Name = "骨骼硬化", Desc = "点最大生命值 +10\n线最大生命值 +5\n更块(三声)了",        Price = 3,  MaxLevel = 15 },
			new() { Id = "lifesteal",   Name = "噬血骨刺", Desc = "击杀敌人回复 1生命值",           Price = 5, MaxLevel = 2 },
			new() { Id = "camera_scale",   Name = "上帝视角", Desc = "扩大视野 10%",           Price = 2, MaxLevel = 6 },
			new() { Id = "skill_invincible",   Name = "超渡众生", Desc = "每60秒获得10秒无敌时间",Price = 1,MaxLevel = 1,isSpecial = true},
			new() { Id = "skill_super_bullet",   Name = "超级弹夹", Desc = "射不完的子弹",Price = 2,MaxLevel = 1,isSpecial = true},
		};
	}

	public override void _EnterTree() => Instance = this;
	public override void _ExitTree() => Instance = null;

	public override void _Ready()
	{
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
				current += DataManager.Instance.GetUpgradeLevel(b.Id);
			}
		}

		if (total == 0)
		{
			TayTool.Log("难度","难度计算错误: 总升级为 0",TayTool.LogTypeEnum.Err);
			return 0;
		}
		GameManager.Instance.difficulty = (float)current / total;
		TayTool.Log("难度",$"当前难度为: {(int)(GameManager.Instance.difficulty * 100 - GameManager.Instance.difficulty * 10000 % 100 * 0.01)}%");
		return GameManager.Instance.difficulty;
	}
	/// <summary> 读档时把超出 MaxLevel 的脏数据修正 </summary>
	private void SanitizeSaveData()
	{
		foreach (var (_, items) in _catalog)
			foreach (var item in items)
			{
				int stored = DataManager.Instance.GetUpgradeLevel(item.Id);
				if (stored > item.MaxLevel)
					DataManager.Instance.SetUpgradeLevel(item.Id, item.MaxLevel);
			}
	}

	// ==================== 货币兑换 ====================

	private void OnExchangeGoldToDiamond(int count)
	{
		if (DataManager.Instance.playerData.GoldCurrency < 10 * count)
		{
			TayTool.Log("商店","货币不足!",TayTool.LogTypeEnum.Err);
			return;
		}
		DataManager.Instance.Spend(FallItem.FallItemType.GOLD, 10 * count);
		DataManager.Instance.Add(FallItem.FallItemType.DIAMOND, 1 * count);
		RefreshDisplay();
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
	}

	private void OnExchangeDiamondToEmerald(int count)
	{
		if (DataManager.Instance.playerData.DiamondCurrency < 10 * count)
		{
			TayTool.Log("商店","货币不足!",TayTool.LogTypeEnum.Err);
			return;
		}
		DataManager.Instance.Spend(FallItem.FallItemType.DIAMOND, 10 * count);
		DataManager.Instance.Add(FallItem.FallItemType.EMERALD, 1 * count);
		RefreshDisplay();
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
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
				int level = DataManager.Instance.GetUpgradeLevel(items[i].Id);
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

		if (!DataManager.Instance.Spend(currency, si.Item.Price))
			return;

		int newLevel = DataManager.Instance.GetUpgradeLevel(si.Item.Id) + 1;
		DataManager.Instance.SetUpgradeLevel(si.Item.Id, newLevel);
		ApplyUpgrade(si.Item.Id, newLevel);

		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
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
		DataManager.Instance.SetUpgradeLevel(si.Item.Id, newLevel);
		DataManager.Instance.Add(currency, refund);
		ApplyUpgrade(si.Item.Id, newLevel);

		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
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

	public void RefreshDisplay()
	{
		PlayerData pdata = DataManager.Instance.playerData;
		_goldLabel.Text    = pdata.GoldCurrency > maxMoneyChar ? $"{maxMoneyChar}+" : pdata.GoldCurrency.ToString();
		_diamondLabel.Text = pdata.DiamondCurrency > maxMoneyChar ? $"{maxMoneyChar}+" : pdata.DiamondCurrency.ToString();
		_emeraldLabel.Text = pdata.EmeraldCurrency >  maxMoneyChar ? $"{maxMoneyChar}+" : pdata.EmeraldCurrency.ToString();
	}

	public override void _Process(double delta) => UpdateLineRotation();

	private void UpdateLineRotation()
	{
		if (!_isInShop) return;
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
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		if (_isInShop)
		{
			animationPlayer.Play("exit_shop");
		}
		else
		{
			RefreshDisplay();
			animationPlayer.Play("enter_shop");
		}
		_isInShop = !_isInShop;
		SettingPanel.Instance.Visible = !_isInShop;
		MainMenuRoot.Instance.SwitchCentreVisible(!_isInShop);
	}

	private void OnGoldButtonPressed()
	{
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		currentItemEnum = ItemEnum.GOLD;    
		PopulateGrid();
	}

	private void OnDiamondButtonPressed()
	{
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		currentItemEnum = ItemEnum.DIAMOND; 
		PopulateGrid();
	}

	private void OnEmeraldButtonPressed()
	{
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		currentItemEnum = ItemEnum.EMERALD; 
		PopulateGrid();
	}
}
