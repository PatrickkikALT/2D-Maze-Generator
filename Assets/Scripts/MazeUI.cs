using System;
using UnityEngine;

public class MazeUI : MonoBehaviour {
  
  public void Generate() {
    MazeGenerator.Instance.ScheduleGeneration(MazeGenerator.Instance.seed);
  }

  public void SetSeed(string seed) {
    //input fields are always integers so we can safely parse.
    MazeGenerator.Instance.seed = ulong.Parse(seed);
  }

  public void SetWidth(string width) {
    MazeGenerator.Instance.width = int.Parse(width);
  }

  public void SetHeight(string height) {
    MazeGenerator.Instance.height = int.Parse(height);
  }
}