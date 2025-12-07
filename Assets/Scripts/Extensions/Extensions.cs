using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Extensions {
  public static ulong Random(this ulong ul) {
    System.Random rnd = new System.Random();
    byte[] buffer = new byte[8];
    rnd.NextBytes(buffer);
    ulong result = BitConverter.ToUInt64(buffer, 0);
    return result;
  }
}