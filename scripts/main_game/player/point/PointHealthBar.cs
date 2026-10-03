using System;
using Godot;

public partial class PointHealthBar : Line2D
{
	private float _radius = 80;

	[Export]
	public float radius
	{
		get => _radius;
		set
		{
			_radius = value;
			update_polygon();
		}
	}

	[Export] public float shake_intensity = 12;
	private int _point_count = 50;

	[Export]
	public int point_count
	{
		get => _point_count;
		set
		{
			_point_count = value;
			update_polygon();
		}
	}

	[Export] public float shake_duration = 0.5f;
	[Export] public float recover_speed = 3.0f;

	private float shake_timer = 0.0f;
	private bool is_shaking = false;
	private float[] _point_offsets;
	private float[] _offset_velocities;

	// 血量驱动
	private float _hp_ratio = 1.0f;
	private float _display_ratio = 1.0f;
	private float _display_radius;

	private SignalManager signalManager;

	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		signalManager.OnPlayerHealthChanged += on_player_got_hurt;
		_display_radius = radius;
		_display_ratio = 1.0f;
		_init_offsets();
		update_polygon();
	}

	public override void _ExitTree()
	{
		signalManager.OnPlayerHealthChanged -= on_player_got_hurt;
	}

	private void _init_offsets()
	{
		Array.Resize(ref _point_offsets, point_count);
		Array.Resize(ref _offset_velocities, point_count);
	}

	public override void _Process(double delta)
	{
		// 平滑过渡 display_ratio → hp_ratio
		_display_ratio = Mathf.MoveToward(_display_ratio, _hp_ratio, 0.8f * (float)delta);
		_display_radius = Mathf.MoveToward(_display_radius, radius * _display_ratio, radius * (float)delta);

		// 振动逻辑
		if (is_shaking)
		{
			shake_timer -= (float)delta;
			if (shake_timer <= 0)
			{
				is_shaking = false;
			}
			_shake_update((float)delta);
		}
		else
		{
			_recover_update((float)delta);
		}

		// 每帧根据当前显示的半径和颜色更新
		_Draw();
	}

	public override void _Draw()
	{
		var pts = new Vector2[point_count];
		float r = _display_radius;
		var half_alpha_red = Colors.Red;
		half_alpha_red.A = 0.7f;
		var half_alpha_green = Colors.Green;
		half_alpha_green.A = 0.7f;
		var color = half_alpha_red.Lerp(half_alpha_green, _display_ratio);
		DefaultColor = color;

		for (int i = 0; i < point_count; i++)
		{
			float angle = Mathf.DegToRad(360.0f * i / point_count);
			pts[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (r + _point_offsets[i]);
		}
		Points = pts;
	}

	public Vector2 get_pos_in_circle_with_index(float r, int index)
	{
		float angle = Mathf.DegToRad(360.0f * index / point_count);
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
	}

	public void update_polygon()
	{
	}

	private void on_player_got_hurt(int _damage, int hp, int max_hp)
	{
		_hp_ratio = Mathf.Clamp((float)hp / (float)max_hp, 0.0f, 1.0f);
		shake_timer = shake_duration;
		is_shaking = true;
		for (int i = 0; i < point_count; i++)
		{
			_offset_velocities[i] = (float)GD.RandRange(-shake_intensity, shake_intensity) * 4.0f;
		}
	}

	private void _shake_update(float delta)
	{
		for (int i = 0; i < point_count; i++)
		{
			float spring = -_point_offsets[i] * 8.0f;
			float damping = -_offset_velocities[i] * 2.5f;
			_offset_velocities[i] += (spring + damping) * delta;
			_point_offsets[i] += _offset_velocities[i] * delta;
		}
	}

	private void _recover_update(float delta)
	{
		for (int i = 0; i < point_count; i++)
		{
			_point_offsets[i] = Mathf.MoveToward(_point_offsets[i], 0.0f, shake_intensity * recover_speed * delta);
			_offset_velocities[i] = Mathf.MoveToward(_offset_velocities[i], 0.0f, shake_intensity * 2.0f * delta);
		}
	}
}
