using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ArTranslatorController : MonoBehaviour
{
    public ArCameraCapture cameraCapture;
    public OcrClient ocrClient;
    public TranslatorClient translatorClient;

    public Button translateButton;
    public Text resultText;
    public Dropdown languageDropdown;

    public string targetLang = "ko";
    bool isBusy = false;

    void Start()
    {
        translateButton.onClick.AddListener(OnClickTranslate);
    }

    void OnClickTranslate()
    {
        if (!isBusy)
            StartCoroutine(TranslateFlow());
    }

    IEnumerator TranslateFlow()
    {
        isBusy = true;

        // 캡처
        if (resultText) resultText.text = "캡처 중...";

        Texture2D tex = null;

        yield return StartCoroutine(
            cameraCapture.CaptureCenterBox(t => tex = t)
        );

        if (tex == null)
        {
            if (resultText) resultText.text = "캡처 실패";
            isBusy = false;
            yield break;
        }

        // 언어/도메인 결정
        string srcLang;
        OcrClient.OcrDomainType domainType;

        if (languageDropdown.value == 0)
        {
            srcLang = "en";
            domainType = OcrClient.OcrDomainType.Korean;
        }
        else
        {
            srcLang = "ja";
            domainType = OcrClient.OcrDomainType.Japanese;
        }

        // 3) OCR
        if (resultText) resultText.text = "텍스트 인식 중...";
        string recognizedText = null;

        yield return StartCoroutine(
            ocrClient.RequestOcr(tex, domainType, t => recognizedText = t)
        );

        if (string.IsNullOrEmpty(recognizedText))
        {
            if (resultText) resultText.text = "텍스트 인식 실패";
            isBusy = false;
            yield break;
        }

        // 4) 번역
        if (resultText) resultText.text = "번역 중...";
        string translated = null;

        yield return StartCoroutine(
            translatorClient.RequestTranslate(recognizedText, srcLang, targetLang, t => translated = t)
        );

        if (string.IsNullOrEmpty(translated))
            resultText.text = "번역 실패";
        else
            resultText.text = translated;

        isBusy = false;
    }
}
