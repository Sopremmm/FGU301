using System;
using UnityEngine;

public class CurrencyManager : BaseSingleton<CurrencyManager>
{
    [Header("Currency")]
    [Min(0)]
    [SerializeField] private int startingPoints = 100; // Điểm khởi đầu

    private int currentPoints; // Điểm hiện tại

    public int CurrentPoints => currentPoints;
    public event Action<int> PointsChanged;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        currentPoints = startingPoints;
    }

    private void Start()
    {
        if (Instance == this)
        {
            NotifyPointsChanged();
        }
    }

    public bool CanAfford(int cost)
    {
        return cost <= currentPoints;
    }

    public bool Spend(int cost)
    {
        if (cost < 0)
        {
            Debug.LogWarning($"{nameof(CurrencyManager)} cannot spend a negative cost: {cost}.", this);
            return false;
        }

        if (!CanAfford(cost))
        {
            return false;
        }

        currentPoints -= cost;
        NotifyPointsChanged();
        return true;
    }

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentPoints += amount;
        NotifyPointsChanged();
    }

    private void NotifyPointsChanged()
    {
        PointsChanged?.Invoke(currentPoints);
        UIManager.Instance?.SetCurrencyText(currentPoints);
    }
}
