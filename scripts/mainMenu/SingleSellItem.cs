using Godot;
using System;

public partial class SingleSellItem : Button
{
	[Export] private String name;
	[Export] private Texture2D icon;
	public override void _Ready()
	{
		GetNode<Label>("Label").Text = name;
		GetNode<TextureRect>("TextureRect").Texture = icon;
	}

	private void OnPressed()
	{
		GD.Print(name + "请求购买");
	}
}
