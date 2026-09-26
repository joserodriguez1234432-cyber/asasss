// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.UniqueVehicleUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public static class UniqueVehicleUtility
{
  private static readonly Action<Def, Type, HashSet<ushort>> GiveShortHash = (Action<Def, Type, HashSet<ushort>>) AccessTools.Method(typeof (ShortHashGiver), nameof (GiveShortHash), (Type[]) null, (Type[]) null).CreateDelegate(typeof (Action<Def, Type, HashSet<ushort>>));
  private static readonly Dictionary<Type, HashSet<ushort>> takenHashesPerDeftype = AccessTools.StaticFieldRefAccess<Dictionary<Type, HashSet<ushort>>>(typeof (ShortHashGiver), nameof (takenHashesPerDeftype));
  internal static readonly FastInvokeHandler GeneratePathData;

  static UniqueVehicleUtility()
  {
    MethodInfo methodInfo = AccessTools.Method("Vehicles.PathDataContainer:CreatePathData", (Type[]) null, (Type[]) null);
    if ((object) methodInfo == null)
      return;
    UniqueVehicleUtility.GeneratePathData = MethodInvoker.GetHandler(methodInfo, false);
  }

  public static bool get_IsUniqueVehicle(VehicleDef def)
  {
    return ((Def) def).HasModExtension<VehicleMapProps_Unique>();
  }

  private static string GetDefName(VehicleDef parentDef, int index)
  {
    return $"{index.ToString()}_{((Def) parentDef).defName}";
  }

  public static VehicleDef GenerateUniqueVehicleDef(VehicleDef parentDef, int index)
  {
    VehicleDef uniqueVehicleDef = DefDatabase<VehicleDef>.GetNamedSilentFail(UniqueVehicleUtility.GetDefName(parentDef, index));
    bool flag = uniqueVehicleDef != null;
    if (uniqueVehicleDef == null)
      uniqueVehicleDef = UniqueVehicleUtility.GenerateInner(parentDef, index);
    VehicleMod.GenerateImpliedDefs(uniqueVehicleDef, flag);
    DefGenerator.AddImpliedDef<VehicleDef>(uniqueVehicleDef, flag);
    if (!flag)
      DefDatabase<ThingDef>.Add((ThingDef) uniqueVehicleDef);
    return uniqueVehicleDef;
  }

  private static VehicleDef GenerateInner(VehicleDef parentDef, int index)
  {
    VehicleMapProps_Unique modExtension = ((Def) parentDef).GetModExtension<VehicleMapProps_Unique>();
    if (modExtension == null)
      return parentDef;
    VehicleDef inner = Gen.MemberwiseClone<VehicleDef>(parentDef);
    ((Def) inner).defName = UniqueVehicleUtility.GetDefName(parentDef, index);
    inner.graphicData = new GraphicDataRGB();
    ((GraphicDataLayered) inner.graphicData).CopyFrom((GraphicDataLayered) parentDef.graphicData);
    inner.drawProperties = Gen.MemberwiseClone<VehicleDrawProperties>(inner.drawProperties);
    if (parentDef.components != null)
    {
      inner.components = new List<VehicleComponentProperties>();
      foreach (VehicleComponentProperties component in parentDef.components)
      {
        VehicleComponentProperties componentProperties = Gen.MemberwiseClone<VehicleComponentProperties>(component);
        componentProperties.hitbox = Gen.MemberwiseClone<ComponentHitbox>(component.hitbox);
        inner.components.Add(componentProperties);
      }
    }
    VehicleMapProps_Unique vehicleMapPropsUnique = Gen.MemberwiseClone<VehicleMapProps_Unique>(modExtension);
    vehicleMapPropsUnique.baseDef = parentDef;
    ((Def) inner).modExtensions = ((Def) parentDef).modExtensions.ToList<DefModExtension>();
    ((Def) inner).modExtensions.Remove((DefModExtension) modExtension);
    ((Def) inner).modExtensions.Add((DefModExtension) vehicleMapPropsUnique);
    VehicleDef vehicleDef = inner;
    if (((ThingDef) vehicleDef).comps == null)
      ((ThingDef) vehicleDef).comps = new List<CompProperties>();
    ((ThingDef) inner).comps.Add(new CompProperties()
    {
      compClass = typeof (CompVehicleDrawOffset)
    });
    ((Def) inner).shortHash = (ushort) 0;
    UniqueVehicleUtility.GiveShortHash((Def) inner, typeof (ThingDef), UniqueVehicleUtility.takenHashesPerDeftype[typeof (ThingDef)]);
    return inner;
  }

  public static VehicleDef ClaimUniqueVehicleDef(VehicleDef parentDef)
  {
    return Current.Game.GetComponent<UniqueVehicleManager>()?.ClaimUniqueVehicleDef(parentDef) ?? parentDef;
  }

  public static void ReleaseUniqueVehicleDef(VehicleDef def)
  {
    Current.Game.GetComponent<UniqueVehicleManager>()?.ReleaseUniqueVehicleDef(def);
  }

  public static void ReinitializeComponents(VehicleDef def)
  {
    if (def.components == null)
      return;
    foreach (VehicleComponentProperties component in def.components)
    {
      component.hitbox.Hitbox.Clear();
      component.hitbox.Initialize(def);
    }
  }

  public static bool AllowGenerate(VehicleDef def)
  {
    if (!UniqueVehicleUtility.get_IsUniqueVehicle(def))
      return true;
    UniqueVehicleManager component = Current.Game.GetComponent<UniqueVehicleManager>();
    if (component == null)
      return false;
    int num = component.ClaimedCount(def);
    int? count = GenCollection.TryGetValue<VehicleDef, List<VehicleDef>>((IReadOnlyDictionary<VehicleDef, List<VehicleDef>>) UniqueVehicleManager.PlaceholderDefs, def, (List<VehicleDef>) null)?.Count;
    int valueOrDefault = count.GetValueOrDefault();
    return num < valueOrDefault & count.HasValue;
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024552049708062CA4A074824F23C0F249A
  {
    [ExtensionMarker("<M>$18789635076E0F3F428A60F20094F6AF")]
    public bool IsUniqueVehicle
    {
      [ExtensionMarker("<M>$18789635076E0F3F428A60F20094F6AF")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u002418789635076E0F3F428A60F20094F6AF
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(VehicleDef def)
      {
      }
    }
  }
}
