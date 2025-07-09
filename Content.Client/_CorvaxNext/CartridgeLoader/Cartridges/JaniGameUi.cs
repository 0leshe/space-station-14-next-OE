using Content.Client.UserInterface.Fragments;
using Content.Shared._CorvaxNext.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;
namespace Content.Client._CorvaxNext.CartridgeLoader.Cartridges;
public sealed partial class JaniGameUi : UIFragment
{
    private JaniGameUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new JaniGameUiFragment();

        _fragment.OnMessageSent += (type, id, amount) =>
        {
            SendUiMessage(type, id, amount, userInterface);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is JaniGameUiState cast)
            _fragment?.UpdateState(cast);
    }

    private static void SendUiMessage(JaniGameUiMessageType type,
        uint id,
        uint amount,
        BoundUserInterface userInterface)
    {
        var message = new CartridgeUiMessage(new JaniGameUiMessageEvent(type, id, amount));
        userInterface.SendMessage(message);
    }
}
