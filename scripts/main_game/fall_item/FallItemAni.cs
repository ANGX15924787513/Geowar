using Godot;

public partial class FallItemAni : AnimationPlayer
{
	public void shake()
	{
		Play("fallItemAni/up_down_shake");
	}
}
