// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.BedInteractionCellSearchPattern3xN
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class BedInteractionCellSearchPattern3xN : BedInteractionCellSearchPattern
{
  public virtual void BedCellOffsets(List<IntVec3> offsets, IntVec2 size, int slot)
  {
    if (IntVec2.op_Equality(size, IntVec2.One))
    {
      BedInteractionCellSearchPattern.BedCellOffsets1x1(offsets);
    }
    else
    {
      bool flag1 = slot == 0;
      bool flag2 = slot == BedUtility.GetSleepingSlotsCount(size) - 1;
      BedInteractionCellSearchPattern.BedCellOffsets2xN(offsets, flag1, flag2);
    }
  }
}
