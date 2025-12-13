using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class OcrClient : MonoBehaviour
{
    [Header("OCR API 설정")]
    public string endpointUrl;
    public string apiKey;

    public IEnumerator RequestOcr(Texture2D image, Action<string> onResult)
    {
        if (image == null)
        {
            onResult?.Invoke(null);
            yield break;
        }

        byte[] pngBytes = image.EncodeToPNG();

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", pngBytes, "image.png", "image/png");

        UnityWebRequest uwr = UnityWebRequest.Post(endpointUrl, form);
        uwr.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            string json = uwr.downloadHandler.text;
            // TODO: json 파싱해서 인식된 텍스트만 뽑기
            string recognizedText = ParseOcrResult(json);
            onResult?.Invoke(recognizedText);
        }
        else
            onResult?.Invoke(null);
    }

    string ParseOcrResult(string json)
    {
        return json;
    }
}
