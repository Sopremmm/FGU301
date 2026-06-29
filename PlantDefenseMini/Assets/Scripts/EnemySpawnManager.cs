using UnityEngine;

public class EnemySpawnManager : BaseSingleton<EnemySpawnManager>
{
    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint; // Điểm lấy toạ độ X để spawn enemy

    protected override void Awake()
    {
        base.Awake();
    }

    public UnitController SpawnEnemy(EnemyData enemyData, int laneIndex)
    {
        return SpawnEnemy(enemyData, laneIndex, 0f, 0f);
    }

    public UnitController SpawnEnemy(EnemyData enemyData, int laneIndex, float healthBonusPercent, float damageBonusPercent)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            return null;
        }

        if (enemyData == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawnManager)} cannot spawn enemy because EnemyData is missing.", this);
            return null;
        }

        if (enemyData.Prefab == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawnManager)} cannot spawn enemy because {enemyData.name} has no prefab.", this);
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawnManager)} cannot spawn enemy because spawnPoint is missing.", this);
            return null;
        }

        GridGenerator2D grid = GridGenerator2D.Instance;

        if (grid == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawnManager)} cannot spawn enemy because GridGenerator2D is missing.", this);
            return null;
        }

        if (laneIndex < 0 || laneIndex >= grid.Rows)
        {
            Debug.LogWarning($"{nameof(EnemySpawnManager)} cannot spawn enemy in invalid lane {laneIndex}.", this);
            return null;
        }

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.y = grid.GetLaneY(laneIndex);
        spawnPosition.z = 0f;

        GameObject enemyObject = Instantiate(enemyData.Prefab, spawnPosition, Quaternion.identity);
        enemyObject.name = enemyData.UnitName;

        UnitController unit = enemyObject.GetComponent<UnitController>();

        if (unit == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawnManager)} spawned {enemyObject.name}, but it has no UnitController.", enemyObject);
            Destroy(enemyObject);
            return null;
        }

        unit.Init(enemyData, laneIndex, healthBonusPercent, damageBonusPercent);
        return unit;
    }

}
