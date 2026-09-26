// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.WebTexture
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class WebTexture : IDisposable
{
  public Texture2D texture;

  public WebTexture() => this.Status = DownloadStatus.InProgress;

  public DownloadStatus Status { get; internal set; }

  public void SetTexture(Texture2D newTexture)
  {
    this.Dispose();
    this.texture = newTexture;
  }

  public void Dispose()
  {
    if (!Object.op_Implicit((Object) this.texture))
      return;
    Object.Destroy((Object) this.texture);
  }

  public static string StringStatus(string key, DownloadStatus status)
  {
    switch (status)
    {
      case DownloadStatus.Failed:
        return $"<i>{key} failed to load.</i>";
      case DownloadStatus.InProgress:
        return $"<i>{key} loading{GenText.MarchingEllipsis(0.0f)}</i>";
      case DownloadStatus.Success:
        return $"<i>{key} successfully loaded.</i>";
      default:
        throw new MissingMemberException("DownloadStatus");
    }
  }
}
