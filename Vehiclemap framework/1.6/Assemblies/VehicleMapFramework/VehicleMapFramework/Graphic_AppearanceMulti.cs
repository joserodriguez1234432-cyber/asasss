// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Graphic_AppearanceMulti
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Graphic_AppearanceMulti : Graphic_Appearances
{
  public virtual void Init(GraphicRequest req)
  {
    ((Graphic) this).data = req.graphicData;
    ((Graphic) this).path = req.path;
    ((Graphic) this).color = req.color;
    ((Graphic) this).drawSize = req.drawSize;
    List<StuffAppearanceDef> defsListForReading = DefDatabase<StuffAppearanceDef>.AllDefsListForReading;
    this.subGraphics = new Graphic[defsListForReading.Count];
    Graphic graphic = (Graphic) null;
    for (int index = 0; index < this.subGraphics.Length; ++index)
    {
      StuffAppearanceDef stuffAppearanceDef = defsListForReading[index];
      string str1 = req.path;
      string str2 = ((IEnumerable<string>) str1.Split('/', StringSplitOptions.None)).Last<string>();
      if (!GenText.NullOrEmpty(stuffAppearanceDef.pathPrefix))
      {
        string str3 = str1;
        int length = str2.Length;
        str1 = str3.Substring(0, str3.Length - length) + stuffAppearanceDef.pathPrefix + str2;
      }
      if (Object.op_Implicit((Object) ContentFinder<Texture2D>.Get(str1 + "_north", false)))
      {
        this.subGraphics[index] = GraphicDatabase.Get<Graphic_Multi>(str1, req.shader, ((Graphic) this).drawSize, ((Graphic) this).color);
        if (graphic == null)
          graphic = this.subGraphics[index];
      }
    }
    for (int index1 = 0; index1 < this.subGraphics.Length; ++index1)
    {
      Graphic[] subGraphics = this.subGraphics;
      int index2 = index1;
      if (subGraphics[index2] == null)
        subGraphics[index2] = graphic;
    }
  }

  public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
  {
    if (Color.op_Inequality(newColorTwo, Color.white))
      Log.ErrorOnce("Cannot use Graphic_AppearanceMulti.GetColoredVersion with a non-white colorTwo.", 9910251);
    return GraphicDatabase.Get<Graphic_AppearanceMulti>(((Graphic) this).path, newShader, ((Graphic) this).drawSize, newColor, Color.white, ((Graphic) this).data, (string) null);
  }
}
