using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TranslatorClient : MonoBehaviour
{
    [Header("Papago Translation 설정")]
    public string endpointUrl = "https://papago.apigw.ntruss.com/nmt/v1/translation";

    public string clientId;
    public string clientSecret;

    [Serializable]
    private class PapagoResult
    {
        public string srcLangType;
        public string tarLangType;
        public string translatedText;
    }

    [Serializable]
    private class PapagoMessage
    {
        public PapagoResult result;
    }

    [Serializable]
    private class PapagoResponse
    {
        public PapagoMessage message;
    }

    // 번역 요청
    public IEnumerator RequestTranslate(string text, string sourceLang, string targetLang, Action<string> onResult)
    {
        if (string.IsNullOrEmpty(text))
        {
            onResult?.Invoke(null);
            yield break;
        }

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            onResult?.Invoke(null);
            yield break;
        }

        string body =
            $"source={UnityWebRequest.EscapeURL(sourceLang)}" +
            $"&target={UnityWebRequest.EscapeURL(targetLang)}" +
            $"&text={UnityWebRequest.EscapeURL(text)}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);

        using (UnityWebRequest uwr = new UnityWebRequest(endpointUrl, "POST"))
        {
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            uwr.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", clientId);
            uwr.SetRequestHeader("X-NCP-APIGW-API-KEY", clientSecret);

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                onResult?.Invoke(null);
                yield break;
            }

            string json = uwr.downloadHandler.text;

            PapagoResponse res = null;
            try
            {
                res = JsonUtility.FromJson<PapagoResponse>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("[Papago] JSON 파싱 오류: " + e);
            }

            if (res != null && res.message != null && res.message.result != null &&
                !string.IsNullOrEmpty(res.message.result.translatedText))
            {
                onResult?.Invoke(res.message.result.translatedText);
            }
            else
            {
                onResult?.Invoke(null);
            }
        }
    }
}
