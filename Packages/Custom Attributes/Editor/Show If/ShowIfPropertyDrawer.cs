using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameDevBuddies.CustomAttributes.Editor
{
    /// <summary>
    /// Property drawer for the <see cref="ShowIfAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            bool shouldShow = CheckIfPropertyShouldBeShown(property, showIf);

            if (shouldShow)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            bool shouldShow = CheckIfPropertyShouldBeShown(property, showIf);

            return shouldShow ? EditorGUI.GetPropertyHeight(property, label, true) : -EditorGUIUtility.standardVerticalSpacing;
        }

        /// <summary>
        /// Checks if the property value is true or false.
        /// It will search for Properties, Field and finally Methods.
        /// </summary>
        /// <param name="property">Property that is targeted.</param>
        /// <param name="attribute">Attribute value.</param>
        /// <returns>True if the value of the property/field/method is true.</returns>
        private bool CheckIfPropertyShouldBeShown(SerializedProperty property, ShowIfAttribute attribute)
        {
            object targetObject = property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();

            // Check for a property
            PropertyInfo propInfo = targetType.GetProperty(attribute.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propInfo != null && propInfo.PropertyType == typeof(bool))
            {
                return (bool)propInfo.GetValue(targetObject);
            }

            // Check for a field if property not found
            FieldInfo fieldInfo = targetType.GetField(attribute.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // Check bool field.
            if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
            {
                if(attribute.Value != null && attribute.Value.GetType() == typeof(bool))
                {
                    return (bool)fieldInfo.GetValue(targetObject) == (bool)attribute.Value;
                }

                return (bool)fieldInfo.GetValue(targetObject);
            }
            // Check enum field.
            if (fieldInfo != null && fieldInfo.FieldType.IsEnum)
            {
                object fieldValue = fieldInfo.GetValue(targetObject);
                try
                {
                    object enumValueParsed = Enum.Parse(fieldInfo.FieldType, attribute.Value.ToString());
                    return fieldValue.Equals(enumValueParsed);
                }
                catch
                {
                    Debug.LogError($"{nameof(ShowIfPropertyDrawer)}:: Enum type mismatch or invalid enum value '{attribute.Value}' for field '{fieldInfo.Name}'.");
                }
            }

            // Check for a method if no property or field is found
            MethodInfo methodInfo = targetType.GetMethod(attribute.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo != null && methodInfo.ReturnType == typeof(bool) && methodInfo.GetParameters().Length == 0)
            {
                return (bool)methodInfo.Invoke(targetObject, null);
            }

            return false; // Default to false if no valid member is found
        }
    }
}
