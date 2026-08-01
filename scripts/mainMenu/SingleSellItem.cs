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

	public override void _Ready()
	{
		_nameLabel  = GetNode<Label>("VBoxContainer/Label");
		_descLabel  = GetNode<Label>("VBoxContainer/descLabel");
		_priceLabel = GetNode<Label>("VBoxContainer/priceLabel");
	}

	public void Setup(ShopItem item, int currentLevel,
		System.Action<SingleSellItem> onBuy, System.Action<SingleSellItem> onSell)
	{
		Item = item;
		CurrentLevel = currentLevel;
		_onBuy = onBuy;
		_onSell = onSell;
		Refresh();
	}

	public void Refresh()
	{
		bool maxed = CurrentLevel >= Item.MaxLevel;
		_nameLabel.Text  = $"{Item.Name}\n Lv.{CurrentLevel}";
		_descLabel.Text  = Item.Desc;
		_priceLabel.Text = maxed ? "MAX" : $"价格:{Item.Price}";
		GetNode<Button>("HBoxContainer/Button").Disabled = maxed;
		GetNode<Button>("HBoxContainer/Button2").Disabled = CurrentLevel == 0;
	}

	private void OnPressed() => _onBuy?.Invoke(this);

	private void OnSellButtonPressed() => _onSell?.Invoke(this);
}
