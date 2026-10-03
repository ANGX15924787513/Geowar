using Godot;

public partial class ExchangeButtonDiamEmerald : Button
{
    [Export] public LineEdit line_edit;
    [Export] public Label emer_label;

    [Signal]
    public delegate void on_diam_emeraldEventHandler(int count);

    private void _on_pressed()
    {
        if (DataManager.Instance.playerData.DiamondCurrency < int.Parse(line_edit.Text) * 10)
        {
            (emer_label as LabelShake)?.ShowNotEnough();
            return;
        }
        EmitSignal(SignalName.on_diam_emerald, int.Parse(line_edit.Text));
    }
}