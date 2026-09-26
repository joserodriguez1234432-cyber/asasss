// Decompiled with JetBrains decompiler
// Type: Vehicles.LaunchRestriction_ComponentHealth
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class LaunchRestriction_ComponentHealth : LaunchRestriction
{
  private SimpleDictionary<string, float> components = new SimpleDictionary<string, float>();

  public override bool CanStartProtocol(VehiclePawn vehicle, Map map, IntVec3 position, Rot4 rot)
  {
    if (!GenDictionary.NullOrEmpty<string, float>((Dictionary<string, float>) this.components))
    {
      foreach (KeyValuePair<string, float> component in (Dictionary<string, float>) this.components)
      {
        string str;
        float num1;
        component.Deconstruct(ref str, ref num1);
        string key = str;
        float num2 = num1;
        if ((double) vehicle.statHandler.GetComponentHealthPercent(key) < (double) num2)
          return false;
      }
    }
    return true;
  }
}
