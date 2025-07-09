using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


namespace Content.Shared._CorvaxNext.JaniratorPDAGame;
[RegisterComponent, NetworkedComponent, Access(typeof(SharedJaniGameSystem))]
[AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class JaniGameCardComponent : Component
{
    /// <summary>
    ///     The number assigned to this card.
    /// </summary>
    [DataField]
    public uint Balance = 0;

    /// <summary>
    ///     All messages stored on this card, keyed by recipient number.
    /// </summary>
    [DataField]
    public List<JaniGameHistory> TransactionHistory = new();

    [DataField]
    public uint MaxHistoryCount = 20;

    [DataField]
    public uint? ID;

    /// <summary>
    ///     The currently selected chat recipient number.
    /// </summary>
    [DataField]
    public uint Upgrades = 0;

    /// <summary>
    ///     The maximum amount of recipients this card supports.
    /// </summary>
    [DataField]
    public uint NextUpgradeCost = 20;

    /// <summary>
    ///     Last time a message was sent, for rate limiting.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextPassiveAccount = TimeSpan.FromSeconds(2, 500); // TODO: actually use this, compare against actor and not the card

    /// <summary>
    ///     Whether to send notifications.
    /// </summary>
    [DataField]
    public bool PassiveAccountActive;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan PassiveAccountTime;

    [DataField, AutoNetworkedField]
    public bool CartDefeated;
}

[Serializable, NetSerializable]
public struct JaniGameHistory
{
    public string From;
    public string To;
    public uint Amount;
    public uint? ReciverID;
    public uint? OwnerID;
}
