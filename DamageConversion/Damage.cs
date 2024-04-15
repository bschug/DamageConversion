namespace DamageConversion;

public record Damage(
    float Physical,
    float Fire,
    float Cold,
    float Lightning,
    float Chaos)
{
    public float Total => Physical + Fire + Cold + Lightning + Chaos;

    public static Damage operator + (Damage a, Damage b)
    {
        return new(
            a.Physical + b.Physical,
            a.Fire + b.Fire,
            a.Cold + b.Cold,
            a.Lightning + b.Lightning,
            a.Chaos + b.Chaos);
    }

    public static readonly Damage Empty = new(0, 0, 0, 0, 0);

    public Damage With(float amount, DamageType type) => type switch
    {
        DamageType.Physical => this with { Physical = amount },
        DamageType.Fire => this with { Fire = amount },
        DamageType.Cold => this with { Cold = amount },
        DamageType.Lightning => this with { Lightning = amount },
        DamageType.Chaos => this with { Chaos = amount },
        _ => throw new ArgumentException()
    };

    public static Damage OfType(float amount, DamageType type) => Empty.With(amount, type);
}
