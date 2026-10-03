using Godot;

public partial class LineDestroy : Line2D
{
	public void destroy()
	{
		QueueFree();
	}
}
