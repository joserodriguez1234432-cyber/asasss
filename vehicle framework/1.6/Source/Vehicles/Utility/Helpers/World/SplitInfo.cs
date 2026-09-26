// Decompiled with JetBrains decompiler
// Type: Vehicles.World.SplitInfo
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld.Planet;
using System;
using System.Reflection;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class SplitInfo : ICaravanInfo
{
  private static readonly MethodInfo CountToTransferChangedMethod = AccessTools.Method(typeof (Dialog_SplitCaravan), "CountToTransferChanged", (System.Type[]) null, (System.Type[]) null);
  private readonly Dialog_SplitCaravan splitCaravan;
  private readonly Caravan caravan;
  private readonly Action countToTransferChanged;

  public SplitInfo(Dialog_SplitCaravan splitCaravan, Caravan caravan)
  {
    this.splitCaravan = splitCaravan;
    this.caravan = caravan;
    this.countToTransferChanged = (Action) Delegate.CreateDelegate(typeof (Action), (object) splitCaravan, SplitInfo.CountToTransferChangedMethod);
  }

  bool ICaravanInfo.AllowSelectionOfAllVehicles => true;

  public void NotifyTransferablesChanged() => this.countToTransferChanged();
}
