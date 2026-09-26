// Decompiled with JetBrains decompiler
// Type: Vehicles.AirdropProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public struct AirdropProperties
{
  public required float angle;
  public bool packIntoContainer;

  public AirdropProperties()
  {
    this.angle = 0.0f;
    this.packIntoContainer = false;
  }

  public static AirdropProperties Default
  {
    get
    {
      return new AirdropProperties()
      {
        angle = (float) Rand.Range(-30, 30)
      };
    }
  }
}
