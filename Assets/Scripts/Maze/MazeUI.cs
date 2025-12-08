using System;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class MazeUI : MonoBehaviour {
  [SerializeField] public TMP_InputField seedInput;
  public void Generate() {
    MazeGenerator.Instance.ScheduleGeneration(MazeGenerator.Instance.seed);
  }

  public void SetSeed(string seed) {
    if (seed == string.Empty) return;
    //input fields are always integers so we can safely parse.
    MazeGenerator.Instance.seed = ulong.Parse(seed);
  }

  public void RandomizeSeed() {
    ulong seed = 0;
    seed = seed.Random();
    MazeGenerator.Instance.seed = seed;
    seedInput.text = seed.ToString();
  }

  public void SetWidth(string width) {
    if (width == string.Empty) return;
    var i = int.Parse(width);
    if (i is 0 or 1) return;
    MazeGenerator.Instance.width = i;
  }

  public void SetHeight(string height) {
    if (height == string.Empty) return;
    var i = int.Parse(height);
    if (i is 0 or 1) return;
    MazeGenerator.Instance.height = i;
  }
}