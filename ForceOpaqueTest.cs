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
        // 1����, 2����, 3���� �̸����� ã��
        var slots = new string[] { "1����", "2����", "3����" };

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
                        Debug.LogError($"{slotName}�� {img.name} ���İ�: {img.color.a}");
                        Debug.LogError($"�θ� ü��: {GetParentChain(img.transform)}");
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