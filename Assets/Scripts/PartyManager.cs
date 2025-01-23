using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PartyMemberInfo[] allMembers;
    [SerializeField] private List<PartyMember> currentParty;

    [SerializeField] private PartyMemberInfo defaultPartyMember;

    private void Awake()
    {
        GeneratePartyMemberByName(defaultPartyMember.MemberName);
    }

    private void GeneratePartyMemberByName(string memberName)
    {
        for (int i = 0; i < allMembers.Length; i++)
        {
            if (allMembers[i].MemberName == memberName)
            {

                PartyMember newPartyMember = new PartyMember
                {
                    MemberName = allMembers[i].MemberName,
                    Level = allMembers[i].StartingLevel,
                    MaxHealth = allMembers[i].BaseHealth,
                    CurrentHealth = allMembers[i].BaseHealth,
                    Initiative = allMembers[i].BaseInitiative,
                    MemberBattleVisualPrefab = allMembers[i].MemberBattleVisualPrefab,
                    Resistances = allMembers[i].resistances // Set resistances from scriptable object
                };

                // Add the newly created member to the current party list
                currentParty.Add(newPartyMember);
            }
        }
    }

    public List<PartyMember> GetCurrentParty()
    {
        return currentParty;
    }
}

[System.Serializable]
public class PartyMember
{
    public string MemberName;
    public int Level;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Initiative;
    public int CurrentExp;
    public int MaxExp;
    public GameObject MemberBattleVisualPrefab;  // Visual prefab reference
    public BattleVisuals BattleVisuals;  // Direct reference to BattleVisuals for real-time updates

    // Resistances to different damage types
    public DamageResistances Resistances;
}
