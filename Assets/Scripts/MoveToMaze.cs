using UnityEngine;
using UnityEngine.Tilemaps;

public class MoveToMaze : MonoBehaviour 
{
  public void UpdateCamera(int width, int height, Camera cam, float padding = 1f) 
  {
    float worldWidth = width;
    float worldHeight = height;
    Vector3 center = new Vector3((worldWidth - 1f) * 0.5f, (worldHeight - 1f) * 0.5f, 0f);
    
    worldWidth += padding * 2f;
    worldHeight += padding * 2f;
    
    float requiredSizeY = worldHeight * 0.5f;
    float requiredSizeX = (worldWidth * 0.5f) / cam.aspect;

    cam.orthographicSize = Mathf.Max(requiredSizeY, requiredSizeX);
    
    Vector3 camPos = cam.transform.position;
    camPos.x = center.x;
    camPos.y = center.y;
    cam.transform.position = camPos;
  }
}