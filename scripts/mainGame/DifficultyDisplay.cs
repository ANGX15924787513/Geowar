using Godot;
using System;

public partial class DifficultyDisplay : Control
{
	Polygon2D progressPolygon;
	Polygon2D backgroundPolygon;

	private Label label;
	//0,1为右侧顶点
	private float left, right;
	
	float difficulty;
	public override void _Ready()
	{
		progressPolygon = GetNode<Polygon2D>("Background/Progress");
		backgroundPolygon = GetNode<Polygon2D>("Background");
		label = GetNode<Label>("Label");
		left = backgroundPolygon.Polygon[3].X;
		right = backgroundPolygon.Polygon[0].X;
		difficulty = SingleGameData.Instance.currentDifficulty;
		OnTimeout();
	}
	
	public override void _Process(double delta)
	{
		difficulty = SingleGameData.Instance.currentDifficulty;
		Visible = GameManager.Instance.gameState == GameManager.GameState.GAMING;
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
