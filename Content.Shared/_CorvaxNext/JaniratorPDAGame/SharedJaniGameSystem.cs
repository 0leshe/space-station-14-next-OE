
using Robust.Shared.Timing;

namespace Content.Shared._CorvaxNext.JaniratorPDAGame;
public abstract class SharedJaniGameSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Тут кароче загрузку из базы данных аэаэа для сервера НЕ ЗАБЫТЬ ТВАРЬ
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

    public bool ResetLastAccount(Entity<JaniGameCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        card.Comp.LastPassiveAccount = _timing.CurTime;

        Dirty(card);

        return true;
    }

    public bool AddUpgrade(Entity<JaniGameCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        SubFromBalance(card, card.Comp.NextUpgradeCost);
        card.Comp.NextUpgradeCost = (uint)Math.Floor(card.Comp.NextUpgradeCost * 1.4);
        card.Comp.Upgrades += 1;

        Dirty(card);

        return true;
    }

    public Dictionary<string, uint>? GetTransactions(Entity<JaniGameCardComponent?> card)
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
