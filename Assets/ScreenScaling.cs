using UnityEngine;

public class ScreenScaling : MonoBehaviour {
  private void Start() {
    Camera cam = Camera.main;
    float targetAspect = 16f / 9f;
    float windowAspect = (float)Screen.width / Screen.height;
    float scaleHeight = windowAspect / targetAspect;

    if (scaleHeight < 1.0f) {
      cam.orthographicSize /= scaleHeight;
    }
  }
}