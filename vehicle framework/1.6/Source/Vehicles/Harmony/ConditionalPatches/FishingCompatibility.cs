// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.FishingCompatibility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

[PublicAPI]
public static class FishingCompatibility
{
  private static readonly List<Thing> FishingResult = new List<Thing>();
  private static readonly Dictionary<BiomeDef, FishingCompatibility.FishList> FishDefs = new Dictionary<BiomeDef, FishingCompatibility.FishList>();

  public static bool Active { get; private set; }

  public static bool CanFishAt(VehiclePawn vehicle, IntVec3 cell)
  {
    BiomeDef biome = ((Thing) vehicle).Map.Biome;
    WaterBodyType waterBodyType = GridsUtility.GetWaterBodyType(cell, ((Thing) vehicle).Map);
    FishingCompatibility.FishList fishList;
    return FishingCompatibility.FishDefs.TryGetValue(biome, out fishList) && !fishList.IsEmpty(waterBodyType);
  }

  public static void AddFishDef(
    BiomeDef biomeDef,
    WaterBodyType waterBodyType,
    ThingDef thingDef,
    float commonality,
    float fishYield = 1f)
  {
    FishingCompatibility.Active = true;
    FishingCompatibility.FishList fishList;
    if (!FishingCompatibility.FishDefs.TryGetValue(biomeDef, out fishList))
      FishingCompatibility.FishDefs[biomeDef] = fishList = new FishingCompatibility.FishList();
    fishList[waterBodyType, thingDef] = commonality;
    fishList.SetYield(thingDef, fishYield);
  }

  public static List<Thing> GetCatchesFor(
    VehiclePawn vehicle,
    IntVec3 cell,
    BiomeDef biomeDef,
    out bool rare)
  {
    if (ModsConfig.OdysseyActive)
      return FishingUtility.GetCatchesFor((Pawn) vehicle, cell, false, ref rare);
    rare = false;
    WaterBodyType waterBodyType = GridsUtility.GetWaterBodyType(cell, ((Thing) vehicle).Map);
    FishingCompatibility.FishingResult.Clear();
    FishingCompatibility.FishList fishList;
    if (!FishingCompatibility.FishDefs.TryGetValue(biomeDef, out fishList) || fishList.IsEmpty(waterBodyType))
    {
      Trace.Fail($"Fishing in biome {biomeDef} which does not have any fish registered.");
      return (List<Thing>) null;
    }
    FishingProperties fishingProperties = vehicle.VehicleDef.fishingProperties;
    ThingDef randomFishDef = fishList.GetRandomFishDef(waterBodyType);
    float num = (float) Mathf.CeilToInt((float) ((int?) fishingProperties?.animalSkillOverride ?? vehicle.AverageSkillOfCapablePawns(SkillDefOf.Animals)) / 15f);
    Thing thing = ThingMaker.MakeThing(randomFishDef, (ThingDef) null);
    thing.stackCount = Mathf.Clamp(Mathf.RoundToInt(num * fishList.GetYield(randomFishDef) * VehicleMod.settings.main.fishingMultiplier), 1, thing.def.stackLimit * 3);
    FishingCompatibility.FishingResult.Add(thing);
    return FishingCompatibility.FishingResult;
  }

  internal class FishList
  {
    internal const float DefaultYieldModifier = 1f;
    private readonly Dictionary<ThingDef, float> yieldModifiers = new Dictionary<ThingDef, float>();
    private readonly Dictionary<ThingDef, float> fishDefsFreshWater = new Dictionary<ThingDef, float>();
    private readonly Dictionary<ThingDef, float> fishDefsSaltWater = new Dictionary<ThingDef, float>();
    private readonly Dictionary<ThingDef, float> fishDefsOther = new Dictionary<ThingDef, float>();

    public float this[WaterBodyType waterBodyType, ThingDef thingDef]
    {
      get
      {
        float num;
        switch (waterBodyType - 1)
        {
          case 0:
            num = GenCollection.TryGetValue<ThingDef, float>((IReadOnlyDictionary<ThingDef, float>) this.fishDefsFreshWater, thingDef, 0.0f);
            break;
          case 1:
            num = GenCollection.TryGetValue<ThingDef, float>((IReadOnlyDictionary<ThingDef, float>) this.fishDefsSaltWater, thingDef, 0.0f);
            break;
          case 2:
            num = GenCollection.TryGetValue<ThingDef, float>((IReadOnlyDictionary<ThingDef, float>) this.fishDefsOther, thingDef, 0.0f);
            break;
          default:
            num = 0.0f;
            break;
        }
        return num;
      }
      set
      {
        switch ((int) waterBodyType)
        {
          case 0:
            throw new InvalidOperationException("Trying to register fish for water body of type 'None'");
          case 1:
            this.fishDefsFreshWater[thingDef] = value;
            break;
          case 2:
            this.fishDefsSaltWater[thingDef] = value;
            break;
          case 3:
            this.fishDefsOther[thingDef] = value;
            break;
          default:
            throw new NotImplementedException(waterBodyType.ToString());
        }
      }
    }

    public float GetYield(ThingDef fishDef)
    {
      return GenCollection.TryGetValue<ThingDef, float>((IReadOnlyDictionary<ThingDef, float>) this.yieldModifiers, fishDef, 1f);
    }

    public void SetYield(ThingDef fishDef, float modifier)
    {
      if (Mathf.Approximately(modifier, 1f))
        return;
      this.yieldModifiers[fishDef] = modifier;
    }

    public bool IsCompletelyEmpty()
    {
      return this.IsEmpty((WaterBodyType) 1) && this.IsEmpty((WaterBodyType) 2);
    }

    public bool IsEmpty(WaterBodyType waterBodyType)
    {
      return waterBodyType == 1 ? this.fishDefsFreshWater.Count == 0 : waterBodyType != 2 || this.fishDefsSaltWater.Count == 0;
    }

    public ThingDef GetRandomFishDef(WaterBodyType waterBodyType)
    {
      switch ((int) waterBodyType)
      {
        case 0:
          return (ThingDef) null;
        case 1:
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          return GenCollection.RandomElementByWeightWithFallback<KeyValuePair<ThingDef, float>>((IEnumerable<KeyValuePair<ThingDef, float>>) this.fishDefsFreshWater, FishingCompatibility.FishList.\u003C\u003EO.\u003C0\u003E__FishWeightSelector ?? (FishingCompatibility.FishList.\u003C\u003EO.\u003C0\u003E__FishWeightSelector = new Func<KeyValuePair<ThingDef, float>, float>(FishingCompatibility.FishList.FishWeightSelector)), new KeyValuePair<ThingDef, float>()).Key;
        case 2:
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          return GenCollection.RandomElementByWeightWithFallback<KeyValuePair<ThingDef, float>>((IEnumerable<KeyValuePair<ThingDef, float>>) this.fishDefsSaltWater, FishingCompatibility.FishList.\u003C\u003EO.\u003C0\u003E__FishWeightSelector ?? (FishingCompatibility.FishList.\u003C\u003EO.\u003C0\u003E__FishWeightSelector = new Func<KeyValuePair<ThingDef, float>, float>(FishingCompatibility.FishList.FishWeightSelector)), new KeyValuePair<ThingDef, float>()).Key;
        case 3:
          Log.ErrorOnce("Fishing in water body type that only exists in Odyssey, but Odyssey isn't loaded.", "OdysseyNotLoadedForFishing".GetHashCode());
          goto case 0;
        default:
          throw new NotImplementedException("WaterBodyType");
      }
    }

    private static float FishWeightSelector(KeyValuePair<ThingDef, float> kvp) => kvp.Value;
  }
}
