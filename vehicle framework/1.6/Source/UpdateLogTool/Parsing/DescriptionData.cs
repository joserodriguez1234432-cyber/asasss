// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.DescriptionData
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

#nullable disable
namespace UpdateLogTool;

public class DescriptionData
{
  public string text;
  public TaggedSegment tag;
  public string[] underlineText;
  public string[] hyperlinks;

  public DescriptionData()
  {
  }

  public DescriptionData(string text) => this.text = text;
}
