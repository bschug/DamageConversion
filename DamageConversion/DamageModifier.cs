namespace DamageConversion;

/// <summary>
/// Modifier that can be applied to a damage instance.
/// See the wiki for explanation of the different modifier types.
/// </summary>
public record DamageModifier(
    float Added = 0,
    float Increased = 0,
    float More = 1);
