using Godot;
using System;

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
			subParticleMaterial = subParticle2D.ProcessMaterial as ParticleProcessMaterial;
		}
		
		particleMaterial = ProcessMaterial as ParticleProcessMaterial;
		if (particleMaterial != null) colorGrad = particleMaterial.ColorRamp as GradientTexture1D;
		if (colorGrad != null) colorGradient = colorGrad.Gradient;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (colorGradient != null && colorGradient.Colors.Length > 1)
		{
			var currentColor = colorGradient.GetColor(1);
			if (currentColor != color)
			{
				colorGradient.SetColor(1, color);
				subParticleMaterial.Color = (color + new Color(1,1,1)) / 2;
			}
		}
	}
}
