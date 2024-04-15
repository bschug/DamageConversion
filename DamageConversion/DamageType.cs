namespace DamageConversion;

public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Lightning,
    Chaos,
    Elemental,
}

public static class DamageTypeExtensions
{
    public static bool IsElemental(this DamageType type) => type switch
    {
        DamageType.Fire => true,
        DamageType.Cold => true,
        DamageType.Lightning => true,
        _ => false
    };
}
