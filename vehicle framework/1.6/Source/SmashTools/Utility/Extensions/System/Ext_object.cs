// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_object
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

#nullable disable
namespace SmashTools;

public static class Ext_object
{
  public static object GetDefaultValue<T>(this T obj)
  {
    if (obj is IDefaultValue defaultValue)
      return defaultValue.DefaultValue;
    ref T local1 = ref obj;
    if ((object) default (T) == null)
    {
      T obj1 = local1;
      ref T local2 = ref obj1;
      if ((object) obj1 == null)
        return (object) null;
      local1 = ref local2;
    }
    return local1.GetType().GetDefaultValue();
  }
}
