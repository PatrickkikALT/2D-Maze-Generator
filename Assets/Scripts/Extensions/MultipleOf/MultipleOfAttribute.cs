using UnityEngine;

public class MultipleOfAttribute : PropertyAttribute {
  public readonly int Step;

  public MultipleOfAttribute(int step) {
    Step = step;
  }
}