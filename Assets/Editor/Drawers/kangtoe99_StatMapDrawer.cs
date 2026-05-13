using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(kangtoe99_StatMap))]
public class kangtoe99_StatMapDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valuesProp = property.FindPropertyRelative("values");
        var statNames = Enum.GetNames(typeof(kangtoe99_StatType));

        // 배열 크기 보정 (enum 추가/제거 시)
        if (valuesProp.arraySize != statNames.Length)
        {
            valuesProp.arraySize = statNames.Length;
        }

        float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

        if (!property.isExpanded) return;

        EditorGUI.indentLevel++;
        for (int i = 0; i < statNames.Length; i++)
        {
            var rect = new Rect(position.x, position.y + lineHeight * (i + 1), position.width, EditorGUIUtility.singleLineHeight);
            var elem = valuesProp.GetArrayElementAtIndex(i);
            EditorGUI.PropertyField(rect, elem, new GUIContent(statNames[i]));
        }
        EditorGUI.indentLevel--;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        var names = Enum.GetNames(typeof(kangtoe99_StatType));
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (names.Length + 1);
    }
}
