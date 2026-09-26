// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompGravshipFacilityPossibly
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompGravshipFacilityPossibly : CompGravshipFacility
{
  public virtual string CompInspectStringExtra()
  {
    return GravshipUtility.GetPlayerGravEngine_NewTemp(((Thing) ((ThingComp) this).parent).Map) != null ? base.CompInspectStringExtra() : (string) null;
  }
}
