using UnityEngine;
using UnityEngine.UI;

public class ForceOpaqueTest : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("ForceOpaque", 0f, 0.1f);
    }

    void ForceOpaque()
    {
        // 1일차, 2일차, 3일차 이름으로 찾기
        var slots = new string[] { "1일차", "2일차", "3일차" };

        foreach (var slotName in slots)
        {
            var slotObj = GameObject.Find(slotName);
            if (slotObj != null)
            {
                var images = slotObj.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.color.a < 1f)
                    {
                        Debug.LogError($"{slotName}의 {img.name} 알파값: {img.color.a}");
                        Debug.LogError($"부모 체인: {GetParentChain(img.transform)}");
                    }
                    img.color = new Color(1, 1, 1, 1);
                }
            }
        }
    }

    string GetParentChain(Transform t)
    {
        string chain = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            chain = t.name + " > " + chain;
        }
        return chain;
    }
}