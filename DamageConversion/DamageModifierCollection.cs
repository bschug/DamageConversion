namespace DamageConversion;

/// <summary>
/// Collection of all Added / Increased / More damage modifiers for each damage type.
/// See the wiki for how these must be combined.
/// </summary>
public record DamageModifierCollection(
    DamageModifier Physical,
    DamageModifier Fire,
    DamageModifier Cold,
    DamageModifier Lightning,
    DamageModifier Chaos,
    DamageModifier Elemental)
{
    public static readonly DamageModifierCollection Empty = new DamageModifierCollection(
        new(), new(), new(), new(), new(), new());

    public DamageModifier GetModifierForType(DamageType type) => type switch
    {
        DamageType.Physical => Physical,
        DamageType.Fire => Fire,
        DamageType.Cold => Cold,
        DamageType.Lightning => Lightning,
        DamageType.Chaos => Chaos,
        DamageType.Elemental => Elemental,
        _ => throw new ArgumentException()
    };

    private DamageModifierCollection WithModifierForType(DamageType type, DamageModifier modifier) => type switch
    {
        DamageType.Physical => this with { Physical = modifier },
        DamageType.Fire => this with { Fire = modifier },
        DamageType.Cold => this with { Cold = modifier },
        DamageType.Lightning => this with { Lightning = modifier },
        DamageType.Chaos => this with { Chaos = modifier },
        DamageType.Elemental => this with { Elemental = modifier },
        _ => throw new ArgumentException()
    };

    public DamageModifierCollection WithAdded(float amount, DamageType type)
    {
        var mod = GetModifierForType(type);
        return this.WithModifierForType(type, mod with { Added = mod.Added + amount });
    }

    public DamageModifierCollection WithIncreased(float amount, DamageType type)
    {
        var mod = GetModifierForType(type);
        return this.WithModifierForType(type, mod with { Increased = mod.Increased + amount });
    }

    public DamageModifierCollection WithMore(float amount, DamageType type)
    {
        var mod = GetModifierForType(type);
        return this.WithModifierForType(type, mod with { More = mod.More * (1f + amount) });
    }
}
