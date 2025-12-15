using UnityEngine;

public class Test : MonoBehaviour
{
    void Update()
    {
        // 10프레임마다 한 번씩만 찍어서 로그 폭주 방지
        if (Time.frameCount % 10 == 0)
        {
            Debug.Log($"[TouchLogger] touchCount = {Input.touchCount}");
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            Debug.Log($"[TouchLogger] #{i} phase={t.phase}, pos={t.position}");
        }
    }
}
