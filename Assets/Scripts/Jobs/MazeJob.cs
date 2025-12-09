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
  //recursive backtrack algorithm
  public void Execute() {
    int cells = width * height;
    for (int i = 0; i < cells; i++) outGrid[i] = 0; //grid starts with just walls

    //unity's built in random isnt compatible with burst so we use Unity.Mathematics.Random instead.
    Random rng = new Random((uint)(seed == 0 ? 1 : seed));

    int sx = 1; //starting x & y
    int sy = 1;

    NativeList<int> stack = new NativeList<int>(Allocator.Temp);
    Set(outGrid, sx, sy, 1);
    stack.Add(Pack(sx, sy)); //push starting cell to memory

    while (stack.Length > 0) {
      int packed = stack[^1]; //get current cell
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
    
    int entranceX = 1;
    int entranceY = 1;

    Set(outGrid, entranceX, entranceY, 2);
    Set(outGrid, entranceX, 0, 2);

    int exitX = width - 2; 
    int exitY = height - 2;
    
    if ((exitX & 1) == 0) exitX--; //make sure exit is on an odd cell, otherwise it looks weird.
    if ((exitY & 1) == 0) exitY--;

    exitX = math.max(1, exitX); //clamp exit
    exitY = math.max(1, exitY);
    
    Set(outGrid, exitX, exitY, 1);

    int finalExitY = math.max(1, height - 2);
    if ((finalExitY & 1) == 0) finalExitY--;

    int exitPathX = width - 2;
    int exitPathY = finalExitY;
    
    if ((exitPathX & 1) == 0) exitPathX--;
    exitPathX = math.max(1, exitPathX);
    Set(outGrid, exitPathX, exitPathY, 2); //open path to exit

    if (exitPathX < width - 2) {
        Set(outGrid, exitPathX + 1, exitPathY, 2);
    }
    
    Set(outGrid, width - 1, exitPathY, 2); 
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
  // keep only the lower 16 bits of x, shift y 16 bits left, then combine both into one int
  private static int Pack(int x, int y) => (x & 0xFFFF) | (y << 16); 
  // extract the lower 16 bits of p which represent the value
  private static int UnpackX(int p) => p & 0xFFFF;
  // shift p 16 bits right to get the original y value
  private static int UnpackY(int p) => (p >> 16) & 0xFFFF;
}