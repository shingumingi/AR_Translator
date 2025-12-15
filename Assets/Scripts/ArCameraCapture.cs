using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArCameraCapture : MonoBehaviour
{
    [Header("캡쳐할 UI 박스 (Canvas 안의 CenterBox)")]
    public RectTransform centerBox;

    [Header("화면을 렌더하는 카메라 (AR 메인 카메라)")]
    public Camera captureCamera;

    public IEnumerator CaptureCenterBox(Action<Texture2D> onCaptured)
    {
        // 한 프레임 다 그린 뒤 픽셀 읽기
        yield return new WaitForEndOfFrame();

        if (centerBox == null)
        {
            Debug.LogError("[ArCameraCapture] centerBox 가 비어 있습니다.");
            onCaptured?.Invoke(null);
            yield break;
        }

        // 1) CenterBox 의 월드 코너 4개 얻기
        Vector3[] worldCorners = new Vector3[4];
        centerBox.GetWorldCorners(worldCorners);
        // [0]=왼쪽 아래, [2]=오른쪽 위

        // 2) 화면 좌표로 변환
        Camera cam = captureCamera;

        // Canvas가 Screen Space - Overlay 면 카메라 null 로 처리
        var canvas = centerBox.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            cam = null;

        Vector3 sp0 = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[0]); // BL
        Vector3 sp2 = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[2]); // TR

        // 혹시 순서가 뒤집혀 있더라도 안전하게 min/max 로 처리
        float xMin = Mathf.Min(sp0.x, sp2.x);
        float yMin = Mathf.Min(sp0.y, sp2.y);
        float width = Mathf.Abs(sp2.x - sp0.x);
        float height = Mathf.Abs(sp2.y - sp0.y);

        // 3) 화면 범위 안으로 클램프
        xMin = Mathf.Clamp(xMin, 0, Screen.width - 1);
        yMin = Mathf.Clamp(yMin, 0, Screen.height - 1);

        if (xMin + width > Screen.width) width = Screen.width - xMin;
        if (yMin + height > Screen.height) height = Screen.height - yMin;

        // 너무 작으면 실패로 처리
        if (width < 10 || height < 10)
        {
            Debug.LogWarning($"[ArCameraCapture] 캡쳐 영역이 너무 작습니다. ({width} x {height})");
            onCaptured?.Invoke(null);
            yield break;
        }

        Rect rect = new Rect(xMin, yMin, width, height);

        // 4) 실제 픽셀 캡쳐
        Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();

        onCaptured?.Invoke(tex);
    }
}
