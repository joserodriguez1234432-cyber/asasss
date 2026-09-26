// Decompiled with JetBrains decompiler
// Type: Vehicles.AdditionalShaderPropertyIDs
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;

#nullable disable
namespace Vehicles;

public static class AdditionalShaderPropertyIDs
{
  private const string PatternTexName = "_PatternTex";
  private const string MainTexName = "_MainTex";
  private const string ColorOneName = "_ColorOne";
  private const string ColorThreeName = "_ColorThree";
  private const string SkinTexName = "_SkinTex";
  private const string TileNumName = "_TileNum";
  private const string DisplacementXName = "_DisplacementX";
  private const string DisplacementYName = "_DisplacementY";
  private const string ScaleXName = "_ScaleX";
  private const string ScaleYName = "_ScaleY";
  public static readonly int MainTex = Shader.PropertyToID("_MainTex");
  public static readonly int PatternTex = Shader.PropertyToID("_PatternTex");
  public static readonly int ColorOne = Shader.PropertyToID("_ColorOne");
  public static readonly int ColorThree = Shader.PropertyToID("_ColorThree");
  public static readonly int SkinTex = Shader.PropertyToID("_SkinTex");
  public static readonly int TileNum = Shader.PropertyToID("_TileNum");
  public static readonly int DisplacementX = Shader.PropertyToID("_DisplacementX");
  public static readonly int DisplacementY = Shader.PropertyToID("_DisplacementY");
  public static readonly int ScaleX = Shader.PropertyToID("_ScaleX");
  public static readonly int ScaleY = Shader.PropertyToID("_ScaleY");
}
