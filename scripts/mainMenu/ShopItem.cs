using Godot;

/// <summary> 商店商品定义 </summary>
public partial class ShopItem : Resource
{
	public string Id;          // 唯一标识，用于持久化
	public string Name;        // 显示名称
	public string Desc;        // 描述
	public int Price;          // 价格
	public int MaxLevel = 99;  // 最大购买等级
}
