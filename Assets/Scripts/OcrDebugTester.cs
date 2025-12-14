using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OcrDebugTester : MonoBehaviour
{
    public OcrClient ocrClient;
    public TranslatorClient translatorClient;
    public Texture2D testImage;

    public OcrClient.OcrDomainType domainType
        = OcrClient.OcrDomainType.Korean;

    public Text resultText;

    private void Start()
    {
        StartCoroutine(RunTest());
    }

    IEnumerator RunTest()
    {
        if (testImage == null)
        {
            Debug.LogError("[OCR TEST] testImage가 비어 있습니다.");
            yield break;
        }

        Debug.Log("[OCR TEST] 시작 - DomainType = " + domainType);

        string ocrResult = null;

        // OCR
        yield return StartCoroutine(
            ocrClient.RequestOcr(testImage, domainType, r => ocrResult = r)
        );

        if (string.IsNullOrEmpty(ocrResult))
        {
            Debug.LogWarning("[OCR TEST] 인식 실패 (null 또는 빈 문자열)");
            if (resultText != null) resultText.text = "인식 실패";
            yield break;
        }

        Debug.Log("[OCR TEST] 인식 결과(원문): " + ocrResult);

        // 번역 (영어 -> 한국어)
        if (translatorClient == null)
        {
            Debug.LogError("[OCR TEST] TranslatorClient가 비어 있습니다.");
            yield break;
        }

        string translated = null;

        yield return StartCoroutine(
            translatorClient.RequestTranslate(
                ocrResult,
                "en",
                "ko",
                t => translated = t
            )
        );

        if (string.IsNullOrEmpty(translated))
        {
            Debug.LogWarning("[OCR TEST] 번역 실패");
            if (resultText != null) resultText.text = "번역 실패";
        }
        else
        {
            Debug.Log("[OCR TEST] 번역 결과(한국어): " + translated);
            if (resultText != null) resultText.text = translated;
        }
    }
}
