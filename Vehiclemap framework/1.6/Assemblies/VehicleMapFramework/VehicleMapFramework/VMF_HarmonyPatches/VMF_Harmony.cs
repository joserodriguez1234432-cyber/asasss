// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.VMF_Harmony
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

public class VMF_Harmony
{
  internal static readonly Harmony Instance = new Harmony("OELS.VehicleMapFramework");
  internal static readonly List<string> Categories = new List<string>();
  private static readonly AccessTools.FieldRef<PatchClassProcessor, object> patchMethodsRef;
  private static readonly AccessTools.FieldRef<object, HarmonyMethod> infoRef;
  private static readonly MethodInfo m_RemoveAll;

  internal static Dictionary<string, List<Type>> PatchesInCategories
  {
    get
    {
      if (VMF_Harmony.\u003CPatchesInCategories\u003Ek__BackingField == null)
      {
        List<Assembly> assemblies = VehicleMapFramework.VehicleMapFramework.mod.Content.assemblies.loadedAssemblies;
        VMF_Harmony.\u003CPatchesInCategories\u003Ek__BackingField = GenTypes.AllTypes.AsParallel<Type>().Where<Type>((Func<Type, bool>) (t => assemblies.Contains(t.Assembly) && t.CustomAttributes.Select<CustomAttributeData, Type>((Func<CustomAttributeData, Type>) (attribute => attribute.AttributeType)).Contains<Type>(typeof (HarmonyPatch)))).GroupBy<Type, string>((Func<Type, string>) (t =>
        {
          CustomAttributeData customAttributeData = t.CustomAttributes.FirstOrDefault<CustomAttributeData>((Func<CustomAttributeData, bool>) (attribute => attribute.AttributeType == typeof (HarmonyPatchCategory)));
          return customAttributeData == null ? "" : customAttributeData.ConstructorArguments.Select<CustomAttributeTypedArgument, object>((Func<CustomAttributeTypedArgument, object>) (c => c.Value)).OfType<string>().FirstOrDefault<string>() ?? "";
        })).ToDictionary<IGrouping<string, Type>, string, List<Type>>((Func<IGrouping<string, Type>, string>) (group => group.Key), (Func<IGrouping<string, Type>, List<Type>>) (group => group.Key == "VMF_Patches_VehicleFramework" ? group.Where<Type>((Func<Type, bool>) (t =>
        {
          VFVersionalPatchAttribute customAttribute = t.GetCustomAttribute<VFVersionalPatchAttribute>();
          return customAttribute == null || customAttribute.Available;
        })).ToList<Type>() : group.ToList<Type>()));
      }
      return VMF_Harmony.\u003CPatchesInCategories\u003Ek__BackingField;
    }
  }

  internal static Level CurrentPatchLevel { get; private set; }

  internal static Level PrevPatchLevel { get; set; }

  internal static bool OutOfRange(Level level)
  {
    if (level < VMF_Harmony.PrevPatchLevel && level < VMF_Harmony.CurrentPatchLevel || level > VMF_Harmony.PrevPatchLevel && level > VMF_Harmony.CurrentPatchLevel)
      return true;
    Level level1 = VMF_Harmony.PrevPatchLevel > VMF_Harmony.CurrentPatchLevel ? VMF_Harmony.PrevPatchLevel : VMF_Harmony.CurrentPatchLevel;
    return level == level1;
  }

  internal static void AdjustPatchLevel(PatchClassProcessor patchClassProcessor)
  {
    VMF_Harmony.m_RemoveAll.Invoke(VMF_Harmony.patchMethodsRef.Invoke(patchClassProcessor), SingleParam.Get((object) new Predicate<object>(Predicate)));

    static bool Predicate(object attributePatch)
    {
      MethodInfo method = VMF_Harmony.infoRef.Invoke(attributePatch).method;
      PatchLevelAttribute customAttribute = method.GetCustomAttribute<PatchLevelAttribute>();
      int num;
      if (customAttribute == null)
      {
        Type declaringType = method.DeclaringType;
        num = (int) ((object) declaringType != null ? declaringType.GetCustomAttribute<PatchLevelAttribute>()?.level : new Level?()).GetValueOrDefault();
      }
      else
        num = (int) customAttribute.level;
      return VMF_Harmony.OutOfRange((Level) num);
    }
  }

