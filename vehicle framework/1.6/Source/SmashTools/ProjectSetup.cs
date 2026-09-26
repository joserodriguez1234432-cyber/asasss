// Decompiled with JetBrains decompiler
// Type: SmashTools.ProjectSetup
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Patching;
using SmashTools.Performance;
using SmashTools.Xml;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Verse;

#nullable disable
namespace SmashTools;

public class ProjectSetup : Mod
{
  public const string ProjectLabel = "SmashTools";
  public const string LogLabel = "[SmashTools]";
  public const string HarmonyId = "SmashPhil.SmashTools";

  public ProjectSetup(ModContentPack content)
    : base(content)
  {
    HarmonyPatcher.Init(content);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: method pointer
    SceneManager.sceneLoaded += ProjectSetup.\u003C\u003EO.\u003C0\u003E__OnSceneChanged ?? (ProjectSetup.\u003C\u003EO.\u003C0\u003E__OnSceneChanged = new UnityAction<Scene, LoadSceneMode>((object) null, __methodptr(OnSceneChanged)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GameEvent.OnWorldUnloading += ProjectSetup.\u003C\u003EO.\u003C1\u003E__ReleaseAll ?? (ProjectSetup.\u003C\u003EO.\u003C1\u003E__ReleaseAll = new Action(ThreadManager.ReleaseAll));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GameEvent.OnWorldUnloading += ProjectSetup.\u003C\u003EO.\u003C2\u003E__ClearAll ?? (ProjectSetup.\u003C\u003EO.\u003C2\u003E__ClearAll = new Action(ComponentCache.ClearAll));
    XmlParseHelper.RegisterParseTypes();
    HarmonyPatcher.Run(PatchSequence.Mod);
    HarmonyPatcher.Run(PatchSequence.Async);
    ProjectSetup.StaticConstructorOnModInit();
  }

  private static void StaticConstructorOnModInit()
  {
    foreach (Type type in GenTypes.AllTypesWithAttribute<StaticConstructorOnModInitAttribute>())
    {
      try
      {
        RuntimeHelpers.RunClassConstructor(type.TypeHandle);
      }
      catch (Exception ex)
      {
        SmashLog.Error($"Exception thrown running constructor of type <type>{type}</type>. Ex=\"{ex}\"");
      }
    }
  }
}
