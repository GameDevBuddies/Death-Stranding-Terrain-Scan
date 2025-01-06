using UnityEditor;
using UnityEngine;

namespace GameDevBuddies.CustomAttributes.Editor
{
    /// <summary>
    /// Custom <see cref="PropertyDrawer"/> that draws serialized object in the inspector, 
    /// but sets <see cref="GUI.enabled"/> to <see cref="false"/> so it's represented/visualized as read-only 
    /// (non-editable in the inspector).
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyPropertyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