  internal static bool CheckClassPatchLevel(Type type)
  {
    PatchLevelAttribute customAttribute = type.GetCustomAttribute<PatchLevelAttribute>();
    return customAttribute == null || !VMF_Harmony.OutOfRange(customAttribute.level);
  }

  internal static void DynamicPatchAllNow(Level patchLevel)
  {
    if (VMF_Harmony.CurrentPatchLevel < patchLevel)
    {
      VMF_Harmony.PrevPatchLevel = VMF_Harmony.CurrentPatchLevel;
      VMF_Harmony.CurrentPatchLevel = patchLevel;
      int num1 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
      VMF_Harmony.PatchAllUncategorized();
      foreach (string category in VMF_Harmony.Categories)
        VMF_Harmony.PatchCategory(category);
      int num2 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
      VMF_Log.Message($"Dynamic patches applied: {(num2 - num1).ToString()} Total: {num2.ToString()}");
    }
    else
    {
      if (!VehicleMapFramework.VehicleMapFramework.settings.dynamicUnpatchEnabled || VMF_Harmony.CurrentPatchLevel == patchLevel)
        return;
      VMF_Harmony.PrevPatchLevel = VMF_Harmony.CurrentPatchLevel;
      VMF_Harmony.CurrentPatchLevel = patchLevel;
      int num3 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
      VMF_Harmony.UnpatchAllUncategorized();
      foreach (string category in VMF_Harmony.Categories)
        VMF_Harmony.UnpatchCategory(category);
      int num4 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
      VMF_Log.Message($"Dynamic patches unapplied: {(num3 - num4).ToString()} Total: {num4.ToString()}");
    }
  }

  internal static void DynamicPatchAll(Level patchLevel)
  {
    if (VMF_Harmony.CurrentPatchLevel < patchLevel)
    {
      LongEventHandler.QueueLongEvent((Action) (() =>
      {
        VMF_Harmony.PrevPatchLevel = VMF_Harmony.CurrentPatchLevel;
        VMF_Harmony.CurrentPatchLevel = patchLevel;
        int num1 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
        VMF_Harmony.PatchAllUncategorized();
        foreach (string category in VMF_Harmony.Categories)
          VMF_Harmony.PatchCategory(category);
        int num2 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
        VMF_Log.Message($"Dynamic patches applied: {(num2 - num1).ToString()} Total: {num2.ToString()}");
      }), "VMF_ApplyingDynamicPatches", false, (Action<Exception>) null, false, false, (Action) null);
    }
    else
    {
      if (!VehicleMapFramework.VehicleMapFramework.settings.dynamicUnpatchEnabled || VMF_Harmony.CurrentPatchLevel == patchLevel)
        return;
      LongEventHandler.QueueLongEvent((Action) (() =>
      {
        VMF_Harmony.PrevPatchLevel = VMF_Harmony.CurrentPatchLevel;
        VMF_Harmony.CurrentPatchLevel = patchLevel;
        int num3 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
        VMF_Harmony.UnpatchAllUncategorized();
        foreach (string category in VMF_Harmony.Categories)
          VMF_Harmony.UnpatchCategory(category);
        int num4 = VMF_Harmony.Instance.GetPatchedMethods().Count<MethodBase>();
        VMF_Log.Message($"Dynamic patches unapplied: {(num3 - num4).ToString()} Total: {num4.ToString()}");
      }), "VMF_UnpatchingDynamicPatches", false, (Action<Exception>) null, false, false, (Action) null);
    }
  }

  internal static List<Type> PatchClassesInCategory(string category)
  {
    List<Type> typeList;
    if (VMF_Harmony.PatchesInCategories.TryGetValue(category, out typeList))
      return typeList;
    VMF_Log.Error($"Patches for category ({category}) not included in this mod");
    return new List<Type>();
  }

