using Content.Shared.Examine;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._CorvaxNext.JaniratorPDAGame;
public abstract class SharedJaniGameSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JaniGameCardComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<JaniGameCardComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("janigame-card-examine-number", ("balance", $"{ent.Comp.Balance}")));
    }
    public bool AddToBalance(Entity<JaniGameCardComponent?> card, uint value)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        card.Comp.Balance += value;

        Dirty(card);

        return true;
    }
    public bool SubFromBalance(Entity<JaniGameCardComponent?> card, uint value)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        card.Comp.Balance -= value;

        Dirty(card);

        return true;
    }

    public bool AddToHistory(Entity<JaniGameCardComponent?> card, JaniGameHistory history)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        if (card.Comp.MaxHistoryCount >= card.Comp.TransactionHistory.Count)
        {
            card.Comp.TransactionHistory.Remove(card.Comp.TransactionHistory.Last());
        }
        card.Comp.TransactionHistory.Add(history);

        Dirty(card);

        return true;
    }

    public bool ResetLastAccount(Entity<JaniGameCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        card.Comp.NextPassiveAccount = _timing.CurTime + card.Comp.PassiveAccountTime;

        Dirty(card);

        return true;
    }

    public bool AddUpgrade(Entity<JaniGameCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        SubFromBalance(card, card.Comp.NextUpgradeCost);
        card.Comp.NextUpgradeCost = (uint)Math.Floor(card.Comp.NextUpgradeCost * 1.3);
        card.Comp.Upgrades += 1;

        Dirty(card);

        return true;
    }

    public List<JaniGameHistory>? GetTransactions(Entity<JaniGameCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.TransactionHistory;
    }
    public uint? GetNextUpgradeCost(Entity<JaniGameCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.NextUpgradeCost;
    }
}
