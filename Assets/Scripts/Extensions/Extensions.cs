using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Extensions {
  public static ulong Random(this ulong ul) {
    System.Random rnd = new System.Random();
    //create a buffer (8 bytes = 64 bits, ulong is 64 bits)
    byte[] buffer = new byte[8];
    //fill with random bytes
    rnd.NextBytes(buffer);
    //turn back into a ulong for use as a seed
    ulong result = BitConverter.ToUInt64(buffer, 0);
    return result;
  }
}