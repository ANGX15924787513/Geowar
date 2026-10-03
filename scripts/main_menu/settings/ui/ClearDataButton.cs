using Godot;

public partial class ClearDataButton : Button
{
	private int count = 0;
	private SceneTreeTimer yesTimer;

	private void _on_pressed()
	{
		count += 1;
		if (count == 1)
		{
			Text = "你的进度会永远离开你!(真的很久! 再按一次确认)";
			Modulate.Lightened(0.2f);
			yesTimer = GetTree().CreateTimer(1);
			yesTimer.Timeout += on_timeout;
		}
		if (count >= 2)
		{
			DataManager.Instance.ClearAll();
			GetParent().Call("RefreshDisplay");
			count = 0;
			Text = "清除成功";
		}
	}

	public override void _ExitTree()
	{
		if (yesTimer != null)
		{
			yesTimer.Timeout -= on_timeout;
			yesTimer = null;
		}
	}

	private void on_timeout()
	{
		count = 0;
		Text = "清除数据";
		Modulate.Darkened(0.2f);
	}
}
