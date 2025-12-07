using UnityEngine;
//used for chunk size because it doesnt look pretty
//if we have a chunk size that isn't a multiple of 16 :(
public class MultipleOfAttribute : PropertyAttribute {
  public readonly int Step;

  public MultipleOfAttribute(int step) {
    Step = step;
  }
}