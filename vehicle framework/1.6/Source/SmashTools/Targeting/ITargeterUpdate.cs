// Decompiled with JetBrains decompiler
// Type: SmashTools.Targeting.ITargeterUpdate`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable disable
namespace SmashTools.Targeting;

public interface ITargeterUpdate<T>
{
  void TargeterOnGUI();

  void TargeterUpdate([RequiresLocation, In] ref TargetData<T> targetData);
}
