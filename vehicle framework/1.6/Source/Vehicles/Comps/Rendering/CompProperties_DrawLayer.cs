// Decompiled with JetBrains decompiler
// Type: Vehicles.CompProperties_DrawLayer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class CompProperties_DrawLayer : CompProperties
{
  public List<GraphicData> graphicDatas;

  public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
  {
    foreach (string configError in base.ConfigErrors(parentDef))
      yield return configError;
    if (GenList.NullOrEmpty<GraphicData>((IList<GraphicData>) this.graphicDatas))
      yield return "<field>graphicDatas</field> must be populated.".ConvertRichText();
  }
}
