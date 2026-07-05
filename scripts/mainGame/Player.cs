using Godot;
using System;

public partial class Player : CharacterBody2D//这个节点没用
{
	[Export]
	private PackedScene[] typeOfPlayer; //1.点 2.线 3.多边形
	private AnimationPlayer animationPlayer;
	[Export]
	private float moveSpeed = 200f;
	[Export] private PackedScene pointScene;
	[Export] float fireRate = 0.1f;
	GameManager gameManager;
	
	float _fireRate = 0f;

	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		AddChild(typeOfPlayer[(int)gameManager.playerType].Instantiate());
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 vec = Input.GetVector("player_left","player_right","player_up","player_down");
		Velocity = vec * moveSpeed * (float)delta * 60;
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		Vector2 mousePos = GetGlobalMousePosition();
		LookAt(mousePos);
		_fireRate += (float)delta;
		if (_fireRate < fireRate)
		{
			return;
		}
		_fireRate = 0f;
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			NormalAttack(Input.IsActionPressed("shift"));
		}else if (Input.IsMouseButtonPressed(MouseButton.Right))
		{
			SuperAttack(Input.IsActionPressed("shift"));
		}
	}

	private void NormalAttack(bool isShift)
	{
		Node2D bullet = (Node2D)pointScene.Instantiate();
		bullet.Rotation = Rotation + (float)Math.PI/2;
		bullet.GlobalPosition = GetNode<Node2D>("bulletSummonPoint").GlobalPosition;
		GetTree().CurrentScene.AddChild(bullet);
	}
	private void SuperAttack(bool isShift)
	{
		
	}
}
