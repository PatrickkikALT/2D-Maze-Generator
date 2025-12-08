using System;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeGenerator : MonoBehaviour {
  [Header("Maze Settings")] [Range(1, 250)]
  public int width;

  [Range(1, 250)] public int height;
  public ulong seed; //ulong allows for largest seed

  [Header("Tilemap Visualization")] public bool useTilemap;
  public TileBase floorTile;
  public TileBase wallTile;
  public Tilemap tilemap;
  public TMP_Text loadingText;
  [MultipleOf(16)] public int chunkSize;

  private NativeArray<byte> _gridNative;
  private JobHandle _jobHandle;
  private MazeJob _mazeJob;
  private byte[] _managedGrid;
  private bool _jobScheduled = false;
  private MoveToMaze _moveCamera;
  private bool _finishedClearing;
  private int _previousDrawnHeight;
  private int _previousDrawnWidth;
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
    loadingText.text = "Generating...";
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
  private IEnumerator DrawTilemap(byte[] grid) {
    StartCoroutine(ClearTilemap());
    yield return new WaitUntil(() => _finishedClearing);
    _previousDrawnHeight = height;
    _previousDrawnWidth = width;
    for (int y0 = 0; y0 < height; y0 += chunkSize) {
      for (int x0 = 0; x0 < width; x0 += chunkSize) {
        int cw = Mathf.Min(chunkSize, width - x0);
        int ch = Mathf.Min(chunkSize, height - y0);

        BoundsInt bounds = new BoundsInt(x0, y0, 0, cw, ch, 1);
        TileBase[] tiles = new TileBase[cw * ch];

        for (int y = 0; y < ch; y++) {
          for (int x = 0; x < cw; x++) {
            int idx = (y0 + y) * width + (x0 + x);
            tiles[y * cw + x] = (grid[idx] == 1) ? floorTile : wallTile;
          }
        }

        tilemap.SetTilesBlock(bounds, tiles);
        yield return null;
      }
    }
  }

  //we do this in chunks because unity doesnt like clearing a tilemap for some reason
  //tilemap.ClearAllTiles() drops fps to 1 frame per second for a solid 10 second for a large maze
  private IEnumerator ClearTilemap() {
    _finishedClearing = false;
    for (int y0 = 0; y0 < _previousDrawnHeight; y0 += chunkSize) {
      for (int x0 = 0; x0 < _previousDrawnWidth; x0 += chunkSize) {
        int cw = Mathf.Min(chunkSize, _previousDrawnWidth - x0);
        int ch = Mathf.Min(chunkSize, _previousDrawnHeight - y0);
        BoundsInt bounds = new BoundsInt(x0, y0, 0, cw, ch, 1);
        TileBase[] empty = new TileBase[cw * ch];
        tilemap.SetTilesBlock(bounds, empty);
        yield return null;
      }
    }

    _finishedClearing = true;
  }
}