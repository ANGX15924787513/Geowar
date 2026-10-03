using Godot;

public partial class AutoAimCheckBox : CheckBox
{
	public override void _Ready()
	{
		ButtonPressed = DataManager.Instance.GetBoolConfig("auto_aim");
	}

	private void _on_toggled(bool toggled_on)
	{
		DataManager.Instance.SetBoolConfig("auto_aim", toggled_on);
	}
}
