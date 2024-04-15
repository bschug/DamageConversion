namespace DamageConversion;

public record DamageConversions(
    float PhysicalToLightning = 0,
    float PhysicalToCold = 0,
    float PhysicalToFire = 0,
    float PhysicalToChaos = 0,
    float LightningToCold = 0,
    float LightningToFire = 0,
    float LightningToChaos = 0,
    float ColdToFire = 0,
    float ColdToChaos = 0,
    float FireToChaos = 0);
