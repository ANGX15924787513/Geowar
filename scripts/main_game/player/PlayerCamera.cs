using Godot;

public partial class PlayerCamera : Camera2D
{
    public Vector2 toZoom;
    public bool needZoom = true;
    public static PlayerCamera Instance;

    // 抖动相关
    private float _shakeTime = 0f;
    private float _shakeDuration = 0f;
    private float _shakeIntensity = 0f;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (needZoom)
            Zoom = Zoom.Lerp(toZoom, 0.05f);

        // 抖动处理
        if (_shakeTime > 0)
        {
            _shakeTime -= (float)delta;

            // 随时间衰减，抖完归零
            float decay = _shakeTime / _shakeDuration;
            float strength = _shakeIntensity * decay;

            Offset = new Vector2(
                (float)GD.RandRange(-strength, strength),
                (float)GD.RandRange(-strength, strength)
            );
        }
        else if (Offset != Vector2.Zero)
        {
            Offset = Vector2.Zero;
        }
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    /// <summary>
    /// 触发抖动
    /// </summary>
    /// <param name="amount">抖动强度（像素）</param>
    /// <param name="duration">持续时间（秒），默认 0.3</param>
    public void Shake(float amount, float duration = 0.3f)
    {
        // 如果新抖动更强，就覆盖；否则叠加
        if (amount >= _shakeIntensity || _shakeTime <= 0)
        {
            _shakeIntensity = amount;
            _shakeDuration = duration;
            _shakeTime = duration;
        }
        else
        {
            // 多次抖动叠加时长和强度（避免被弱抖动打断强抖动）
            _shakeTime = Mathf.Max(_shakeTime, duration);
        }
    }
}