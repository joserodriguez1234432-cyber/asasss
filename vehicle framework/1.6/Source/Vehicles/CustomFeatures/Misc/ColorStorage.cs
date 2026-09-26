// Decompiled with JetBrains decompiler
// Type: Vehicles.ColorStorage
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class ColorStorage : IExposable
{
  public const int PaletteCount = 20;
  public const int PaletteCountPerRow = 5;
  public List<(Color, Color, Color)> colorPalette = new List<(Color, Color, Color)>();
  private List<Color> paletteColorOnes = new List<Color>();
  private List<Color> paletteColorTwos = new List<Color>();
  private List<Color> paletteColorThrees = new List<Color>();

  public static int PaletteRowCount => 4;

  public void ResetPalettes()
  {
    this.colorPalette = new List<(Color, Color, Color)>()
    {
      (new Color(0.306f, 0.329f, 0.306f), new Color(0.255f, 0.263f, 0.239f), new Color(0.388f, 0.271f, 0.231f)),
      (new Color(0.569f, 0.318f, 0.259f), new Color(0.557f, 0.282f, 0.263f), new Color(0.333f, 0.2f, 0.157f)),
      (new Color(0.565f, 0.545f, 0.533f), new Color(0.388f, 0.365f, 0.353f), new Color(0.278f, 0.259f, 0.251f)),
      (new Color(0.533f, 0.576f, 0.753f), new Color(0.396f, 0.451f, 0.569f), new Color(0.38f, 0.38f, 0.424f)),
      (new Color(0.58f, 0.58f, 0.58f), new Color(0.392f, 0.392f, 0.392f), new Color(0.196f, 0.196f, 0.196f)),
      (new Color(0.502f, 0.216f, 0.216f), new Color(0.263f, 0.349f, 0.699f), new Color(0.722f, 0.722f, 0.722f)),
      (new Color(0.886f, 0.843f, 0.647f), new Color(0.561f, 0.478f, 0.353f), new Color(0.463f, 0.38f, 0.271f)),
      (new Color(0.412f, 0.643f, 0.565f), new Color(0.345f, 0.49f, 0.467f), new Color(0.337f, 0.353f, 0.376f)),
      (new Color(0.788f, 0.804f, 0.753f), new Color(0.686f, 0.698f, 0.631f), new Color(0.49f, 0.518f, 0.427f)),
      (new Color(0.361f, 0.275f, 0.118f), new Color(0.29f, 0.204f, 0.063f), new Color(0.667f, 0.553f, 0.388f)),
      (new Color(0.573f, 0.537f, 0.514f), new Color(0.329f, 0.294f, 0.267f), new Color(0.263f, 0.239f, 0.227f)),
      (new Color(0.451f, 0.573f, 0.58f), new Color(0.345f, 0.478f, 0.475f), new Color(0.227f, 0.29f, 0.278f)),
      (new Color(0.741f, 0.714f, 0.678f), new Color(0.792f, 0.769f, 0.733f), new Color(0.365f, 0.345f, 0.302f)),
      (new Color(0.549f, 0.667f, 0.482f), new Color(0.349f, 0.494f, 0.275f), new Color(0.2f, 0.353f, 0.125f)),
      (new Color(0.871f, 0.741f, 0.235f), new Color(0.969f, 0.843f, 0.42f), new Color(0.627f, 0.502f, 0.039f)),
      (new Color(0.208f, 0.192f, 0.188f), new Color(0.333f, 0.314f, 0.302f), new Color(0.141f, 0.141f, 0.141f)),
      (new Color(0.42f, 0.443f, 0.192f), new Color(0.31f, 0.341f, 0.149f), new Color(0.365f, 0.243f, 0.031f)),
      (new Color(0.937f, 0.937f, 0.937f), new Color(0.722f, 0.725f, 0.722f), new Color(0.776f, 0.847f, 0.902f)),
      (new Color(0.576f, 0.537f, 0.49f), new Color(0.369f, 0.302f, 0.243f), new Color(0.149f, 57f / 500f, 0.094f)),
      (new Color(0.58f, 0.592f, 0.459f), new Color(0.451f, 0.475f, 0.353f), new Color(0.839f, 0.859f, 0.259f))
    };
  }

  public void AddPalette(Color colorOne, Color colorTwo, Color colorThree, int index)
  {
    if (index >= 20 || index < 0)
    {
      Log.Error("Attempting to set size of ColorPalette that is larger than predetermined.");
    }
    else
    {
      this.colorPalette[index] = (colorOne, colorTwo, colorThree);
      VehicleMod.settings.Write();
    }
  }

  public (Color colorOne, Color colorTwo, Color colorThree) GetRandomPalette()
  {
    return GenCollection.RandomElement<(Color, Color, Color)>((IEnumerable<(Color, Color, Color)>) this.colorPalette);
  }

  public void ExposeData()
  {
    if (Scribe.mode == 1)
    {
      if (this.colorPalette == null || this.colorPalette.Count != 20)
        this.ResetPalettes();
      this.paletteColorOnes = this.colorPalette.Select<(Color, Color, Color), Color>((Func<(Color, Color, Color), Color>) (c => c.Item1)).ToList<Color>();
      this.paletteColorTwos = this.colorPalette.Select<(Color, Color, Color), Color>((Func<(Color, Color, Color), Color>) (c => c.Item2)).ToList<Color>();
      this.paletteColorThrees = this.colorPalette.Select<(Color, Color, Color), Color>((Func<(Color, Color, Color), Color>) (c => c.Item3)).ToList<Color>();
      if (this.paletteColorOnes.Count != this.paletteColorTwos.Count || this.paletteColorOnes.Count != this.paletteColorThrees.Count)
        Log.Error("Unequal count of color palettes in unzipped lists. All 3 lists should contain the same amount.");
    }
    Scribe_Collections.Look<Color>(ref this.paletteColorOnes, "paletteColorOnes", (LookMode) 1, Array.Empty<object>());
    Scribe_Collections.Look<Color>(ref this.paletteColorTwos, "paletteColorTwos", (LookMode) 1, Array.Empty<object>());
    Scribe_Collections.Look<Color>(ref this.paletteColorThrees, "paletteColorThrees", (LookMode) 1, Array.Empty<object>());
    if (Scribe.mode != 2)
      return;
    this.colorPalette = new List<(Color, Color, Color)>();
    for (int index = 0; index < this.paletteColorOnes.Count; ++index)
      this.colorPalette.Add((this.paletteColorOnes[index], this.paletteColorTwos[index], this.paletteColorThrees[index]));
  }
}
