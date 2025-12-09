using UnityEditor;

[CustomEditor(typeof(ReadmeStage))]
public class ReadmeEditor : Editor {
  public override void OnInspectorGUI() {
    ReadmeStage readme = (ReadmeStage)target;
    
    if (!string.IsNullOrEmpty(readme.description)) {
      EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
      EditorGUILayout.LabelField(readme.description, EditorStyles.wordWrappedLabel);
    }
  }
}