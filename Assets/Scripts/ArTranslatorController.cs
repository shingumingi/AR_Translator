using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArTranslatorController : MonoBehaviour
{
    public ArCameraCapture cameraCapture;
    public OcrClient ocrClient;
    public TranslatorClient translatorClient;

    public Button translateButton;
    public Text resultText;

    public string sourceLang = "en"; // 찍을거
    public string targetLang = "ko"; // 나올거

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
        resultText.text = "인식 중...";

        Texture2D captured = null;
        yield return StartCoroutine(cameraCapture.CaptureCenterBox(tex => captured = tex));

        if (captured == null)
        {
            resultText.text = "카메라 캡처 실패";
            isBusy = false;
            yield break;
        }

        // OCR
        string recognizedText = null;
        resultText.text = "텍스트 인식 중...";
        yield return StartCoroutine(ocrClient.RequestOcr(captured, text => recognizedText = text));

        if (string.IsNullOrEmpty(recognizedText))
        {
            resultText.text = "텍스트 인식 실패";
            isBusy = false;
            yield break;
        }

        // 번역
        string translatedText = null;
        resultText.text = "번역 중...";
        yield return StartCoroutine(translatorClient.RequestTranslate(recognizedText, sourceLang, targetLang, t => translatedText = t));

        if (string.IsNullOrEmpty(translatedText))
        {
            resultText.text = "번역 실패";
            isBusy = false;
            yield break;
        }

        resultText.text = translatedText;
        isBusy = false;
    }
}
