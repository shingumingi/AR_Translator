using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainWaiting : MonoBehaviour
{
    public void ChangeSceneWithDelay()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    IEnumerator LoadSceneAfterDelay()
    {
        // 0.2초 기다림
        yield return new WaitForSeconds(0.2f);

        // "SampleScene"으로 전환
        SceneManager.LoadScene("SampleScene");
    }
}