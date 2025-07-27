using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using System.Collections;

public class AchievementManager : MonoBehaviour
{
    private static AchievementManager instance;
    public static AchievementManager Instance => instance;

    [Header("�������� ������")]
    [SerializeField] private AchievementData[] allAchievements;

    [Header("�˾� UI")]
    [SerializeField] private GameObject achievementPopup;
    [SerializeField] private Text popupTitleText;
    [SerializeField] private Text popupDescriptionText;
    [SerializeField] private float popupDuration = 3f;

    private ICustomVariableManager variableManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }
    }

    private void InitializeServices()
    {
        Engine.OnInitializationFinished -= InitializeServices;
        variableManager = Engine.GetService<ICustomVariableManager>();
    }

    // �������� �޼� üũ (���ϳ뺧 ��ũ��Ʈ���� ȣ��)
    public void UnlockAchievement(string achievementID)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.achievementID == achievementID)
            {
                // �̹� �޼��ߴ��� üũ
                if (IsAchievementUnlocked(achievement.unlockVariable))
                    return;

                // �޼� ó��
                if (variableManager != null)
                {
                    variableManager.SetVariableValue(achievement.unlockVariable, new CustomVariableValue("true"));
                }

                // �˾� ǥ��
                ShowAchievementPopup(achievement);
                break;
            }
        }
    }

    private bool IsAchievementUnlocked(string unlockVariable)
    {
        if (variableManager == null || string.IsNullOrEmpty(unlockVariable))
            return false;

        if (variableManager.VariableExists(unlockVariable))
        {
            var value = variableManager.GetVariableValue(unlockVariable);
            return value.ToString() == "true";
        }

        return false;
    }

    private void ShowAchievementPopup(AchievementData achievement)
    {
        if (achievementPopup == null) return;

        // �˾� �ؽ�Ʈ ����
        if (popupTitleText != null)
            popupTitleText.text = "�������� �޼�!";

        if (popupDescriptionText != null)
            popupDescriptionText.text = achievement.title;

        // �˾� ǥ��
        StartCoroutine(ShowPopupCoroutine());
    }

    private IEnumerator ShowPopupCoroutine()
    {
        achievementPopup.SetActive(true);

        // ���̵��� ȿ�� (���û���)
        CanvasGroup canvasGroup = achievementPopup.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            float elapsed = 0;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / 0.5f);
                yield return null;
            }
        }

        yield return new WaitForSeconds(popupDuration);

        // ���̵�ƿ� ȿ�� (���û���)
        if (canvasGroup != null)
        {
            float elapsed = 0;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / 0.5f);
                yield return null;
            }
        }

        achievementPopup.SetActive(false);
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;
    }
}