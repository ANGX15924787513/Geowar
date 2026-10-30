using Godot;

public partial class Title : TextureRect
{
    public static Title Instance;
    private Vector2 mousePos;
    private Vector2 selfPos;
    private AnimationPlayer animationPlayer;
    [Export] private float transitionR;
    [Export] private float maxFeelDistance;
    [Export] private float resetSpeed = 0.6f;

    public override void _Ready()
    {
        selfPos = GlobalPosition;
        animationPlayer = GetNode<AnimationPlayer>("../../AnimationPlayer");
    }
    
    public override void _EnterTree() => Instance = this;
    public override void _ExitTree() => Instance = null;

    public override void _Process(double delta)
    {
        if (animationPlayer.IsPlaying()) return;
        mousePos = GetGlobalMousePosition();
        Vector2 toPos;
        if (mousePos.Y < selfPos.Y)
        {
            toPos = selfPos;
        }
        else
        {
            float distance = selfPos.DistanceTo(mousePos);
            bool over = distance > maxFeelDistance;
            Vector2 direction = (mousePos - GlobalPosition).Normalized();
            if (over)
            {
                toPos = selfPos + direction * transitionR;
            }
            else
            {
                toPos = selfPos + direction * (distance / maxFeelDistance) * transitionR;
            }
        }

        GlobalPosition = GlobalPosition.Lerp(toPos, resetSpeed);
    }
}