// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.Dialog_DefDropdown
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class Dialog_DefDropdown(
  Rect rect,
  Type defType,
  Action<Def> onDefPicked,
  Func<Def, bool> isSelected) : Dialog_ItemDropdown<Def>(rect, Dialog_DefDropdown.DefsOfType(defType), onDefPicked, Dialog_DefDropdown.\u003C\u003EO.\u003C0\u003E__DefName ?? (Dialog_DefDropdown.\u003C\u003EO.\u003C0\u003E__DefName = new Func<Def, string>(Dialog_DefDropdown.DefName)), isSelected)
{
  private static string DefName(Def def) => def.defName;

  private static List<Def> DefsOfType(Type defType)
  {
    List<Def> defList = new List<Def>();
    defList.AddRange(GenDefDatabase.GetAllDefsInDatabaseForDef(defType));
    return defList;
  }
}
