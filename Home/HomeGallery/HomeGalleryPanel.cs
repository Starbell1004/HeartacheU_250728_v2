using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using System.Collections.Generic;

public class HomeGalleryPanel : MonoBehaviour
{
    [Header("�� �ý���")]
    [SerializeField] private Button cgTabButton;
    [SerializeField] private Button achievementTabButton;
    [SerializeField] private GameObject cgGalleryContent;
    [SerializeField] private GameObject achievementContent;
    [SerializeField] private Button galleryCloseButton;
    [SerializeField] private TMPro.TextMeshProUGUI achievementProgressText;

    [Header("�� ��ư ��Ÿ��")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;

    [Header("CG ������")]
    [SerializeField] private CGSlot[] cgSlots;
    [SerializeField] private GameObject fullImagePanel;
    [SerializeField] private Image fullImageDisplay;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("��������")]
    [SerializeField] private Transform achievementListContent;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private AchievementData[] achievements;

    // ���� �� (0: CG, 1: ��������)
    private int currentTab = 0;

    // CG ���� ����
    private ICustomVariableManager variableManager;
    private int currentSlotIndex = -1;
    private int currentImageIndex = 0;

    // �������� ������ ����Ʈ
    private List<AchievementItem> achievementItems = new List<AchievementItem>();

