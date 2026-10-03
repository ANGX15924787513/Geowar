using Godot;

public partial class Fps : SpinBox
{
	public override void _Ready()
	{
		int saved = DataManager.Instance.IsExistsSetting("fps")
			? DataManager.Instance.GetIntConfig("fps")
			: (int)DisplayServer.ScreenGetRefreshRate();
		Engine.MaxFps = saved >= 20 ? saved : -1;
		Value = saved >= 20 ? saved : 0;
	}

	private void _on_value_changed(double value)
	{
		int fps = (int)value;
		Engine.MaxFps = fps;
		DataManager.Instance.SetIntConfig("fps", fps);
	}
}
