// Decompiled with JetBrains decompiler
// Type: Vehicles.ActionOnSettingsInputAttribute
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ActionOnSettingsInputAttribute : Attribute
{
  public MethodInfo Method { get; private set; }

  public ActionOnSettingsInputAttribute(System.Type type, string methodName)
  {
    this.Method = AccessTools.Method(type, methodName, (System.Type[]) null, (System.Type[]) null);
  }

  public void Invoke() => this.Method.Invoke((object) null, new object[0]);

  public static void InvokeIfApplicable(FieldInfo field)
  {
    GenAttribute.TryGetAttribute<ActionOnSettingsInputAttribute>((MemberInfo) field)?.Invoke();
  }
}
