using Godot;

public partial class CustomImageViewer : TextureRect
{
	TextureRect customImageTextureRect;
	public override void _Ready()
	{
		string path = DataManager.Instance.GetStringConfig(CustomalizeBgImage.pathDataId);
		Image loader = new Image();
		var loadRst = loader.Load(path);
		GD.Print("加载图像:" + loadRst);
		if (loadRst != Error.Ok) return;
		ImageTexture img = ImageTexture.CreateFromImage(loader);
		SelfModulate = new Color(Colors.White, DataManager.Instance.GetFloatConfig(CustomalizeBgImage.alphaDataId));
		customImageTextureRect = new TextureRect();
		customImageTextureRect.ZIndex = ZIndex - 1;
		customImageTextureRect.ExpandMode = ExpandModeEnum.FitHeightProportional;
		customImageTextureRect.StretchMode = StretchModeEnum.KeepAspectCentered;
		customImageTextureRect.Texture = img;
		customImageTextureRect.Size = Size;
		var mat = new ShaderMaterial();
		mat.Shader = GD.Load<Shader>("res://shaders/blur_shader.gdshader");
		mat.SetShaderParameter("blur_amount", DataManager.Instance.GetFloatConfig(CustomalizeBgImage.blurDataId));
		customImageTextureRect.Material = mat;
		AddChild(customImageTextureRect);
	}
}
