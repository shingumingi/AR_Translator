using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OcrClient : MonoBehaviour
{
    public enum OcrDomainType
    {
        Korean,
        Japanese
    }

    [Header("Korean / English OCR 도메인 (지원 언어: 한국어)")]
    public string koreanOcrUrl;
    public string koreanSecretKey;

    [Header("Japanese OCR 도메인 (지원 언어: 일본어)")]
    public string japaneseOcrUrl;
    public string japaneseSecretKey;

    [Serializable]
    private class OcrImage
    {
        public string format;
        public string name;
        public string data;
    }

    [Serializable]
    private class OcrRequest
    {
        public string version;
        public string requestId;
        public long timestamp;
        public OcrImage[] images;
    }

    [Serializable]
    private class OcrField
    {
        public string inferText;
    }

    [Serializable]
    private class OcrImageResult
    {
        public OcrField[] fields;
    }

    [Serializable]
    private class OcrResponse
    {
        public OcrImageResult[] images;
    }

    public IEnumerator RequestOcr(Texture2D image, OcrDomainType domainType, Action<string> onResult)
    {
        // 0) 이미지 체크
        if (image == null)
        {
            Debug.LogError("[CLOVA OCR] image 가 null 입니다.");
            onResult?.Invoke(null);
            yield break;
        }

        // 1) 도메인 선택
        string url = null;
        string key = null;

        switch (domainType)
        {
            case OcrDomainType.Korean:
            case OcrDomainType.Japanese:
                url = koreanOcrUrl;
                key = koreanSecretKey;
                break;
        }

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
        {
            Debug.LogError($"[CLOVA OCR] 선택한 도메인({domainType})의 URL 또는 SecretKey가 비어 있습니다.");
            onResult?.Invoke(null);
            yield break;
        }

        Debug.Log($"[CLOVA OCR] Domain = {domainType}, Url = {url}");

        // 2) 이미지 → JPG → Base64
        byte[] imageBytes = null;
        try
        {
            imageBytes = image.EncodeToJPG(90);
        }
        catch (Exception e)
        {
            Debug.LogError("[CLOVA OCR] EncodeToJPG 예외: " + e);
            onResult?.Invoke(null);
            yield break;
        }

        if (imageBytes == null || imageBytes.Length == 0)
        {
            Debug.LogError("[CLOVA OCR] EncodeToJPG 결과 byte[] 가 null 이거나 길이 0 입니다.");
            onResult?.Invoke(null);
            yield break;
        }

        string imageBase64 = Convert.ToBase64String(imageBytes);

        var req = new OcrRequest
        {
            version = "V2",
            requestId = Guid.NewGuid().ToString(),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            images = new[]
            {
            new OcrImage
            {
                format = "jpg",
                name   = "centerBox",
                data   = imageBase64
            }
        }
        };

        string json = JsonUtility.ToJson(req);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // 3) HTTP 요청
        using (UnityWebRequest uwr = new UnityWebRequest(url, "POST"))
        {
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            uwr.SetRequestHeader("X-OCR-SECRET", key);

            Debug.Log("[CLOVA OCR] HTTP 요청 전송 중...");
            yield return uwr.SendWebRequest();
            Debug.Log($"[CLOVA OCR] HTTP 결과: {uwr.result}, code={uwr.responseCode}");

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CLOVA OCR] HTTP 실패: {uwr.error}");
                Debug.LogError("[CLOVA OCR] 응답 바디: " + uwr.downloadHandler.text);
                onResult?.Invoke(null);
                yield break;
            }

            string resJson = uwr.downloadHandler.text;
            Debug.Log("[CLOVA OCR] 응답 JSON: " + resJson);

            // 4) JSON 파싱
            OcrResponse res = null;
            try
            {
                res = JsonUtility.FromJson<OcrResponse>(resJson);
            }
            catch (Exception e)
            {
                Debug.LogError("[CLOVA OCR] JSON 파싱 오류: " + e);
                onResult?.Invoke(null);
                yield break;
            }

            if (res == null || res.images == null || res.images.Length == 0)
            {
                Debug.LogWarning("[CLOVA OCR] 응답에 images 배열이 없습니다.");
                onResult?.Invoke(null);
                yield break;
            }

            if (res.images[0].fields == null || res.images[0].fields.Length == 0)
            {
                Debug.LogWarning("[CLOVA OCR] images[0].fields 가 비어 있습니다. (텍스트 인식 실패)");
                onResult?.Invoke(null);
                yield break;
            }

            // 5) 텍스트 합치기
            var sb = new StringBuilder();
            foreach (var field in res.images[0].fields)
            {
                if (field == null || string.IsNullOrEmpty(field.inferText))
                    continue;

                if (sb.Length > 0) sb.Append(" ");
                sb.Append(field.inferText);
            }

            string recognizedText = sb.ToString();
            Debug.Log("[CLOVA OCR] 최종 텍스트: " + recognizedText);

            onResult?.Invoke(recognizedText);
        }
    }
}
