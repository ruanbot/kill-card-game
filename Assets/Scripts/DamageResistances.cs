using UnityEngine;

[System.Serializable]
public class DamageResistances
{
    [Range(0, 1)] public float Slash = 0f;
    [Range(0, 1)] public float Blunt = 0f;
    [Range(0, 1)] public float Ranged = 0f;
    [Range(0, 1)] public float Fire = 0f;
    [Range(0, 1)] public float Frost = 0f;
    [Range(0, 1)] public float Lightning = 0f;
    [Range(0, 1)] public float Dark = 0f;
    [Range(0, 1)] public float Holy = 0f;

    // Immunity flags for each damage type
    public bool ImmuneSlash = false;
    public bool ImmuneBlunt = false;
    public bool ImmuneRanged = false;
    public bool ImmuneFire = false;
    public bool ImmuneFrost = false;
    public bool ImmuneLightning = false;
    public bool ImmuneDark = false;
    public bool ImmuneHoly = false;

    // Method to check immunity
    public bool IsImmune(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Slash => ImmuneSlash,
            DamageType.Blunt => ImmuneBlunt,
            DamageType.Ranged => ImmuneRanged,
            DamageType.Fire => ImmuneFire,
            DamageType.Frost => ImmuneFrost,
            DamageType.Lightning => ImmuneLightning,
            DamageType.Dark => ImmuneDark,
            DamageType.Holy => ImmuneHoly,
            _ => false,
        };
    }

    public float GetResistance(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Slash => Slash,
            DamageType.Blunt => Blunt,
            DamageType.Ranged => Ranged,
            DamageType.Fire => Fire,
            DamageType.Frost => Frost,
            DamageType.Lightning => Lightning,
            DamageType.Dark => Dark,
            DamageType.Holy => Holy,
            _ => 0f,
        };
    }
}
