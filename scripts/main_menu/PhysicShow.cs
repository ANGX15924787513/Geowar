using Godot;

public partial class PhysicShow : HBoxContainer
{
	[Export] public int count = 3;
	[Export] public Godot.Collections.Array<Texture2D> itemTextures = new();

	public override void _Ready()
	{
		if (itemTextures.Count == 0)
		{
			return;
		}

		var loaded = new Godot.Collections.Array<Texture2D>();
		foreach (var tex in itemTextures)
		{
			if (tex != null)
			{
				loaded.Add(tex);
			}
		}

		for (int i = 0; i < count; i++)
		{
			create_item(loaded.PickRandom());
		}
	}

	public void create_item(Texture2D texture)
	{
		var file_name = texture.ResourcePath.GetFile().GetBaseName();

		var vbox = new VBoxContainer();
		vbox.Alignment = BoxContainer.AlignmentMode.Center;

		var rect = new TextureRect();
		rect.Texture = texture;
		rect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		rect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		rect.CustomMinimumSize = new Vector2(64, 64);
		rect.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
		vbox.AddChild(rect);

		var label = new Label();
		label.Text = file_name.Length > 15 ? file_name.Substr(0, 13) + "..." : file_name;
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.AddThemeFontSizeOverride("font_size", 21);
		label.AddThemeFontOverride("font", GD.Load<Font>("res://fonts/BoutiqueBitmap9x9_1.9.ttf"));
		vbox.AddChild(label);

		AddChild(vbox);
	}
}
