using System.Collections;
using UnityEngine;

public class FPSTest : MonoBehaviour
{
    float fps;

    private void Start()
    {
        StartCoroutine(CheckFPS());
    }

    IEnumerator CheckFPS()
    {
        fps = 1f / Time.deltaTime;
        yield return new WaitForSeconds(1);
        StartCoroutine(CheckFPS());
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10,10,200,20), "FPS: " + Mathf.Round(fps));
    }
}
