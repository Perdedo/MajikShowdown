using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ApplyCursor();
    }

    public void ApplyCursor()
    {
        if (cursorTexture == null) return;
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
    }
}