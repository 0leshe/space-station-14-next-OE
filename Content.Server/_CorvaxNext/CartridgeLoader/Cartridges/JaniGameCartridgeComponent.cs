namespace Content.Server._CorvaxNext.CartridgeLoader.Cartridges;
[RegisterComponent, Access(typeof(JaniGameCartridgeSystem))]
public sealed partial class JaniGameCartridgeComponent : Component
{
    /// <summary>
    ///     The NanoChat card to keep track of.
    /// </summary>
    [DataField]
    public EntityUid? Card;
    /// <summary>
    ///     Station entity to keep track of.
    /// </summary>
    [DataField]
    public EntityUid? Station;
}
