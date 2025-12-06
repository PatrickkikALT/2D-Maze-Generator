using UnityEngine;

public class MultipleOfAttribute : PropertyAttribute {
  public int Step;

  public MultipleOfAttribute(int step) {
    Step = step;
  }
}