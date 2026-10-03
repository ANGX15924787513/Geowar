using Godot;

public partial class CustomalizeBgImage : HBoxContainer
{
	public const string pathDataId = "custom_image_path";
	public const string alphaDataId = "custom_image_alpha";
	public const string blurDataId = "custom_image_blur";
	
	[Export]private Label PathLabel;
	[Export]private LineEdit PathEditor;
	[Export]private Button ResetButton;
	[Export] private HSlider AlphaSlider;
	[Export] private HSlider BlurSlider;
	
	Theme theme = GD.Load<Theme>("res://resources/setting_theme.tres");
	public override void _Ready()
	{
		UpdateDetail();
	}

	private void SetDetail(bool visible)
	{
		PathLabel.Visible = visible;
		PathEditor.Visible = visible;
		ResetButton.Visible = visible;
		AlphaSlider.Visible = visible;
		BlurSlider.Visible = visible;
	}

	private void UpdateDetail()
	{
		var path = DataManager.Instance.GetStringConfig(pathDataId);
		SetDetail(FileAccess.FileExists(path));
		PathEditor.Text = path;
		AlphaSlider.Value = DataManager.Instance.GetFloatConfig(alphaDataId);
		BlurSlider.Value = DataManager.Instance.GetFloatConfig(blurDataId);
		
		//同步主菜单背景
		CustomImageMenuViewer.Instance.LoadImage();
	}

	private void onChooseButtonPressed()
	{
		FileDialog dg =  new FileDialog();
		dg.DialogText = "选择图像";
		dg.Theme = theme;
		dg.DisplayMode = FileDialog.DisplayModeEnum.Thumbnails;
		dg.ContentScaleFactor = 0.6f;
		dg.FileMode = FileDialog.FileModeEnum.OpenFile;
		dg.Access = FileDialog.AccessEnum.Filesystem;
		dg.CurrentPath = "C:/Users/Administrator/Pictures/";
		dg.Filters = ["*.png, *.jpg, *.jpeg, *.bmp, *.webp ; 图片文件"];
		dg.FileSelected += DgOnFileSelected;
		AddChild(dg);
		dg.PopupCentered();
	}

	private void DgOnFileSelected(string path)
	{
		if (!FileAccess.FileExists(path))
		{
			AcceptDialog ac = new AcceptDialog();
			ac.Theme = theme;
			ac.Title = "What's wrong with you?";
			ac.SetSize(new Vector2I(800,200));
			ac.DialogText = "路径错误";
			ac.OkButtonText = "关闭";
			ac.ContentScaleFactor = 0.6f;
			AddChild(ac);
			ac.PopupCentered();
			UpdateDetail();
			return;
		}
		ConfirmationDialog dialog = new ConfirmationDialog();
		dialog.Theme = theme;
		dialog.Title = "请选择";
		dialog.DialogText = "你要把图片存储在游戏内吗?";
		dialog.CancelButtonText = "不需要";
		dialog.OkButtonText = "当然";
		dialog.ContentScaleFactor = 0.6f;
		dialog.Canceled += () => { HandleChoice(path, false);};
		dialog.Confirmed += () => { HandleChoice(path, true); };
		AddChild(dialog);
		dialog.PopupCentered();

		void HandleChoice(string s, bool b)
		{
			if (b)
			{
				DirAccess.MakeDirRecursiveAbsolute("user://background_images/");
				DirAccess.RemoveAbsolute(DataManager.Instance.GetStringConfig(pathDataId));
				string localPath = $"user://background_images/{System.IO.Path.GetFileName(s)}";
				DirAccess.CopyAbsolute(s, localPath);
				GD.Print("图像存储到:" + localPath);
				DataManager.Instance.SetStringConfig(pathDataId, localPath);
			}
			else
			{
				DataManager.Instance.SetStringConfig(pathDataId, s);
			}

			UpdateDetail();
		}
	}

	private void OnResetButtonPressed()
	{
		DataManager.Instance.SetStringConfig(pathDataId, "");
		UpdateDetail();
	}

	private void OnAlphaSliderValueChanged(float v)
	{
		DataManager.Instance.SetFloatConfig(alphaDataId, v);
	}
	
	private void OnBlurSliderValueChanged(float v)
	{
		DataManager.Instance.SetFloatConfig(blurDataId, v);
	}
}
