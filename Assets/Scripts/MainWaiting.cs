using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainWaiting : MonoBehaviour
{
    // 버튼의 OnClick 이벤트에 연결할 함수
    public void ChangeSceneWithDelay()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    // 0.2초 대기 후 씬을 로드하는 코루틴
    IEnumerator LoadSceneAfterDelay()
    {
        // 0.2초 기다림
        yield return new WaitForSeconds(0.2f);

        // "SampleScene"으로 전환
        SceneManager.LoadScene("SampleScene");
    }
}