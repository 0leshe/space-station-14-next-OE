using Content.Server.Administration.Logs;
using Content.Server.CartridgeLoader;
using Content.Server.Station.Systems;
using Content.Shared._CorvaxNext.CartridgeLoader.Cartridges;
using Content.Shared._CorvaxNext.JaniratorPDAGame;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared.PDA;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxNext.CartridgeLoader.Cartridges;
public sealed class JaniGameCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJaniGameSystem _janiGame = default!;
    [Dependency] private readonly NanoChatCartridgeSystem _nanoChat = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JaniGameCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<JaniGameCartridgeComponent, CartridgeMessageEvent>(OnMessage);
        SubscribeLocalEvent<JaniGameCartridgeComponent, MapInitEvent>(OnCardInit);
    }
    private void OnCardInit(Entity<JaniGameCartridgeComponent> ent, ref MapInitEvent args)
    {
        // Assign a random number
        ent.Comp.Coefficient = _random.NextFloat(0.5f, 2);
        Dirty(ent);
    }

    private void OnUiReady(Entity<JaniGameCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        _cartridge.RegisterBackgroundProgram(args.Loader, ent);
        UpdateUI(ent, args.Loader);
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
                if (newCard != currentCard)
                {
                    // Update card reference
                    janiGame.Card = newCard;
                }
            }
            if (janiGame.Card != null && TryComp<JaniGameCardComponent>(janiGame.Card, out var cardComp))
            {
                Entity<JaniGameCardComponent> card = (janiGame.Card.Value, cardComp); // idk, help me please
                if (cardComp.PassiveAccountActive && cardComp.NextPassiveAccount <= _timing.CurTime)
                {
                    _janiGame.AddToBalance((card, cardComp), CoeficentAdjust(janiGame, cardComp.Upgrades));
                    _janiGame.ResetLastAccount((card, cardComp));
                    Dirty(card);
                }
            }
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
                _janiGame.AddToBalance((card, card.Comp), CoeficentAdjust(ent.Comp, 1));
                break;
            case JaniGameUiMessageType.BuyUpgrade:
                if (card.Comp.Balance >= card.Comp.NextUpgradeCost)
                {
                    _janiGame.AddUpgrade((card, card.Comp));
                }
                break;
            case JaniGameUiMessageType.Transaction:
                if (TryComp<JaniGameCardComponent>(card, out var ownCardComp) &&
                        TryComp<IdCardComponent>(card, out var ownIdCard))
                {
                    var (deliveryFailed, reciver) = AttemptDelivery((card, ent), msg.ID);
                    _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(msg.Actor):user} attempted {(deliveryFailed ? " (FAILED)" : "")}transaction {msg.Amount.ToString()} to {ToPrettyString(reciver)}");
                    if (!deliveryFailed &&
                        TryComp<JaniGameCardComponent>(reciver.Owner, out var reciverCardComp) &&
                        TryComp<IdCardComponent>(reciver.Owner, out var reciverIdCard))
                    {
                        _janiGame.AddToBalance((reciver.Owner, reciverCardComp), msg.Amount);
                        _janiGame.SubFromBalance((card, ownCardComp), msg.Amount);
                        _janiGame.AddToHistory((reciver.Owner, reciverCardComp), new JaniGameHistory()
                        {
                            Amount = msg.Amount,
                            OwnerID = ownCardComp.ID,
                            ReciverID = reciverCardComp.ID,
                            From = reciverIdCard.FullName ?? "?",
                            To = ownIdCard.FullName ?? "?",
                        });
                        _janiGame.AddToHistory((card, ownCardComp), new JaniGameHistory()
                        {
                            Amount = msg.Amount,
                            OwnerID = ownCardComp.ID,
                            ReciverID = reciverCardComp.ID,
                            From = reciverIdCard.FullName ?? "?",
                            To = ownIdCard.FullName ?? "?",
                        });
                        Dirty((Entity<JaniGameCardComponent>)reciver.Owner!);
                        Dirty(card);
                    }
                }
                break;
        }
        UpdateUI(ent, GetEntity(args.LoaderUid));
    }

    private (bool failed, Entity<JaniGameCardComponent>) AttemptDelivery(
        Entity<JaniGameCartridgeComponent> sender,
        uint reciverID)
    {
        var foundRecipients = new List<Entity<JaniGameCardComponent>>();

        // Find all cards with matching number
        var cardQuery = EntityQueryEnumerator<JaniGameCardComponent>();
        while (cardQuery.MoveNext(out var cardUid, out var card))
        {
            if (card.ID != reciverID)
                continue;

            foundRecipients.Add((cardUid, card));
        }

        if (foundRecipients.Count == 0)
            return (true, new());
        else
            return (false, foundRecipients[0]);
    }
    private void UpdateUI(Entity<JaniGameCartridgeComponent> ent, EntityUid loader)
    {
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        List<JaniGameHistory> transactionHistory = new();
        uint nextUpgradeCost = 0;
        uint balance = 0;
        uint upgrades = 0;
        bool passiveAccountActive = false;

        if (ent.Comp.Card != null && TryComp<JaniGameCardComponent>(ent.Comp.Card, out var cardComp))
        {
            transactionHistory = cardComp.TransactionHistory;
            nextUpgradeCost = cardComp.NextUpgradeCost;
            balance = cardComp.Balance;
            upgrades = cardComp.Upgrades;
            passiveAccountActive = cardComp.PassiveAccountActive;
        }

        var state = new JaniGameUiState(transactionHistory,
            nextUpgradeCost,
            balance,
            upgrades,
            passiveAccountActive);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }

    private static uint CoeficentAdjust(JaniGameCartridgeComponent ent, uint num)
    {
        return (uint)Math.Floor(num * ent.Coefficient);
    }
}
