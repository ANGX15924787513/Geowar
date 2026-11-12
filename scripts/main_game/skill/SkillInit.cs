using Godot;

public partial class SkillInit : CanvasLayer
{
	[Export] private PackedScene separateSkillScene;
	private Node lib;

	private static readonly SingleSkill[] skills = [
	new() {
		id = "skill_tnt", name = "TNT", desc = "比核弹强一点", cooldown = 90f,
		iconPath = "res://texture/MCTexture/minecart_tnt.png" },
	new() {
		id = "skill_collect_all_coins", name = "旋风吸入", desc = "捐需~", cooldown = 30f,
		iconPath = "res://texture/MCTexture/hopper.png" ,boardColor = Colors.Burlywood},
	new() {
		id = "skill_invincible", name = "抗性提升", desc = "你就是上帝", cooldown = 60f,
		iconPath = "res://texture/MCTexture/resistance.png" ,boardColor = Colors.DarkOrange},
	new() {
		id = "skill_super_bullet", name = "超级弹夹", desc = "你的弹夹没我大", cooldown = 45f,
		iconPath = "res://texture/MCTexture/arrow.png" ,boardColor = Colors.LightSlateGray},
	];

	public override void _Ready()
	{
		lib = GetNode<Node>("VContainer");
		
		//初始化所有技能
		foreach (SingleSkill skill in skills)
		{
			if (DataManager.Instance.GetUpgradeLevel(skill.id) == 1)
			{
				AddSkillBox(skill);
			}
		}
	}

	private void AddSkillBox(SingleSkill _skill)
	{
		SkillBehaviour box = separateSkillScene.Instantiate<SkillBehaviour>();
		box.Setup(_skill);
		lib.AddChild(box);
	}
}
