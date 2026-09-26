// Decompiled with JetBrains decompiler
// Type: Vehicles.IndicatorDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class IndicatorDef : Def
{
  public string iconPath;

  public Texture2D Icon { get; private set; }

  public virtual void PostLoad()
  {
    if (string.IsNullOrEmpty(this.iconPath))
      return;
    LongEventHandler.ExecuteWhenFinished((Action) (() => this.Icon = ContentFinder<Texture2D>.Get(this.iconPath, true)));
  }
}
