
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioController))]
public class AudioControllerEditor : Editor
{
    private SerializedProperty sfxVolumeBoostDb;
    private SerializedProperty sfxFullVolumeDistance3D;

    private void OnEnable()
    {
        sfxVolumeBoostDb = serializedObject.FindProperty("sfxVolumeBoostDb");
        sfxFullVolumeDistance3D = serializedObject.FindProperty("sfxFullVolumeDistance3D");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.name == "sfxVolumeBoostDb" || property.name == "sfxFullVolumeDistance3D") continue;

            using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("SFX Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(3);

        DrawSetting(
            sfxVolumeBoostDb,
            "SFX Volume Boost",
            "Additional volume gain applied to sound effects, in decibels (dB). Higher values make spell sounds louder."
        );

        DrawSetting(
            sfxFullVolumeDistance3D,
            "SFX Full Volume Distance",
            "Distance at which 3D spell sounds remain at maximum volume. Beyond this distance, volume gradually decreases."
        );

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSetting(SerializedProperty property, string label, string description)
    {
        EditorGUILayout.HelpBox(description, MessageType.None);
        EditorGUILayout.PropertyField(property, new GUIContent(label));
        EditorGUILayout.Space(5);
    }
}
