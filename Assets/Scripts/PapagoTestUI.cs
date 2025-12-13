using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PapagoTestUI : MonoBehaviour
{
    public TranslatorClient translator;
    public InputField inputField;
    public Text resultText;
    public Button translateButton;

    public string sourceLang = "ko";
    public string targetLang = "en";

    bool isBusy = false;

    void Start()
    {
        translateButton.onClick.AddListener(OnClickTranslate);
    }

    void OnClickTranslate()
    {
        if (isBusy) return;
        StartCoroutine(TranslateFlow());
    }

    IEnumerator TranslateFlow()
    {
        isBusy = true;
        string text = inputField.text;
        resultText.text = "번역 중...";

        string translated = null;
        yield return StartCoroutine(
            translator.RequestTranslate(text, sourceLang, targetLang, t => translated = t)
        );

        if (string.IsNullOrEmpty(translated))
        {
            resultText.text = "번역 실패 (콘솔 로그 확인)";
        }
        else
        {
            resultText.text = translated;
        }

        isBusy = false;
    }
}
