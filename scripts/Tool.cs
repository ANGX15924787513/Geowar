using System.Collections.Generic;
using Godot;
using Godot.Collections;

public static class TayTool
{
    private static PackedScene collideLightScene;

    public static void Initialize()
    {
        collideLightScene = GD.Load<PackedScene>("res://scenes/main/particles/collide_light_particle.tscn");
    }
    public static Vector2 GetClosestVecInArray(Vector2 self, Array<Vector2> to)
    {
        float dist = float.MaxValue;
        Vector2 closest = Vector2.Zero;
        foreach (Vector2 _to in to)
        {
            dist = dist > _to.DistanceTo(self) ? _to.DistanceTo(self) : dist;
            closest = _to;
        }
        return closest;
    }
    
    private static readonly Queue<GpuParticles2D> free =  new();
    
    public static void CollideLight(Vector2 position,Color color,SceneTree tree,int amount = 10,float distMult = 1)
    {
        GpuParticles2D particle;
        if (free.Count > 0)
        {
            particle = free.Dequeue();
            particle.GetParent()?.RemoveChild(particle);
        }
        else
        {
            particle = collideLightScene.Instantiate<GpuParticles2D>();
            particle.Finished += () =>
            {
                if (!GodotObject.IsInstanceValid(particle)) return;
                particle.GetParent()?.RemoveChild(particle);
                free.Enqueue(particle);
            };
        }
        
        particle.GlobalPosition = position;
        particle.Amount = amount;
        var mat = (ParticleProcessMaterial)particle.ProcessMaterial;
        if (mat == null)
        {
            Log("碰撞光效","材质为空");
            return;
        }
        mat.Color = color;
        mat.InitialVelocity = new Vector2(100, distMult * 400);
        
        tree.CurrentScene.AddChild(particle);
        particle.Restart();
    }

    public enum LogTypeEnum {Info,Err}
    public static void Log(string head,string msg,LogTypeEnum logType = LogTypeEnum.Info)
    {
        switch (logType)
        {
            case LogTypeEnum.Info:
                GD.Print($"[{head}]: {msg}");
                break;
            case LogTypeEnum.Err:
                GD.PrintErr($"[{head}]: {msg}");
                break;
            default:
                return;
        }
    }

    public static Vector2I GetRandVec2I(int range)
    {
        var a = range / 2;
        return new Vector2I(GD.RandRange(-a, a), GD.RandRange(-a, a));
    }
}
