using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ArCameraCapture : MonoBehaviour
{
    [SerializeField] ARCameraManager cameraManager;
    [SerializeField] RectTransform centerBox;

    public IEnumerator CaptureCenterBox(Action<Texture2D> onCaptured)
    {
        if (cameraManager == null)
        {
            onCaptured?.Invoke(null);
            yield break;
        }

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage))
        {
            onCaptured?.Invoke(null);
            yield break;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
            outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorX
        };

        var rawTextureData = new NativeArray<byte>(conversionParams.outputDimensions.x * conversionParams.outputDimensions.y * 4, Allocator.Temp);
        cpuImage.Convert(conversionParams, rawTextureData);
        cpuImage.Dispose();

        Texture2D fullTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false
        );
        fullTexture.LoadRawTextureData(rawTextureData);
        fullTexture.Apply();
        rawTextureData.Dispose();

        Vector3[] worldCorners = new Vector3[4];
        centerBox.GetWorldCorners(worldCorners);

        Vector2 minScreen = RectTransformUtility.WorldToScreenPoint(null, worldCorners[0]);
        Vector2 maxScreen = RectTransformUtility.WorldToScreenPoint(null, worldCorners[2]);

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        int x = Mathf.RoundToInt(minScreen.x / screenWidth * fullTexture.width);
        int y = Mathf.RoundToInt(minScreen.y / screenHeight * fullTexture.height);
        int w = Mathf.RoundToInt((maxScreen.x - minScreen.x) / screenWidth * fullTexture.width);
        int h = Mathf.RoundToInt((maxScreen.y - minScreen.y) / screenHeight * fullTexture.height);

        x = Mathf.Clamp(x, 0, fullTexture.width - 1);
        y = Mathf.Clamp(y, 0, fullTexture.height - 1);
        w = Mathf.Clamp(w, 1, fullTexture.width - x);
        h = Mathf.Clamp(h, 1, fullTexture.height - y);

        Texture2D cropped = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] pixels = fullTexture.GetPixels(x, y, w, h);
        cropped.SetPixels(pixels);
        cropped.Apply();

        Destroy(fullTexture);

        onCaptured?.Invoke(cropped);
    }
}
