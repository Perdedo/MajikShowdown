using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SimpleInt))]
[CustomPropertyDrawer(typeof(SimpleFloat))]
public class SimpleDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        // foldout
        var y = position.y;
        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
        if (property.isExpanded)
        {
            var type = property.FindPropertyRelative("type");
            var value = property.FindPropertyRelative("value");
            var min = property.FindPropertyRelative("min");
            var max = property.FindPropertyRelative("max");

            y += EditorGUIUtility.singleLineHeight + 2f;
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), type);
            y += EditorGUIUtility.singleLineHeight + 2f;

            switch ((SimpleVar.ValueType)type.enumValueIndex)
            {
                case SimpleVar.ValueType.Fixed:
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), value);
                    break;
                case SimpleVar.ValueType.Random:
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), min);
                    y += EditorGUIUtility.singleLineHeight + 2f;
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), max);
                    break;
                case SimpleVar.ValueType.Infinity:
                    // nothing else to draw
                    break;
            }
            EditorGUI.indentLevel--;
        }


        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight; // foldout
        if (!property.isExpanded) return h;

        h += EditorGUIUtility.singleLineHeight + 2f; // type
        var type = property.FindPropertyRelative("type");

        switch ((SimpleVar.ValueType)type.enumValueIndex)
        {
            case SimpleVar.ValueType.Fixed:
                h += EditorGUIUtility.singleLineHeight + 2f; // value
                break;
            case SimpleVar.ValueType.Random:
                h += (EditorGUIUtility.singleLineHeight + 2f) * 2; // min + max
                break;
            case SimpleVar.ValueType.Infinity:
                break;
        }
        return h;
    }
}
#endif