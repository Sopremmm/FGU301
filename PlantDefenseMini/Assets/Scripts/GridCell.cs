using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private GridGenerator2D grid; // Grid sở hữu cell này
    [SerializeField] private int row; // Hàng của cell
    [SerializeField] private int column; // Cột của cell
    [SerializeField] private UnitController occupyingUnit; // Unit đang chiếm cell

    public GridGenerator2D Grid => grid;
    public int Row => row;
    public int Column => column;
    public bool IsOccupied => occupyingUnit != null && !occupyingUnit.IsDead;
    public UnitController OccupyingUnit => occupyingUnit;

    public void Init(int row, int column, GridGenerator2D grid)
    {
        this.row = row;
        this.column = column;
        this.grid = grid;
    }

    public bool CanPlace(CharacterData data)
    {
        return data != null && data.Prefab != null && !IsOccupied;
    }

    public void Place(UnitController unit)
    {
        occupyingUnit = unit;
    }

    public void Clear()
    {
        occupyingUnit = null;
    }
}
