using Content.Server.Administration.Logs;
using Content.Server.CartridgeLoader;
using Content.Server.Station.Systems;
using Content.Shared._CorvaxNext.CartridgeLoader.Cartridges;
using Content.Shared._CorvaxNext.JaniratorPDAGame;
using Content.Shared.CartridgeLoader;
using Content.Shared.PDA;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxNext.CartridgeLoader.Cartridges;
public sealed class JaniGameCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJaniGameSystem _janiGame = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        //   SubscribeLocalEvent<JaniGameCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<JaniGameCartridgeComponent, CartridgeMessageEvent>(OnMessage);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Update card references for any cartridges that need it
        var query = EntityQueryEnumerator<JaniGameCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var janiGame, out var cartridge))
        {

            if (cartridge.LoaderUid == null)
                continue;

            // Check if we need to update our card reference
            if (TryComp<PdaComponent>(cartridge.LoaderUid, out var pda))
            {

                var newCard = pda.ContainedId;
                var currentCard = janiGame.Card;

                // If the cards match, nothing to do
                if (newCard == currentCard)
                {
                    // Update card reference
                    janiGame.Card = newCard;

                }
            }

            UpdateUI((uid, janiGame), cartridge.LoaderUid.Value);
        }

    }

    /// <summary>
    ///     Gets the ID card entity associated with a PDA.
    /// </summary>
    /// <param name="loaderUid">The PDA entity ID</param>
    /// <param name="card">Output parameter containing the found card entity and component</param>
    /// <returns>True if a valid NanoChat card was found</returns>
    private bool GetCardEntity(
        EntityUid loaderUid,
        out Entity<JaniGameCardComponent> card)
    {
        card = default;

        // Get the PDA and check if it has an ID card
        if (!TryComp<PdaComponent>(loaderUid, out var pda) ||
            pda.ContainedId == null ||
            !TryComp<JaniGameCardComponent>(pda.ContainedId, out var idCard))
            return false;

        card = (pda.ContainedId.Value, idCard);
        return true;
    }
    private void OnMessage(Entity<JaniGameCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not JaniGameUiMessageEvent msg)
            return;

        if (!GetCardEntity(GetEntity(args.LoaderUid), out var card))
            return;

        switch (msg.Type)
        {
            case JaniGameUiMessageType.Click:
                _janiGame.AddToBalance((card, card.Comp), 1);
                break;
            case JaniGameUiMessageType.BuyUpgrade:
                if (card.Comp.Balance >= card.Comp.NextUpgradeCost)
                {
                    _janiGame.AddUpgrade((card, card.Comp));
                }
                else
                {
                    // 123123ferfgreAAAA
                }
                break;
            case JaniGameUiMessageType.Transaction:
                //AAAAAAAAAAAAAAAAAA
                break;
        }
        UpdateUI(ent, GetEntity(args.LoaderUid));
    }
    private void UpdateUI(Entity<JaniGameCartridgeComponent> ent, EntityUid loader)
    {
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        Dictionary<string, uint> transactionHistory = [];
        uint nextUpgradeCost = 0;
        uint balance = 0;
        uint upgrades = 0;
        uint leaderBoardPlace = 0;
        bool passiveAccountActive = false;

        if (ent.Comp.Card != null && TryComp<JaniGameCardComponent>(ent.Comp.Card, out var cardComp))
        {
            transactionHistory = cardComp.TransactionHistory;
            nextUpgradeCost = cardComp.NextUpgradeCost;
            balance = cardComp.Balance ?? 0;
            upgrades = cardComp.Upgrades;
            leaderBoardPlace = cardComp.LeaderBoardPlace;
            passiveAccountActive = cardComp.PassiveAccountActive;

            Entity<JaniGameCardComponent> card = (ent.Comp.Card.Value, cardComp);
            if (cardComp.PassiveAccountActive && cardComp.LastPassiveAccount + cardComp.PassiveAccountTime <= _timing.CurTime)
            {
                _janiGame.AddToBalance((card, card.Comp), cardComp.Upgrades);
                _janiGame.ResetLastAccount((card, card.Comp));
            }
        }

        var state = new JaniGameUiState(transactionHistory,
            nextUpgradeCost,
            balance,
            upgrades,
            leaderBoardPlace,
            passiveAccountActive);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }
}
