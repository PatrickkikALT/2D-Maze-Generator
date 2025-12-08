using UnityEngine;

//used for device simulation because prettier
public class EnumIntAttribute : PropertyAttribute {
  public System.Type EnumType;

  public EnumIntAttribute(System.Type enumType) {
    EnumType = enumType;
  }
}