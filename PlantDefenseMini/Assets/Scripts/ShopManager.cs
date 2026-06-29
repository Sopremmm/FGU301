using UnityEngine;

public class ShopManager : BaseSingleton<ShopManager>
{
    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        RefreshShop();
    }

    public void RefreshShop()
    {
        GameManager gameManager = GameManager.Instance;
        UIManager uiManager = UIManager.Instance;

        if (gameManager == null)
        {
            Debug.LogWarning($"{nameof(ShopManager)} cannot refresh shop because GameManager is missing.", this);
            return;
        }

        if (uiManager == null)
        {
            Debug.LogWarning($"{nameof(ShopManager)} cannot refresh shop because UIManager is missing.", this);
            return;
        }

        uiManager.RenderCharacterShop(gameManager.CharacterDatas, BuyCharacter);
    }

    private void BuyCharacter(CharacterData data)
    {
        if (data == null)
        {
            return;
        }

        PlacementManager placementManager = PlacementManager.Instance;
        CurrencyManager currencyManager = CurrencyManager.Instance;

        if (placementManager == null)
        {
            Debug.LogWarning($"{nameof(ShopManager)} cannot buy {data.name} because PlacementManager is missing.", this);
            return;
        }

        if (currencyManager == null)
        {
            Debug.LogWarning($"{nameof(ShopManager)} cannot buy {data.name} because CurrencyManager is missing.", this);
            return;
        }

        if (!currencyManager.CanAfford(data.Cost))
        {
            return;
        }

        placementManager.StartPlacement(data);
    }
}
