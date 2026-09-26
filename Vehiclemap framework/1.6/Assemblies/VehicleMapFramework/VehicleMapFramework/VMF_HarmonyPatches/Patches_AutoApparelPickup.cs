// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_AutoApparelPickup
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
public class Patches_AutoApparelPickup
{
  static Patches_AutoApparelPickup()
  {
    if (!ModCompat.AutoApparelPickup || !(AccessTools.Field("AutoApparelPickup.HarmonyPatches:ignoredJobs")?.GetValue((object) null) is HashSet<JobDef> jobDefSet))
      return;
    jobDefSet.Add(VMF_DefOf.VMF_GotoDestMap);
    jobDefSet.Add(VMF_DefOf.VMF_GotoAcrossMaps);
    jobDefSet.Add(VMF_DefOf.VMF_BoardAcrossMaps);
  }
}
