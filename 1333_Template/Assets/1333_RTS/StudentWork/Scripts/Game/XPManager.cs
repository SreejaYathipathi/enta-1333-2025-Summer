using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    /* Animation */
    [SerializeField, Range(0.05f, 1f)] private float barLerpTime = .3f;
    Coroutine barRoutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        UpdateUI(instant: true);             // draw 0 / first-level target
    }

    // ------------------------------------------------------------------ XP
    public void AddXP(int amount)
    {
        CurrentXP += Mathf.Max(0, amount);

        while (CurrentLevel < levelTable.MaxLevel &&
               CurrentXP >= levelTable.GetXpForLevel(CurrentLevel + 1))
            CurrentLevel++;

        UpdateUI(instant: false);
    }

    public void SetXPAndLevel(int xp, int lvl)
    {
        CurrentXP = Mathf.Max(0, xp);
        CurrentLevel = Mathf.Clamp(lvl, 1, levelTable.MaxLevel);
        UpdateUI(instant: true);
    }

    // ------------------------------------------------------------------ UI
    void UpdateUI(bool instant)
    {
        if (levelText)
            levelText.text = $"Lvl {CurrentLevel}";

        int prevTotal = levelTable.GetXpForLevel(CurrentLevel);
        int nextTotal = levelTable.GetXpForLevel(CurrentLevel + 1);

        // bar fraction against next-level total (0…1)
        float fillTarget = (float)CurrentXP / nextTotal;

        // numerator / denominator text (cumulative!)
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

    IEnumerator LerpFill(float target)
    {
        fgImage.fillAmount = 0f;             // start empty each update
        float t = 0f;
        while (t < barLerpTime)
        {
            t += Time.deltaTime;
            fgImage.fillAmount = Mathf.Lerp(0f, target, t / barLerpTime);
            yield return null;
        }
        fgImage.fillAmount = target;
    }
}
