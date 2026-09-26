// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.StaticVoidFuncPtr`4
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Reflection;

#nullable disable
namespace SmashTools.Performance;

public sealed class StaticVoidFuncPtr<T1, T2, T3, T4>
{
  private readonly __FnPtr<void (T1, T2, T3, T4)> function;

  public unsafe StaticVoidFuncPtr(MethodInfo method)
  {
    if ((object) method == null)
      throw new ArgumentNullException(nameof (method));
    if (!method.IsStatic)
      throw new ArgumentException("Non-static method cannot be used as static delegate.", nameof (method));
    if (method.ReturnType != typeof (void))
      throw new InvalidOperationException($"Unable to get function pointer from {method.Name}, does not have return type void.");
    // ISSUE: cast to a function pointer type
    this.function = method.GetParameters().Length == 3 ? (__FnPtr<void (T1, T2, T3, T4)>) (IntPtr) (void*) method.MethodHandle.GetFunctionPointer() : throw new InvalidOperationException($"Unable to get function pointer from {method.Name}, mismatched parameters.");
  }

  public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
  {
    // ISSUE: function pointer call
    __calli(this.function)(arg1, arg2, arg3, arg4);
  }
}
