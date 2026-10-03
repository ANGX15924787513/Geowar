using Godot;
using Godot.Collections;

public static partial class TayTool
{
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
    
    public static void CollideLight(Vector2 position,Color color,SceneTree tree,Texture2D lightTex = null,float distMult = 1)
    {
        if (lightTex == null)
        {
            lightTex = GD.Load<CompressedTexture2D>("res://texture/circle_glow.svg");
        }
        float duration = 0.8f;
        var light = new PointLight2D();
        light.Texture = lightTex;
        light.TextureScale = 0.1f;
        light.Color = color;
        light.Energy = 2;
        light.Position = position + new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f) * 40;
        tree.CurrentScene.AddChild(light);
        Tween tw = tree.CreateTween();
        Tween tw2 = tree.CreateTween();
        Tween tw3 = tree.CreateTween();
        tw.TweenProperty(light,"texture_scale",0.02f,duration);
        tw2.TweenProperty(light,"position",light.Position + new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f) * (300 * distMult),duration);
        tw3.TweenProperty(light,"modulate",new Color(1, 1, 1, 0.5f), duration);
        tree.CreateTimer(duration).Timeout += light.QueueFree;
    }
}
