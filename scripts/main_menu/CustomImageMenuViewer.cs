using Godot;

public partial class CustomImageMenuViewer : ColorRect
{
    TextureRect customImageTextureRect;
    public static CustomImageMenuViewer Instance { get; set; }
    public override void _Ready()
    {
        LoadImage();
    }

    public override void _EnterTree() => Instance = this;
    public override void _ExitTree() => Instance = null;

    public void LoadImage()
    {
        string path = DataManager.Instance.GetStringConfig(CustomalizeBgImage.pathDataId);
        if (string.IsNullOrEmpty(path)) return;
        Image loader = new Image();
        var loadRst = loader.Load(path);
        GD.Print("加载图像:" + loadRst);
        if (loadRst != Error.Ok) return;
        ImageTexture img = ImageTexture.CreateFromImage(loader);
        SelfModulate = new Color(Colors.White, DataManager.Instance.GetFloatConfig(CustomalizeBgImage.alphaDataId));
        customImageTextureRect = new TextureRect();
        customImageTextureRect.ZIndex = ZIndex - 1;
        customImageTextureRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
        customImageTextureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        customImageTextureRect.Texture = img;
        customImageTextureRect.Size = Size;
        var mat = new ShaderMaterial();
        mat.Shader = GD.Load<Shader>("res://shaders/blur_shader.gdshader");
        mat.SetShaderParameter("blur_amount", DataManager.Instance.GetFloatConfig(CustomalizeBgImage.blurDataId));
        customImageTextureRect.Material = mat;
        foreach (var node in GetChildren()) node.QueueFree();
        AddChild(customImageTextureRect);
    }
}