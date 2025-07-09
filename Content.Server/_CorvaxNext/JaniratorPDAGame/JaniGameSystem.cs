using Content.Server.Access.Systems;
using Content.Server.Kitchen.Components;
using Content.Server.NameIdentifier;
using Content.Shared._CorvaxNext.JaniratorPDAGame;
using Content.Shared.NameIdentifier;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Text;

namespace Content.Server._CorvaxNext.JaniratorPDAGame;
public sealed class JaniGameSystem : SharedJaniGameSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    private static readonly Random rand = new();
    [Dependency] private readonly NameIdentifierSystem _name = default!;

    private readonly ProtoId<NameIdentifierGroupPrototype> _nameIdentifierGroup = "NanoChat";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JaniGameCardComponent, MapInitEvent>(OnCardInit);
        SubscribeLocalEvent<JaniGameCardComponent, BeingMicrowavedEvent>(OnMicrowaved, after: [typeof(IdCardSystem)]);
    }

    private void OnCardInit(Entity<JaniGameCardComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.ID != null)
            return;

        // Assign a random number
        _name.GenerateUniqueName(ent, _nameIdentifierGroup, out int number);
        ent.Comp.ID = (uint)number;
        Dirty(ent);
    }

    private void OnMicrowaved(Entity<JaniGameCardComponent> ent, ref BeingMicrowavedEvent args)
    {
        // Skip if the entity was deleted (e.g., by ID card system burning it)
        if (Deleted(ent))
            return;

        if (!TryComp<MicrowaveComponent>(args.Microwave, out var micro) || micro.Broken)
            return;

        var randomPick = _random.NextFloat();

        // Super lucky - erase all messages (10% chance)
        if (randomPick <= 0.10f)
        {
            ent.Comp.Balance = ReplaceDigitsWithRandom(ent.Comp.Balance);
        }
        else
        {
            ent.Comp.Balance = ReplaceDigitsWithRandom(ent.Comp.Balance, 1);
        }

        Dirty(ent);
    }
    private static uint ReplaceDigitsWithRandom(uint input, int forceNum = -1)
    {
        // Конвертация числа в строку
        string original = input.ToString();
        StringBuilder builder = new(original.Length);

        // Проход по каждому символу и замена
        if (forceNum < 0)
        {
            for (int i = 0; i < original.Length; i++)
            {
                char randomDigit = rand.Next(0, 9).ToString()[0];
                builder.Append(randomDigit);
            }
        }
        else
        {
            string forceNumStr = forceNum.ToString();
            for (int i = 0; i < original.Length; i++)
            {
                builder.Append(forceNumStr);
            }
        }

        // Обратное преобразование в uint
        return uint.Parse(builder.ToString());
    }
}
