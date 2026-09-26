// Decompiled with JetBrains decompiler
// Type: Vehicles.PropellerProtocolProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;

#nullable disable
namespace Vehicles;

public class PropellerProtocolProperties : VerticalProtocolProperties
{
  public int maxTicksPropeller;
  public List<GraphicDataLayered> additionalTexturesPropeller;
  public List<AnimationEvent<LaunchProtocol>> eventsPropeller;
  [GraphEditable]
  public LinearCurve shadowSizeXPropellerCurve;
  [GraphEditable]
  public LinearCurve shadowSizeZPropellerCurve;
  [GraphEditable]
  public LinearCurve shadowAlphaPropellerCurve;
  [GraphEditable]
  public LinearCurve angularVelocityPropeller;
  [GraphEditable(FunctionOfT = true)]
  public LinearCurve offsetPropellerCurve;
  [GraphEditable]
  public LinearCurve xPositionPropellerCurve;
  [GraphEditable]
  public LinearCurve zPositionPropellerCurve;
  [GraphEditable]
  public LinearCurve rotationPropellerCurve;
  [GraphEditable(Prefix = "FleckPropeller")]
  public FleckData fleckDataPropeller;
}
