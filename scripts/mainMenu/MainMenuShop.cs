using Godot;

public partial class MainMenuShop : Node2D
{
	public enum ChoseItem { GOLD, DIAMOND, EMERALD, NONE }
	public ChoseItem currentChoseItem;

	private Line2D line2D;
	private AnimationPlayer animationPlayer;
	private DataManager dataManager;
	private GlobalAudioPlayer globalAudioPlayer;

	private Label _goldLabel;
	private Label _diamondLabel;
	private Label _emeraldLabel;

	private bool _isInShop;

	public override void _Ready()
	{
		dataManager = GetNode<DataManager>("/root/DataManager");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
		line2D = GetNode<Line2D>("Line2D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		_goldLabel    = GetNode<Label>("HBoxContainer/Label");
		_diamondLabel = GetNode<Label>("HBoxContainer/Label2");
		_emeraldLabel = GetNode<Label>("HBoxContainer/Label3");

		RefreshDisplay();
	}

	private void RefreshDisplay()
	{
		_goldLabel.Text    = dataManager.Gold.ToString();
		_diamondLabel.Text = dataManager.Diamond.ToString();
		_emeraldLabel.Text = dataManager.Emerald.ToString();
	}

	public override void _Process(double delta)
	{
		UpdateLineRotation();
	}

	private void UpdateLineRotation()
	{
		line2D.RotationDegrees = currentChoseItem switch
		{
			ChoseItem.GOLD    => 7,
			ChoseItem.EMERALD => -7,
			_                 => 0,
		};
	}

	private void OnChangeButtonPressed()
	{
		if (animationPlayer.IsPlaying()) return;

		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		if (_isInShop)
			animationPlayer.Play("exit_shop");
		else
		{
			RefreshDisplay();
			animationPlayer.Play("enter_shop");
		}
		_isInShop = !_isInShop;
	}

	private void OnGoldButtonPressed()    => currentChoseItem = ChoseItem.GOLD;
	private void OnDiamondButtonPressed() => currentChoseItem = ChoseItem.DIAMOND;
	private void OnEmeraldButtonPressed() => currentChoseItem = ChoseItem.EMERALD;
}
