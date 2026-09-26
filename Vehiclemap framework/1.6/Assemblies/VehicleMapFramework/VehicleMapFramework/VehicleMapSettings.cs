// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapSettings
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleMapSettings : ModSettings
{
  public bool drawPlanet = true;
  public VehicleMapSettings.ForceRotated forceRotated = VehicleMapSettings.ForceRotated.None;
  public float weightFactor = 1f;
  public bool autoGetOffPlayer;
  public bool autoGetOffNonPlayer = true;
  public bool crossMapJobProtect = true;
  public bool drawVehicleMapGrid;
  public bool includeMapThings = true;
  public bool legacyCanReach;
  public bool joyPatches;
  public bool treatAsPlayerHome;
  public VehicleMapSettings.ShowVehiclesOnColonistBar colonistBarMode = VehicleMapSettings.ShowVehiclesOnColonistBar.MouseIsOver;
  public bool roofedPatch;
  public bool debugToolPatches;
  public bool dynamicPatchEnabled;
  public bool dynamicUnpatchEnabled;
  public Level dynamicPatchLevel = Level.Safe;

  public virtual void ExposeData()
  {
    Scribe_Values.Look<bool>(ref this.drawPlanet, "drawPlanet", true, false);
    Scribe_Values.Look<VehicleMapSettings.ForceRotated>(ref this.forceRotated, "forceRotated", VehicleMapSettings.ForceRotated.None, false);
    Scribe_Values.Look<float>(ref this.weightFactor, "weightFactor", 1f, false);
    Scribe_Values.Look<bool>(ref this.autoGetOffPlayer, "autoGetOffPlayer", false, false);
    Scribe_Values.Look<bool>(ref this.autoGetOffNonPlayer, "autoGetOffNonPlayer", true, false);
    Scribe_Values.Look<bool>(ref this.crossMapJobProtect, "crossMapJobProtect", true, false);
    Scribe_Values.Look<bool>(ref this.drawVehicleMapGrid, "drawVehicleMapGrid", false, false);
    Scribe_Values.Look<bool>(ref this.includeMapThings, "includeMapThings", true, false);
    Scribe_Values.Look<bool>(ref this.legacyCanReach, "legacyCanReach", false, false);
    Scribe_Values.Look<bool>(ref this.joyPatches, "joyPatches", false, false);
    Scribe_Values.Look<bool>(ref this.treatAsPlayerHome, "treatAsPlayerHome", false, false);
    Scribe_Values.Look<VehicleMapSettings.ShowVehiclesOnColonistBar>(ref this.colonistBarMode, "colonistBarMode", VehicleMapSettings.ShowVehiclesOnColonistBar.MouseIsOver, false);
    Scribe_Values.Look<bool>(ref this.roofedPatch, "roofedPatch", false, false);
    Scribe_Values.Look<bool>(ref this.debugToolPatches, "debugToolPatches", false, false);
    Scribe_Values.Look<bool>(ref this.dynamicPatchEnabled, "dynamicPatchEnabled", false, false);
    Scribe_Values.Look<bool>(ref this.dynamicUnpatchEnabled, "dynamicUnpatchEnabled", false, false);
    Scribe_Values.Look<Level>(ref this.dynamicPatchLevel, "dynamicPatchLevel", Level.Safe, false);
  }

  internal static class Default
  {
    public const bool drawPlanet = true;
    public const VehicleMapSettings.ForceRotated forceRotated = VehicleMapSettings.ForceRotated.None;
    public const float weightFactor = 1f;
    public const bool autoGetOffPlayer = false;
    public const bool autoGetOffNonPlayer = true;
    public const bool crossMapJobProtect = true;
    public const bool drawVehicleMapGrid = false;
    public const bool includeMapThings = true;
    public const bool legacyCanReach = false;
    public const bool joyPatches = false;
    public const bool treatAsPlayerHome = false;
    public const VehicleMapSettings.ShowVehiclesOnColonistBar colonistBarMode = VehicleMapSettings.ShowVehiclesOnColonistBar.MouseIsOver;
    public const bool roofedPatch = false;
    public const bool debugToolPatches = false;
    public const bool dynamicPatchEnabled = false;
    public const bool dynamicUnpatchEnabled = false;
    public const Level dynamicPatchLevel = Level.Safe;
  }

  public enum ShowVehiclesOnColonistBar
  {
    DontShow,
    MouseIsOver,
    Always,
  }

  [UsedImplicitly]
  public enum ForceRotated
  {
    None = -1, // 0xFFFFFFFF
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    NorthEast = 4,
    NorthWest = 5,
    SouthEast = 6,
    SouthWest = 7,
  }
}
