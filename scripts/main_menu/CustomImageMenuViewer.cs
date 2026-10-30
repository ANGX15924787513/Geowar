using System.Linq;
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
        if (string.IsNullOrEmpty(path)) { ClearImage(); return; }
        Image loader = new Image();
        var loadRst = loader.Load(path);
        TayTool.Log("图像",$"加载图像: {loadRst}");
        if (loadRst != Error.Ok) { ClearImage(); return; }
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
        ClearImage();
        AddChild(customImageTextureRect);
        
        void ClearImage() { foreach (var node in GetChildren()) node.QueueFree(); }
    }

    /// <summary> 拖动时实时更新背景透明度, 不写存档 </summary>
    public void SetAlpha(float v)
    {
        if (customImageTextureRect == null) return;
        SelfModulate = new Color(Colors.White, v);
    }

    /// <summary> 拖动时实时更新背景模糊度, 不写存档 </summary>
    public void SetBlur(float v)
    {
        if (customImageTextureRect?.Material is not ShaderMaterial mat) return;
        mat.SetShaderParameter("blur_amount", v);
    }
}