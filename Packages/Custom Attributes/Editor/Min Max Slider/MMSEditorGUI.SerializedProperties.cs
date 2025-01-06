using UnityEditor;
using UnityEngine;

namespace GameDevBuddies.CustomAttributes.Editor
{
    public static partial class MMSEditorGUI
    {
        #region MinMaxSlider with SerializedProperty
        public static void MinMaxSliderInt(Rect position, SerializedProperty property, int minLimit, int maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSliderInt(position, property.displayName, property, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSliderInt(Rect position, string label, SerializedProperty property, int minLimit, int maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSliderInt(position, EditorGUIUtility.TrTempContent(label), property, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSliderInt(Rect position, GUIContent content, SerializedProperty property, int minLimit, int maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition)
        {
            var propertyLabel = EditorGUI.BeginProperty(position, content, property);
            Vector2Int value = property.vector2IntValue;
            EditorGUI.BeginChangeCheck();
            value = MinMaxSliderInt(position, propertyLabel, value, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);
            if (EditorGUI.EndChangeCheck())
            {
                property.vector2IntValue = value;
            }
            EditorGUI.EndProperty();
        }

        public static void MinMaxSlider(Rect position, SerializedProperty property, float minLimit, float maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSlider(position, property.displayName, property, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSlider(Rect position, string label, SerializedProperty property, float minLimit, float maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSlider(position, EditorGUIUtility.TrTempContent(label), property, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSlider(Rect position, GUIContent content, SerializedProperty property, float minLimit, float maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition)
        {
            var propertyLabel = EditorGUI.BeginProperty(position, content, property);
            Vector2 value = property.vector2Value;
            EditorGUI.BeginChangeCheck();
            value = MinMaxSlider(position, propertyLabel, value, minLimit, maxLimit, property.propertyPath, minValueFieldPosition, maxValueFieldPosition);
            if (EditorGUI.EndChangeCheck())
            {
                property.vector2Value = value;
            }
            EditorGUI.EndProperty();
        }
        #endregion

        #region MinMaxSlider with two SerializedProperties
        public static void MinMaxSliderInt(Rect position, SerializedProperty minProperty, SerializedProperty maxProperty, int minLimit, int maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSliderInt(position, minProperty.displayName, minProperty, maxProperty, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSliderInt(Rect position, string label, SerializedProperty minProperty, SerializedProperty maxProperty, int minLimit, int maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSliderInt(position, EditorGUIUtility.TrTempContent(label), minProperty, maxProperty, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSliderInt(Rect position, GUIContent content, SerializedProperty minProperty, SerializedProperty maxProperty, int minLimit, int maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition)
        {
            Vector2Int value = new Vector2Int(minProperty.intValue, maxProperty.intValue);
            content.tooltip = AddToTooltip(content.tooltip, GetTooltipText(value));
            position = EditorGUI.PrefixLabel(position, content);
            EditorGUI.BeginChangeCheck();
            value = HandleMinMaxSliderInt(position, value, minLimit, maxLimit, minProperty, maxProperty, minValueFieldPosition, maxValueFieldPosition);
            if (EditorGUI.EndChangeCheck())
            {
                minProperty.intValue = value.x;
                maxProperty.intValue = value.y;
            }
        }

        public static void MinMaxSlider(Rect position, SerializedProperty minProperty, SerializedProperty maxProperty, float minLimit, float maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSlider(position, minProperty.displayName, minProperty, maxProperty, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSlider(Rect position, string label, SerializedProperty minProperty, SerializedProperty maxProperty, float minLimit, float maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition) =>
            MinMaxSlider(position, EditorGUIUtility.TrTempContent(label), minProperty, maxProperty, minLimit, maxLimit, minValueFieldPosition, maxValueFieldPosition);

        public static void MinMaxSlider(Rect position, GUIContent content, SerializedProperty minProperty, SerializedProperty maxProperty, float minLimit, float maxLimit, SliderFieldPosition minValueFieldPosition = MinMaxSliderAttribute.DefaultMinFieldPosition, SliderFieldPosition maxValueFieldPosition = MinMaxSliderAttribute.DefaultMaxFieldPosition)
        {
            Vector2 value = new Vector2(minProperty.floatValue, maxProperty.floatValue);
            content.tooltip = AddToTooltip(content.tooltip, GetTooltipText(value));
            position = EditorGUI.PrefixLabel(position, content);
            EditorGUI.BeginChangeCheck();
            value = HandleMinMaxSlider(position, value, minLimit, maxLimit, minProperty, maxProperty, minProperty.propertyPath, minValueFieldPosition, maxValueFieldPosition);
            if (EditorGUI.EndChangeCheck())
            {
                minProperty.floatValue = value.x;
                maxProperty.floatValue = value.y;
            }
        }
        #endregion
    }
}