using Godot;

public partial class CccardLockHandler : Control
{
	private Button button;

	public override void _Ready()
	{
		button = GetNode<Button>("charChooseCard/Button");
		if (Name == "charChooseCard2" && DataManager.Instance.GetUpgradeLevel("line") != 0 ||
		Name == "charChooseCard3" && DataManager.Instance.GetUpgradeLevel("polygon") != 0 ||
		Name == "charChooseCard")
		{
			button.Disabled = false;
		}
		else
		{
			button.Disabled = true;
		}
		if (button.Disabled)
		{
			GetNode<Label>("charChooseCard/descriptionLabel").Text += "\n-在商店中解锁-";
		}
	}
}
