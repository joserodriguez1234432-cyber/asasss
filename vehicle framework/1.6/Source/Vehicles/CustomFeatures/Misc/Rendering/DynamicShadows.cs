// Decompiled with JetBrains decompiler
// Type: Vehicles.DynamicShadows
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class DynamicShadows
{
  private const float AlphaIsoThreshold = 0.5f;
  private static readonly Dictionary<int, Mesh> shadowMeshDict = new Dictionary<int, Mesh>();
  private static int[] bitArray = new int[4]{ 4, 3, 1, 2 };

  public static Mesh GetShadowMesh(Texture2D texture, ShadowData shadowData)
  {
    return DynamicShadows.GetShadowMesh(texture, shadowData.BaseX, shadowData.BaseZ, shadowData.BaseY);
  }

  public static Mesh GetShadowMesh(
    Texture2D texture,
    float baseWidth,
    float baseHeight,
    float tallness)
  {
    int key = DynamicShadows.HashOf(texture, baseWidth, baseHeight, tallness);
    Mesh shadowMesh;
    if (!DynamicShadows.shadowMeshDict.TryGetValue(key, out shadowMesh))
    {
      DynamicShadows.CreateMeshFromTexture(texture, baseWidth, baseHeight, tallness);
      shadowMesh = MeshMakerShadows.NewShadowMesh(baseWidth, baseHeight, tallness);
      DynamicShadows.shadowMeshDict.Add(key, shadowMesh);
    }
    return shadowMesh;
  }

  private static Mesh CreateMeshFromTexture(
    Texture2D texture,
    float baseWidth,
    float baseHeight,
    float tallness)
  {
    Texture2D readableTexture = Ext_Texture.CreateReadableTexture(texture);
    try
    {
      Color[] pixels = readableTexture.GetPixels();
      int length1 = ((Texture) texture).width - 1;
      int length2 = ((Texture) texture).height - 1;
      int[,] numArray = new int[length1, length2];
      for (int index1 = 0; index1 < length1; ++index1)
      {
        for (int index2 = 0; index2 < length2; ++index2)
        {
          int num = 0;
          for (int index3 = 0; index3 < 2; ++index3)
          {
            for (int index4 = 0; index4 < 2; ++index4)
            {
              if ((double) pixels[index1 + index3 + (index2 + index4)].a >= 0.5)
              {
                int bit = DynamicShadows.bitArray[index3 + index4];
                num |= 1 << bit;
              }
            }
          }
          numArray[index1, index2] = num;
        }
      }
    }
    finally
    {
      Object.Destroy((Object) readableTexture);
    }
    return (Mesh) null;
  }

  private static int HashOf(Texture2D texture, float baseWidth, float baseheight, float tallness)
  {
    return Gen.HashCombineInt((int) ((double) baseWidth * 1000.0) * 391 ^ 261231 ^ (int) ((double) baseheight * 1000.0) * 612331 ^ (int) ((double) tallness * 1000.0) * 456123, texture.GetHashCode());
  }
}
