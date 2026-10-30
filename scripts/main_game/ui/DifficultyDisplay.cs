using Godot;

public partial class DifficultyDisplay : Control
{
	[Export]Polygon2D progressPolygon;
	[Export]Polygon2D backgroundPolygon;

	[Export]private Label label;
	//0,1为右侧顶点
	private float left, right;
	
	float difficulty;
	public override void _Ready()
	{
		left = backgroundPolygon.Polygon[3].X;
		right = backgroundPolygon.Polygon[0].X;
		difficulty = SingleGameData.Instance.currentDifficulty;
		GameManager.Instance.OnGameStateChange += UpdateVisible;
		OnTimeout();
	}

	public override void _ExitTree()
	{
		GameManager.Instance.OnGameStateChange -= UpdateVisible;
	}

	private void UpdateVisible(GameManager.GameState newState)
	{
		Visible = newState == GameManager.GameState.GAMING;
	}

	public override void _Process(double delta)
	{
		difficulty = SingleGameData.Instance.currentDifficulty;
		if (Visible)
		{
			ProgressUpdate();
			ColorUpdate();
		}
	}

	private void ProgressUpdate()
	{
		progressPolygon.Position = progressPolygon.Position.Lerp(progressPolygon.Position with {X = -(right - left) + difficulty * (right - left)},0.08f);
	}

	private void OnTimeout()
	{
		label.Text = $"难度: {(int)(difficulty * 100 - difficulty * 10000 % 100 * 0.01)}%";
	}

	private void ColorUpdate()
	{
		Color origin = Colors.SpringGreen.Lerp(Colors.Red, difficulty);
		Color backgroundColor = new Color(origin,0.2f);
		Color progressColor = new Color(origin, 0.4f);
		backgroundPolygon.SelfModulate = backgroundColor;
		progressPolygon.Color = progressColor;
		label.LabelSettings.OutlineColor = origin;
	}
}
