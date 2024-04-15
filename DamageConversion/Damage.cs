namespace DamageConversion;

public record Damage(
    float Physical,
    float Fire,
    float Cold,
    float Lightning,
    float Chaos)
{
    public float Total => Physical + Fire + Cold + Lightning + Chaos;
}
