using DamageConversion;

namespace WikiTests;

// https://www.poewiki.net/wiki/Damage_conversion
public class WikiTests
{
    [Fact]
    public void FirstExampleFromWiki()
    {
        // A player is wielding a bow that deals 100 Physical Damage,
        var character = new CharacterDamageData(new Damage(100, 0, 0, 0, 0));

        character.DamageConverted = new()
        {
            // a Blackgleam quiver that has 50% of Physical Damage converted to Fire Damage,
            PhysicalToFire = 0.50f,

            // and Hrimsorrow gloves that have 50% of Physical Damage converted to Cold Damage.
            PhysicalToCold = 0.50f
        };

        // The player is using the skill Lightning Arrow, which has 50% of Physical Damage converted to Lightning Damage
        character.DamageConvertedFromSkills = new DamageConversions() { PhysicalToLightning = 0.50f };


        var damage = character.CalculateDamage();

        // In summary, the resulting converted damage would be 50 Lightning Damage, 25 Fire Damage, and 25 Cold Damage,
        // and 0 Physical Damage, a total of 100 Damage.
        Assert.Equal(50, damage.Lightning);
        Assert.Equal(25, damage.Fire);
        Assert.Equal(25, damage.Cold);
        Assert.Equal(0, damage.Physical);
        Assert.Equal(100, damage.Total);
    }

    [Fact]
    public void SecondExampleFromWiki()
    {
        // A player is wielding a bow that deals 100 Physical Damage, and using a Blackgleam quiver, Hrimsorrow gloves,
        // and Lightning Arrow, as in the above example.
        var character = new CharacterDamageData(new Damage(100, 0, 0, 0, 0));
        character.DamageConverted = new() { PhysicalToFire = 0.50f, PhysicalToCold = 0.50f };
        character.DamageConvertedFromSkills = new() { PhysicalToLightning = 0.50f };

        // The Lightning Arrow skill is supported by a level 1 Added Fire Damage Support gem, which has 25% of
        // Physical Damage Added as Fire Damage.
        character.ExtraDamageAs = new() { PhysicalToFire = 0.25f };

        var damage = character.CalculateDamage();

        Assert.Equal(50, damage.Lightning);
        Assert.Equal(50, damage.Fire);
        Assert.Equal(25, damage.Cold);
        Assert.Equal(0, damage.Physical);
        Assert.Equal(125, damage.Total);
    }

    [Fact]
    public void ThirdExampleFromWiki()
    {
        // A player is wielding a bow that deals 100 Physical Damage,
        var character = new CharacterDamageData(new Damage(100, 0, 0, 0, 0));

        character.DamageConverted = new()
        {
            // a Blackgleam quiver that has 50% of Physical Damage converted to Fire Damage,
            PhysicalToFire = 0.50f,
            // and Hrimsorrow gloves that have 50% of Physical Damage converted to Cold Damage,
            PhysicalToCold = 0.50f,
        };

        // and finally, a rare amulet with the mod 10% of Non-Chaos Damage as Extra Chaos Damage.
        character.NonChaosAsExtraChaosDamage = 0.10f;

        var damage = character.CalculateDamage();

        Assert.Equal(50, damage.Fire);
        Assert.Equal(50, damage.Cold);
        Assert.Equal(20, damage.Chaos);
        Assert.Equal(120, damage.Total);
    }

    [Fact]
    public void FinalExampleFromWiki()
    {
        // Imagine a player wielding a sword that deals 100 Physical Damage
        var character = new CharacterDamageData(new Damage(100, 0, 0, 0, 0));

        // and performs an attack with the following modifiers:

        // 50% of Physical Damage converted to Cold Damage from a skill gem
        character.DamageConvertedFromSkills = character.DamageConvertedFromSkills with { PhysicalToCold = 0.50f };
        // 30% of Physical Damage converted to Fire Damage from gear
        character.DamageConverted = character.DamageConverted with { PhysicalToFire = 0.30f };
        // 30% of Physical Damage converted to Lightning Damage from gear
        character.DamageConverted = character.DamageConverted with { PhysicalToLightning = 0.30f };
        // 30% of Physical Damage Gained as Extra Fire Damage from a support gem
        character.ExtraDamageAs = character.ExtraDamageAs with { PhysicalToFire = 0.30f };
        // 15% of Physical Damage Gained as Extra Cold Damage from a support gem
        character.ExtraDamageAs = character.ExtraDamageAs with { PhysicalToCold = 0.15f };
        // 50% of Cold Damage converted to Fire Damage from a support gem
        character.DamageConvertedFromSkills = character.DamageConvertedFromSkills with { ColdToFire = 0.50f };

        character.DamageModifiers = character.DamageModifiers
            // Adds 30 Cold Damage from a support gem
            .WithAdded(30, DamageType.Cold)
            // 30% more Physical Damage from a support gem
            .WithMore(.30f, DamageType.Physical)
            // 70% more Weapon Elemental Damage from a support gem
            .WithMore(.70f, DamageType.Elemental)
            // 80% increased Physical Damage with Swords from passive skills
            .WithIncreased(.80f, DamageType.Physical)
            // 30% increased Melee Physical Damage from Strength and passive skills
            .WithIncreased(.30f, DamageType.Physical)
            // 20% increased Fire Damage from passive skills
            .WithIncreased(.20f, DamageType.Fire)
            // 15% increased Cold Damage from passive skills
            .WithIncreased(.15f, DamageType.Cold);

        var damage = character.CalculateDamage();

        // For a total of 190.9 Cold, 490.0 Fire, and 116.0 Lightning Damage, a grand total of 796.9 damage.
        float precision = 0.1f;
        Assert.Equal(190.9f, damage.Cold, precision);
        Assert.Equal(490, damage.Fire, precision);
        Assert.Equal(116, damage.Lightning, precision);
        Assert.Equal(796.9f, damage.Total, precision);
    }
}
