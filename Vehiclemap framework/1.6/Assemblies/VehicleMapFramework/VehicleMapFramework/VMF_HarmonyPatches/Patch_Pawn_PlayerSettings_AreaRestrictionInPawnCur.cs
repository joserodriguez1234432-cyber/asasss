// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Cautious)]
public static class Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap
{
  private static readonly AccessTools.FieldRef<Pawn_PlayerSettings, Pawn> pawn = AccessTools.FieldRefAccess<Pawn_PlayerSettings, Pawn>(nameof (pawn));

  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) AccessTools.Method(typeof (AreaAllowedGUI), "DoAreaSelector", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(typeof (InspectPaneFiller), "DrawAreaAllowed", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (InspectPaneFiller), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name.Contains("<DrawAreaAllowed>")))));
    yield return (MethodBase) AccessTools.Method(typeof (PawnColumnWorker_AllowedArea), "HeaderClicked", (Type[]) null, (Type[]) null);
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    if (UnitTestDetector.IsTestingContext)
      return instructions;
    MethodInfo methodInfo1 = AccessTools.PropertyGetter(typeof (Pawn_PlayerSettings), "AreaRestrictionInPawnCurrentMap");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method1 = (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C0\u003E__get_AreaRestrictionInPawnBaseMap ?? (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C0\u003E__get_AreaRestrictionInPawnBaseMap = new Func<Pawn_PlayerSettings, Area>(Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.get_AreaRestrictionInPawnBaseMap))).Method;
    MethodInfo methodInfo2 = AccessTools.PropertySetter(typeof (Pawn_PlayerSettings), "AreaRestrictionInPawnCurrentMap");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method2 = (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C1\u003E__set_AreaRestrictionInPawnBaseMap ?? (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C1\u003E__set_AreaRestrictionInPawnBaseMap = new Action<Pawn_PlayerSettings, Area>(Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.set_AreaRestrictionInPawnBaseMap))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method3 = (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C2\u003E__AreaAllowedLabel ?? (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C2\u003E__AreaAllowedLabel = new Func<Pawn, string>(AreaUtility.AreaAllowedLabel))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method4 = (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C3\u003E__AreaAllowedLabelBaseMap ?? (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C3\u003E__AreaAllowedLabelBaseMap = new Func<Pawn, string>(Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.AreaAllowedLabelBaseMap))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method5 = (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C4\u003E__MakeAllowedAreaListFloatMenu ?? (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C4\u003E__MakeAllowedAreaListFloatMenu = new Action<Action<Area>, bool, bool, Map>(AreaUtility.MakeAllowedAreaListFloatMenu))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method6 = (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C5\u003E__MakeAllowedAreaListFloatMenuBaseMap ?? (Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.\u003C\u003EO.\u003C5\u003E__MakeAllowedAreaListFloatMenuBaseMap = new Action<Action<Area>, bool, bool, Map>(Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.MakeAllowedAreaListFloatMenuBaseMap))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(((MethodBase) methodInfo1, (MethodBase) method1), ((MethodBase) methodInfo2, (MethodBase) method2), ((MethodBase) method3, (MethodBase) method4), ((MethodBase) method5, (MethodBase) method6));
  }

  private static Area get_AreaRestrictionInPawnBaseMap(Pawn_PlayerSettings playerSettings)
  {
    Pawn pawn = Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.pawn.Invoke(playerSettings);
    Map map = ((Thing) pawn).MapHeldBaseMap();
    if (Find.CurrentMap != map || ((Thing) pawn).MapHeld == map)
      return playerSettings.AreaRestrictionInPawnCurrentMap;
    using (new VirtualTeleporter((Thing) pawn, map, new IntVec3?(VehicleMapUtility.get_PositionOnBaseMap((Thing) pawn)), true))
      return playerSettings.AreaRestrictionInPawnCurrentMap;
  }

  private static void set_AreaRestrictionInPawnBaseMap(
    Pawn_PlayerSettings playerSettings,
    Area value)
  {
    Pawn pawn = Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.pawn.Invoke(playerSettings);
    Map mapHeld = ((Thing) pawn).MapHeld;
    Map map = mapHeld.BaseMap();
    if (Find.CurrentMap == map && mapHeld != map)
    {
      using (new VirtualTeleporter((Thing) pawn, map, new IntVec3?(VehicleMapUtility.get_PositionOnBaseMap((Thing) pawn)), true))
      {
        playerSettings.AreaRestrictionInPawnCurrentMap = value;
        CrossMapReachabilityCache.ClearCacheFor(map);
      }
    }
    else
    {
      playerSettings.AreaRestrictionInPawnCurrentMap = value;
      CrossMapReachabilityCache.ClearCacheFor(mapHeld);
    }
  }

  private static string AreaAllowedLabelBaseMap(Pawn _pawn)
  {
    Pawn_PlayerSettings playerSettings = _pawn.playerSettings;
    return AreaUtility.AreaAllowedLabel_Area(playerSettings != null ? Patch_Pawn_PlayerSettings_AreaRestrictionInPawnCurrentMap.get_AreaRestrictionInPawnBaseMap(playerSettings) : (Area) null);
  }

  private static void MakeAllowedAreaListFloatMenuBaseMap(
    Action<Area> selAction,
    bool addNullAreaOption,
    bool addManageOption,
    Map map)
  {
    Map groundMap = VehicleMapUtility.get_GroundMap(map);
    if (Find.CurrentMap == groundMap && map != groundMap)
      AreaUtility.MakeAllowedAreaListFloatMenu(selAction, addNullAreaOption, addManageOption, groundMap);
    else
      AreaUtility.MakeAllowedAreaListFloatMenu(selAction, addNullAreaOption, addManageOption, map);
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024871A5DE5D74614497D02A547F3B0E7A5
  {
    [ExtensionMarker("<M>$C73F3520B3353DF9C7F2F2DE93AC0169")]
    private Area AreaRestrictionInPawnBaseMap
    {
      [ExtensionMarker("<M>$C73F3520B3353DF9C7F2F2DE93AC0169")] get
      {
        throw new NotSupportedException();
      }
      [ExtensionMarker("<M>$C73F3520B3353DF9C7F2F2DE93AC0169")] set
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024C73F3520B3353DF9C7F2F2DE93AC0169
    {
    }
  }
}
