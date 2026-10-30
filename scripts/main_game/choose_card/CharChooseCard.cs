using Godot;

public partial class CharChooseCard : Line2D
{
	private Label nameLabel;
	private Label descriptionLabel;
	private Button Button;
	private int indexInContainer;
	public override void _Ready()
	{
		nameLabel = GetNode<Label>("nameLabel");
		descriptionLabel = GetNode<Label>("descriptionLabel");
		Button = GetNode<Button>("Button");
		indexInContainer = GetParent().GetIndex();
		var json = new Json();
		string jsonString = FileAccess.Open("res://texts/character.json", FileAccess.ModeFlags.Read).GetAsText();
		json.Parse(jsonString);
		// 1. 获取整个字典
		var data = json.Data.AsGodotDictionary();

// 2. 获取 characters 数组
		var characters = data["characters"].AsGodotArray();

// 3. 获取指定索引的角色（是一个字典）
		var character = characters[indexInContainer].AsGodotDictionary();

// 4. 从字典中按键名取值
		var name = character["name"].AsString();
		var description = character["description"].AsString();

// 5. 赋值
		nameLabel.Text = name;
		descriptionLabel.Text = description;
	}

	private void _on_button_pressed()
	{
		var animationPlayer = GetNode<AnimationPlayer>("/root/mainGame/AnimationPlayer");
		if (animationPlayer.IsPlaying()) return;
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		GameManager.Instance.playerType =  (GameManager.PlayerType)indexInContainer;
		animationPlayer.Play("card_out");
		GameManager.Instance.charChose = true;
		GameManager.Instance.SummonPlayer(GetTree());
	}
}
