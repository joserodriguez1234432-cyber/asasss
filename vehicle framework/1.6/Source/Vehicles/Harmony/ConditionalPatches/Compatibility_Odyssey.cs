// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.Compatibility_Odyssey
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools.Patching;
using Vehicles.Config;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

internal class Compatibility_Odyssey : ConditionalVehiclePatch
{
  public override string PackageId => "ludeon.rimworld.odyssey";

  public override PatchSequence PatchAt => PatchSequence.PostDefDatabase;

  public override void PatchAll(ModMetaData mod)
  {
    if (!FeatureFlags.FishingEnabled)
      return;
    foreach (BiomeDef biomeDef in DefDatabase<BiomeDef>.AllDefsListForReading)
    {
      if (biomeDef.fishTypes != null)
      {
        foreach (FishChance fishChance in biomeDef.fishTypes.freshwater_Common)
          FishingCompatibility.AddFishDef(biomeDef, (WaterBodyType) 1, fishChance.fishDef, 1f);
        foreach (FishChance fishChance in biomeDef.fishTypes.saltwater_Common)
          FishingCompatibility.AddFishDef(biomeDef, (WaterBodyType) 2, fishChance.fishDef, 1f);
        foreach (FishChance fishChance in biomeDef.fishTypes.freshwater_Uncommon)
          FishingCompatibility.AddFishDef(biomeDef, (WaterBodyType) 1, fishChance.fishDef, 0.05f);
        foreach (FishChance fishChance in biomeDef.fishTypes.saltwater_Uncommon)
          FishingCompatibility.AddFishDef(biomeDef, (WaterBodyType) 2, fishChance.fishDef, 0.05f);
      }
    }
  }
}
