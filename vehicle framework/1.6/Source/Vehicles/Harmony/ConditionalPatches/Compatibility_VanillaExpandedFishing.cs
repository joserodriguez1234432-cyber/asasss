// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.Compatibility_VanillaExpandedFishing
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vehicles.Config;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

internal class Compatibility_VanillaExpandedFishing : ConditionalVehiclePatch
{
  public override string PackageId => "VanillaExpanded.VCEF";

  public override PatchSequence PatchAt => PatchSequence.PostDefDatabase;

  public override void PatchAll(ModMetaData mod)
  {
    if (!FeatureFlags.FishingEnabled || ModsConfig.OdysseyActive)
      return;
    System.Type typeInAnyAssembly1 = GenTypes.GetTypeInAnyAssembly("VCE_Fishing.FishDef", (string) null);
    System.Type typeInAnyAssembly2 = GenTypes.GetTypeInAnyAssembly("VCE_Fishing.BiomeTempDef", (string) null);
    FieldInfo biomeTempLabelField = AccessTools.Field(typeInAnyAssembly2, "biomeTempLabel");
    FieldInfo fieldInfo1 = AccessTools.Field(typeInAnyAssembly2, "biomes");
    FieldInfo fieldInfo2 = AccessTools.Field(typeInAnyAssembly1, "thingDef");
    FieldInfo fieldInfo3 = AccessTools.Field(typeInAnyAssembly1, "allowedBiomes");
    FieldInfo fieldInfo4 = AccessTools.Field(typeInAnyAssembly1, "canBeFreshwater");
    FieldInfo fieldInfo5 = AccessTools.Field(typeInAnyAssembly1, "canBeSaltwater");
    FieldInfo fieldInfo6 = AccessTools.Field(typeInAnyAssembly1, "commonality");
    FieldInfo fieldInfo7 = AccessTools.Field(typeInAnyAssembly1, "baseFishingYield");
    Dictionary<string, Def> dictionary = GenDefDatabase.GetAllDefsInDatabaseForDef(typeInAnyAssembly2).ToDictionary<Def, string, Def>((Func<Def, string>) (def => (string) biomeTempLabelField.GetValue((object) def)), (Func<Def, Def>) (def => def));
    foreach (Def def1 in GenDefDatabase.GetAllDefsInDatabaseForDef(typeInAnyAssembly1))
    {
      ThingDef thingDef = (ThingDef) fieldInfo2.GetValue((object) def1);
      List<string> stringList = (List<string>) fieldInfo3.GetValue((object) def1);
      bool flag1 = (bool) fieldInfo4.GetValue((object) def1);
      bool flag2 = (bool) fieldInfo5.GetValue((object) def1);
      float commonality = (float) fieldInfo6.GetValue((object) def1);
      int num = (int) fieldInfo7.GetValue((object) def1);
      foreach (string str1 in stringList)
      {
        Def def2 = GenCollection.TryGetValue<string, Def>((IReadOnlyDictionary<string, Def>) dictionary, str1, (Def) null);
        foreach (string str2 in (List<string>) fieldInfo1.GetValue((object) def2))
        {
          BiomeDef named = DefDatabase<BiomeDef>.GetNamed(str2, true);
          if (named != null)
          {
            if (flag1)
              FishingCompatibility.AddFishDef(named, (WaterBodyType) 1, thingDef, commonality, (float) num / 1f);
            if (flag2)
              FishingCompatibility.AddFishDef(named, (WaterBodyType) 2, thingDef, commonality, (float) num / 1f);
          }
        }
      }
    }
  }
}
