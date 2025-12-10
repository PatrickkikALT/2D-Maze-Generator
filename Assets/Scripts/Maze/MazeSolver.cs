using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class MazeSolver : MonoBehaviour {
  private MazeGenerator _generator;

  private void Start() {
    _generator = MazeGenerator.Instance;
  }

  public void SolveAndDraw() {
    if (_generator.managedGrid == null) {
      return;
    }

    NativeArray<byte> solverInput = new NativeArray<byte>(_generator.managedGrid.Length, Allocator.TempJob);
    solverInput.CopyFrom(_generator.managedGrid);

    NativeArray<byte> solverOutput = new NativeArray<byte>(_generator.managedGrid.Length, Allocator.TempJob);

    SolverJob solveJob = new SolverJob {
      width = _generator.width,
      height = _generator.height,
      inGrid = solverInput,
      outGrid = solverOutput,
      startX = 1,
      startY = 1,
      endX = _generator.width - 2,
      endY = _generator.height - 2
    };

    solveJob.Schedule().Complete();
    solverOutput.CopyTo(_generator.managedGrid);

    solverInput.Dispose();
    solverOutput.Dispose();

    StopAllCoroutines();
    StartCoroutine(_generator.DrawTilemap(_generator.managedGrid, true));
  }
}