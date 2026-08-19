using Godot;
using System;

public partial class AdditionSkillManager : CanvasLayer
{
	[Export] private PackedScene separateSkillScene;
	private Node lib;

	private static readonly Skill[] skills = [
	new() {id = "skill_tnt",name = "TNT",desc = "比核弹强一点",cooldown = 3f,iconPath = "res://texture/MCTexture/minecart_tnt.png"},
	];

	public override void _Ready()
	{
		lib = GetNode<Node>("VContainer");
		
		//初始化所有技能
		foreach (Skill skill in skills)
		{
			if (DataManager.Instance.GetUpgradeLevel(skill.id) == 1)
			{
				AddSkillBox(skill);
			}
		}
	}

	private void AddSkillBox(Skill _skill)
	{
		SeparateSkill box = separateSkillScene.Instantiate<SeparateSkill>();
		box.Setup(_skill);
		lib.AddChild(box);
	}
}
