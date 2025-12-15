using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CenterBoxDraggable : MonoBehaviour
{
    [Header("최소/최대 크기(px)")]
    public float minWidth = 100f;
    public float maxWidth = 800f;
    public float minHeight = 100f;
    public float maxHeight = 800f;

    RectTransform rectTransform;
    Canvas canvas;

    // 이전 프레임의 두 손가락 사이 벡터
    bool hasPrevDiff = false;
    Vector2 prevDiff;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (Input.touchCount < 2)
        {
            hasPrevDiff = false;
            return;
        }

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 p0 = t0.position;
        Vector2 p1 = t1.position;

        Vector2 diff = p1 - p0;

        if (!hasPrevDiff)
        {
            prevDiff = diff;
            hasPrevDiff = true;
            return;
        }

        float deltaX = Mathf.Abs(diff.x) - Mathf.Abs(prevDiff.x);
        float deltaY = Mathf.Abs(diff.y) - Mathf.Abs(prevDiff.y);

        prevDiff = diff;

        float scale = (canvas != null) ? canvas.scaleFactor : 1f;
        deltaX /= scale;
        deltaY /= scale;

        Vector2 size = rectTransform.sizeDelta;
        size.x += deltaX;
        size.y += deltaY;

        size.x = Mathf.Clamp(size.x, minWidth, maxWidth);
        size.y = Mathf.Clamp(size.y, minHeight, maxHeight);

        rectTransform.sizeDelta = size;
    }
}