  internal static void PatchCategory(string category)
  {
    if (!VMF_Harmony.Categories.Contains(category))
      VMF_Harmony.Categories.Add(category);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    IEnumerable<Type> source = VMF_Harmony.PatchClassesInCategory(category).Where<Type>(VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel ?? (VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel = new Func<Type, bool>(VMF_Harmony.CheckClassPatchLevel)));
    if (category == "VMF_Patches_VehicleFramework")
      source = source.Where<Type>((Func<Type, bool>) (t =>
      {
        VFVersionalPatchAttribute customAttribute = t.GetCustomAttribute<VFVersionalPatchAttribute>();
        return customAttribute == null || customAttribute.Available;
      }));
    CollectionExtensions.Do<PatchClassProcessor>(source.Select<Type, PatchClassProcessor>((Func<Type, PatchClassProcessor>) (t => VMF_Harmony.Instance.CreateClassProcessor(t))), (Action<PatchClassProcessor>) (patchClass =>
    {
      try
      {
        VMF_Harmony.AdjustPatchLevel(patchClass);
        patchClass.Patch();
      }
      catch (Exception ex)
      {
        VMF_Log.Error($"Error while apply patching.\n{ex}");
      }
    }));
  }

  internal static void UnpatchCategory(string category)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    CollectionExtensions.Do<PatchClassProcessor>(VMF_Harmony.PatchClassesInCategory(category).Where<Type>(VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel ?? (VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel = new Func<Type, bool>(VMF_Harmony.CheckClassPatchLevel))).Select<Type, PatchClassProcessor>((Func<Type, PatchClassProcessor>) (t => VMF_Harmony.Instance.CreateClassProcessor(t))), (Action<PatchClassProcessor>) (patchClass =>
    {
      try
      {
        VMF_Harmony.AdjustPatchLevel(patchClass);
        patchClass.Unpatch();
      }
      catch (Exception ex)
      {
        VMF_Log.Error($"Error while apply unpatching.\n{ex}");
      }
    }));
  }

  internal static void PatchAllUncategorized()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    CollectionExtensions.Do<PatchClassProcessor>(VMF_Harmony.PatchClassesInCategory("").Where<Type>(VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel ?? (VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel = new Func<Type, bool>(VMF_Harmony.CheckClassPatchLevel))).Select<Type, PatchClassProcessor>((Func<Type, PatchClassProcessor>) (t => VMF_Harmony.Instance.CreateClassProcessor(t))), (Action<PatchClassProcessor>) (patchClass =>
    {
      try
      {
        VMF_Harmony.AdjustPatchLevel(patchClass);
        patchClass.Patch();
      }
      catch (Exception ex)
      {
        VMF_Log.Error($"Error while apply patching\n{ex}");
      }
    }));
  }

  internal static void UnpatchAllUncategorized()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    CollectionExtensions.Do<PatchClassProcessor>(VMF_Harmony.PatchClassesInCategory("").Where<Type>(VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel ?? (VMF_Harmony.\u003C\u003EO.\u003C1\u003E__CheckClassPatchLevel = new Func<Type, bool>(VMF_Harmony.CheckClassPatchLevel))).Select<Type, PatchClassProcessor>((Func<Type, PatchClassProcessor>) (t => VMF_Harmony.Instance.CreateClassProcessor(t))), (Action<PatchClassProcessor>) (patchClass =>
    {
      try
      {
        VMF_Harmony.AdjustPatchLevel(patchClass);
        patchClass.Unpatch();
      }
      catch (Exception ex)
      {
        VMF_Log.Error($"Error while apply unpatching\n{ex}");
      }
    }));
  }

  static VMF_Harmony()
  {
    VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;
    // ISSUE: reference to a compiler-generated field
    VMF_Harmony.\u003CCurrentPatchLevel\u003Ek__BackingField = (settings != null ? (settings.dynamicPatchEnabled ? 1 : 0) : 0) != 0 ? VehicleMapFramework.VehicleMapFramework.settings.dynamicPatchLevel : Level.All;
    // ISSUE: reference to a compiler-generated field
    VMF_Harmony.\u003CPrevPatchLevel\u003Ek__BackingField = Level.Mandatory;
    VMF_Harmony.patchMethodsRef = AccessTools.FieldRefAccess<PatchClassProcessor, object>("patchMethods");
    VMF_Harmony.infoRef = AccessTools.FieldRefAccess<HarmonyMethod>("HarmonyLib.AttributePatch:info");
    VMF_Harmony.m_RemoveAll = AccessTools.Method(typeof (List<>).MakeGenericType(AccessTools.TypeByName("HarmonyLib.AttributePatch")), "RemoveAll", (Type[]) null, (Type[]) null);
  }
}
