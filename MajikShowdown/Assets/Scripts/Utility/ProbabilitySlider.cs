using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ProbabilitySlider<T>
{
    public List<ProbabilityEntry<T>> Entries = new List<ProbabilityEntry<T>>();
    public void AddEntry(string label, float weight, T value = default(T))
    {
        Entries.Add(new ProbabilityEntry<T> { label = label, Weight = weight, Value = value });
    }
    public ProbabilitySlider()
    {

    }
    public ProbabilitySlider(List<(string label, float weight, T value)> entries)
    {
        foreach (var entry in entries)
        {
            AddEntry(entry.label, entry.weight, entry.value);
        }
    }
    public T GetRandomEntry()
    {
        float totalWeight = Entries.Sum(e => e.Weight);
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);

        float cumulativeWeight = 0f;
        foreach (var entry in Entries)
        {
            cumulativeWeight += entry.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return entry.Value;
            }
        }

        return Entries[Entries.Count - 1].Value; 
    }
}
[Serializable]
public class ProbabilityEntry<T>
{
    [Range(0, 1)]
    public float Weight;
    public string label;
    public T Value;
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ProbabilitySlider<>))]
public class ProbabilitySliderDrawer : PropertyDrawer
{
    const float BAR_HEIGHT = 30f;
    const float HANDLE_WIDTH = 6f;

    public override float GetPropertyHeight(
    SerializedProperty property,
    GUIContent label)
    {
        SerializedProperty entries =
            property.FindPropertyRelative("Entries");

        return BAR_HEIGHT +
               EditorGUIUtility.singleLineHeight +
               entries.arraySize * (EditorGUIUtility.singleLineHeight + 2) +
               30 + 10; // Extra space for total weight and padding
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty entries =
            property.FindPropertyRelative("Entries");

        if (entries.arraySize < 2)
        {
            EditorGUI.HelpBox(
                position,
                "Need at least 2 entries.",
                MessageType.Warning);

            EditorGUI.EndProperty();
            return;
        }

        Rect labelRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(labelRect, label);

        Rect barRect = new Rect(
            position.x,
            position.y + EditorGUIUtility.singleLineHeight + 4,
            position.width,
            BAR_HEIGHT);

        float[] weights = new float[entries.arraySize];
        float total = 0;

        for (int i = 0; i < entries.arraySize; i++)
        {
            weights[i] = entries
                .GetArrayElementAtIndex(i)
                .FindPropertyRelative("Weight")
                .floatValue;
            weights[i] = Mathf.Max(0f, weights[i]);
            if (weights[i] < 0.0001f)
            {
                weights[i] = 0;
            }
            total += weights[i];
        }

        if (total <= 0)
            total = 1;

        Color[] colors =
        {
            new Color(0.4f,0.8f,1f),
            new Color(1f,0.8f,0.3f),
            new Color(1f,0.4f,0.4f),
            new Color(0.6f,1f,0.6f),
            new Color(0.8f,0.5f,1f),
        };

        float currentX = barRect.x;
        float y = barRect.yMax + 5;

        for (int i = 0; i < entries.arraySize; i++)
        {
            float width =
                barRect.width * (weights[i] / total);

            Rect segmentRect =
                new Rect(currentX, barRect.y, width, barRect.height);

            EditorGUI.DrawRect(
                segmentRect,
                colors[i % colors.Length]);

            EditorGUI.DrawRect(new Rect(segmentRect.xMax - 1, segmentRect.y, 2, segmentRect.height), Color.black);

            SerializedProperty entry =
                entries.GetArrayElementAtIndex(i);

            string Label =
                entry.FindPropertyRelative("label").stringValue;

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = Color.black;
            labelStyle.hover.textColor = Color.black;
            labelStyle.active.textColor = Color.black;
            labelStyle.focused.textColor = Color.black;
            GUI.Label(
                segmentRect,
                $"{Label}\n{weights[i] / total:P1}",
                labelStyle);

            currentX += width;

            // Draw entry fields below the bar

            SerializedProperty weightProp =
                entry.FindPropertyRelative("Weight");

            Rect rowRect = new Rect(
                position.x,
                y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            float percent =
                total > 0
                ? (weightProp.floatValue / total) * 100f
                : 0f;

            float trueValue;
            trueValue = Mathf.Max(0f,weightProp.floatValue);
            if (trueValue < 0.0001f)
            {
                trueValue = 0;
            }
            weightProp.floatValue =
                EditorGUI.FloatField(
                    rowRect,
                    $"{Label} ({percent:F1}%)",
                    trueValue);

            y += EditorGUIUtility.singleLineHeight + 2;
        }
        Rect totalRect = new Rect(
        position.x,
        y + 4,
        position.width,
        EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(
            totalRect,
            $"Total Weight: {total:F1}");

        HandleDragging(entries, barRect, weights, total);

        EditorGUI.EndProperty();
    }

    void HandleDragging(
    SerializedProperty entries,
    Rect barRect,
    float[] weights,
    float total)
    {
        Event e = Event.current;

        float accumulated = 0;

        for (int i = 0; i < entries.arraySize - 1; i++)
        {
            accumulated += weights[i];

            float handleX =
                barRect.x +
                (accumulated / total) * barRect.width;

            Rect handleRect = new Rect(
                handleX - HANDLE_WIDTH * 0.5f,
                barRect.y,
                HANDLE_WIDTH,
                barRect.height);

            EditorGUIUtility.AddCursorRect(
                handleRect,
                MouseCursor.ResizeHorizontal);

            int controlID = GUIUtility.GetControlID(
                FocusType.Passive,
                handleRect);

            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 0 &&
                            handleRect.Contains(e.mousePosition))
                        {
                            GUIUtility.hotControl = controlID;
                            e.Use();
                        }
                        break;
                    }

                case EventType.MouseDrag:
                    {
                        if (GUIUtility.hotControl == controlID)
                        {
                            float deltaPercent =
                                e.delta.x / barRect.width;

                            float deltaWeight =
                                deltaPercent * total;

                            SerializedProperty left =
                                entries.GetArrayElementAtIndex(i)
                                .FindPropertyRelative("Weight");

                            SerializedProperty right =
                                entries.GetArrayElementAtIndex(i + 1)
                                .FindPropertyRelative("Weight");

                            float minWeight = 0;

                            float newLeft =
                                Mathf.Clamp(
                                    left.floatValue + deltaWeight,
                                    minWeight,
                                    left.floatValue + right.floatValue - minWeight);

                            float actualDelta =
                                newLeft - left.floatValue;

                            left.floatValue += actualDelta;
                            right.floatValue -= actualDelta;

                            entries.serializedObject.ApplyModifiedProperties();

                            e.Use();
                        }
                        break;
                    }

                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == controlID)
                        {
                            GUIUtility.hotControl = 0;
                            e.Use();
                        }
                        break;
                    }

                case EventType.Repaint:
                    {
                        //EditorGUI.DrawRect(handleRect, Color.black);
                        break;
                    }
            }
        }
    }
}
#endif