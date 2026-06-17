using Godot;
using System;
public partial class MyButton : Node2D
{
	[Export] private AudioStream switchSound;
	private AudioStreamPlayer audioPlayer;
	private Node transition;
	[Export] PackedScene switch_scene;
	[Export] AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		transition = GetNode("/root/EasyTransition");
	}

	private void _on_exit_button_pressed()
	{
		audioPlayer.Stream = switchSound;
		audioPlayer.Play();
		GetTree().Quit();
	}
	
	private void _on_start_button_pressed()
	{
		audioPlayer.Stream = switchSound;
		audioPlayer.Play();
		transition.Call("transition_to",switch_scene.ResourcePath,1f,0,Colors.White);
		animationPlayer.Play("fade_out");
		/*func transition_to(
			path: String,
			duration: float = 0.5,
			animation: TransitionAnim = TransitionAnim.FADE,
			color: Color = Color.BLACK,
			mask_texture: Texture2D = null,
			dither: bool = false,
			dither_intensity: float = 0.5,
			dither_scale: float = 2.0,
		) -> void:
		
		0	FADE	淡入淡出	屏幕逐渐变黑/变亮
		1	WIPE_LINEAR	线性擦除	一条直线方向扫过屏幕
		2	WIPE_RADIAL	径向擦除	从中心向外圆形扫除
		3	WIPE_DIAGONAL	对角线擦除	沿对角线方向扫过屏幕
		4	DUAL_WIPE_LINEAR	双线性擦除	从中间向上下/左右同时扫除
		5	DUAL_WIPE_RADIAL	双径向擦除	从中心向内外同时扩散
		6	DUAL_WIPE_DIAGONAL	双对角线擦除	从中心向两个对角线方向扫除
		7	BLUR	模糊	画面逐渐模糊消失
		8	CIRCLE_CENTER_EXPAND	圆形中心展开	从中心圆点向外扩大显示
		9	CIRCLE_CENTER_COLLAPSE	圆形中心收缩	从外向中心圆点收缩
		10	TEXTURE_CENTER_EXPAND	纹理中心展开	使用自定义纹理从中心展开
		11	TEXTURE_CENTER_COLLAPSE	纹理中心收缩	使用自定义纹理向中心收缩
		12	CURTAIN	幕布效果	像幕布一样向两侧拉开
		13	WAVE	波浪效果	波浪形状擦除
		14	SPIRAL	螺旋效果	螺旋形旋转擦除
		15	TEXTURE_LUMINANCE	纹理亮度	根据纹理亮度阈值进行过渡
		 */
	}
}
