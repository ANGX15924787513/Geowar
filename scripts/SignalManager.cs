using Godot;
using System;

public partial class SignalManager : Node
{
    [Signal] public delegate void OnCardOutEventHandler();
    [Signal] public delegate void OnBulletDestroyedEventHandler();
    [Signal] public delegate void OnPlayerHealthChangedEventHandler(int damage, int health, int maxHealth);
    [Signal] public delegate void OnEnemyGotHurtEventHandler(int damage,Vector2 pos);
    [Signal] public delegate void OnPlayerDiedEventHandler();
    [Signal] public delegate void ShowDeathScreenEventHandler();
    [Signal] public delegate void OnCollectedItemEventHandler(FallItem.FallItemType type);
    [Signal] public delegate void RequestCollectionSpawnEventHandler(Vector2 pos);
    [Signal] public delegate void OnEnemyDiedEventHandler();
}
