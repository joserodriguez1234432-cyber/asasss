// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SoundStarter_PlayOneShot
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (SoundStarter), "PlayOneShot")]
[PatchLevel(Level.Safe)]
public static class Patch_SoundStarter_PlayOneShot
{
  public static void Prefix(ref SoundInfo info)
  {
    TargetInfo maker1 = ((SoundInfo) ref info).Maker;
    if (!((TargetInfo) ref maker1).IsValid)
      return;
    TargetInfo maker2 = ((SoundInfo) ref info).Maker;
    VehiclePawnWithMap vehicle;
    if (!((TargetInfo) ref maker2).Map.IsVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned)
      return;
    ref SoundInfo local = ref info;
    TargetInfo maker3 = ((SoundInfo) ref info).Maker;
    SoundInfo soundInfo = SoundInfo.InMap(new TargetInfo(((TargetInfo) ref maker3).Cell.ToBaseMapCoord(vehicle), ((Thing) vehicle).Map, false), ((SoundInfo) ref info).Maintenance);
    local = soundInfo;
  }
}
