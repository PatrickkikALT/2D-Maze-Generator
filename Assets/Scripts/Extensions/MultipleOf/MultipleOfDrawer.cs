using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MultipleOfAttribute))]
public class MultipleOfDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    EditorGUI.BeginProperty(position, label, property);
    
    var attr = (MultipleOfAttribute)attribute;
    
    if (property.propertyType == SerializedPropertyType.Integer) {
      int value = EditorGUI.IntField(position, label, property.intValue);
      int snapped = Mathf.RoundToInt((float)value / attr.Step) * attr.Step;
      property.intValue = snapped;
    }

    EditorGUI.EndProperty();
  }
}