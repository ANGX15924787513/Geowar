using Godot;
using System;

public partial class SignalManager : Node
{
    [Signal] public delegate void OnCardOutEventHandler();
    [Signal] public delegate void OnBulletDestroyedEventHandler();
    [Signal] public delegate void OnPlayerDiedEventHandler();
    [Signal] public delegate void ShowDeathScreenEventHandler();
}
