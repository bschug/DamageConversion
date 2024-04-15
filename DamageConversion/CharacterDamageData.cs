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
        var noExtraTypes = Array.Empty<DamageType>();

        // At first glance, the conversion formula looks like a simple five step process of
        // Physical -> Lightning -> Cold -> Fire -> Chaos.
        // However, it becomes vastly more complicated due to how the Increased / More damage modifiers must be applied.
        // Any portion of damage post-conversion must have the multipliers for any element it had assumed at any point
        // of the conversion pipeline applied exactly once. This includes the generic Elemental multipliers, which
        // must also apply only once, even if the damage has gone through multiple elemental types.
        //
        // This means we need to instead build a conversion tree and at each leaf of the tree, check which elements
        // we have gone through to reach it and apply the respective modifiers. Then we add up all the leaves to get
        // the final result.
        return Damage.Empty
               + ConvertPhysicalDamage(BaseDamage.Physical + DamageModifiers.Physical.Added, noExtraTypes)
               + ConvertFireDamage(BaseDamage.Fire + DamageModifiers.Fire.Added, noExtraTypes)
               + ConvertColdDamage(BaseDamage.Cold + DamageModifiers.Cold.Added, noExtraTypes)
               + ConvertLightningDamage(BaseDamage.Lightning + DamageModifiers.Lightning.Added, noExtraTypes)
               + ConvertChaosDamage(BaseDamage.Chaos + DamageModifiers.Chaos.Added, noExtraTypes);
    }

    Damage ConvertPhysicalDamage(float physicalDamage, DamageType[] additionalDamageTypes)
    {
        // If we convert more than 100% via skills, we need to scale all conversion from this source down to reach
        // a total of 100%. E.g. 75% Phys -> Fire and 50% Phys -> Lightning becomes 66% Fire and 33% Lightning.

        float totalFromSkills = DamageConvertedFromSkills.TotalPhysicalDamageConverted;
        float skillScaleDown = totalFromSkills > 1 ? 1 / totalFromSkills : 1f;
        float remainingAfterSkills = MathF.Max(0, 1 - totalFromSkills);

        // Conversion from gear is scaled down the same if needed, but will not affect conversion from skills.
        // If 70% is converted by skills and 50% by gear, then gear will be scaled down to a total of 30%.
        // If over 100% is converted by skills, gear conversion has no effect.

        float totalFromGear = DamageConverted.TotalPhysicalDamageConverted;
        float gearScaleDown = totalFromGear > remainingAfterSkills
            ? remainingAfterSkills / totalFromGear
            : remainingAfterSkills;

        float totalConverted = (totalFromSkills * skillScaleDown) + (totalFromGear * gearScaleDown);

        var fire =
            physicalDamage * DamageConvertedFromSkills.PhysicalToFire * skillScaleDown
            + physicalDamage * DamageConverted.PhysicalToFire * gearScaleDown
            + physicalDamage * ExtraDamageAs.PhysicalToFire;

        var cold =
            physicalDamage * DamageConvertedFromSkills.PhysicalToCold * skillScaleDown
            + physicalDamage * DamageConverted.PhysicalToCold * gearScaleDown
            + physicalDamage * ExtraDamageAs.PhysicalToCold;

        var lightning =
            physicalDamage * DamageConvertedFromSkills.PhysicalToLightning * skillScaleDown
            + physicalDamage * DamageConverted.PhysicalToLightning * gearScaleDown
            + physicalDamage * ExtraDamageAs.PhysicalToLightning;

        var chaos =
            physicalDamage * DamageConvertedFromSkills.PhysicalToChaos * skillScaleDown
            + physicalDamage * DamageConverted.PhysicalToChaos * gearScaleDown
            + physicalDamage * ExtraDamageAs.PhysicalToChaos
            + physicalDamage * NonChaosAsExtraChaosDamage;

        // Remove converted portions from physical damage and apply damage modifiers to the remaining one.

        var remainingDamage = physicalDamage * (1 - totalConverted);
        var applicableDamageTypes = additionalDamageTypes.Append(DamageType.Physical).Distinct().ToArray();
        var scaledPhysicalDamage = ScaleDamage(remainingDamage, applicableDamageTypes);
        var damage = Damage.OfType(scaledPhysicalDamage, DamageType.Physical);

        // Damage converted to other types may undergo further conversions.
        // Invoke their conversion methods and tell them to apply Physical modifiers as well,
        // then add the outcome to our total.

        if (fire > 0)
        {
            damage += ConvertFireDamage(fire, applicableDamageTypes);
        }

        if (cold > 0)
        {
            damage += ConvertColdDamage(cold, applicableDamageTypes);
        }

        if (lightning > 0)
        {
            damage += ConvertLightningDamage(lightning, applicableDamageTypes);
        }

        if (chaos > 0)
        {
            damage += ConvertChaosDamage(chaos, applicableDamageTypes);
        }

        return damage;
    }

    Damage ConvertFireDamage(float fireDamage, DamageType[] additionalDamageTypes)
    {
        var totalConverted = DamageConvertedFromSkills.FireToChaos + DamageConverted.FireToChaos;
        totalConverted = MathF.Min(1, totalConverted);

        var chaos =
            fireDamage * totalConverted
            + fireDamage * ExtraDamageAs.FireToChaos
            + fireDamage * NonChaosAsExtraChaosDamage
            + fireDamage * ElementalAsExtraChaosDamage;

        var remainingDamage = fireDamage * (1f - totalConverted);
        var applicableDamageTypes = additionalDamageTypes.Append(DamageType.Fire).Distinct().ToArray();
        var scaledFireDamage = ScaleDamage(remainingDamage, applicableDamageTypes);
        var damage = Damage.OfType(scaledFireDamage, DamageType.Fire);

        if (chaos > 0)
        {
            damage += ConvertChaosDamage(chaos, applicableDamageTypes);
        }

        return damage;
    }

    Damage ConvertColdDamage(float coldDamage, DamageType[] additionalDamageTypes)
    {
        float totalFromSkills = DamageConvertedFromSkills.TotalColdDamageConverted;
        float skillScaleDown = totalFromSkills > 1 ? 1 / totalFromSkills : 1f;
        float remainingAfterSkills = MathF.Max(0, 1 - totalFromSkills);

        float totalFromGear = DamageConverted.TotalColdDamageConverted;
        float gearScaleDown = totalFromGear > remainingAfterSkills
            ? remainingAfterSkills / totalFromGear
            : remainingAfterSkills;

        float totalConverted = (totalFromSkills * skillScaleDown) + (totalFromGear * gearScaleDown);

        var fire =
            coldDamage * DamageConvertedFromSkills.ColdToFire * skillScaleDown
            + coldDamage * DamageConverted.ColdToFire * gearScaleDown
            + coldDamage * ExtraDamageAs.ColdToFire;

        var chaos =
            coldDamage * DamageConvertedFromSkills.ColdToChaos * skillScaleDown
            + coldDamage * DamageConverted.ColdToChaos * gearScaleDown
            + coldDamage * ExtraDamageAs.ColdToChaos
            + coldDamage * NonChaosAsExtraChaosDamage
            + coldDamage * ElementalAsExtraChaosDamage;

        var remainingDamage = coldDamage * (1f - totalConverted);
        var applicableDamageTypes = additionalDamageTypes.Append(DamageType.Cold).Distinct().ToArray();
        var scaledColdDamage = ScaleDamage(remainingDamage, applicableDamageTypes);
        var damage = Damage.OfType(scaledColdDamage, DamageType.Cold);

        if (fire > 0)
        {
            damage += ConvertFireDamage(fire, applicableDamageTypes);
        }

        if (chaos > 0)
        {
            damage += ConvertChaosDamage(chaos, applicableDamageTypes);
        }

        return damage;
    }

    Damage ConvertLightningDamage(float lightningDamage, DamageType[] additionalDamageTypes)
    {
        float totalFromSkills = DamageConvertedFromSkills.TotalLightningDamageConverted;
        float skillScaleDown = totalFromSkills > 1 ? 1 / totalFromSkills : 1f;
        float remainingAfterSkills = MathF.Max(0, 1 - totalFromSkills);

        float totalFromGear = DamageConverted.TotalLightningDamageConverted;
        float gearScaleDown = totalFromGear > remainingAfterSkills
            ? remainingAfterSkills / totalFromGear
            : remainingAfterSkills;

        float totalConverted = (totalFromSkills * skillScaleDown) + (totalFromGear * gearScaleDown);

        var fire =
            lightningDamage * DamageConvertedFromSkills.LightningToFire * skillScaleDown
            + lightningDamage * DamageConverted.LightningToFire * gearScaleDown
            + lightningDamage * ExtraDamageAs.LightningToFire;

        var cold =
            lightningDamage * DamageConvertedFromSkills.LightningToCold * skillScaleDown
            + lightningDamage * DamageConverted.LightningToCold * gearScaleDown
            + lightningDamage * ExtraDamageAs.LightningToCold;

        var chaos =
            lightningDamage * DamageConvertedFromSkills.LightningToChaos * skillScaleDown
            + lightningDamage * DamageConverted.LightningToChaos * gearScaleDown
            + lightningDamage * ExtraDamageAs.LightningToChaos
            + lightningDamage * NonChaosAsExtraChaosDamage
            + lightningDamage * ElementalAsExtraChaosDamage;

        var remainingDamage = lightningDamage * (1f - totalConverted);
        var applicableDamageTypes = additionalDamageTypes.Append(DamageType.Lightning).Distinct().ToArray();
        var scaledLightningDamage = ScaleDamage(remainingDamage, applicableDamageTypes);
        var damage = Damage.OfType(scaledLightningDamage, DamageType.Lightning);

        if (fire > 0)
        {
            damage += ConvertFireDamage(fire, applicableDamageTypes);
        }

        if (cold > 0)
        {
            damage += ConvertColdDamage(cold, applicableDamageTypes);
        }

        if (chaos > 0)
        {
            damage += ConvertChaosDamage(chaos, applicableDamageTypes);
        }

        return damage;
    }

    Damage ConvertChaosDamage(float chaosDamage, DamageType[] additionalDamageTypes)
    {
        // Chaos damage cannot be further converted, so we only scale it using the applicable damage types
        // this portion of damage has gone through to reach this leaf of the conversion tree.

        var applicableDamageTypes = additionalDamageTypes.Append(DamageType.Chaos).Distinct().ToArray();
        var scaledChaosDamage = ScaleDamage(chaosDamage, applicableDamageTypes);
        var damage = Damage.OfType(scaledChaosDamage, DamageType.Chaos);
        return damage;
    }

    float ScaleDamage(float damage, DamageType[] applicableDamageTypes)
    {
        float totalIncreased = 0f;
        float totalMore = 1f;

        foreach (var damageType in applicableDamageTypes)
        {
            var mod = DamageModifiers.GetModifierForType(damageType);
            totalIncreased += mod.Increased;
            totalMore *= mod.More;
        }

        if (applicableDamageTypes.Any(x => x.IsElemental()))
        {
            var mod = DamageModifiers.GetModifierForType(DamageType.Elemental);
            totalIncreased += mod.Increased;
            totalMore *= mod.More;
        }

        return damage * (1f + totalIncreased) * totalMore;
    }
}
