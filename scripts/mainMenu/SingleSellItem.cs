using Godot;

public partial class SingleSellItem : Control
{
	public ShopItem Item { get; private set; }
	public int CurrentLevel { get; set; }

	private Label _nameLabel;
	private Label _descLabel;
	private Label _priceLabel;

	private System.Action<SingleSellItem> _onBuy;
	private System.Action<SingleSellItem> _onSell;

	public bool rare;

	public override void _Ready()
	{
		_nameLabel  = GetNode<Label>("VBoxContainer/Label");
		_descLabel  = GetNode<Label>("VBoxContainer/descLabel");
		_priceLabel = GetNode<Label>("VBoxContainer/priceLabel");
	}

	public void Setup(ShopItem item, int currentLevel,
		System.Action<SingleSellItem> onBuy, System.Action<SingleSellItem> onSell)
	{
		rare = item.isSpecial;
		Item = item;
		CurrentLevel = currentLevel;
		_onBuy = onBuy;
		_onSell = onSell;
		Refresh();
	}

	public void Refresh()
	{
		bool maxed = CurrentLevel >= Item.MaxLevel;
		Polygon2D d = GetNode<Polygon2D>("RoundedPolygon2D");
		if(rare) d.Color = new Color(Colors.Gold,(float)86/256);
		else d.Color = new Color(Colors.White,(float)86/256);
		_nameLabel.Text  = $"{Item.Name}\n Lv.{CurrentLevel}";
		_descLabel.Text  = Item.Desc;
		_priceLabel.Text = maxed ? "MAX" : $"价格:{Item.Price}";
		GetNode<Button>("HBoxContainer/Button").Disabled = maxed;
		GetNode<Button>("HBoxContainer/Button2").Disabled = CurrentLevel == 0;
	}

	private void OnPressed() => _onBuy?.Invoke(this);

	private void OnSellButtonPressed() => _onSell?.Invoke(this);
}
