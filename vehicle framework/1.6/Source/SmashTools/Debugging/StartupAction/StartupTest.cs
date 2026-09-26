// Decompiled with JetBrains decompiler
// Type: SmashTools.StartupTest
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public static class StartupTest
{
  private static readonly Dictionary<GameState, List<Action>> postLoadActions = new Dictionary<GameState, List<Action>>();
  private static readonly Dictionary<string, StartupTest.StartupAction> actions = new Dictionary<string, StartupTest.StartupAction>();
  private static readonly List<Toggle> actionRadioButtons = new List<Toggle>();

  private static bool NoStartupAction { get; set; }

  private static bool Enabled { get; }

  [Conditional("DEBUG")]
  public static void OpenMenu()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    Find.WindowStack.Add((Window) new Dialog_RadioButtonMenu("Startup Actions", StartupTest.actionRadioButtons, StartupTest.\u003C\u003EO.\u003C0\u003E__Serialize ?? (StartupTest.\u003C\u003EO.\u003C0\u003E__Serialize = new Action(SmashMod.Serialize))));
  }

  private static void InitializeStartupActions()
  {
    SmashMod.LoadFromSettings();
    StartupTest.postLoadActions.Clear();
    foreach (GameState key in Enum.GetValues(typeof (GameState)))
      StartupTest.postLoadActions.Add(key, new List<Action>());
    StartupTest.actions.Clear();
    StartupTest.actionRadioButtons.Clear();
    StartupTest.actionRadioButtons.Add(new Toggle("NoStartupAction", "None", string.Empty, (Func<bool>) (() => StartupTest.NoStartupAction || SmashSettings.startupAction.NullOrEmpty<char>()), (Action<bool>) (value =>
    {
      StartupTest.NoStartupAction = value;
      if (!StartupTest.NoStartupAction)
        return;
      SmashSettings.startupAction = string.Empty;
    })));
    StartupTest.NoStartupAction = true;
    List<MethodInfo> methodInfoList = new List<MethodInfo>();
    foreach (Type allType in GenTypes.AllTypes)
    {
      foreach (MethodInfo methodInfo in ((IEnumerable<MethodInfo>) allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).Where<MethodInfo>((Func<MethodInfo, bool>) (m => !((IEnumerable<ParameterInfo>) m.GetParameters()).Any<ParameterInfo>())))
      {
        MethodInfo method = methodInfo;
        StartupActionAttribute customAttribute = method.GetCustomAttribute<StartupActionAttribute>();
        if (customAttribute != null)
        {
          string name = customAttribute.Name;
          if (string.IsNullOrEmpty(name))
            name = method.Name;
          string str1 = customAttribute.Category;
          if (string.IsNullOrEmpty(str1))
            str1 = "General";
          string str2 = $"{str1}.{name}".Replace(" ", "");
          StartupTest.StartupAction startupAction = new StartupTest.StartupAction()
          {
            FullName = str2,
            DisplayName = name,
            Category = str1,
            GameState = customAttribute.GameState,
            Action = (Action) (() => method.Invoke((object) null, Array.Empty<object>()))
          };
          if (str2 == SmashSettings.startupAction)
            StartupTest.NoStartupAction = false;
          StartupTest.actions.Add(startupAction.FullName, startupAction);
          StartupTest.actionRadioButtons.Add(new Toggle(startupAction.FullName, startupAction.DisplayName, startupAction.Category, (Func<bool>) (() => SmashSettings.startupAction == startupAction.FullName), (Action<bool>) (value =>
          {
            if (!value)
              return;
            SmashSettings.startupAction = startupAction.FullName;
          })));
        }
      }
    }
    GenCollection.SortBy<Toggle, string>(StartupTest.actionRadioButtons, (Func<Toggle, string>) (toggle => toggle.DisplayName));
  }

  private static void PostLoadSetup()
  {
    StartupTest.StartupAction startupAction;
    if (SmashSettings.startupAction.NullOrEmpty<char>() || !StartupTest.actions.TryGetValue(SmashSettings.startupAction, out startupAction))
      return;
    StartupTest.postLoadActions[startupAction.GameState].Add(startupAction.Action);
  }

  internal static void ExecutePostLoadTesting()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      StartupTest.ExecuteTesting(GameState.LoadedSave);
      StartupTest.ExecuteTesting(GameState.Playing);
    }));
  }

  internal static void ExecuteNewGameTesting()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      StartupTest.ExecuteTesting(GameState.NewGame);
      StartupTest.ExecuteTesting(GameState.Playing);
    }));
  }

  internal static void ExecuteOnStartupTesting()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => StartupTest.ExecuteTesting(GameState.OnStartup)));
  }

  private static void ExecuteTesting(GameState gameState)
  {
    if (!StartupTest.Enabled)
      return;
    foreach (Action action in StartupTest.postLoadActions[gameState])
      action();
  }

  private class StartupAction
  {
    public string FullName { get; set; }

    public string DisplayName { get; set; }

    public string Category { get; set; }

    public Action Action { get; set; }

    public GameState GameState { get; set; }
  }
}
