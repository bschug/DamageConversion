namespace DamageConversion;

public class CharacterDamageData
{
    /// <summary>
    /// Base damage of the character, e.g. from equipped weapon or spell.
    /// </summary>
    public Damage BaseDamage { get; set; }

    public DamageModifierCollection DamageModifiers { get; set; } = DamageModifierCollection.Empty;

    /// <summary>
    /// Damage conversions from skills.
    /// These are stored separately because they have precedence over conversions from other sources.
    /// </summary>
    public DamageConversions DamageConvertedFromSkills { get; set; } = new();

    /// <summary>
    /// Damage conversions from other sources like the Passive Tree or gear.
    /// </summary>
    public DamageConversions DamageConverted { get; set; } = new();

    /// <summary>
    /// Any "X% of Y Damage Added as Z Damage" the character has from any sources, including skills, passives and gear.
    /// If the character has multiple sources of the same element, this contains the sum of all sources.
    /// </summary>
    public DamageConversions ExtraDamageAs { get; set; } = new();

    /// <summary>
    /// The sum of any "Elemental Damage as Extra Chaos Damage" on the character.
    /// </summary>
    public float ElementalAsExtraChaosDamage = 0;

    /// <summary>
    /// The sum of any "Non-Chaos Damage as Extra Chaos Damage" on the character
    /// </summary>
    public float NonChaosAsExtraChaosDamage = 0;

    public CharacterDamageData(Damage baseDamage)
    {
        BaseDamage = baseDamage;
    }

    public Damage CalculateDamage()
    {
        throw new NotImplementedException("This is what you need to implement");
    }
}
