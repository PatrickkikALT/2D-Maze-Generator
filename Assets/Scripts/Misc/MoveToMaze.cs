using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MoveToMaze : MonoBehaviour {
  #if UNITY_EDITOR
  [EnumInt(typeof(DeviceType))]
  public int simulateDevice;
  #endif
  private float _size;
  public float phoneSize, ipadSize, desktopSize;
  
  private void Start() {
    DeviceType type = Extensions.GetDeviceType();
    _size = type switch {
      DeviceType.iPhone => phoneSize,
      DeviceType.iPad => ipadSize,
      DeviceType.Desktop => desktopSize,
      _ => _size
    };
    
    #if UNITY_EDITOR
    switch (simulateDevice) {
      case 1:
        _size = phoneSize;
        break;
      case 2:
        _size = ipadSize;
        break;
    }
    #endif
  }

  public void UpdateCamera(int width, int height, Camera cam, float padding = 1f) {
    float worldWidth = width;
    float worldHeight = height;
    //get center of maze
    Vector3 center = new Vector3((worldWidth - 1f) * 0.5f, (worldHeight - 1f) * 0.5f, 0f);
    
    // add padding so we have some breathing room around the maze
    worldWidth += padding * 2f;
    worldHeight += padding * 2f;
    
    float requiredSizeY = worldHeight * 0.5f;
    float requiredSizeX = (worldWidth * 0.5f) / cam.aspect;

    cam.orthographicSize = Mathf.Max(requiredSizeY, requiredSizeX);
    //add size based on the user's device, to hopefully scale the maze correctly
    //this way the ui doesnt go over the maze
    cam.orthographicSize += _size;
    
    Vector3 camPos = cam.transform.position;
    camPos.x = center.x;
    camPos.y = center.y;
    cam.transform.position = camPos;
  }
}