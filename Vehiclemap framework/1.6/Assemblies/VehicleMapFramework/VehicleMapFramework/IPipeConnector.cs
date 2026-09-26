// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.IPipeConnector
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public interface IPipeConnector
{
  CompPipeConnector.PipeMod Mod { get; }

  Texture GizmoIcon { get; }

  IEnumerable<FloatMenuOption> FloatMenuOptions { get; }

  bool ConnectCondition(CompPipeConnector another);

  void ConnectedTickAction();

  void DisconnectedAction();
}
