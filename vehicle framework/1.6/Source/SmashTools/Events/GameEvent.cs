// Decompiled with JetBrains decompiler
// Type: SmashTools.GameEvent
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;

#nullable disable
namespace SmashTools;

[PublicAPI]
public static class GameEvent
{
  public static event Action OnNewGame;

  public static event Action OnLoadGame;

  public static event Action OnMainMenu;

  public static event Action OnGameDisposing;

  public static event Action OnGameDisposed;

  public static event Action OnWorldUnloading;

  public static event Action OnWorldRemoved;

  public static event Action<bool> OnGenerateImpliedDefs;

  internal static void RaiseOnNewGame()
  {
    Action onNewGame = GameEvent.OnNewGame;
    if (onNewGame == null)
      return;
    onNewGame();
  }

  internal static void RaiseOnLoadGame()
  {
    Action onLoadGame = GameEvent.OnLoadGame;
    if (onLoadGame == null)
      return;
    onLoadGame();
  }

  internal static void RaiseOnMainMenu()
  {
    Action onMainMenu = GameEvent.OnMainMenu;
    if (onMainMenu == null)
      return;
    onMainMenu();
  }

  internal static void RaiseOnGameDisposing()
  {
    Action onGameDisposing = GameEvent.OnGameDisposing;
    if (onGameDisposing == null)
      return;
    onGameDisposing();
  }

  internal static void RaiseOnGameDisposed()
  {
    Action onGameDisposed = GameEvent.OnGameDisposed;
    if (onGameDisposed == null)
      return;
    onGameDisposed();
  }

  internal static void RaiseOnWorldUnloading()
  {
    Action onWorldUnloading = GameEvent.OnWorldUnloading;
    if (onWorldUnloading == null)
      return;
    onWorldUnloading();
  }

  internal static void RaiseOnWorldRemoved()
  {
    Action onWorldRemoved = GameEvent.OnWorldRemoved;
    if (onWorldRemoved == null)
      return;
    onWorldRemoved();
  }

  internal static void RaiseOnGenerateImpliedDefs(bool hotReload)
  {
    Action<bool> generateImpliedDefs = GameEvent.OnGenerateImpliedDefs;
    if (generateImpliedDefs == null)
      return;
    generateImpliedDefs(hotReload);
  }
}
