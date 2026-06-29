using System.Collections.Generic;
using UnityEngine;

public class GridGenerator2D : BaseSingleton<GridGenerator2D>
{
    private enum RowOffsetOrder
    {
        TopToBottom,
        BottomToTop
    }

    private enum RowOffsetDirection
    {
        Right,
        Left
    }

    [Header("References")]
    [SerializeField] private GameObject cellPrefab; // Prefab ô grid
    [SerializeField] private Transform gridParent; // Parent chứa các ô grid

    [Header("Grid Size")]
    [Min(1)]
    [SerializeField] private int rows = 5; // Số hàng grid
    [Min(1)]
    [SerializeField] private int columns = 9; // Số cột grid

    [Header("Spacing")]
    [SerializeField] private float horizontalSpacing = 1f; // Khoảng cách ngang giữa các ô
    [SerializeField] private float verticalSpacing = 1f; // Khoảng cách dọc giữa các hàng
    [SerializeField] private float rowOffsetX = 0f; // Độ lệch X giữa các hàng
    [SerializeField] private RowOffsetOrder rowOffsetOrder = RowOffsetOrder.TopToBottom; // Thứ tự tính lệch hàng
    [SerializeField] private RowOffsetDirection rowOffsetDirection = RowOffsetDirection.Right; // Hướng lệch hàng

    [Header("Options")]
    [SerializeField] private bool generateOnStart = false; // Tự generate khi Start
    [SerializeField] private bool clearBeforeGenerate = true; // Xóa grid cũ trước khi generate
    [SerializeField] private string cellNamePrefix = "Grid Cell"; // Tiền tố tên ô grid
    [Min(0f)]
    [SerializeField] private float loseColumnOffset = 1f; // Khoảng cách điểm thua bên trái grid

    private readonly List<GridCell> cells = new List<GridCell>(); // Danh sách ô grid
    private readonly List<Vector3> laneLosePositions = new List<Vector3>(); // Điểm thua theo từng lane

    public IReadOnlyList<GridCell> Cells => cells;
    public IReadOnlyList<Vector3> LaneLosePositions => laneLosePositions;
    public int Rows => rows;
    public int Columns => columns;

    private Transform Parent => gridParent != null ? gridParent : transform;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        RebuildCellListFromChildren();
    }

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        if (generateOnStart)
        {
            GenerateGrid();
        }
    }

    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        if (cellPrefab == null)
        {
            Debug.LogWarning($"{nameof(GridGenerator2D)} needs a cell prefab.", this);
            return;
        }

        Transform parent = Parent;

        if (clearBeforeGenerate)
        {
            ClearGrid(parent);
        }

        Vector3 startOffset = GetCenteredStartOffset();

        for (int row = 0; row < rows; row++)
        {
            float offsetX = GetRowOffsetX(row);

            for (int column = 0; column < columns; column++)
            {
                Vector3 localPosition = startOffset + new Vector3(
                    column * horizontalSpacing + offsetX,
                    -row * verticalSpacing,
                    0f
                );

                GameObject cell = Instantiate(cellPrefab, parent);
                cell.name = $"{cellNamePrefix} ({row}, {column})";
                cell.transform.localPosition = localPosition;
                // cell.transform.localRotation = Quaternion.identity;

                GridCell gridCell = cell.GetComponent<GridCell>();

                if (gridCell == null)
                {
                    gridCell = cell.AddComponent<GridCell>();
                }

                gridCell.Init(row, column, this);
                cells.Add(gridCell);
            }
        }

        RebuildLaneLosePositions();
        GameManager.Instance?.SetLaneLosePositions(laneLosePositions);
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        ClearGrid(Parent);
    }

    public bool TryGetNearestCell(Vector3 worldPosition, float maxDistance, out GridCell nearestCell)
    {
        if (cells.Count == 0)
        {
            RebuildCellListFromChildren();
        }

        nearestCell = null;
        float nearestSqrDistance = maxDistance * maxDistance;

        foreach (GridCell cell in cells)
        {
            if (cell == null)
            {
                continue;
            }

            float sqrDistance = (cell.transform.position - worldPosition).sqrMagnitude;

            if (sqrDistance <= nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestCell = cell;
            }
        }

        return nearestCell != null;
    }

    public float GetLaneY(int row)
    {
        if (cells.Count == 0)
        {
            RebuildCellListFromChildren();
        }

        foreach (GridCell cell in cells)
        {
            if (cell != null && cell.Row == row)
            {
                return cell.transform.position.y;
            }
        }

        Debug.LogWarning($"{nameof(GridGenerator2D)} cannot find lane Y for row {row}.", this);
        return transform.position.y;
    }

    public bool TryGetLaneLosePosition(int row, out Vector3 losePosition)
    {
        if (laneLosePositions.Count == 0)
        {
            RebuildLaneLosePositions();
        }

        if (row < 0 || row >= laneLosePositions.Count)
        {
            losePosition = default;
            return false;
        }

        losePosition = laneLosePositions[row];
        return true;
    }

    private Vector3 GetCenteredStartOffset()
    {
        float minX = 0f;
        float maxX = 0f;

        for (int row = 0; row < rows; row++)
        {
            float rowStartX = GetRowOffsetX(row);
            float rowEndX = rowStartX + (columns - 1) * horizontalSpacing;

            minX = Mathf.Min(minX, rowStartX, rowEndX);
            maxX = Mathf.Max(maxX, rowStartX, rowEndX);
        }

        float gridHeight = (rows - 1) * verticalSpacing;

        return new Vector3(
            -(minX + maxX) * 0.5f,
            gridHeight * 0.5f,
            0f
        );
    }

    private float GetRowOffsetX(int row)
    {
        int offsetIndex = rowOffsetOrder == RowOffsetOrder.TopToBottom
            ? row
            : rows - 1 - row;

        float direction = rowOffsetDirection == RowOffsetDirection.Right ? 1f : -1f;

        return offsetIndex * rowOffsetX * direction;
    }

    private void ClearGrid(Transform parent)
    {
        cells.Clear();
        laneLosePositions.Clear();

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void RebuildCellListFromChildren()
    {
        cells.Clear();

        Transform parent = Parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            GridCell cell = parent.GetChild(i).GetComponent<GridCell>();

            if (cell != null)
            {
                cells.Add(cell);
            }
        }

        RebuildLaneLosePositions();
        GameManager.Instance?.SetLaneLosePositions(laneLosePositions);
    }

    private void RebuildLaneLosePositions()
    {
        laneLosePositions.Clear();

        for (int row = 0; row < rows; row++)
        {
            if (TryGetRowLeftMostCell(row, out GridCell leftMostCell))
            {
                Vector3 position = leftMostCell.transform.position;
                position.x -= loseColumnOffset;
                laneLosePositions.Add(position);
            }
        }
    }

    private bool TryGetRowLeftMostCell(int row, out GridCell leftMostCell)
    {
        leftMostCell = null;

        foreach (GridCell cell in cells)
        {
            if (cell == null || cell.Row != row)
            {
                continue;
            }

            if (leftMostCell == null || cell.transform.position.x < leftMostCell.transform.position.x)
            {
                leftMostCell = cell;
            }
        }

        return leftMostCell != null;
    }
}
