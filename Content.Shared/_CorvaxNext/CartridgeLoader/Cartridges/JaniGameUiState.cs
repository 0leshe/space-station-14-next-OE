using Content.Shared._CorvaxNext.JaniratorPDAGame;
using Robust.Shared.Serialization;
namespace Content.Shared._CorvaxNext.CartridgeLoader.Cartridges;
[Serializable, NetSerializable]
public sealed class JaniGameUiState(
    List<JaniGameHistory> transactionHistory,
    uint nextUpgradeCost,
    uint balance,
    uint upgrades,
    bool passiveAccountActive)
    : BoundUserInterfaceState
{
    public readonly List<JaniGameHistory> TransactionHistory = transactionHistory;
    public readonly uint NextUpgradeCost = nextUpgradeCost;
    public readonly uint Balance = balance;
    public readonly uint Upgrades = upgrades;
    public readonly bool PassiveAccountActive = passiveAccountActive;
}
