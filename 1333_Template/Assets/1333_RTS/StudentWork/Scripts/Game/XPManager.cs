using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;

// Manages XP and level progression, handles UI updates, and persists across scenes
public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private LevelTable levelTable;

    [Header("Bar Images")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image fgImage;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text levelText;

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentXP { get; private set; } = 0;

    [SerializeField, Range(0.05f, 1f)] private float barLerpTime = .3f;
    Coroutine barRoutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        SceneManager.sceneLoaded += OnSceneLoaded;

        UpdateUI(instant: true);
    }

    // Unsubscribe on destroy
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Adds XP and increases level if threshold is passed
    public void AddXP(int amount)
    {
        CurrentXP += Mathf.Max(0, amount);

        while (CurrentLevel < levelTable.MaxLevel &&
               CurrentXP >= levelTable.GetXpForLevel(CurrentLevel + 1))
            CurrentLevel++;

        UpdateUI(instant: false);
    }

    // Directly sets XP and level (used for loading)
    public void SetXPAndLevel(int xp, int lvl)
    {
        CurrentXP = Mathf.Max(0, xp);
        CurrentLevel = Mathf.Clamp(lvl, 1, levelTable.MaxLevel);
        UpdateUI(instant: true);
    }

    // Updates XP bar and text, with optional animation
    void UpdateUI(bool instant)
    {
        if (levelText)
            levelText.text = $"Lvl {CurrentLevel}";

        int prevTotal = levelTable.GetXpForLevel(CurrentLevel);
        int nextTotal = levelTable.GetXpForLevel(CurrentLevel + 1);

        float fillTarget = (float)CurrentXP / nextTotal;

        if (xpText) xpText.text = $"{CurrentXP} / {nextTotal}";

        if (fgImage == null) return;

        if (barRoutine != null) StopCoroutine(barRoutine);

        if (instant || barLerpTime <= 0f)
        {
            fgImage.fillAmount = fillTarget;
        }
        else
        {
            barRoutine = StartCoroutine(LerpFill(fillTarget));
        }
    }

    // Smoothly fills XP bar over time
    IEnumerator LerpFill(float target)
    {
        fgImage.fillAmount = 0f;
        float t = 0f;
        while (t < barLerpTime)
        {
            t += Time.deltaTime;
            fgImage.fillAmount = Mathf.Lerp(0f, target, t / barLerpTime);
            yield return null;
        }
        fgImage.fillAmount = target;
    }

    // Reassigns UI references when PlayerScene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "PlayerScene") return;

        if (fgImage == null) fgImage = GameObject.FindWithTag("XP_FG")?.GetComponent<Image>();
        if (xpText == null) xpText = GameObject.FindWithTag("XP_Text")?.GetComponent<TMP_Text>();
        if (levelText == null) levelText = GameObject.FindWithTag("Level_Text")?.GetComponent<TMP_Text>();

        ForceRefresh();
    }

    // Instantly refreshes UI with current XP/Level
    public void ForceRefresh() => UpdateUI(instant: true);
}
