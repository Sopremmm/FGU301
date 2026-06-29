using System.Collections.Generic;
using UnityEngine;

public class GameManager : BaseSingleton<GameManager>
{
    [Header("Data Source")]
    [SerializeField] private List<CharacterData> characterDatas = new List<CharacterData>(); // Danh sách character cho shop

    private readonly List<Vector3> laneLosePositions = new List<Vector3>(); // Điểm thua theo lane
    private bool isGameEnded; // Game đã kết thúc hay chưa

    public IReadOnlyList<CharacterData> CharacterDatas => characterDatas;
    public bool IsGameEnded => isGameEnded;

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        if (GridGenerator2D.Instance != null)
        {
            SetLaneLosePositions(GridGenerator2D.Instance.LaneLosePositions);
        }
    }

    public CharacterData GetCharacterData(int index)
    {
        if (index < 0 || index >= characterDatas.Count)
        {
            Debug.LogWarning($"{nameof(GameManager)} cannot get character data at invalid index {index}.", this);
            return null;
        }

        return characterDatas[index];
    }

    public void SetLaneLosePositions(IReadOnlyList<Vector3> positions)
    {
        laneLosePositions.Clear();

        if (positions == null)
        {
            return;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            laneLosePositions.Add(positions[i]);
        }
    }

    public bool HasReachedLosePosition(UnitController unit)
    {
        if (unit == null || unit.Faction != Faction.Enemy || !unit.HasLane)
        {
            return false;
        }

        if (unit.LaneIndex < 0 || unit.LaneIndex >= laneLosePositions.Count)
        {
            return false;
        }

        return unit.transform.position.x <= laneLosePositions[unit.LaneIndex].x;
    }

    public void Win()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;
        UIManager uiManager = UIManager.Instance;

        if (uiManager == null)
        {
            Debug.LogWarning($"{nameof(GameManager)} cannot show win menu because UIManager is missing.", this);
            return;
        }

        uiManager.ShowWinMenu();
    }

    public void Lose()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;
        WaveManager.Instance?.StopWaves();
        PlacementManager.Instance?.CancelPlacement();

        UIManager uiManager = UIManager.Instance;

        if (uiManager == null)
        {
            Debug.LogWarning($"{nameof(GameManager)} cannot show lose menu because UIManager is missing.", this);
            return;
        }

        uiManager.ShowLoseMenu();
    }
}
