using Godot;

public partial class TipLabel : Label
{
	[Export] public string[] tips = new string[0];

	public void refresh_tip()
	{
		if (tips.Length == 0)
		{
			TayTool.Log("提示","你没有写提示!");
			Text = "Tip:" + "你没有写提示!";
		}
		Text = "Tip:\n" + tips[GD.RandRange(0, tips.Length - 1)];
	}
}
