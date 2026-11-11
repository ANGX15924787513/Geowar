using Godot;

public partial class Cursor : Sprite2D
{
	public static Cursor Instance { get; private set; }

	private Node2D _enemyRoot;

	[Export] private float normalScale = 0.5f;
	[Export] private float lockScale = 1.5f;
	[Export] private float maxScale = 2f;
	[Export] private float lockRange = 600f;      // 锁定范围（像素）
	[Export] private float scaleSpeed = 4f;
	[Export] private float shakeAmount = 0.12f;
	[Export] private float shakeSpeed = 25f;

	private float _currentScale = 0.5f;
	private float _shakePhase;
	private Node2D _targetBody;

	private bool isAutoAim;

	public bool IsLocked => _targetBody != null && IsInstanceValid(_targetBody);
	public Vector2 AimPosition => GlobalPosition;

	public override void _Ready()
	{
		Instance = this;
		isAutoAim = DataManager.Instance.GetBoolConfig("auto_aim");
		// if(OS.GetName()=="Android" || OS.GetName()== "iOS")
		// {
		// 	isAutoAim = false;
		// 	Visible = false;
		// }
		Scale = Vector2.One * _currentScale;
	}

	public override void _ExitTree()
	{
		if (Instance == this) Instance = null;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		if (GameManager.Instance.gameState != GameManager.GameState.GAMING)
		{
			Visible = false;
			return;
		}
		Visible = true;

		// 每帧找 enemyRoot 里最近的敌人
		if (isAutoAim) { UpdateTarget(); }

		bool shooting = Input.IsActionPressed("player_launch_bullet");
		float targetScale = IsLocked ? lockScale : normalScale;

		if (shooting)
		{
			_shakePhase += dt * shakeSpeed;
			_currentScale = targetScale + Mathf.Sin(_shakePhase) * shakeAmount;
		}
		else
		{
			_currentScale = Mathf.MoveToward(_currentScale, targetScale, scaleSpeed * dt);
		}
		_currentScale = Mathf.Clamp(_currentScale, 0.2f, maxScale);
		Scale = Vector2.One * _currentScale;

		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 targetPos = IsLocked ? _targetBody.GlobalPosition : mousePos;
		GlobalPosition = GlobalPosition.Lerp(targetPos, 0.5f);
	}

	private void UpdateTarget()
	{
		if (_enemyRoot == null)
			_enemyRoot = GetTree().CurrentScene.GetNodeOrNull<Node2D>("enemyRoot");

		Node2D closest = null;
		float minDist = lockRange;

		if (_enemyRoot != null)
		{
			Vector2 mousePos = new Vector2();
			if (GameManager.Instance.isMobile)
			{
				var pos = GameManager.Instance.GetLastPressedPosition();
				if (pos.HasValue) mousePos = pos.Value;
			}
			else
			{
				mousePos = GetGlobalMousePosition();
			}
			int childCount = _enemyRoot.GetChildCount();
			for (int i = 0; i < childCount; i++)
			{
				var child = _enemyRoot.GetChild(i);
				if (child is not Node2D body) continue;
				if (!body.IsInGroup("enemy")) continue;
				if (!IsInstanceValid(body)) continue;

				float dist = body.GlobalPosition.DistanceTo(mousePos);
				if (!(dist < minDist)) continue;
				minDist = dist;
				closest = body;
			}
		}

		_targetBody = closest;
	}
}
