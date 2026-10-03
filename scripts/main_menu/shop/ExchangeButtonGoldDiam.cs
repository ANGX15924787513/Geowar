using Godot;

public partial class ExchangeButtonGoldDiam : Button
{
    [Export] public LineEdit line_edit;
    [Export] public Label diamond_label;

    [Signal]
    public delegate void on_gold_diamEventHandler(int count);

    private void _on_pressed()
    {
        if (DataManager.Instance.playerData.GoldCurrency < int.Parse(line_edit.Text) * 10)
        {
            (diamond_label as LabelShake)?.ShowNotEnough();
            return;
        }
        EmitSignal(SignalName.on_gold_diam, int.Parse(line_edit.Text));
    }
}