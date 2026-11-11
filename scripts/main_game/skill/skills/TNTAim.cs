using Godot;

public partial class TNTAim : TextureRect
{
    private static int counter;
    public Enemy aimNode;
    private float time;

    public override void _EnterTree()
    {
        counter++;
    }
    
    public override void _ExitTree()
    {
        counter--;
        if (counter != 0) return;
        GlobalCamera.Instance.PositionSmoothingEnabled = false;
        GlobalCamera.Instance.Enabled = false;
        PlayerCamera.Instance.MakeCurrent();
        Player.Instance.isInvincible = false;
    }
    
    public override void _Process(double delta)
    {
        if (GameManager.Instance.gameState != GameManager.GameState.GAMING) return;
        time += (float)delta;
        if (!IsInstanceValid(aimNode)) return;
        GlobalPosition = GlobalPosition.Lerp(aimNode.GlobalPosition, 0.15f);
        var dist = GlobalPosition.DistanceTo(aimNode.GlobalPosition);
        if (dist < 80)
        {
            Explode();
        }
    }

    public void Explode()
    {
        //爆炸
        GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.boomSound,0.8f);
        if (IsInstanceValid(aimNode)) aimNode.Hurt(9999999,null);
        TayTool.CollideLight(Position, Colors.Red, GetTree(),50,10f);
        QueueFree();
    }
}
