using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct SolverJob : IJob {
  public int width;
  public int height;

  [ReadOnly] public NativeArray<byte> inGrid;
  public NativeArray<byte> outGrid;

  public int startX;
  public int startY;
  public int endX;
  public int endY;

  public void Execute() {
    int cells = width * height;

    for (int i = 0; i < cells; i++) outGrid[i] = inGrid[i];
    //adjust end position based on if the generation used any even numbers, otherwise this causes the end point to be in a wall
    //which breaks the solver logic, dirty, but works.
    if (width % 2 == 0) endX -= 1;
    if (height % 2 == 0) endY -= 1;
    
    if (!InBounds(startX, startY) || !InBounds(endX, endY)) return;
    if (Get(inGrid, startX, startY) == 0 || Get(inGrid, endX, endY) == 0) return;

    NativeArray<int> cameFrom = new NativeArray<int>(cells, Allocator.Temp);
    for (int i = 0; i < cells; i++) cameFrom[i] = -1;

    NativeList<int> queue = new NativeList<int>(Allocator.Temp);
    int head = 0;

    int startPacked = Pack(startX, startY);
    int endPacked = Pack(endX, endY);

    queue.Add(startPacked);
    cameFrom[Idx(startX, startY)] = startPacked;

    bool found = false;
    while (head < queue.Length) {
      int packed = queue[head++];
      int cx = UnpackX(packed);
      int cy = UnpackY(packed);

      if (packed == endPacked) {
        found = true;
        break;
      }

      TryVisitNeighbor(cx + 1, cy, packed, ref queue, ref cameFrom);
      TryVisitNeighbor(cx - 1, cy, packed, ref queue, ref cameFrom);
      TryVisitNeighbor(cx, cy + 1, packed, ref queue, ref cameFrom);
      TryVisitNeighbor(cx, cy - 1, packed, ref queue, ref cameFrom);
    }

    if (found) {
      int cur = endPacked;
      while (true) {
        int x = UnpackX(cur);
        int y = UnpackY(cur);
        outGrid[y * width + x] = 2;

        int parent = cameFrom[Idx(x, y)];
        if (parent == -1) break;
        if (parent == cur) break;
        cur = parent;
      }
    }

    queue.Dispose();
    cameFrom.Dispose();
  }

  private void TryVisitNeighbor(int nx, int ny, int parentPacked, ref NativeList<int> queue,
    ref NativeArray<int> cameFrom) {
    if (!InBounds(nx, ny)) return;
    if (Get(inGrid, nx, ny) == 0) return;
    int index = Idx(nx, ny);
    if (cameFrom[index] != -1) return;

    int npacked = Pack(nx, ny);
    cameFrom[index] = parentPacked;
    queue.Add(npacked);
  }


  private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

  private void Set(NativeArray<byte> grid, int x, int y, byte v) => grid[y * width + x] = v;
  private byte Get(NativeArray<byte> grid, int x, int y) => grid[y * width + x];

  private int Idx(int x, int y) => y * width + x;

  private static int Pack(int x, int y) => (x & 0xFFFF) | (y << 16);
  private static int UnpackX(int p) => p & 0xFFFF;
  private static int UnpackY(int p) => (p >> 16) & 0xFFFF;
}