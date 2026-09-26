// Decompiled with JetBrains decompiler
// Type: SmashTools.StaticConstructorOnGameInitAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace SmashTools;

[AttributeUsage(AttributeTargets.Class)]
[Obsolete("Do not use.", true)]
public class StaticConstructorOnGameInitAttribute : Attribute
{
  public static void RunGameInitStaticConstructors()
  {
    foreach (Type type in GenTypes.AllTypesWithAttribute<StaticConstructorOnGameInitAttribute>())
    {
      try
      {
        RuntimeHelpers.RunClassConstructor(type.TypeHandle);
      }
      catch (Exception ex)
      {
        SmashLog.Error($"Exception thrown running constructor of type <type>{type}</type>. Ex=\"{ex}\"");
      }
    }
  }
}
