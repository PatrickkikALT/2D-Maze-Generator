using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct MazeJob : IJob {
  public int width;
  public int height;
  public ulong seed;
  public NativeArray<byte> outGrid;

  public void Execute() {
    int cells = width * height;
    for (int i = 0; i < cells; i++) outGrid[i] = 0;

    Random rng = new Random((uint)(seed == 0 ? 1 : seed));

    int sx = 1;
    int sy = 1;

    NativeList<int> stack = new NativeList<int>(Allocator.Temp);
    Set(outGrid, sx, sy, 1);
    stack.Add(Pack(sx, sy));

    while (stack.Length > 0) {
      int packed = stack[stack.Length - 1];
      int cx = UnpackX(packed);
      int cy = UnpackY(packed);

      NativeList<int> neighbours = new NativeList<int>(Allocator.Temp);

      TryAddNeighbour(cx + 2, cy, ref neighbours);
      TryAddNeighbour(cx - 2, cy, ref neighbours);
      TryAddNeighbour(cx, cy + 2, ref neighbours);
      TryAddNeighbour(cx, cy - 2, ref neighbours);

      if (neighbours.Length == 0) {
        stack.RemoveAtSwapBack(stack.Length - 1);
        neighbours.Dispose();
        continue;
      }

      int ni = rng.NextInt(0, neighbours.Length);
      int npacked = neighbours[ni];
      neighbours.Dispose();

      int nx = UnpackX(npacked);
      int ny = UnpackY(npacked);
      int mx = cx + (nx - cx) / 2;
      int my = cy + (ny - cy) / 2;

      Set(outGrid, mx, my, 1);
      Set(outGrid, nx, ny, 1);

      stack.Add(Pack(nx, ny));
    }

    stack.Dispose();
    NativeList<int> top = new NativeList<int>(Allocator.Temp);
    NativeList<int> bottom = new NativeList<int>(Allocator.Temp);
    NativeList<int> left = new NativeList<int>(Allocator.Temp);
    NativeList<int> right = new NativeList<int>(Allocator.Temp);

    for (int x = 1; x < width - 1; x++) {
      if (Get(outGrid, x, 1) == 1) top.Add(Pack(x, 0));
      if (Get(outGrid, x, height - 2) == 1) bottom.Add(Pack(x, height - 1));
    }

    for (int y = 1; y < height - 1; y++) {
      if (Get(outGrid, 1, y) == 1) left.Add(Pack(0, y));
      if (Get(outGrid, width - 2, y) == 1) right.Add(Pack(width - 1, y));
    }

    Edge entranceEdge = PickNonEmptyEdge(ref rng, top, bottom, left, right, exclude: (Edge)(-1));
    Edge exitEdge = PickNonEmptyEdge(ref rng, top, bottom, left, right, exclude: entranceEdge);

    int epacked = PickRandomFromEdge(ref rng, entranceEdge, top, bottom, left, right);
    int ex = UnpackX(epacked);
    int ey = UnpackY(epacked);
    Set(outGrid, ex, ey, 1);

    int opacked = PickRandomFromEdge(ref rng, exitEdge, top, bottom, left, right);
    int ox = UnpackX(opacked);
    int oy = UnpackY(opacked);
    Set(outGrid, ox, oy, 1);

    top.Dispose();
    bottom.Dispose();
    left.Dispose();
    right.Dispose();
  }

  private Edge PickNonEmptyEdge(ref Unity.Mathematics.Random rng, NativeList<int> top, NativeList<int> bottom,
    NativeList<int> left, NativeList<int> right, Edge exclude) {
    for (int i = 0; i < 8; i++) {
      Edge e = rng.NextEdge();
      if (e == exclude) continue;
      if (EdgeHasCandidates(e, top, bottom, left, right)) return e;
    }

    if (exclude != Edge.TOP && EdgeHasCandidates(Edge.TOP, top, bottom, left, right)) return Edge.TOP;
    if (exclude != Edge.BOTTOM && EdgeHasCandidates(Edge.BOTTOM, top, bottom, left, right)) return Edge.BOTTOM;
    if (exclude != Edge.LEFT && EdgeHasCandidates(Edge.LEFT, top, bottom, left, right)) return Edge.LEFT;
    if (exclude != Edge.RIGHT && EdgeHasCandidates(Edge.RIGHT, top, bottom, left, right)) return Edge.RIGHT;

    return Edge.TOP;
  }

  private bool EdgeHasCandidates(Edge e, NativeList<int> top, NativeList<int> bottom, NativeList<int> left,
    NativeList<int> right) {
    switch (e) {
      case Edge.TOP: return top.Length > 0;
      case Edge.BOTTOM: return bottom.Length > 0;
      case Edge.LEFT: return left.Length > 0;
      case Edge.RIGHT: return right.Length > 0;
    }

    return false;
  }

  private int PickRandomFromEdge(ref Unity.Mathematics.Random rng, Edge e, NativeList<int> top, NativeList<int> bottom,
    NativeList<int> left, NativeList<int> right) {
    switch (e) {
      case Edge.TOP: return top[rng.NextInt(0, top.Length)];
      case Edge.BOTTOM: return bottom[rng.NextInt(0, bottom.Length)];
      case Edge.LEFT: return left[rng.NextInt(0, left.Length)];
      case Edge.RIGHT: return right[rng.NextInt(0, right.Length)];
    }

    return top[0];
  }

  private void TryAddNeighbour(int x, int y, ref NativeList<int> list) {
    if (x <= 0 || y <= 0 || x >= width - 1 || y >= height - 1) return;
    if (Get(outGrid, x, y) == 1) return;
    list.Add(Pack(x, y));
  }

  private void Set(NativeArray<byte> grid, int x, int y, byte v) {
    grid[y * width + x] = v;
  }

  private byte Get(NativeArray<byte> grid, int x, int y) {
    return grid[y * width + x];
  }

  private static int Pack(int x, int y) => (x & 0xFFFF) | (y << 16);
  private static int UnpackX(int p) => p & 0xFFFF;
  private static int UnpackY(int p) => (p >> 16) & 0xFFFF;
}