using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName = "";

    [Header("Icon")]
    [SerializeField] private Image ImageLoader = null;

    private void Start()
    {
        //if (GlobalManager.Instance != null) GlobalManager.TimeStartScene = 0;
        StartCoroutine(AsyncLoad());
    }

    IEnumerator AsyncLoad()
    {
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            ImageLoader.fillAmount = operation.progress / 0.9f;

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }

}
