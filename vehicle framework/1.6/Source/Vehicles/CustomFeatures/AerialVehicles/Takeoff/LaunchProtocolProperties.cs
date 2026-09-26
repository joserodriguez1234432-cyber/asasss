// Decompiled with JetBrains decompiler
// Type: Vehicles.LaunchProtocolProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class LaunchProtocolProperties
{
  public int maxTicks = 250;
  public int delayByTicks;
  public Rot4? forcedRotation;
  public Rot4 flipHorizontal = Rot4.Invalid;
  public Rot4 flipVertical = Rot4.Invalid;
  public Rot4 flipRotation = Rot4.Invalid;
  public LaunchRestriction restriction;
  public List<GraphicDataLayered> additionalTextures;
  public List<AnimationEvent<LaunchProtocol>> events;
  public bool renderShadow = true;
  public bool lockShadowX;
  public bool lockShadowZ;
  public Vector2 shadowOffset = Vector2.zero;
  [GraphEditable]
  public LinearCurve shadowSizeXCurve;
  [GraphEditable]
  public LinearCurve shadowSizeZCurve;
  [GraphEditable]
  public LinearCurve shadowAlphaCurve;
  [GraphEditable(FunctionOfT = true)]
  public LinearCurve offsetCurve;
  [GraphEditable]
  public LinearCurve xPositionCurve;
  [GraphEditable]
  public LinearCurve zPositionCurve;
  [GraphEditable]
  public LinearCurve rotationCurve;
  [GraphEditable(Prefix = "Fleck")]
  public List<FleckData> fleckData;
  public List<FleckOneShot> fleckOneShots;
}
