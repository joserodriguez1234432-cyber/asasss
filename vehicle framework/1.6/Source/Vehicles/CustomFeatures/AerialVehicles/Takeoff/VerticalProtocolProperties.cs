// Decompiled with JetBrains decompiler
// Type: Vehicles.VerticalProtocolProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;

#nullable disable
namespace Vehicles;

public class VerticalProtocolProperties : LaunchProtocolProperties
{
  public int maxTicksVertical;
  public List<AnimationEvent<LaunchProtocol>> eventsVertical;
  [GraphEditable]
  public LinearCurve shadowSizeXVerticalCurve;
  [GraphEditable]
  public LinearCurve shadowSizeZVerticalCurve;
  [GraphEditable]
  public LinearCurve shadowAlphaVerticalCurve;
  [GraphEditable(FunctionOfT = true)]
  public LinearCurve offsetVerticalCurve;
  [GraphEditable]
  public LinearCurve xPositionVerticalCurve;
  [GraphEditable]
  public LinearCurve zPositionVerticalCurve;
  [GraphEditable]
  public LinearCurve rotationVerticalCurve;
  [GraphEditable(Prefix = "FleckVTOL")]
  public FleckData fleckDataVertical;
}
