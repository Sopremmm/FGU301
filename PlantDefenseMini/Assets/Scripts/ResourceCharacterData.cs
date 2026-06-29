using UnityEngine;

[CreateAssetMenu(fileName = "New Resource Character Data", menuName = "Plant Defense/Data/Resource Character Data")]
public class ResourceCharacterData : CharacterData
{
    [Header("Resource")]
    [Min(0)]
    [SerializeField] private int pointAmount = 25; // Số điểm tạo ra mỗi lần
    [Min(0.01f)]
    [SerializeField] private float generateCooldown = 5f; // Thời gian nghỉ giữa hai lần tạo điểm

    public int PointAmount => pointAmount;
    public float GenerateCooldown => generateCooldown;
}
