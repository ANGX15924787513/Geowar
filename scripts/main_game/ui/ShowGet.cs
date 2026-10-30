using Godot;

public partial class ShowGet : Node
{
	[Export]private Label ironLabel;
	[Export]private Label goldLabel;
	[Export]private Label diamondLabel;
	[Export]private Label scoreLabel;

	public override void _Ready()
	{
		// 将本局收获写入持久化存储
		DataManager.Instance.Add(FallItem.FallItemType.GOLD,    SingleGameData.Instance.goldCount + SingleGameData.Instance.ironCount / 4);
		DataManager.Instance.Add(FallItem.FallItemType.DIAMOND, SingleGameData.Instance.diamondCount);

		// 显示本局收获
		ironLabel.Text    = SingleGameData.Instance.ironCount.ToString();
		goldLabel.Text    = SingleGameData.Instance.goldCount.ToString();
		diamondLabel.Text = SingleGameData.Instance.diamondCount.ToString();
		scoreLabel.Text   = $"得分:{SingleGameData.Instance.ironCount + 
		                          SingleGameData.Instance.goldCount * 4 + 
		                          SingleGameData.Instance.diamondCount * 50}";
	}
}
