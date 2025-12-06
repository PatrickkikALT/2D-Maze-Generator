using UnityEngine;
using UnityEngine.Tilemaps;

public static class Extensions {
  public static Edge NextEdge(this Unity.Mathematics.Random rand) {
    int i = rand.NextInt(0, 4);
    return (Edge)i;
  }

  public static Bounds GetRenderedBounds(this Tilemap tilemap) {
    tilemap.CompressBounds();

    BoundsInt cellBounds = tilemap.cellBounds;
    if (cellBounds.size == Vector3Int.zero) {
      return new Bounds(tilemap.transform.position, Vector3.zero);
    }

    Vector3 cellCenter = cellBounds.center;
    Vector3 worldCenter = tilemap.CellToWorld(cellCenter.ToVector3Int());

    Grid grid = tilemap.layoutGrid;
    Vector3 cellSize = grid ? grid.cellSize : Vector3.one;
    Vector3 worldSize = new Vector3(
      Mathf.Abs(cellBounds.size.x) * cellSize.x,
      Mathf.Abs(cellBounds.size.y) * cellSize.y,
      Mathf.Abs(cellBounds.size.z) * cellSize.z
    );

    return new Bounds(worldCenter, worldSize);
  }

  public static Vector3Int ToVector3Int(this Vector3 vec3) {
    return new Vector3Int((int)vec3.x, (int)vec3.y, (int)vec3.z);
  }
}