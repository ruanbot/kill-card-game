using UnityEngine;

[System.Serializable]
public class DamageResistances
{
    [Range(0, 1)] public float Slash = 0f;
    [Range(0, 1)] public float Blunt = 0f;
    [Range(0, 1)] public float Fire = 0f;
    [Range(0, 1)] public float Frost = 0f;
    [Range(0, 1)] public float Lightning = 0f;
    [Range(0, 1)] public float Dark = 0f;
    [Range(0, 1)] public float Holy = 0f;

    public float GetResistance(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Slash => Slash,
            DamageType.Blunt => Blunt,
            DamageType.Fire => Fire,
            DamageType.Frost => Frost,
            DamageType.Lightning => Lightning,
            DamageType.Dark => Dark,
            DamageType.Holy => Holy,
            _ => 0f,
        };
    }
}
