// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.JobDriverBodyOffset
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public abstract class JobDriverBodyOffset : JobDriver, IBodyOffsetJobDriver
{
  public Vector3 drawOffset;

  public virtual Vector3 ForcedBodyOffset => this.drawOffset;

  float IBodyOffsetJobDriver.PawnDrawPosOffset_Y => this.drawOffset.y;
}
