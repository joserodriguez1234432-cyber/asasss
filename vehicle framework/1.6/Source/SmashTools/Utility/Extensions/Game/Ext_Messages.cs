// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Messages
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_Messages
{
  private const float DefaultMessageLifespan = 13f;
  private static readonly FieldInfo messageStartingTime = AccessTools.Field(typeof (Verse.Message), "startingTime");

  public static void Message(
    string text,
    MessageTypeDef messageTypeDef,
    float time = 13f,
    bool historical = true)
  {
    Verse.Message message = new Verse.Message(GenText.CapitalizeFirst(text), messageTypeDef);
    Ext_Messages.messageStartingTime.SetValue((object) message, (object) (float) ((double) RealTime.LastRealTime - (13.0 - (double) time)));
    Messages.Message(message, historical);
  }
}
