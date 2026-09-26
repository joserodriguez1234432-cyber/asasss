// Decompiled with JetBrains decompiler
// Type: Vehicles.AntiAircraftDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class AntiAircraftDef : WorldObjectDef
{
  public float damage;
  public float accuracy;
  public AntiAircraftDef.AirDefenseProperties properties = new AntiAircraftDef.AirDefenseProperties();
  public int ticksBetweenShots = 20;
  public System.Type antiAircraftWorker;
  public TechLevel minTechLevel = (TechLevel) 4;
  public GraphicData explosionGraphic;
  public int framesForExplosion = -1;
  public float drawSizeMultiplier = 1f;

  public virtual IEnumerable<string> ConfigErrors()
  {
    foreach (string configError in base.ConfigErrors())
      yield return configError;
    if ((double) this.accuracy < 0.0 || (double) this.accuracy > 1.0)
      yield return "<field<accuracy</field> must be a float value between 0 and 1.".ConvertRichText();
    if (this.explosionGraphic != null && this.explosionGraphic.graphicClass == typeof (Graphic_Animate) && this.framesForExplosion < 0)
      yield return "using <type>Graphic_Animate</type> class requires <field>framesPerExplosion</field> to be populated".ConvertRichText();
    if ((object) this.antiAircraftWorker == null)
      yield return "<field>antiAircraftWorker</field> cannot be null.".ConvertRichText();
  }

  public class AirDefenseProperties
  {
    public float distance = 4f;
    public IntRange altitude = new IntRange(1000, 10000);
    public int arc = 30;
    public IntRange buildings = new IntRange(1, 4);
  }
}
