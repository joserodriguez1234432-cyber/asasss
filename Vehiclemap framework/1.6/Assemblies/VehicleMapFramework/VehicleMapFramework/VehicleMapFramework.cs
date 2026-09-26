// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapFramework
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VehicleMapFramework.Settings;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleMapFramework : Mod
{
  public const string CategoryName = "Vehicle Map Framework";
  public static VehicleMapFramework.VehicleMapFramework mod;
  public static VehicleMapSettings settings;
  private static readonly List<TabRecord> tabs = new List<TabRecord>();

  public VehicleMapFramework(ModContentPack content)
    : base(content)
  {
    VehicleMapFramework.VehicleMapFramework.mod = this;
    VehicleMapFramework.VehicleMapFramework.settings = this.GetSettings<VehicleMapSettings>();
    EarlyPatchCore.EarlyPatch();
  }

  private static SettingsTabDrawer CurrentTab { get; set; }

  private static void InitializeTabs()
  {
    VehicleMapFramework.VehicleMapFramework.tabs.Clear();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    List<SettingsTabDrawer> list = GenTypes.AllSubclassesNonAbstract(typeof (SettingsTabDrawer)).Select<Type, object>(VehicleMapFramework.VehicleMapFramework.\u003C\u003EO.\u003C0\u003E__CreateInstance ?? (VehicleMapFramework.VehicleMapFramework.\u003C\u003EO.\u003C0\u003E__CreateInstance = new Func<Type, object>(Activator.CreateInstance))).Cast<SettingsTabDrawer>().OrderBy<SettingsTabDrawer, int>((Func<SettingsTabDrawer, int>) (tab => tab.Index)).ToList<SettingsTabDrawer>();
    VehicleMapFramework.VehicleMapFramework.CurrentTab = list[0];
    VehicleMapFramework.VehicleMapFramework.tabs.AddRange(list.Select<SettingsTabDrawer, TabRecord>((Func<SettingsTabDrawer, TabRecord>) (tab => new TabRecord(tab.Label, (Action) (() => VehicleMapFramework.VehicleMapFramework.CurrentTab = tab), (Func<bool>) (() => VehicleMapFramework.VehicleMapFramework.CurrentTab == tab)))));
  }

  public virtual void DoSettingsWindowContents(Rect inRect)
  {
    if (VehicleMapFramework.VehicleMapFramework.CurrentTab == null)
      VehicleMapFramework.VehicleMapFramework.InitializeTabs();
    base.DoSettingsWindowContents(inRect);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref inRect).x, ((Rect) ref inRect).y + 32f, ((Rect) ref inRect).width, ((Rect) ref inRect).height - 32f);
    Widgets.DrawMenuSection(rect);
    TabDrawer.DrawTabs<TabRecord>(rect, VehicleMapFramework.VehicleMapFramework.tabs, 200f);
    VehicleMapFramework.VehicleMapFramework.CurrentTab.Draw(GenUI.ContractedBy(rect, 10f));
  }

  public virtual void WriteSettings()
  {
    base.WriteSettings();
    Level patchLevel = !VehicleMapFramework.VehicleMapFramework.settings.dynamicPatchEnabled || Find.Maps != null && !Find.Maps.All<Map>((Func<Map, bool>) (map => VehicleMapParentsComponent.GetCachedVehicle(map) == null)) ? Level.All : VehicleMapFramework.VehicleMapFramework.settings.dynamicPatchLevel;
    if (VMF_Harmony.CurrentPatchLevel != patchLevel)
      VMF_Harmony.DynamicPatchAll(patchLevel);
    MethodInfo methodInfo = AccessTools.Method(typeof (RoofGrid), "Roofed", new Type[1]
    {
      typeof (IntVec3)
    }, (Type[]) null);
    if (VMF_Harmony.Instance.GetPatchedMethods().Contains<MethodBase>((MethodBase) methodInfo))
    {
      if (!VehicleMapFramework.VehicleMapFramework.settings.roofedPatch)
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        MethodInfo method = (VehicleMapFramework.VehicleMapFramework.\u003C\u003EO.\u003C1\u003E__Postfix ?? (VehicleMapFramework.VehicleMapFramework.\u003C\u003EO.\u003C1\u003E__Postfix = new \u003C\u003EA\u007B00000040\u007D<IntVec3, Map, bool>(Patch_RoofGrid_Roofed.Postfix))).Method;
        VMF_Harmony.Instance.Unpatch((MethodBase) methodInfo, method);
      }
    }
    else if (VehicleMapFramework.VehicleMapFramework.settings.roofedPatch)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      MethodInfo method = (VehicleMapFramework.VehicleMapFramework.\u003C\u003EO.\u003C1\u003E__Postfix ?? (VehicleMapFramework.VehicleMapFramework.\u003C\u003EO.\u003C1\u003E__Postfix = new \u003C\u003EA\u007B00000040\u007D<IntVec3, Map, bool>(Patch_RoofGrid_Roofed.Postfix))).Method;
      VMF_Harmony.Instance.Patch((MethodBase) methodInfo, (HarmonyMethod) null, HarmonyMethod.op_Implicit(method), (HarmonyMethod) null, (HarmonyMethod) null);
    }
    MethodInfo method1 = DebugToolsGeneral.GenericRectTool.Method;
    if (VMF_Harmony.Instance.GetPatchedMethods().Contains<MethodBase>((MethodBase) method1))
    {
      if (VehicleMapFramework.VehicleMapFramework.settings.debugToolPatches)
        return;
      Patches_DebugTools.ApplyPatches(true);
    }
    else
    {
      if (!VehicleMapFramework.VehicleMapFramework.settings.debugToolPatches)
        return;
      Patches_DebugTools.ApplyPatches();
    }
  }

  public virtual string SettingsCategory() => "Vehicle Map Framework";
}
