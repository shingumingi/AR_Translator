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
        if (image == null)
        {
            onResult?.Invoke(null);
            yield break;
        }

        string url = null;
        string key = null;

        switch (domainType)
        {
            case OcrDomainType.Korean:
                url = koreanOcrUrl;
                key = koreanSecretKey;
                break;
            case OcrDomainType.Japanese:
                url = japaneseOcrUrl;
                key = japaneseSecretKey;
                break;
        }

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
        {
            onResult?.Invoke(null);
            yield break;
        }

        // 2) 이미지 → JPG → Base64
        byte[] imageBytes = image.EncodeToJPG(90);
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

        using (UnityWebRequest uwr = new UnityWebRequest(url, "POST"))
        {
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            uwr.SetRequestHeader("X-OCR-SECRET", key);

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                onResult?.Invoke(null);
                yield break;
            }

            string resJson = uwr.downloadHandler.text;

            OcrResponse res = null;
            try
            {
                res = JsonUtility.FromJson<OcrResponse>(resJson);
            }
            catch (Exception e)
            {
                Debug.LogError("[CLOVA OCR] JSON 파싱 오류: " + e);
            }

            if (res == null || res.images == null || res.images.Length == 0 ||
                res.images[0].fields == null || res.images[0].fields.Length == 0)
            {
                onResult?.Invoke(null);
                yield break;
            }

            var sb = new StringBuilder();
            foreach (var field in res.images[0].fields)
            {
                if (field == null || string.IsNullOrEmpty(field.inferText))
                    continue;

                if (sb.Length > 0) sb.Append(" ");
                sb.Append(field.inferText);
            }

            string recognizedText = sb.ToString();

            onResult?.Invoke(recognizedText);
        }
    }
}
