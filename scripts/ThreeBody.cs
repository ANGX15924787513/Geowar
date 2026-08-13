using Godot;
using System.Collections.Generic;

public partial class ThreeBody : Node2D
{
    [Export] public float Gravity = 500f;
    [Export] public float MinDist = 80f;
    [Export] public bool DrawTrace;

    // 新增：根节点（原点）处的中心引力质量
    [Export] public float CenterMass = 5000f;

    private struct Body { public Vector2 Pos, Vel; public float Mass, Radius; }

    private List<Body> _bodies = new();
    private List<Vector2[]> _trails = new();

    public override void _Ready()
    {
        float posRange = 150f;
        // 初始条件：三质点
        float totalDist;
        (Vector2 pos, Vector2 vel, float mass)[] init;
        do
        {
            init = [
                (new Vector2((float)GD.RandRange(-posRange, posRange), (float)GD.RandRange(-posRange, posRange)),
                    new Vector2(0, 120), 200),
                (new Vector2((float)GD.RandRange(-posRange, posRange), (float)GD.RandRange(-posRange, posRange)),
                    new Vector2(-80, -60), 200),
                (new Vector2((float)GD.RandRange(-posRange, posRange), (float)GD.RandRange(-posRange, posRange)),
                    new Vector2(120, -40), 200)
            ];
            totalDist = init[0].pos.DistanceTo(init[1].pos) + init[1].pos.DistanceTo(init[2].pos) + init[2].pos.DistanceTo(init[0].pos);
        }while (totalDist < 400f);
        foreach (var (pos, vel, mass) in init)
        {
            _bodies.Add(new Body { Pos = pos, Vel = vel, Mass = mass, Radius = Mathf.Sqrt(mass) * 1.2f });
            _trails.Add(new Vector2[100]);
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // 1. 质点之间的引力
        for (int i = 0; i < _bodies.Count; i++)
            for (int j = i + 1; j < _bodies.Count; j++)
                ApplyGravity(i, j);

        // 2. 根节点原点对每个质点的中心引力（防止飞远）
        for (int i = 0; i < _bodies.Count; i++)
        {
            var b = _bodies[i];
            Vector2 toCenter = -b.Pos;           // 指向原点
            float r = toCenter.Length();
            if (r < 1f) continue;                // 已在原点，跳过

            float softened = Mathf.Max(r, MinDist);
            // 加速度大小 = G * M_center / r²，方向指向原点
            Vector2 acc = r * Gravity * CenterMass / (softened * softened * softened * 100) * toCenter;
            b.Vel += acc * dt;
            _bodies[i] = b;
        }

        // 3. 更新位置
        for (int i = 0; i < _bodies.Count; i++)
        {
            var b = _bodies[i];
            b.Pos += b.Vel * dt;
            _bodies[i] = b;

            // 轨迹记录
            if (DrawTrace)
            {
                var trail = _trails[i];
                for (int k = trail.Length - 1; k > 0; k--)
                    trail[k] = trail[k - 1];
                trail[0] = b.Pos;
            }
        }

        QueueRedraw();
    }

    private void ApplyGravity(int i, int j)
    {
        var a = _bodies[i];
        var b = _bodies[j];

        Vector2 dir = b.Pos - a.Pos;
        float r = dir.Length();
        if (r < 1f) return;

        float softened = Mathf.Max(r, MinDist);
        float k = Gravity / (softened * softened * softened);

        a.Vel += k * b.Mass * dir;
        b.Vel -= k * a.Mass * dir;

        _bodies[i] = a;
        _bodies[j] = b;
    }

    public override void _Draw()
    {
        foreach (var b in _bodies)
            DrawCircle(b.Pos, b.Radius, new Color(1, 1, 1, 0.6f));

        if (DrawTrace)
        {
            foreach (var trail in _trails)
            {
                var pts = new Vector2[trail.Length];
                for (int i = 0; i < trail.Length; i++)
                    pts[i] = trail[i];
                DrawPolyline(pts, new Color(1f, 1f, 1f, 0.2f), 10);
            }
        }
    }
}