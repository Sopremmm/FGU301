using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : BaseSingleton<UIManager>
{
    [Header("Shop")]
    [SerializeField] private Transform shopItemsParent; // Parent chứa các item shop
    [SerializeField] private GameObject shopItemPrefab; // Prefab item shop

    [Header("Wave")]
    [SerializeField] private TMP_Text waveText; // Text hiển thị wave hiện tại
    [SerializeField] private TMP_Text waveCountdownText; // Text hiển thị đếm ngược wave

    [Header("Currency")]
    [SerializeField] private TMP_Text currencyText; // Text hiển thị điểm hiện tại

    [Header("Game Result")]
    [SerializeField] private GameObject winMenu; // Menu thắng
    [SerializeField] private GameObject loseMenu; // Menu thua
    [SerializeField] private Button winHomeButton; // Nút về home ở menu thắng
    [SerializeField] private Button loseHomeButton; // Nút về home ở menu thua
    [SerializeField] private string homeSceneName; // Tên scene home

    [Header("Main Menu")]
    [SerializeField] private Button playGameButton; // Nút bắt đầu game
    [SerializeField] private string gameplaySceneName = "GamePlay"; // Tên scene gameplay

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        HideResultMenus();
        BindHomeButtons();
        BindPlayGameButton();
    }

    public void ShowWinMenu()
    {
        if (winMenu != null)
        {
            winMenu.SetActive(true);
        }

        if (loseMenu != null)
        {
            loseMenu.SetActive(false);
        }
    }

    public void ShowLoseMenu()
    {
        if (winMenu != null)
        {
            winMenu.SetActive(false);
        }

        if (loseMenu != null)
        {
            loseMenu.SetActive(true);
        }
    }

    public void SetWaveText(int currentWave, int totalWaves)
    {
        if (waveText == null)
        {
            return;
        }

        waveText.text = $"Wave: {currentWave}/{totalWaves}";
    }

    public void SetInitialCountdownText(int seconds)
    {
        SetCountdownText($"Starts in {seconds}s");
    }

    public void SetWaveCountdownText(int waveNumber, int seconds)
    {
        SetCountdownText($"Wave {waveNumber} starts in {seconds}s");
    }

    public void ClearCountdownText()
    {
        SetCountdownText(string.Empty);
    }

    public void SetCurrencyText(int points)
    {
        if (currencyText == null)
        {
            return;
        }

        currencyText.text = $"Summon Points:\n{points}";
    }

    public void RenderCharacterShop(IReadOnlyList<CharacterData> characterDatas, Action<CharacterData> onBuyClicked)
    {
        if (shopItemsParent == null)
        {
            Debug.LogWarning($"{nameof(UIManager)} cannot render shop because shopItemsParent is missing.", this);
            return;
        }

        if (shopItemPrefab == null)
        {
            Debug.LogWarning($"{nameof(UIManager)} cannot render shop because shopItemPrefab is missing.", this);
            return;
        }

        ClearShopItems();

        if (characterDatas == null)
        {
            return;
        }

        foreach (CharacterData characterData in characterDatas)
        {
            if (characterData == null)
            {
                continue;
            }

            CreateCharacterShopItem(characterData, onBuyClicked);
        }
    }

    private void CreateCharacterShopItem(CharacterData characterData, Action<CharacterData> onBuyClicked)
    {
        GameObject item = Instantiate(shopItemPrefab, shopItemsParent);
        item.name = $"{characterData.name} Shop Item";

        SetShopItemIcon(item, characterData);

        Button button = item.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onBuyClicked?.Invoke(characterData));
        }
        else
        {
            Debug.LogWarning($"{nameof(UIManager)} shop item prefab is missing Button component.", item);
        }

        Transform costTransform = item.transform.Find("Cost");

        if (costTransform == null)
        {
            Debug.LogWarning($"{nameof(UIManager)} shop item prefab is missing child object named Cost.", item);
            return;
        }

        TMP_Text costText = costTransform.GetComponent<TMP_Text>();

        if (costText == null)
        {
            Debug.LogWarning($"{nameof(UIManager)} Cost object is missing TextMeshProUGUI/TMP_Text component.", costTransform);
            return;
        }

        costText.text = characterData.Cost.ToString();
    }

    private void SetShopItemIcon(GameObject item, CharacterData characterData)
    {
        Transform iconTransform = item.transform.Find("Icon");

        if (iconTransform != null)
        {
            RawImage rawImage = iconTransform.GetComponent<RawImage>();

            if (rawImage != null && characterData.RenderTexture != null)
            {
                rawImage.texture = characterData.RenderTexture;
                return;
            }

            Image childImage = iconTransform.GetComponent<Image>();

            if (childImage != null && characterData.Icon != null)
            {
                childImage.sprite = characterData.Icon;
                return;
            }
        }

        Image rootImage = item.GetComponent<Image>();

        if (rootImage != null && characterData.Icon != null)
        {
            rootImage.sprite = characterData.Icon;
        }
    }

    private void ClearShopItems()
    {
        for (int i = shopItemsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(shopItemsParent.GetChild(i).gameObject);
        }
    }

    private void SetCountdownText(string text)
    {
        if (waveCountdownText == null)
        {
            return;
        }

        waveCountdownText.text = text;
    }

    private void HideResultMenus()
    {
        if (winMenu != null)
        {
            winMenu.SetActive(false);
        }

        if (loseMenu != null)
        {
            loseMenu.SetActive(false);
        }
    }

    private void BindHomeButtons()
    {
        if (winHomeButton != null)
        {
            winHomeButton.onClick.RemoveAllListeners();
            winHomeButton.onClick.AddListener(GoHome);
        }

        if (loseHomeButton != null)
        {
            loseHomeButton.onClick.RemoveAllListeners();
            loseHomeButton.onClick.AddListener(GoHome);
        }
    }

    private void BindPlayGameButton()
    {
        if (playGameButton == null)
        {
            return;
        }

        playGameButton.onClick.RemoveAllListeners();
        playGameButton.onClick.AddListener(PlayGame);
    }

    private void PlayGame()
    {
        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.LogWarning($"{nameof(UIManager)} cannot play game because gameplaySceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void GoHome()
    {
        if (!string.IsNullOrEmpty(homeSceneName))
        {
            SceneManager.LoadScene(homeSceneName);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
