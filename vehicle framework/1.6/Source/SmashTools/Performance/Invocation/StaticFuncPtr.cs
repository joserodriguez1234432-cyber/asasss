// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.StaticFuncPtr`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Reflection;

#nullable disable
namespace SmashTools.Performance;

public sealed class StaticFuncPtr<R>
{
  private readonly __FnPtr<R ()> function;

  public unsafe StaticFuncPtr(MethodInfo method)
  {
    if ((object) method == null)
      throw new ArgumentNullException(nameof (method));
    if (!method.IsStatic)
      throw new ArgumentException("Non-static method cannot be used as static delegate.", nameof (method));
    if (method.ReturnType != typeof (R))
      throw new ArgumentException($"Unable to get function pointer from {method.Name}, does not have return type {typeof (R).Name}.");
    // ISSUE: cast to a function pointer type
    this.function = method.GetParameters().Length == 0 ? (__FnPtr<R ()>) (IntPtr) (void*) method.MethodHandle.GetFunctionPointer() : throw new ArgumentException($"Unable to get function pointer from {method.Name}, mismatched parameters.");
  }

  public R Invoke() => __calli(this.function)();
}
