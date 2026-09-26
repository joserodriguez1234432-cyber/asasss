// Decompiled with JetBrains decompiler
// Type: Vehicles.CursorSettings
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;

#nullable enable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class CursorSettings
{
  private static readonly 
  #nullable disable
  Dictionary<CursorSettings.Type, CursorSettings.Entry> CursorLookup = new Dictionary<CursorSettings.Type, CursorSettings.Entry>();

  static CursorSettings()
  {
    CursorSettings.CursorLookup[CursorSettings.Type.OpenHand] = new CursorSettings.Entry()
    {
      type = CursorSettings.Type.OpenHand,
      texture = ContentFinder<Texture2D>.Get("UI/Cursors/MouseHandOpen", true),
      hotspot = new Vector2(3f, 3f)
    };
    CursorSettings.CursorLookup[CursorSettings.Type.CloseHand] = new CursorSettings.Entry()
    {
      type = CursorSettings.Type.CloseHand,
      texture = ContentFinder<Texture2D>.Get("UI/Cursors/MouseHandClosed", true),
      hotspot = new Vector2(3f, 3f)
    };
  }

  public static void SetCursor(CursorSettings.Type type)
  {
    CursorSettings.Entry entry;
    if (!CursorSettings.CursorLookup.TryGetValue(type, out entry))
      Trace.Fail($"Unable to load cursor for type {type}");
    else
      Cursor.SetCursor(entry.texture, entry.hotspot, (CursorMode) 0);
  }

  public static void Reset()
  {
    if (Prefs.CustomCursorEnabled)
      CustomCursor.Activate();
    else
      CustomCursor.Deactivate();
  }

  public enum Type
  {
    OpenHand,
    CloseHand,
  }

  private record Entry()
  {
    public required CursorSettings.Type type;
    public required Texture2D texture;
    public Vector2 hotspot;

    [CompilerGenerated]
    protected virtual bool PrintMembers(
    #nullable enable
    StringBuilder builder)
    {
      RuntimeHelpers.EnsureSufficientExecutionStack();
      builder.Append("type = ");
      builder.Append(this.type.ToString());
      builder.Append(", texture = ");
      builder.Append((object) this.texture);
      builder.Append(", hotspot = ");
      builder.Append(this.hotspot.ToString());
      return true;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
      return ((EqualityComparer<System.Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<CursorSettings.Type>.Default.GetHashCode(this.type)) * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(this.texture)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.hotspot);
    }

    [CompilerGenerated]
    public virtual bool Equals(CursorSettings.Entry? other)
    {
      if ((object) this == (object) other)
        return true;
      return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<CursorSettings.Type>.Default.Equals(this.type, other.type) && EqualityComparer<Texture2D>.Default.Equals(this.texture, other.texture) && EqualityComparer<Vector2>.Default.Equals(this.hotspot, other.hotspot);
    }

    [CompilerGenerated]
    [SetsRequiredMembers]
    protected Entry(CursorSettings.Entry original)
    {
      this.type = original.type;
      this.texture = original.texture;
      this.hotspot = original.hotspot;
    }
  }
}
