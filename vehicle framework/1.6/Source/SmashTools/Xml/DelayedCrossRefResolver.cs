// Decompiled with JetBrains decompiler
// Type: SmashTools.Xml.DelayedCrossRefResolver
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools.Xml;

public static class DelayedCrossRefResolver
{
  private static readonly List<DelayedCrossRefResolver.Wanter> wanters = new List<DelayedCrossRefResolver.Wanter>();

  public static bool Resolved { get; private set; }

  public static void Register<T>(T obj, Type defType, FieldInfo fieldInfo, string defName)
  {
    if (DelayedCrossRefResolver.Resolved)
    {
      Log.Error("CrossRefs have already been resolved. You can load defs directly.");
    }
    else
    {
      DelayedCrossRefResolver.wanters.Add((DelayedCrossRefResolver.Wanter) new DelayedCrossRefResolver.WanterForObject((object) obj, defType, defName));
      throw new NotImplementedException();
    }
  }

  public static void RegisterIndex(Array array, Type defType, string defName, int index)
  {
    if (DelayedCrossRefResolver.Resolved)
      Log.Error("CrossRefs have already been resolved. You can load defs directly.");
    else
      DelayedCrossRefResolver.wanters.Add((DelayedCrossRefResolver.Wanter) new DelayedCrossRefResolver.WanterForIndex(array, defType, defName, index));
  }

  internal static void ResolveAll()
  {
    if (DelayedCrossRefResolver.Resolved)
    {
      Trace.Fail("Cannot resolve delayed cross references again. This is only meant to gather def references\r\nin non-def xml files and resolve them when DefDatabase has been loaded.");
    }
    else
    {
      foreach (DelayedCrossRefResolver.Wanter wanter in DelayedCrossRefResolver.wanters)
      {
        try
        {
          if (!wanter.TryResolve())
            Log.Error("Unable to resolve cross reference " + wanter.defName);
        }
        catch (Exception ex)
        {
          Log.Error($"Exception thrown resolving cross reference {wanter.defName}\nException={ex}");
        }
      }
      DelayedCrossRefResolver.Clear();
      DelayedCrossRefResolver.Resolved = true;
    }
  }

  private static void Clear() => DelayedCrossRefResolver.wanters.Clear();

  private abstract class Wanter
  {
    public readonly Type defType;
    public readonly string defName;

    public Wanter(Type defType, string defName)
    {
      this.defType = defType;
      this.defName = defName;
    }

    public abstract bool TryResolve();
  }

  private class WanterForObject(object wanter, Type defType, string defName) : 
    DelayedCrossRefResolver.Wanter(defType, defName)
  {
    public readonly object wanter;

    public override bool TryResolve() => throw new NotImplementedException();
  }

  private class WanterForIndex : DelayedCrossRefResolver.Wanter
  {
    public readonly Array wanter;
    public readonly int index;

    public WanterForIndex(Array wanter, Type defType, string defName, int index)
      : base(defType, defName)
    {
      this.wanter = wanter;
      this.index = index;
    }

    public override bool TryResolve()
    {
      Def def = GenDefDatabase.GetDef(this.defType, this.defName, true);
      this.wanter.SetValue((object) def, this.index);
      return def != null;
    }
  }
}
