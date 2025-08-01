using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroUI : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    private int page = 0;

    private void OnEnable() => Show(0);

    public void Next()
    {
        if (page < pages.Length - 1)
            Show(++page);
    }

    public void Complete() => Skip();
    public void Skip() => StartCoroutine(BeginNewGame());

    private void Show(int idx)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == idx);
        page = idx;
    }

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
