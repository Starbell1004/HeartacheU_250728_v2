using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaterialDebugger : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CheckMaterials());
    }

    IEnumerator CheckMaterials()
    {
        yield return new WaitForSeconds(1f);

        var slots = GameObject.FindObjectsOfType<Image>();
        foreach (var img in slots)
        {
            if (img.name.Contains("일차") || img.transform.parent?.name.Contains("일차") == true)
            {
                Debug.Log($"{img.name} - Material: {img.material}, Color: {img.color}");

                // Material 강제 리셋
                img.material = null;
            }
        }

        // CanvasRenderer도 체크
        var canvasRenderers = GameObject.FindObjectsOfType<CanvasRenderer>();
        foreach (var cr in canvasRenderers)
        {
            if (cr.name.Contains("일차"))
            {
                Debug.Log($"{cr.name} - CanvasRenderer Alpha: {cr.GetAlpha()}");
                cr.SetAlpha(1f);
            }
        }
    }
}