using UnityEngine;

[CreateAssetMenu(fileName = "ReadmeStage", menuName = "Documentation/Readme")]
public class ReadmeStage : ScriptableObject {
  [TextArea(10, 50)] public string description;
}