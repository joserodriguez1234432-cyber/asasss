// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.LatePatchCore
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(0)]
public static class LatePatchCore
{
  public const string Category = "VehicleMapFramework.LatePatches";

  static LatePatchCore()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VMF_Harmony.PatchCategory("VehicleMapFramework.LatePatches");
      string[] source = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version?.Split('.', StringSplitOptions.None);
      if (source != null)
        VMF_Log.Message($"{((IEnumerable<string>) source).ElementAtOrDefault<string>(0) ?? "0"}.{((IEnumerable<string>) source).ElementAtOrDefault<string>(1) ?? "0"}.{((IEnumerable<string>) source).ElementAtOrDefault<string>(2) ?? "0"} rev{((IEnumerable<string>) source).ElementAtOrDefault<string>(3) ?? "0"}");
      VMF_Log.Message($"{VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>()} patches applied.");
    }));
  }
}
