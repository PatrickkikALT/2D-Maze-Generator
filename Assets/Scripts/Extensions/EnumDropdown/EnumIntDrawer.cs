using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumIntAttribute))]
public class EnumIntDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    var enumType = ((EnumIntAttribute)attribute).EnumType;
    var names = System.Enum.GetNames(enumType);

    property.intValue = EditorGUI.Popup(
      position,
      label.text,
      property.intValue,
      names
    );
  }
}