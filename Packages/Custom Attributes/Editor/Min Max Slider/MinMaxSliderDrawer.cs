using UnityEditor;
using UnityEngine;

namespace GameDevBuddies.CustomAttributes.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MinMaxSliderAttribute minMaxAttribute = (MinMaxSliderAttribute)attribute;
            // Check for float
            if (property.propertyType == SerializedPropertyType.Float)
            {
                string maxVariableName = minMaxAttribute.MaxVariableName;
                if (FindMaxProperty(property, maxVariableName, SerializedPropertyType.Float, out SerializedProperty maxProperty, out string warning))
                {
                    label.text = minMaxAttribute.DisplayName ?? label.text;
                    MMSEditorGUI.MinMaxSlider(position, label, property, maxProperty, minMaxAttribute.Min, minMaxAttribute.Max, minMaxAttribute.MinFieldPosition, minMaxAttribute.MaxFieldPosition);
                }
                else
                    EditorGUI.LabelField(position, EditorGUIUtility.TrTextContent(minMaxAttribute.DisplayName ?? property.displayName, warning), EditorGUIUtility.TrTempContent(warning));
            }
            // Check for int
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                string maxVariableName = minMaxAttribute.MaxVariableName;
                if (FindMaxProperty(property, maxVariableName, SerializedPropertyType.Integer, out SerializedProperty maxProperty, out string warning))
                {
                    label.text = minMaxAttribute.DisplayName ?? label.text;
                    MMSEditorGUI.MinMaxSliderInt(position, label, property, maxProperty, Mathf.RoundToInt(minMaxAttribute.Min), Mathf.RoundToInt(minMaxAttribute.Max), minMaxAttribute.MinFieldPosition, minMaxAttribute.MaxFieldPosition);
                }
                else
                    EditorGUI.LabelField(position, EditorGUIUtility.TrTextContent(minMaxAttribute.DisplayName ?? property.displayName, warning), EditorGUIUtility.TrTempContent(warning));
            }
            // Check for Vector2
            else if (property.propertyType == SerializedPropertyType.Vector2)
            {
                MMSEditorGUI.MinMaxSlider(position, label, property, minMaxAttribute.Min, minMaxAttribute.Max, minMaxAttribute.MinFieldPosition, minMaxAttribute.MaxFieldPosition);
            }
            // Check for Vector2Int
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                MMSEditorGUI.MinMaxSliderInt(position, label, property, Mathf.RoundToInt(minMaxAttribute.Min), Mathf.RoundToInt(minMaxAttribute.Max), minMaxAttribute.MinFieldPosition, minMaxAttribute.MaxFieldPosition);
            }
            else
            {
                EditorGUI.LabelField(position, label, "Use MinMaxSlider with Vector2, Vector2Int, float or int.");
            }
        }

        /// <summary>
        /// Find the max property by it's variable name. Returns true on success and false with a warning message otherwise.
        /// </summary>
        bool FindMaxProperty(SerializedProperty property, string variableName, SerializedPropertyType desiredType, out SerializedProperty maxProperty, out string warning)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                warning = $"You need to supply MaxVariableName when using {desiredType} to supply the max value. This needs to be a serialized field of type {desiredType}.";
                maxProperty = null;
                return false;
            }

            maxProperty = property.serializedObject.FindProperty(property.propertyPath.Replace(property.name, variableName));
            if (maxProperty == null)
            {
                warning = $"Variable '{variableName}' needs to exist and be serializable.";
                return false;
            }

            if (maxProperty.propertyType != desiredType)
            {
                warning = $"Variable '{variableName}' needs to be a {desiredType}.";
                return false;
            }

            warning = null;
            return true;
        }
    }
}