    private void Awake()
    {
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += OnEngineInitialized;
        }
    }

    private void OnEngineInitialized()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
        InitializeServices();
    }

    private void InitializeServices()
    {
        variableManager = Engine.GetService<ICustomVariableManager>();
        SetupButtons();
        SetupTabs();
        InitializeAchievements();
    }

    private void SetupButtons()
    {
        // ���� CG ���� ��ư��
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseFullImage);
        }
        if (galleryCloseButton != null)
        {
            galleryCloseButton.onClick.RemoveAllListeners();
            galleryCloseButton.onClick.AddListener(() => {
                Hide();
                // HomePanel ǥ��
                var homePanel = transform.parent?.Find("HomePanel");
                if (homePanel != null)
                {
                    homePanel.gameObject.SetActive(true);
                }
            });
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ShowNextImage);
        }

        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(ShowPrevImage);
        }

        // CG ���� ��ư��
        for (int i = 0; i < cgSlots.Length; i++)
        {
            int index = i;
            var slot = cgSlots[i];

            if (slot.button != null)
            {
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() => OnCGClicked(index));
            }
        }

        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(false);
        }
    }

    private void SetupTabs()
    {
        if (cgTabButton != null)
        {
            cgTabButton.onClick.RemoveAllListeners();
            cgTabButton.onClick.AddListener(() => ShowTab(0));
        }

        if (achievementTabButton != null)
        {
            achievementTabButton.onClick.RemoveAllListeners();
            achievementTabButton.onClick.AddListener(() => ShowTab(1));
        }

        // �ʱ� �� ����
        ShowTab(0);
    }

    private void ShowTab(int tabIndex)
    {
        currentTab = tabIndex;

        // ������ ǥ��/����
        if (cgGalleryContent != null)
            cgGalleryContent.SetActive(tabIndex == 0);

        if (achievementContent != null)
            achievementContent.SetActive(tabIndex == 1);

        // �� ��ư ��Ÿ�� ������Ʈ
        UpdateTabButtonStyle();

        // �� �� ������ ���ΰ�ħ
        if (tabIndex == 0)
        {
            RefreshCGGallery();
        }
        else
        {
            RefreshAchievements();
        }
    }

    private void UpdateTabButtonStyle()
    {
        // CG �� ��ư
        if (cgTabButton != null)
        {
            var cgButtonImage = cgTabButton.GetComponent<Image>();
            if (cgButtonImage != null)
            {
                cgButtonImage.color = currentTab == 0 ? activeTabColor : inactiveTabColor;
            }
        }

        // �������� �� ��ư
        if (achievementTabButton != null)
        {
            var achievementButtonImage = achievementTabButton.GetComponent<Image>();
            if (achievementButtonImage != null)
            {
                achievementButtonImage.color = currentTab == 1 ? activeTabColor : inactiveTabColor;
            }
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ShowTab(currentTab); // ���� �� ���ΰ�ħ
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #region CG Gallery Methods

    private void RefreshCGGallery()
    {
        if (variableManager == null)
        {
            InitializeServices();
        }

        foreach (var slot in cgSlots)
        {
            bool isUnlocked = CheckCGUnlocked(slot.unlockVariable);

            if (slot.lockOverlay != null)
            {
                slot.lockOverlay.SetActive(!isUnlocked);
            }

            if (slot.button != null)
            {
                slot.button.interactable = isUnlocked;
            }
        }
    }

    private bool CheckCGUnlocked(string unlockVariable)
    {
        if (string.IsNullOrEmpty(unlockVariable) || variableManager == null)
            return false;

        try
        {
            if (variableManager.VariableExists(unlockVariable))
            {
                var value = variableManager.GetVariableValue(unlockVariable);
                return value.ToString() == "true" || value.ToString() == "1";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeGalleryPanel] ���� Ȯ�� ����: {ex.Message}");
        }

        return false;
    }

    private void OnCGClicked(int index)
    {
        if (index < 0 || index >= cgSlots.Length) return;

        var slot = cgSlots[index];

        if (!CheckCGUnlocked(slot.unlockVariable)) return;

        if (slot.fullSizeImages == null || slot.fullSizeImages.Length == 0) return;

        currentSlotIndex = index;
        currentImageIndex = 0;

        ShowCurrentImage();

        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(true);
        }
    }

    private void ShowCurrentImage()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= cgSlots.Length) return;

        var slot = cgSlots[currentSlotIndex];
        if (slot.fullSizeImages == null || currentImageIndex >= slot.fullSizeImages.Length) return;

        if (fullImageDisplay != null)
        {
            fullImageDisplay.sprite = slot.fullSizeImages[currentImageIndex];
        }

        UpdateNavigationButtons();
    }

    private void ShowNextImage()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= cgSlots.Length) return;

        var slot = cgSlots[currentSlotIndex];
        if (slot.fullSizeImages == null) return;

        currentImageIndex++;
        if (currentImageIndex >= slot.fullSizeImages.Length)
        {
            currentImageIndex = 0;
        }

        ShowCurrentImage();
    }

    private void ShowPrevImage()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= cgSlots.Length) return;

        var slot = cgSlots[currentSlotIndex];
        if (slot.fullSizeImages == null) return;

        currentImageIndex--;
        if (currentImageIndex < 0)
        {
            currentImageIndex = slot.fullSizeImages.Length - 1;
        }

        ShowCurrentImage();
    }

    private void UpdateNavigationButtons()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= cgSlots.Length) return;

        var slot = cgSlots[currentSlotIndex];
        bool hasMultipleImages = slot.fullSizeImages != null && slot.fullSizeImages.Length > 1;

        if (nextButton != null)
            nextButton.gameObject.SetActive(hasMultipleImages);

        if (prevButton != null)
            prevButton.gameObject.SetActive(hasMultipleImages);
    }

    private void CloseFullImage()
    {
        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(false);
        }

        currentSlotIndex = -1;
        currentImageIndex = 0;
    }

    #endregion

    #region Achievement Methods

    private void InitializeAchievements()
    {
        if (achievementItemPrefab == null || achievementListContent == null)
            return;

        // ���� ������ ����
        foreach (var item in achievementItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        achievementItems.Clear();

        // �������� ������ ����
        foreach (var achievementData in achievements)
        {
            var itemGO = Instantiate(achievementItemPrefab, achievementListContent);
            var achievementItem = itemGO.GetComponent<AchievementItem>();

            if (achievementItem != null)
            {
                
                bool isUnlocked = CheckAchievementUnlocked(achievementData.unlockVariable);
                achievementItem.Setup(achievementData, isUnlocked);

                achievementItems.Add(achievementItem);
            }
        }
    }

    private void RefreshAchievements()
    {
        int unlockedCount = 0;
        int totalCount = achievements.Length;

        for (int i = 0; i < achievements.Length && i < achievementItems.Count; i++)
        {
            var data = achievements[i];
            var item = achievementItems[i];

            bool isUnlocked = CheckAchievementUnlocked(data.unlockVariable);
            if (isUnlocked) unlockedCount++;  //  �޼��� ���� ī��Ʈ

            item.Setup(data, isUnlocked);
        }

        //  ���൵ �ؽ�Ʈ ������Ʈ
        if (achievementProgressText != null)
        {
            achievementProgressText.text = $"{unlockedCount}/{totalCount}";
        }
    }

    private bool CheckAchievementUnlocked(string unlockVariable)
    {
        // CG �ر� üũ�� ������ ����
        return CheckCGUnlocked(unlockVariable);
    }

    #endregion

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        if (nextButton != null)
            nextButton.onClick.RemoveAllListeners();

        if (prevButton != null)
            prevButton.onClick.RemoveAllListeners();

        if (cgTabButton != null)
            cgTabButton.onClick.RemoveAllListeners();

        if (achievementTabButton != null)
            achievementTabButton.onClick.RemoveAllListeners();

        foreach (var slot in cgSlots)
        {
            if (slot.button != null)
            {
                slot.button.onClick.RemoveAllListeners();
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test: Unlock All CGs")]
    public void UnlockAllCGs()
    {
        if (variableManager == null) return;

        foreach (var slot in cgSlots)
        {
            if (!string.IsNullOrEmpty(slot.unlockVariable))
            {
                variableManager.SetVariableValue(slot.unlockVariable, new CustomVariableValue("true"));
            }
        }

        RefreshCGGallery();
    }
#endif
}

// CGSlot Ŭ������ ������ ����
[System.Serializable]
public class CGSlot
{
    public Button button;
    public Image cgImage;
    public GameObject lockOverlay;
    public string unlockVariable;
    public Sprite[] fullSizeImages;
}