using Godot;

public partial class PointParticle : GpuParticles2D
{
	[Export] public Color color;
	private ParticleProcessMaterial particleMaterial;
	private GradientTexture1D colorGrad;
	private Gradient colorGradient;
	private GpuParticles2D subParticle2D;
	private ParticleProcessMaterial subParticleMaterial;

	public override void _Ready()
	{
		subParticle2D = GetNode<GpuParticles2D>("GPUParticles2D");
		if (subParticle2D != null)
		{
			MaterialInit();
			subParticle2D.ProcessMaterial = subParticleMaterial;
		}
		
		particleMaterial = ProcessMaterial as ParticleProcessMaterial;
		if (particleMaterial != null) colorGrad = particleMaterial.ColorRamp as GradientTexture1D;
		if (colorGrad != null) colorGradient = colorGrad.Gradient;
	}

	private void MaterialInit()
	{
		ParticleProcessMaterial pm = new ParticleProcessMaterial();
		pm.LifetimeRandomness = 0.19f;
		pm.ParticleFlagDisableZ = true;
		pm.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.SphereSurface;
		pm.EmissionSphereRadius = 15.69f;
		pm.Direction = new Vector3(-1, 0, 0);
		pm.Spread = 30;
		pm.InitialVelocity = new Vector2(0.45f,0.45f);
		pm.Gravity = new Vector3(-186, 0, 0);
		pm.Scale = new Vector2(0.03f, 0.03f);
			
		CurveTexture sc = new CurveTexture();
		sc.Curve = new Curve();
		sc.Curve.ClearPoints();
		sc.Curve.AddPoint(new Vector2(0,1));
		sc.Curve.AddPoint(new Vector2(1,0.361f));
			
		pm.ScaleCurve = sc;
		pm.Color = (color + new Color(1, 1, 1)) / 2;
			
		CurveTexture ac =  new CurveTexture();
		ac.Curve = new Curve();
		ac.Curve.ClearPoints();
		ac.Curve.AddPoint(new Vector2(0,1));
		ac.Curve.AddPoint(new Vector2(0.622f,0.392f));
		ac.Curve.AddPoint(new Vector2(1,0));

		pm.AlphaCurve = ac;
			
		subParticleMaterial = pm;
	}

	//Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (colorGradient != null && colorGradient.Colors.Length > 1)
		{
			var currentColor = colorGradient.GetColor(1);
			if (currentColor != color)
			{
				colorGradient.SetColor(1, color);
				subParticleMaterial.Color = (color + new Color(1, 1, 1)) / 2;
			}
		}
	}
}
