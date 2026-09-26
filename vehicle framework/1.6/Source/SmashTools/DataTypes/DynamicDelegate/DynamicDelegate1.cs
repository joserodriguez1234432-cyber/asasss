// Decompiled with JetBrains decompiler
// Type: SmashTools.DynamicDelegate`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System.Reflection;

#nullable disable
namespace SmashTools;

[PublicAPI]
public class DynamicDelegate<T> : DynamicDelegateBase
{
  public DynamicDelegate()
  {
  }

  public DynamicDelegate(MethodInfo method)
    : base(method)
  {
  }

  public object Invoke(object obj, T param, params object[] injectedArgs)
  {
    this.InjectArguments(injectedArgs);
    this.args[0] = (object) param;
    return this.method.Invoke(obj, this.args);
  }
}
