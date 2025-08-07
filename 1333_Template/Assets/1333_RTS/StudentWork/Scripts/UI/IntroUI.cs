using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Simple multi-page introduction UI with “Next” and “Skip/Complete”.
public class IntroUI : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    private int page = 0;

    // When enabled, show the first page.
    private void OnEnable() => Show(0);

    // Advance to the next page if available.
    public void Next()
    {
        if (page < pages.Length - 1)
            Show(++page);
    }

    // Called by “Done” button; starts game load
    public void Complete() => Skip();

    // Skip all remaining pages and load the PlayerScene
    public void Skip() => StartCoroutine(BeginNewGame());

    // Activates the specified page and hides the others.
    private void Show(int idx)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == idx);
        page = idx;
    }

    // Loads PlayerScene asynchronously then switches state to Gameplay.
    private IEnumerator BeginNewGame()
    {
        GameManager.Instance.SetState(GameState.Loading);

        AsyncOperation op = SceneManager.LoadSceneAsync("PlayerScene");
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
            yield return null;
        op.allowSceneActivation = true;
    }
}
