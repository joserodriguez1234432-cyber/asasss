// Decompiled with JetBrains decompiler
// Type: Vehicles.AnimationProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class AnimationProperties
{
  public int cycles = 1;
  public FloatRange exactRotation = new FloatRange(0.0f, 0.0f);
  public float rotationRate;
  public float scale = 1f;
  public FloatRange growthRate = new FloatRange(0.0f, 0.0f);
  public Vector3 offset = Vector3.zero;
  public Color color = Color.white;
  public FloatRange speedThrown = new FloatRange(0.0f, 0.0f);
  public FloatRange deceleration = new FloatRange(0.0f, 0.0f);
  public float fixedAcceleration;
  public FloatRange angleThrown = new FloatRange(0.0f, 0.0f);
  public ThingDef moteDef;
  public AnimationWrapperType animationType;
}
