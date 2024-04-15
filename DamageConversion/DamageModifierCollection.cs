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

    public DamageModifierCollection WithAdded(float amount, DamageType type)
    {
        // TODO Implement this
        return this;
    }

    public DamageModifierCollection WithIncreased(float amount, DamageType type)
    {
        // TODO Implement this
        return this;
    }

    public DamageModifierCollection WithMore(float amount, DamageType type)
    {
        // TODO Implement this
        return this;
    }
}
