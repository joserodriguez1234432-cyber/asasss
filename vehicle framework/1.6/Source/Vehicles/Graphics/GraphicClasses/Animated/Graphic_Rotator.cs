// Decompiled with JetBrains decompiler
// Type: Vehicles.Graphic_Rotator
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

#nullable disable
namespace Vehicles;

public abstract class Graphic_Rotator : Graphic_Rgb
{
  public abstract int RegistryKey { get; }

  public virtual float ModifyIncomingRotation(float rotation) => rotation;
}
