using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class UIGameSceneManager : MonoBehaviour
{
    [Header("Additional Loading Scene Feature")]
    [SerializeField] private Slider _loadingBar = null;
    [SerializeField] private Text _loadingText = null;

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            float progress = asyncLoad.progress * 1000 / 9f;

            if (_loadingBar != null)
                _loadingBar.value = progress;

            if (_loadingText != null)
                _loadingText.text = $"{progress}%";
            
            yield return null;
        }
    }
}
