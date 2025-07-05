using Robust.Shared.Serialization;
namespace Content.Shared._CorvaxNext.CartridgeLoader.Cartridges;
[Serializable, NetSerializable]
public sealed class JaniGameUiState(
    Dictionary<string, uint> transactionHistory,
    uint nextUpgradeCost,
    uint balance,
    uint upgrades,
    uint leaderBoardPlace,
    bool passiveAccountActive)
    : BoundUserInterfaceState
{
    public readonly Dictionary<string, uint> TransactionHistory = transactionHistory;
    public readonly uint NextUpgradeCost = nextUpgradeCost;
    public readonly uint Balance = balance;
    public readonly uint Upgrades = upgrades;
    public readonly uint LeaderBoardPlace = leaderBoardPlace;
    public readonly bool PassiveAccountActive = passiveAccountActive;
}
