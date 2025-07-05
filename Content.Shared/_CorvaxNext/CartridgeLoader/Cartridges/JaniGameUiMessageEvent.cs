

using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxNext.CartridgeLoader.Cartridges;
[Serializable, NetSerializable]
public sealed class JaniGameUiMessageEvent(
    JaniGameUiMessageType type,
    uint ncNumber,
    uint amount
    ) : CartridgeMessageEvent
{
    /// <summary>
    ///     The type of UI message being sent.
    /// </summary>
    public readonly JaniGameUiMessageType Type = type;

    public readonly uint NCNumber = ncNumber;

    public readonly uint Amount = amount;
}

public enum JaniGameUiMessageType : byte
{
    Click,
    BuyUpgrade,
    Transaction
}
