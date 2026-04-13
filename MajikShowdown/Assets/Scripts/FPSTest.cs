using System.Collections;
using TMPro;
using UnityEngine;

public class FPSTest : MonoBehaviour
{
    float fps;
    public TextMeshProUGUI txt;

    private void Start()
    {
        StartCoroutine(CheckFPS());
    }

    IEnumerator CheckFPS()
    {
        fps = 1f / Time.deltaTime;
        if(txt != null)
        {
            txt.text = "FPS: " + Mathf.Round(fps);
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(CheckFPS());
    }

    /*private void OnGUI()
    {
        GUI.Label(new Rect(10,10,200,20), "FPS: " + Mathf.Round(fps));
    }*/
}
