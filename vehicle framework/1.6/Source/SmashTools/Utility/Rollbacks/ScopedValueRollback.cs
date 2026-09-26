// Decompiled with JetBrains decompiler
// Type: SmashTools.ScopedValueRollback`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;

#nullable disable
namespace SmashTools;

[PublicAPI]
public readonly struct ScopedValueRollback<T> : IDisposable where T : unmanaged
{
  private readonly unsafe T* ptr;
  private readonly T value;

  public unsafe ScopedValueRollback(ref T obj)
  {
    fixed (T* objPtr = &obj)
    {
      this.ptr = objPtr;
      this.value = obj;
    }
  }

  public unsafe ScopedValueRollback(ref T obj, T value)
  {
    fixed (T* objPtr = &obj)
    {
      this.ptr = objPtr;
      this.value = obj;
      obj = value;
    }
  }

  public unsafe void Dispose() => *this.ptr = this.value;
}
