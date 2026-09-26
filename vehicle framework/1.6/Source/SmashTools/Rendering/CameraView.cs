// Decompiled with JetBrains decompiler
// Type: SmashTools.CameraView
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using SmashTools.Rendering;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public static class CameraView
{
  private const float MinZoom = 2f;
  private const float MaxZoom = 60f;
  private const float DefaultCameraSize = 8f;
  private const float PageKeyZoomRate = 4f;
  private const float ZoomScaleFromAltDenominator = 35f;
  private const float CameraViewerZoomRate = 0.55f;
  private static Camera camera;
  private static RenderTexture renderTexture;
  private static float orthographicSize;
  private static CameraMapConfig cameraConfig = (CameraMapConfig) new CameraMapConfig_Normal();
  public static Dialog_GraphEditor.AnimationSettings animationSettings = new Dialog_GraphEditor.AnimationSettings();
  private static int lastViewRectGetFrame = -1;
  private static CellRect lastViewRect;
  private static float desiredSize = 24f;
  private static Vector3 rootPos;
  private static float rootSize;
  public static readonly Texture2D pauseTexture = ContentFinder<Texture2D>.Get("SmashTools/VideoPause", true);
  public static readonly Texture2D playTexture = ContentFinder<Texture2D>.Get("SmashTools/VideoPlay", true);
  public static readonly Texture2D dragHandleIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult", true);

  public static Vector3 RootPos
  {
    get => CameraView.rootPos;
    set => CameraView.rootPos = value;
  }

  public static float OrthographicSize
  {
    get => CameraView.orthographicSize;
    set => CameraView.orthographicSize = value;
  }

  public static bool InUse { get; private set; }

  private static bool Patched { get; set; }

  private static bool LockedToMainCamera { get; set; } = false;

  public static CellRect CurrentViewRect
  {
    get
    {
      if (Time.frameCount != CameraView.lastViewRectGetFrame)
      {
        CameraView.lastViewRect = new CellRect();
        float num = (float) UI.screenWidth / (float) UI.screenHeight;
        Vector3 position = ((Component) CameraView.camera).transform.position;
        CameraView.lastViewRect.minX = Mathf.FloorToInt((float) ((double) position.x - (double) CameraView.rootSize * (double) num - 1.0));
        CameraView.lastViewRect.maxX = Mathf.CeilToInt(position.x + CameraView.rootSize * num);
        CameraView.lastViewRect.minZ = Mathf.FloorToInt((float) ((double) position.z - (double) CameraView.rootSize - 1.0));
        CameraView.lastViewRect.maxZ = Mathf.CeilToInt(position.z + CameraView.rootSize);
        CameraView.lastViewRectGetFrame = Time.frameCount;
      }
      return CameraView.lastViewRect;
    }
  }

  public static void HandleZoom()
  {
    float num = 0.0f;
    if (Event.current.type == 6)
    {
      num -= Event.current.delta.y * 0.55f;
      Event.current.Use();
    }
    if (KeyBindingDefOf.MapZoom_In.KeyDownEvent)
    {
      num += 4f;
      Event.current.Use();
    }
    if (KeyBindingDefOf.MapZoom_Out.KeyDownEvent)
    {
      num -= 4f;
      Event.current.Use();
    }
    CameraView.desiredSize -= (float) ((double) num * (double) CameraView.cameraConfig.zoomSpeed * (double) CameraView.rootSize / 35.0);
    CameraView.desiredSize = Mathf.Clamp(CameraView.desiredSize, 2f, 60f);
  }

  public static void Update(Vector3 position)
  {
    if (!CameraView.InUse)
      return;
    CameraView.rootPos = new Vector3(position.x, (float) (15.0 + ((double) CameraView.rootSize - (double) CameraView.cameraConfig.sizeRange.min) / ((double) CameraView.cameraConfig.sizeRange.max - (double) CameraView.cameraConfig.sizeRange.min) * 50.0), position.z);
    CameraView.OrthographicSize = Mathf.Lerp(CameraView.OrthographicSize, CameraView.desiredSize, 0.05f);
    CameraView.camera.orthographicSize = CameraView.OrthographicSize;
    ((Component) CameraView.camera).transform.position = CameraView.rootPos;
    if (!CameraView.LockedToMainCamera)
      return;
    ((Component) Find.Camera).transform.position = ((Component) CameraView.camera).transform.position;
    Find.Camera.orthographicSize = CameraView.OrthographicSize;
  }

  public static bool RenderAt(Rect rect)
  {
    if (!CameraView.InUse)
      return false;
    try
    {
      GUI.DrawTexture(rect, (Texture) CameraView.renderTexture);
      if (Mouse.IsOver(rect))
        CameraView.HandleZoom();
    }
    catch (Exception ex)
    {
      Log.ErrorOnce($"Exception thrown in CameraView. Exception = {ex}", "CameraView_RT".GetHashCode());
      return false;
    }
    return true;
  }

  public static void DrawMapGridInView()
  {
    CellRect currentViewRect = CameraView.CurrentViewRect;
    foreach (IntVec3 intVec3 in currentViewRect)
    {
      GenDraw.DrawLineBetween(((IntVec3) ref intVec3).ToVector3(), Vector3.op_Addition(((IntVec3) ref intVec3).ToVector3(), new Vector3(1f, 0.0f, 0.0f)));
      GenDraw.DrawLineBetween(((IntVec3) ref intVec3).ToVector3(), Vector3.op_Addition(((IntVec3) ref intVec3).ToVector3(), new Vector3(0.0f, 0.0f, 1f)));
    }
  }

  public static void Close()
  {
    CameraView.InUse = false;
    if (!Object.op_Implicit((Object) CameraView.camera) || !Object.op_Implicit((Object) ((Component) CameraView.camera).gameObject))
      return;
    CameraView.renderTexture.Release();
    Object.Destroy((Object) CameraView.renderTexture);
    Object.Destroy((Object) ((Component) CameraView.camera).gameObject);
    CameraView.camera = (Camera) null;
    CameraView.renderTexture = (RenderTexture) null;
  }

  public static void ResetSize()
  {
    CameraView.desiredSize = 8f;
    CameraView.rootSize = CameraView.desiredSize;
  }

  public static void Start(float orthographicSize = 11f, CameraMapConfig cameraConfig = null)
  {
    CameraView.Start(new IntVec2(512 /*0x0200*/, 512 /*0x0200*/), orthographicSize, cameraConfig: cameraConfig);
  }

  public static void Start(
    IntVec2 size,
    float orthographicSize = 11f,
    RenderTextureFormat renderTextureFormat = 11,
    CameraMapConfig cameraConfig = null)
  {
    CameraView.InUse = true;
    try
    {
      if (cameraConfig == null)
        cameraConfig = (CameraMapConfig) Activator.CreateInstance(typeof (CameraMapConfig_Normal));
      CameraView.cameraConfig = cameraConfig;
      CameraView.camera = CameraView.CreateCamera(orthographicSize);
      CameraView.renderTexture = CameraView.CreateRenderTexture(size, renderTextureFormat);
      CameraView.camera.targetTexture = CameraView.renderTexture;
      ((Component) CameraView.camera).transform.position = ((Component) Find.Camera).transform.position;
      ((Component) CameraView.camera).transform.rotation = ((Component) Find.Camera).transform.rotation;
      CameraView.ResetSize();
      CameraView.PatchOcclusionCulling();
    }
    catch
    {
      CameraView.Close();
      throw;
    }
  }

  internal static Camera CreateCamera(float orthographicSize)
  {
    GameObject gameObject = new GameObject("CameraView_GameObject");
    gameObject.SetActive(true);
    CameraView.orthographicSize = orthographicSize;
    Camera camera = gameObject.AddComponent<Camera>();
    camera.orthographic = true;
    camera.cameraType = (CameraType) 1;
    camera.orthographicSize = orthographicSize;
    return camera;
  }

  internal static RenderTexture CreateRenderTexture(
    IntVec2 size,
    RenderTextureFormat renderTextureFormat)
  {
    RenderTexture renderTexture = new RenderTexture(size.x, size.z, 16 /*0x10*/, renderTextureFormat.OrNextSupportedFormat());
    ((Object) renderTexture).name = "CameraView_RT";
    renderTexture.Create();
    return renderTexture;
  }

  private static void PatchOcclusionCulling()
  {
    if (CameraView.Patched)
      return;
    CameraView.Patched = true;
    try
    {
      HarmonyPatcher.Harmony.Patch((MethodBase) AccessTools.PropertyGetter(typeof (MapDrawer), "ViewRect"), (HarmonyMethod) null, new HarmonyMethod(typeof (CameraView), "CameraPreviewViewRect", (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null);
    }
    catch (Exception ex)
    {
      CameraView.LockedToMainCamera = true;
      Log.Error($"Failed to patch CameraView rect for occlusion culling. Animations will not work outside main camera rect, locking main camera to viewer.\nException={ex}");
    }
  }

  private static void CameraPreviewViewRect(ref CellRect __result, Map ___map)
  {
    if (!CameraView.InUse)
      return;
    __result = new CellRect(0, 0, ___map.Size.x, ___map.Size.z);
  }
}
