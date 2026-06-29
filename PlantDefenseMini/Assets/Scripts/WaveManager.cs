using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : BaseSingleton<WaveManager>
{
    [System.Serializable]
    private class WaveConfig
    {
        [Min(0f)]
        [SerializeField] private float countdownBeforeWave = 10f; // Thời gian chờ trước wave này
        [SerializeField] private List<EnemyData> enemyDatas = new List<EnemyData>(); // Danh sách enemy có thể spawn trong wave
        [Min(1)]
        [SerializeField] private int enemyCount = 5; // Số enemy cần spawn trong wave
        [Min(0.01f)]
        [SerializeField] private float spawnInterval = 1f; // Thời gian giữa hai lần spawn enemy
        [SerializeField] private bool randomLane = true; // Có spawn lane ngẫu nhiên hay không
        [Min(0)]
        [SerializeField] private int laneIndex; // Lane cố định nếu không random
        [Min(0f)]
        [SerializeField] private float healthBonusPercent; // Phần trăm tăng máu enemy
        [Min(0f)]
        [SerializeField] private float damageBonusPercent; // Phần trăm tăng damage enemy

        public float CountdownBeforeWave => countdownBeforeWave;
        public IReadOnlyList<EnemyData> EnemyDatas => enemyDatas;
        public int EnemyCount => enemyCount;
        public float SpawnInterval => spawnInterval;
        public bool RandomLane => randomLane;
        public int LaneIndex => laneIndex;
        public float HealthBonusPercent => healthBonusPercent;
        public float DamageBonusPercent => damageBonusPercent;
    }

    [Header("Waves")]
    [Min(0f)]
    [SerializeField] private float initialCountdown = 30f; // Thời gian chờ trước khi bắt đầu hệ thống wave
    [SerializeField] private bool startOnStart = true; // Tự chạy wave khi Start
    [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>(); // Danh sách wave

    private Coroutine waveRoutine; // Coroutine wave hiện tại
    private int currentWaveIndex = -1; // Index wave hiện tại
    private bool isRunning; // Wave manager đang chạy hay không
    private int currentWaveSpawnedCount; // Số enemy đã spawn thành công trong wave hiện tại
    private int currentWaveDeadCount; // Số enemy đã chết trong wave hiện tại
    private bool currentWaveSpawnFinished; // Wave hiện tại đã spawn xong hay chưa
    private readonly HashSet<UnitController> currentWaveEnemies = new HashSet<UnitController>(); // Enemy thuộc wave hiện tại

    public int CurrentWaveNumber => currentWaveIndex + 1;
    public bool IsRunning => isRunning;

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        if (startOnStart)
        {
            StartWaves();
        }
    }

    [ContextMenu("Start Waves")]
    public void StartWaves()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            return;
        }

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }

        waveRoutine = StartCoroutine(StartWaveSequence());
    }

    [ContextMenu("Stop Waves")]
    public void StopWaves()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }

        isRunning = false;
        currentWaveIndex = -1;
        currentWaveSpawnedCount = 0;
        currentWaveDeadCount = 0;
        currentWaveSpawnFinished = false;
        currentWaveEnemies.Clear();
    }

    public void NotifyEnemyDied(UnitController enemy)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            return;
        }

        if (!isRunning || currentWaveIndex < 0)
        {
            return;
        }

        if (enemy == null || !currentWaveEnemies.Remove(enemy))
        {
            return;
        }

        currentWaveDeadCount++;
        CheckCurrentWaveCompleted();
    }

    private IEnumerator StartWaveSequence()
    {
        isRunning = true;
        currentWaveIndex = -1;
        currentWaveSpawnedCount = 0;
        currentWaveDeadCount = 0;
        currentWaveSpawnFinished = false;
        currentWaveEnemies.Clear();
        UIManager.Instance?.SetWaveText(0, waves.Count);

        if (initialCountdown > 0f)
        {
            yield return Countdown(initialCountdown, remainingSeconds =>
            {
                UIManager.Instance?.SetInitialCountdownText(remainingSeconds);
            });
        }

        StartWave(0);
    }

    private IEnumerator Countdown(float duration, System.Action<int> onTick)
    {
        float remaining = duration;

        while (remaining > 0f)
        {
            onTick?.Invoke(Mathf.CeilToInt(remaining));
            yield return null;
            remaining -= Time.deltaTime;
        }

        onTick?.Invoke(0);
    }

    private void StartWave(int waveIndex)
    {
        if (waveIndex >= waves.Count)
        {
            CompleteAllWaves();
            return;
        }

        WaveConfig wave = waves[waveIndex];

        if (wave == null)
        {
            StartWave(waveIndex + 1);
            return;
        }

        currentWaveIndex = waveIndex;
        currentWaveSpawnedCount = 0;
        currentWaveDeadCount = 0;
        currentWaveSpawnFinished = false;
        currentWaveEnemies.Clear();
        UIManager.Instance?.SetWaveText(CurrentWaveNumber, waves.Count);

        waveRoutine = StartCoroutine(CountdownAndSpawnWave(wave));
    }

    private IEnumerator CountdownAndSpawnWave(WaveConfig wave)
    {
        if (wave.CountdownBeforeWave > 0f)
        {
            int waveNumber = CurrentWaveNumber;

            yield return Countdown(wave.CountdownBeforeWave, remainingSeconds =>
            {
                UIManager.Instance?.SetWaveCountdownText(waveNumber, remainingSeconds);
            });
        }

        UIManager.Instance?.ClearCountdownText();
        yield return SpawnWave(wave);

        currentWaveSpawnFinished = true;
        CheckCurrentWaveCompleted();
    }

    private IEnumerator SpawnWave(WaveConfig wave)
    {
        EnemySpawnManager enemySpawnManager = EnemySpawnManager.Instance;

        if (enemySpawnManager == null)
        {
            Debug.LogWarning($"{nameof(WaveManager)} cannot spawn wave because EnemySpawnManager is missing.", this);
            yield break;
        }

        if (wave.EnemyDatas == null || wave.EnemyDatas.Count == 0)
        {
            Debug.LogWarning($"{nameof(WaveManager)} cannot spawn wave {CurrentWaveNumber} because enemy data list is empty.", this);
            yield break;
        }

        for (int i = 0; i < wave.EnemyCount; i++)
        {
            EnemyData enemyData = GetRandomEnemyData(wave);

            if (enemyData == null)
            {
                continue;
            }

            int laneIndex = wave.RandomLane ? GetRandomLaneIndex() : wave.LaneIndex;

            UnitController enemy = enemySpawnManager.SpawnEnemy(
                enemyData,
                laneIndex,
                wave.HealthBonusPercent,
                wave.DamageBonusPercent
            );

            if (enemy != null)
            {
                currentWaveSpawnedCount++;
                currentWaveEnemies.Add(enemy);
            }

            if (i < wave.EnemyCount - 1)
            {
                yield return new WaitForSeconds(wave.SpawnInterval);
            }
        }
    }

    private EnemyData GetRandomEnemyData(WaveConfig wave)
    {
        int index = Random.Range(0, wave.EnemyDatas.Count);
        return wave.EnemyDatas[index];
    }

    private int GetRandomLaneIndex()
    {
        GridGenerator2D grid = GridGenerator2D.Instance;

        if (grid == null || grid.Rows <= 0)
        {
            return 0;
        }

        return Random.Range(0, grid.Rows);
    }

    private void CheckCurrentWaveCompleted()
    {
        if (!currentWaveSpawnFinished)
        {
            return;
        }

        if (currentWaveDeadCount < currentWaveSpawnedCount)
        {
            return;
        }

        StartWave(currentWaveIndex + 1);
    }

    private void CompleteAllWaves()
    {
        isRunning = false;
        waveRoutine = null;
        currentWaveIndex = waves.Count - 1;
        UIManager.Instance?.ClearCountdownText();
        GameManager.Instance?.Win();
    }
}
