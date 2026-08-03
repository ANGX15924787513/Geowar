using System;
using Godot;

public partial class LineEnemy : Enemy
{
    private bool isRushing;
    public override void _Process(double delta)
    {
        if (gameManager.gameState != GameManager.GameState.GAMING)
        {
            LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, 0.05f);
            return;
        }
        if(!isRushing) base._Process(delta);
        else
        {
            float targetDir = (_targetPlayer.Position - Position).Angle();
            AngularVelocity = 2*(targetDir - Rotation);
        }
    }

    private async void Rush()
    {
        isRushing = true;
        LinearVelocity = new Vector2(0, 0);
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        LinearVelocity = (_targetPlayer.Position - Position).Normalized() * 3000;
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        isRushing = false;
    }

    private void OnRushTimerTimeout()
    {
        if(!isRushing && gameManager.gameState == GameManager.GameState.GAMING) Rush();
    }
}
