// Decompiled with JetBrains decompiler
// Type: Vehicles.CaravanInfoHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class CaravanInfoHelper
{
  private static readonly List<ThingCount> tmpThingCounts = new List<ThingCount>();

  public static float Capacity(List<ThingCount> thingCounts, StringBuilder explanation = null)
  {
    float num1 = 0.0f;
    for (int index = 0; index < thingCounts.Count; ++index)
    {
      ThingCount thingCount = thingCounts[index];
      if (((ThingCount) ref thingCount).Count > 0)
      {
        thingCount = thingCounts[index];
        if (((ThingCount) ref thingCount).Thing is Pawn thing && !thing.InVehicle() && !CaravanHelper.assignedSeats.IsAssigned(thing))
        {
          double num2 = (double) num1;
          double num3 = (double) MassUtility.Capacity(thing, explanation);
          thingCount = thingCounts[index];
          double count = (double) ((ThingCount) ref thingCount).Count;
          double num4 = num3 * count;
          num1 = (float) (num2 + num4);
        }
      }
    }
    return Mathf.Max(num1, 0.0f);
  }
}
