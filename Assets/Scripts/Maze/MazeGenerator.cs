using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Burst;

public class MazeGenerator : MonoBehaviour {
  [Header("Maze Settings")] [Range(1, 250)]
  public int width;

  [Range(1, 250)] public int height;
  public ulong seed; //ulong allows for largest seed

  [Header("Tilemap Visualization")] public bool useTilemap;
  public TileBase floorTile;
  public TileBase wallTile;
  public TileBase pathTile;
  public Tilemap tilemap;

  public TMP_Text loadingText;
  [MultipleOf(16)] public int chunkSize;

  private NativeArray<byte> _gridNative;
  private JobHandle _jobHandle;
  private MazeJob _mazeJob;
  private byte[] _managedGrid;
  private bool _jobScheduled;
  private MoveToMaze _moveCamera;
  private bool _finishedClearing;
  public static MazeGenerator Instance { get; private set; }

  public void Awake() {
    if (Instance && Instance != this) {
      Destroy(this);
    }
    else {
      Instance = this;
    }
  }
  private void OnDisable() {
    //make sure we complete job before disabling
    if (_jobScheduled) {
      _jobHandle.Complete();
      _jobScheduled = false;
    }

    if (_gridNative.IsCreated) _gridNative.Dispose(); //also make sure we get rid of native array to clear up memory
  }

  private void Start() {
    _moveCamera = Camera.main.GetComponent<MoveToMaze>();
  }

  public void ScheduleGeneration(ulong generationSeed) {
    if (_jobScheduled) {
      //stop old job before we start a new one
      _jobHandle.Complete();
      _jobScheduled = false;
      if (_gridNative.IsCreated) _gridNative.Dispose();
    }

    int cells = width * height;
    _gridNative = new NativeArray<byte>(cells, Allocator.Persistent);

    _mazeJob = new MazeJob {
      //'this' keyword isn't necessary here but i prefer it for clarity sake
      width = this.width,
      height = this.height,
      seed = generationSeed,
      outGrid = _gridNative,
    };

    _jobHandle = _mazeJob.Schedule();
    _jobScheduled = true;
  }

  //fixed to save resources
  private void FixedUpdate() {
    if (!_jobScheduled) return;
    if (_jobHandle.IsCompleted) {
      loadingText.text = "";
      _jobHandle.Complete();
      _jobScheduled = false;

      int cells = width * height;
      _managedGrid = new byte[cells];
      _gridNative.CopyTo(_managedGrid);
      //we use a tilemap because prefabs are too expensive and we're going 2d anyway
      StartCoroutine(DrawTilemap(_managedGrid));

      //move the camera to make the maze fit otherwise 3/4 of the maze isnt visible lol
      _moveCamera.UpdateCamera(width, height, Camera.main);
      if (_gridNative.IsCreated) {
        _gridNative.Dispose();
      }
    }
  }

  //not only does this look better for what im going for, it also prevents having to draw the entire tilemap in one frame
  //which causes a huge fps dip
  private IEnumerator DrawTilemap(byte[] grid, bool isSolved = false) {
    tilemap.ClearAllTiles();
    yield return null;
    loadingText.text = "Generating...";
    for (int y0 = 0; y0 < height; y0 += chunkSize) {
      for (int x0 = 0; x0 < width; x0 += chunkSize) {
        int cw = Mathf.Min(chunkSize, width - x0);
        int ch = Mathf.Min(chunkSize, height - y0);

        BoundsInt bounds = new BoundsInt(x0, y0, 0, cw, ch, 1);
        TileBase[] tiles = new TileBase[cw * ch];

        for (int y = 0; y < ch; y++) {
          for (int x = 0; x < cw; x++) {
            int idx = (y0 + y) * width + (x0 + x);

            byte cell = grid[idx];

            TileBase tile = cell switch {
              0 => wallTile,
              1 => floorTile,
              2 => isSolved ? pathTile : floorTile,
              _ => wallTile
            };

            tiles[y * cw + x] = tile;
          }
        }
        tilemap.SetTilesBlock(bounds, tiles);

        yield return null;
      }
    }

    loadingText.text = "";
  }

  public void SolveAndDraw() {
    if (_managedGrid == null) {
      return;
    }

    NativeArray<byte> solverInput = new NativeArray<byte>(_managedGrid.Length, Allocator.TempJob);
    solverInput.CopyFrom(_managedGrid);

    NativeArray<byte> solverOutput = new NativeArray<byte>(_managedGrid.Length, Allocator.TempJob);

    SolverJob solveJob = new SolverJob {
      width = width,
      height = height,
      inGrid = solverInput,
      outGrid = solverOutput,
      startX = 1,
      startY = 1,
      endX = width - 2,
      endY = height - 2
    };

    solveJob.Schedule().Complete();
    solverOutput.CopyTo(_managedGrid);

    solverInput.Dispose();
    solverOutput.Dispose();

    StopAllCoroutines();
    StartCoroutine(DrawTilemap(_managedGrid, true));
  }
}