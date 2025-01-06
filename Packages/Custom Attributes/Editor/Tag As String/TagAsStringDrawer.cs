using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace GameDevBuddies.CustomAttributes.Editor
{
    [CustomPropertyDrawer(typeof(TagAsStringAttribute))]
    public class TagAsStringDrawer : PropertyDrawer
    {
        private const int FIRST_ELEMENT_INDEX = 0;
        private const int SECOND_ELEMENT_INDEX = 1;
        private const int LABEL_WIDTH = 200;
        private const int SPACING = 5;

        private const string UNDERSCORE = "_";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            List<string> allTagNames = GetAllTagNames();
            EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.BeginHorizontal();

            // Draw variable name 
            float labelWidth = Mathf.Min(LABEL_WIDTH, position.width);
            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            EditorGUI.LabelField(labelRect, GetCorrectVariableName(property));

            // Draw popup for picking tag
            string currentlySelectedTagName = property.stringValue;
            int currentlySelectedTagIndex = allTagNames.Contains(currentlySelectedTagName) ?
                allTagNames.IndexOf(currentlySelectedTagName) :
                FIRST_ELEMENT_INDEX;

            Rect popupPickerRect = new Rect(position.x + labelRect.width + SPACING, position.y, position.width - labelRect.width, position.height);
            currentlySelectedTagIndex = EditorGUI.Popup(popupPickerRect, currentlySelectedTagIndex, allTagNames.ToArray());
            property.stringValue = allTagNames[currentlySelectedTagIndex];

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndProperty();
        }

        private List<string> GetAllTagNames()
        {
            List<string> allTagsNames = new List<string>();
            allTagsNames.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
            return allTagsNames;
        }

        private string GetCorrectVariableName(SerializedProperty property)
        {
            string variableName = property.name;
            if (variableName.StartsWith(UNDERSCORE))
            {
                variableName = variableName.Substring(SECOND_ELEMENT_INDEX);
            }

            variableName = variableName.First().ToString().ToUpper() + variableName.Substring(SECOND_ELEMENT_INDEX);
            return variableName;
        }
    }
}