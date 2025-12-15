using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OcrDebugTester : MonoBehaviour
{
    [Header("참조")]
    public TranslatorClient translatorClient;     // Papago 호출 담당

    [Header("UI")]
    public InputField inputField;             // 번역할 문장 입력
    public Dropdown directionDropdown;        // 0: 한->영, 1: 영->한
    public Text resultText;            // 번역 결과 표시

    bool isBusy = false;

    // 버튼에서 이 함수 연결
    public void OnClickTranslate()
    {
        if (isBusy) return;

        if (translatorClient == null)
        {
            if (resultText != null)
                resultText.text = "TranslatorClient 없음";
            return;
        }

        if (inputField == null)
        {
            if (resultText != null)
                resultText.text = "InputField 없음";
            return;
        }

        string text = inputField.text;
        if (string.IsNullOrWhiteSpace(text))
        {
            if (resultText != null)
                resultText.text = "입력 문장이 없습니다.";
            return;
        }

        // 번역 방향 결정
        string srcLang, targetLang;
        if (directionDropdown == null || directionDropdown.value == 0)
        {
            // 0: 한국어 -> 영어
            srcLang = "ko";
            targetLang = "en";
        }
        else
        {
            // 1: 영어 -> 한국어
            srcLang = "en";
            targetLang = "ko";
        }

        StartCoroutine(TranslateRoutine(text, srcLang, targetLang));
    }

    IEnumerator TranslateRoutine(string text, string srcLang, string targetLang)
    {
        isBusy = true;

        if (resultText != null)
            resultText.text = "Papago 번역 중...";

        string translated = null;

        // TranslatorClient의 Papago 요청 호출
        yield return StartCoroutine(
            translatorClient.RequestTranslate(
                text,
                srcLang,
                targetLang,
                t => translated = t
            )
        );

        if (string.IsNullOrEmpty(translated))
        {
            if (resultText != null)
                resultText.text = "번역 실패 (null 또는 빈 문자열)";
        }
        else
        {
            if (resultText != null)
                resultText.text = translated;
        }

        isBusy = false;
    }
}
