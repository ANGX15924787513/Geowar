using System;
using Godot;

public partial class LineEnemy : Enemy
{
    private bool isRushing;
    private Line2D line;
    
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
            AngularVelocity = 3*(targetDir - Rotation);
            line.Points = [GlobalPosition,line.Points[1].Lerp(_targetPlayer.GlobalPosition,0.08f)];
        }
    }

    private void Rush()
    {
        line = new Line2D();
        line.JointMode = Line2D.LineJointMode.Round;
        line.BeginCapMode = Line2D.LineCapMode.Round;
        line.EndCapMode = Line2D.LineCapMode.Round;
        line.DefaultColor = Color.Color8(255,110,56,80);
        line.Width = 30;
        line.Points = [GlobalPosition,GlobalPosition];
        GetTree().CurrentScene.AddChild(line);
        isRushing = true;
        LinearVelocity = new Vector2(0, 0);
        GetTree().CreateTimer(1f).Timeout += () =>
        {
            //rushing
            CollisionMask &= ~(uint)(1 << (4 - 1));
            LockRotation = true;
            LinearVelocity = (_targetPlayer.Position - Position).Normalized() * 3000;
            GetTree().CurrentScene.RemoveChild(line);
            GetTree().CreateTimer(1f).Timeout += () =>
            {
                //after rush
                CollisionMask |= 1 << (4 - 1);
                LockRotation = false;
                line = null;
                isRushing = false;
            };
        };
    }

    private void OnRushTimerTimeout()
    {
        if(!isRushing && gameManager.gameState == GameManager.GameState.GAMING && SingleGameData.Instance.currentDifficulty >= gameManager.LINE_RUSH) Rush();
    }

    public override void _ExitTree()
    {
        if (line != null) GetTree().CurrentScene.CallDeferred("remove_child", line);
        base._ExitTree();
    }
}
