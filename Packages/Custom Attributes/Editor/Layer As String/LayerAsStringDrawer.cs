using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace GameDevBuddies.CustomAttributes.Editor
{
    [CustomPropertyDrawer(typeof(LayerAsStringAttribute))]
    public class LayerAsStringDrawer : PropertyDrawer
    {
        private const int FIRST_ELEMENT_INDEX = 0;
        private const int SECOND_ELEMENT_INDEX = 1;
        private const int MAX_NUMBER_OF_LAYERS = 31;
        private const int LABEL_WIDTH = 200;
        private const int SPACING = 5;

        private const string UNDERSCORE = "_";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            List<string> allLayerNames = GetAllLayerNames();
            EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.BeginHorizontal();

            // Draw variable name 
            float labelWidth = Mathf.Min(LABEL_WIDTH, position.width);
            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            EditorGUI.LabelField(labelRect, GetCorrectVariableName(property));

            // Draw popup for picking layer
            string currentlySelectedLayerName = property.stringValue;
            int currentlySelectedLayerIndex = allLayerNames.Contains(currentlySelectedLayerName) ?
                allLayerNames.IndexOf(currentlySelectedLayerName) :
                FIRST_ELEMENT_INDEX;

            Rect popupPickerRect = new Rect(position.x + labelRect.width + SPACING, position.y, position.width - labelRect.width, position.height);
            currentlySelectedLayerIndex = EditorGUI.Popup(popupPickerRect, currentlySelectedLayerIndex, allLayerNames.ToArray());
            property.stringValue = allLayerNames[currentlySelectedLayerIndex];

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndProperty();
        }

        private List<string> GetAllLayerNames()
        {
            List<string> allLayerNames = new List<string>();
            for(int i = FIRST_ELEMENT_INDEX; i <= MAX_NUMBER_OF_LAYERS; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    allLayerNames.Add(layerName);
                }
            }

            return allLayerNames;
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