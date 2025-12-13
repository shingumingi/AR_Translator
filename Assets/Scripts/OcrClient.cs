using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OcrClient : MonoBehaviour
{
    [Header("CLOVA OCR 설정")]
    public string ocrUrl;
    public string secretKey;

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

    public IEnumerator RequestOcr(Texture2D image, Action<string> onResult)
    {
        if (image == null)
        {
            onResult?.Invoke(null);
            yield break;
        }

        if (string.IsNullOrEmpty(ocrUrl) || string.IsNullOrEmpty(secretKey))
        {
            onResult?.Invoke(null);
            yield break;
        }

        // 이미지 -> JPG -> 문자열
        byte[] imageBytes = image.EncodeToJPG(90);
        string imageBase64 = Convert.ToBase64String(imageBytes);

        // 요청 바디
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

        using (UnityWebRequest uwr = new UnityWebRequest(ocrUrl, "POST"))
        {
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            uwr.SetRequestHeader("X-OCR-SECRET", secretKey);

            // 요청
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

                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(field.inferText);
            }

            string recognizedText = sb.ToString();

            onResult?.Invoke(recognizedText);
        }
    }
}
