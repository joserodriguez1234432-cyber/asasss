// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Graphics
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools.Patching;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_Graphics : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Mod;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GraphicData), "Init", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Graphics), "GraphicInit", (System.Type[]) null));
  }

  private static void GraphicInit(GraphicData __instance)
  {
    if (!(__instance is GraphicDataLayered graphicDataLayered) || !graphicDataLayered.shaderType.Shader.SupportsRGBMaskTex())
      return;
    graphicDataLayered.Init((IMaterialCacheTarget) null);
    Log.Error($"Calling Init for {__instance.GetType()} with path: {__instance.texPath} " + "from GraphicData which means it's being cached in vanilla when it should be using RGBMaterialPool.");
  }
}
