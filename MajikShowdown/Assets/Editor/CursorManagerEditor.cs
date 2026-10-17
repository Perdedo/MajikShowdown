#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CursorManager))]
public class CursorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SerializedProperty texture = serializedObject.FindProperty("cursorTexture");
        SerializedProperty hotspot = serializedObject.FindProperty("hotspot");

        Texture2D tex = texture.objectReferenceValue as Texture2D;
        if (tex == null) return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Click to Set Hotspot", EditorStyles.boldLabel);

        float size = 256f;
        Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));

        EditorGUI.DrawTextureTransparent(rect, tex, ScaleMode.ScaleToFit);

        Vector2 point = hotspot.vector2Value;
        Vector2 visualPoint = new Vector2(
            rect.x + point.x / tex.width * size,
            rect.y + point.y / tex.height * size
        );

        Handles.color = Color.red;
        Handles.DrawLine(visualPoint + Vector2.left * 10, visualPoint + Vector2.right * 10);
        Handles.DrawLine(visualPoint + Vector2.up * 10, visualPoint + Vector2.down * 10);

        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
        {
            Vector2 local = e.mousePosition - rect.position;

            hotspot.vector2Value = new Vector2(
                Mathf.RoundToInt(local.x / size * tex.width),
                Mathf.RoundToInt(local.y / size * tex.height)
            );

            serializedObject.ApplyModifiedProperties();

            ((CursorManager)target).ApplyCursor();

            e.Use();
            Repaint();
        }
    }
}
#endif