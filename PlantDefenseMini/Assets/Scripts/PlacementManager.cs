using UnityEngine;

public class PlacementManager : BaseSingleton<PlacementManager>
{
    [Header("References")]
    [SerializeField] private Camera worldCamera; // Camera dùng để đổi vị trí chuột sang world

    [Header("Placement")]
    [SerializeField] private float maxCellPickDistance = 0.75f; // Khoảng cách tối đa để bắt ô grid gần chuột

    private CharacterData currentData; // Character đang được chọn để đặt
    private GameObject previewInstance; // Object preview đi theo chuột
    private GridCell currentCell; // Ô grid hiện tại dưới chuột

    private bool IsPlacing => currentData != null && previewInstance != null;
    private GridGenerator2D Grid => GridGenerator2D.Instance;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

    }

    private void Update()
    {
        if (Instance != this)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            CancelPlacement();
            return;
        }

        if (!IsPlacing)
        {
            return;
        }

        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceCurrentData();
        }
    }

    public void StartPlacement(CharacterData data)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            return;
        }

        if (data == null)
        {
            Debug.LogWarning($"{nameof(PlacementManager)} cannot start placement because CharacterData is missing.", this);
            return;
        }

        if (data.Prefab == null)
        {
            Debug.LogWarning($"{nameof(PlacementManager)} cannot start placement because {data.name} has no prefab.", this);
            return;
        }

        CancelPlacement();

        currentData = data;
        previewInstance = Instantiate(data.Prefab);
        previewInstance.name = $"{data.name} Preview";
        DisablePreviewBehaviours(previewInstance);
    }

    public void CancelPlacement()
    {
        currentData = null;
        currentCell = null;

        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    private void UpdatePreviewPosition()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        if (Grid != null && Grid.TryGetNearestCell(mouseWorldPosition, maxCellPickDistance, out GridCell nearestCell))
        {
            currentCell = nearestCell;
            previewInstance.transform.position = nearestCell.transform.position;
            return;
        }

        currentCell = null;
        previewInstance.transform.position = mouseWorldPosition;
    }

    private void TryPlaceCurrentData()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            CancelPlacement();
            return;
        }

        if (currentCell == null)
        {
            return;
        }

        if (!currentCell.CanPlace(currentData))
        {
            return;
        }

        if (currentData.Prefab.GetComponent<UnitController>() == null)
        {
            Debug.LogWarning($"{nameof(PlacementManager)} cannot place {currentData.name} because its prefab has no UnitController.", currentData.Prefab);
            CancelPlacement();
            return;
        }

        CurrencyManager currencyManager = CurrencyManager.Instance;

        if (currencyManager == null)
        {
            Debug.LogWarning($"{nameof(PlacementManager)} cannot place {currentData.name} because CurrencyManager is missing.", this);
            CancelPlacement();
            return;
        }

        if (!currencyManager.Spend(currentData.Cost))
        {
            CancelPlacement();
            return;
        }

        GameObject unitObject = Instantiate(currentData.Prefab, currentCell.transform.position, Quaternion.identity);
        unitObject.name = currentData.UnitName;

        UnitController unit = unitObject.GetComponent<UnitController>();

        if (unit == null)
        {
            Debug.LogWarning($"{nameof(PlacementManager)} spawned {unitObject.name}, but it has no UnitController.", unitObject);
            Destroy(unitObject);
            return;
        }

        unit.Init(currentData, currentCell.Row);
        currentCell.Place(unit);

        if (!currencyManager.CanAfford(currentData.Cost))
        {
            CancelPlacement();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (worldCamera == null)
        {
            Debug.LogWarning($"{nameof(PlacementManager)} is missing a camera.", this);
            return Vector3.zero;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(worldCamera.transform.position.z);

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    private void DisablePreviewBehaviours(GameObject preview)
    {
        MonoBehaviour[] behaviours = preview.GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            behaviour.enabled = false;
        }
    }
}
