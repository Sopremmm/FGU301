using UnityEngine;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Plant Defense/Data/Character Data")]
public class CharacterData : UnitData
{
    [Header("Character")]
    [Min(0)]
    [SerializeField] private int cost = 50; // Giá mua character

    public override Faction Faction => global::Faction.Character;
    public int Cost => cost;
}
