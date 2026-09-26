// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_LongEventHandler
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_LongEventHandler
{
  private static readonly FieldInfo currentEventField = AccessTools.Field(typeof (LongEventHandler), "currentEvent");
  private static readonly FieldInfo longEventTextField = AccessTools.Field(AccessTools.TypeByName("Verse.LongEventHandler+QueuedLongEvent"), "eventText");
  private static readonly object currentEventTextLock = AccessTools.Field(typeof (LongEventHandler), "CurrentEventTextLock").GetValue((object) null);

  public static string GetLongEventText()
  {
    object obj = Ext_LongEventHandler.currentEventField.GetValue((object) null);
    if (obj == null)
      return (string) null;
    lock (Ext_LongEventHandler.currentEventTextLock)
      return (string) Ext_LongEventHandler.longEventTextField.GetValue(obj);
  }
}
