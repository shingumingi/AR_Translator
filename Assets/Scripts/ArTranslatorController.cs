using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArTranslatorController : MonoBehaviour
{
    [Header("참조")]
    public ArCameraCapture cameraCapture;
    public OcrClient ocrClient;
    public TranslatorClient translatorClient;

    [Header("UI")]
    public Dropdown languageDropdown;   // 0: 영어, 1: 한국어
    public Text resultText;
    public Text originText;

    bool isBusy = false;

    // 번역 버튼에서 이 함수 호출하게 연결해 두기
    public void OnClickTranslate()
    {
        if (isBusy) return;
        StartCoroutine(TranslateFlow());
    }

    IEnumerator TranslateFlow()
    {
        isBusy = true;
        if (resultText != null) resultText.text = "텍스트 인식 중...";
        if (originText != null) originText.text = "";

        // 1) 카메라 캡처 (코루틴 + 콜백)
        Texture2D tex = null;

        // 캡처가 끝나면 콜백에서 tex에 넣어줌
        yield return StartCoroutine(
            cameraCapture.CaptureCenterBox(t => tex = t)
        );

        if (tex == null)
        {
            if (resultText != null) resultText.text = "캡처 실패";
            isBusy = false;
            yield break;
        }

        // --- 여기 아래로는 아까 만든 언어/번역 방향 결정 코드 그대로 ---
        // 2) 언어 / 번역 방향 결정
        string srcLang;
        string targetLang;

        if (languageDropdown.value == 0)      // 영어 선택
        {
            srcLang = "en";
            targetLang = "ko";
        }
        else                                  // 한국어 선택
        {
            srcLang = "ko";
            targetLang = "en";
        }

        // 3) OCR
        string recognizedText = null;
        yield return StartCoroutine(
            ocrClient.RequestOcr(
                tex,
                OcrClient.OcrDomainType.Korean,
                t => recognizedText = t
            )
        );

        if (string.IsNullOrEmpty(recognizedText))
        {
            if (resultText != null) resultText.text = "텍스트 인식 실패";
            isBusy = false;
            yield break;
        }

        if (originText != null) originText.text = recognizedText;

        // 4) 번역
        if (resultText != null) resultText.text = "번역 중...";

        string translated = null;
        yield return StartCoroutine(
            translatorClient.RequestTranslate(
                recognizedText,
                srcLang,
                targetLang,
                t => translated = t
            )
        );

        if (string.IsNullOrEmpty(translated))
        {
            if (resultText != null) resultText.text = "번역 실패";
        }
        else
        {
            if (resultText != null) resultText.text = translated;
        }

        isBusy = false;
    }
}
