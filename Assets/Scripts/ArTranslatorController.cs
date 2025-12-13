using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArTranslatorController : MonoBehaviour
{
    [Header("참조")]
    public ArCameraCapture cameraCapture;
    public OcrClient ocrClient;
    public TranslatorClient translatorClient;

    [Header("UI")]
    public Button translateButton;
    public Text resultText;

    [Header("언어 설정")]
    public string sourceLang = "en";        // 바뀔거
    public string targetLang = "ko";        // 바꿀거

    bool isBusy = false;

    void Start()
    {
        if (translateButton != null)
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

        if (resultText != null)
            resultText.text = "카메라 캡처 중...";

        // 카메라에서 캡처
        Texture2D captured = null;
        yield return StartCoroutine(cameraCapture.CaptureCenterBox(tex => captured = tex));

        if (captured == null)
        {
            if (resultText != null)
                resultText.text = "캡처 실패";
            isBusy = false;
            yield break;
        }

        // OCR
        if (resultText != null)
            resultText.text = "텍스트 인식 중...";

        string recognizedText = null;
        yield return StartCoroutine(
            ocrClient.RequestOcr(captured, t => recognizedText = t)
        );

        if (string.IsNullOrEmpty(recognizedText))
        {
            if (resultText != null)
                resultText.text = "텍스트 인식 실패";
            isBusy = false;
            yield break;
        }

        Debug.Log("OCR 결과: " + recognizedText);

        // 번역
        if (resultText != null)
            resultText.text = "번역 중...";

        string translatedText = null;
        yield return StartCoroutine(
            translatorClient.RequestTranslate(recognizedText, sourceLang, targetLang, t => translatedText = t)
        );

        if (string.IsNullOrEmpty(translatedText))
        {
            if (resultText != null)
                resultText.text = "번역 실패 (콘솔 확인)";
            isBusy = false;
            yield break;
        }

        // 결과
        if (resultText != null)
            resultText.text = translatedText;

        isBusy = false;
    }
}
