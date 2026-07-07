using Godot;

public partial class ShowGet : Node2D
{
	private DataManager dataManager;
	private SingleGameData gameData;

	private Label ironLabel;
	private Label goldLabel;
	private Label diamondLabel;
	private Label scoreLabel;

	public override void _Ready()
	{
		dataManager = GetNode<DataManager>("/root/DataManager");
		gameData = GetNode<SingleGameData>("/root/mainGame/SingleGameData");

		ironLabel    = GetNode<Label>("ironLabel");
		goldLabel    = GetNode<Label>("goldLabel");
		diamondLabel = GetNode<Label>("diamondLabel");
		scoreLabel   = GetNode<Label>("../score");

		// 将本局收获写入持久化存储
		dataManager.Add(FallItem.FallItemType.GOLD,    gameData.goldCount + gameData.ironCount / 4);
		dataManager.Add(FallItem.FallItemType.DIAMOND, gameData.diamondCount);

		// 显示本局收获
		ironLabel.Text    = gameData.ironCount.ToString();
		goldLabel.Text    = gameData.goldCount.ToString();
		diamondLabel.Text = gameData.diamondCount.ToString();
		scoreLabel.Text   = $"得分:{gameData.ironCount + 
		                          gameData.goldCount * 4 + 
		                          gameData.diamondCount * 50}";
	}
}
