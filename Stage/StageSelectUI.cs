using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using System.Collections.Generic;
using System.Collections;
using System;

public class StageSelectUI : MonoBehaviour
{
    [System.Serializable]
    public class StageSlot
    {
        [Header("기본 요소")]
        public Button button;                    // 전체 버튼
        public Image slotImage;                  // 슬롯 전체 이미지

        [Header("스프라이트")]
        public Sprite unlockedSprite;            // 해금된 상태 (컬러 하트 + 텍스트)
        public Sprite lockedSprite;              // 잠긴 상태 (회색 + 자물쇠)

        [Header("설정")]
        public string scriptName;                // 실행할 스크립트 (#9, #23 등)
        public int dayNumber;                    // 1, 2, 3
    }

    [Header("Stage Slots")]
    [SerializeField] private List<StageSlot> stageSlots = new List<StageSlot>();

    [Header("UI Elements")]
    [SerializeField] private Button closeButton;              // X 버튼

    private ICustomVariableManager variableManager;
    private IScriptPlayer scriptPlayer;
    private IStateManager stateManager;
    private IUIManager uiManager;

    private void Awake()
    {
        // 엔진 초기화 상태 확인
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += OnEngineInitialized;
        }
    }
    void Start()
    {
        // 시작하자마자 강제로 불투명
        var allRenderers = GetComponentsInChildren<CanvasRenderer>(true);
        foreach (var cr in allRenderers)
        {
            cr.SetAlpha(1f);
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
        scriptPlayer = Engine.GetService<IScriptPlayer>();
        stateManager = Engine.GetService<IStateManager>();
        uiManager = Engine.GetService<IUIManager>();

        SetupButtons();

        Debug.Log($"[StageSelectUI] Services initialized - variableManager: {variableManager != null}");
    }

    private void SetupButtons()
    {
        Debug.Log("[StageSelectUI] SetupButtons 시작");

        // X 버튼 (닫기)
        if (closeButton != null)
        {
            Debug.Log("[StageSelectUI] closeButton 설정");
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                Hide();
                // HomeUI 표시
                var homePanel = transform.parent.Find("HomePanel");
                if (homePanel != null)
                {
                    homePanel.gameObject.SetActive(true);
                }
            });
        }
        else
        {
            Debug.LogWarning("[StageSelectUI] closeButton이 null입니다!");
        }

        // 스테이지 버튼들
        Debug.Log($"[StageSelectUI] stageSlots 개수: {stageSlots?.Count ?? 0}");

        if (stageSlots != null)
        {
            foreach (var slot in stageSlots)
            {
                if (slot == null)
                {
                    Debug.LogError("[StageSelectUI] slot이 null입니다!");
                    continue;
                }
                if (slot.button == null)
                {
                    Debug.LogError("[StageSelectUI] slot.button이 null입니다!");
                    continue;
                }

                var capturedSlot = slot;
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() => OnStageSelected(capturedSlot));
            }
        }
    }
    private void OnEnable()
    {
        // 활성화될 때마다 실행
        StartCoroutine(ForceAlphaEveryFrame());
    }

    private IEnumerator ForceAlphaEveryFrame()
    {
        // 0.5초 동안 매 프레임마다 강제 설정
        float timer = 0f;
        while (timer < 0.5f)
        {
            var allRenderers = GetComponentsInChildren<CanvasRenderer>(true);
            foreach (var cr in allRenderers)
            {
                if (cr.GetAlpha() != 1f)
                {
                    Debug.LogWarning($"Alpha {cr.GetAlpha()} → 1로 강제 변경!");
                    cr.SetAlpha(1f);
                }
            }

            timer += Time.deltaTime;
            yield return null;  // 다음 프레임까지 대기
        }

        Debug.Log("Alpha 강제 설정 종료");
    }
    public void Show()
    {
        gameObject.SetActive(true);

        if (variableManager == null)
        {
            InitializeServices();
        }

        RefreshStageStates();

        // 0.1초 후에 강제로 불투명 처리
        Invoke("FixAlpha", 0.1f);
    }

    private void FixAlpha()
    {
        var allRenderers = GetComponentsInChildren<CanvasRenderer>(true);
        foreach (var cr in allRenderers)
        {
            cr.SetAlpha(1f);
        }
    }
  

    
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ForceSlotOpacity()
    {
        foreach (var slot in stageSlots)
        {
            if (slot.slotImage != null)
            {
                slot.slotImage.color = Color.white;

                // CanvasRenderer도 강제로! 
                var cr = slot.slotImage.GetComponent<CanvasRenderer>();
                if (cr != null) cr.SetAlpha(1f);
            }

            // 버튼의 모든 이미지들도
            if (slot.button != null)
            {
                var images = slot.button.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    img.color = Color.white;
                    var cr = img.GetComponent<CanvasRenderer>();
                    if (cr != null) cr.SetAlpha(1f);
                }
            }
        }
    }

    private void RefreshStageStates()
    {
        foreach (var slot in stageSlots)
        {
            bool isUnlocked = CheckStageUnlocked(slot.dayNumber);
            UpdateSlotVisual(slot, isUnlocked);
        }
    }


    // StageSelectUI.cs의 CheckStageUnlocked 메서드에 디버그 추가
    private bool CheckStageUnlocked(int day)
    {
        string unlockVariable = $"G_Day{day}_Unlocked";

        if (variableManager == null) return false;

        // bool로 먼저 시도
        if (variableManager.TryGetVariableValue<bool>(unlockVariable, out var boolValue))
        {
            Debug.Log($"[StageSelectUI] {unlockVariable} = {boolValue} (bool)");
            return boolValue;
        }

        // string으로 시도
        if (variableManager.TryGetVariableValue<string>(unlockVariable, out var stringValue))
        {
            Debug.Log($"[StageSelectUI] {unlockVariable} = '{stringValue}' (string)");
            // "true", "1", "True" 등을 true로 처리
            return stringValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   stringValue == "1";
        }

        Debug.Log($"[StageSelectUI] {unlockVariable} 변수가 없거나 읽을 수 없음");
        return false;
    }

    private void UpdateSlotVisual(StageSlot slot, bool isUnlocked)
    {
        slot.button.interactable = isUnlocked;

        if (slot.slotImage != null)
        {
            slot.slotImage.sprite = isUnlocked ? slot.unlockedSprite : slot.lockedSprite;
            slot.slotImage.color = Color.white;

            // CanvasRenderer 강제 설정! 
            var canvasRenderer = slot.slotImage.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                canvasRenderer.SetAlpha(1f);
            }
        }
    }
    private void OnStageSelected(StageSlot slot)
    {
        Debug.Log($"[StageSelectUI] {slot.dayNumber}일차 선택됨");

        // 이 부분이 실행되고 있나요?
        if (variableManager != null)
        {
            variableManager.SetVariableValue("SelectedDay", new CustomVariableValue(slot.dayNumber.ToString()));
            Debug.Log($"[StageSelectUI] SelectedDay = {slot.dayNumber}");
        }
        else
        {
            Debug.LogError("[StageSelectUI] variableManager가 NULL입니다!");
        }

        // HomeUI까지 올라가기
        Transform homeUITransform = transform.parent?.parent;

        if (homeUITransform != null)
        {
            var homeButtonHandler = homeUITransform.GetComponent<HomeUIButtonHandler>();

            gameObject.SetActive(false);

            if (homeButtonHandler != null)
            {
                // 새로 만든 메서드 호출
                homeButtonHandler.StartGameForStage();
            }
        }
    }

    private IEnumerator ResetGameState()
    {
        Debug.Log("[StageSelectUI] 게임 상태 초기화 시작");

        // 아이템 초기화
        var itemSystem = StateBasedItemSystem.Instance;
        if (itemSystem != null)
        {
            foreach (var itemData in itemSystem.GetAllItemData())
            {
                itemSystem.SetItemState(itemData.Key, StateBasedItemSystem.ItemState.NotOwned);
            }
        }

        // ItemDisplayUI 초기화
        if (uiManager != null)
        {
            var itemDisplayUI = uiManager.GetUI<ItemDisplayUI>();
            if (itemDisplayUI != null)
            {
                var restoreMethod = itemDisplayUI.GetType().GetMethod("RestoreItemIconsFromState");
                restoreMethod?.Invoke(itemDisplayUI, null);
            }
        }

        // 일반 변수 초기화
        ClearNonGlobalVariables();

        yield return new WaitForSeconds(0.1f);
        Debug.Log("[StageSelectUI] 게임 상태 초기화 완료");
    }

    private void ClearNonGlobalVariables()
    {
        // 스테이지 진행 관련 변수만 초기화
        // G_로 시작하지 않는 변수들
        if (variableManager != null && variableManager.VariableExists("CurrentDay"))
        {
            variableManager.SetVariableValue("CurrentDay", new CustomVariableValue("0"));
        }
    }

    private void InitializeStageVariables(int day)
    {
        if (variableManager != null)
        {
            variableManager.SetVariableValue("CurrentDay", new CustomVariableValue(day.ToString()));
            Debug.Log($"[StageSelectUI] {day}일차 초기화");
        }
    }

    private void OnDestroy()
    {
        // 엔진 이벤트 제거
        Engine.OnInitializationFinished -= OnEngineInitialized;

        // 모든 버튼 리스너 제거
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();

        if (stageSlots != null)
        {
            foreach (var slot in stageSlots)
            {
                if (slot != null && slot.button != null)
                {
                    slot.button.onClick.RemoveAllListeners();
                }
            }
        }
    }
}