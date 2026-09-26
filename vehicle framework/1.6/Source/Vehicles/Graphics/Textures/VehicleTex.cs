// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTex
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class VehicleTex
{
  internal const string DefaultVehicleIconTexPath = "UI/Icons/DefaultVehicleIcon";
  private const string DefaultShuttleIconTexPath = "UI/Icons/DefaultPlaneIcon";
  private const string DefaultBoatIconTexPath = "UI/Icons/DefaultBoatIcon";
  public static readonly Texture2D DefaultVehicleIcon = ContentFinder<Texture2D>.Get("UI/Icons/DefaultVehicleIcon", true);
  public static readonly Texture2D DraftVehicle = ContentFinder<Texture2D>.Get("UI/Gizmos/DraftVehicle", true);
  public static readonly Texture2D HaltVehicle = ContentFinder<Texture2D>.Get("UI/Gizmos/HaltVehicle", true);
  public static readonly Texture2D UnloadAll = ContentFinder<Texture2D>.Get("UI/Gizmos/UnloadAll", true);
  public static readonly Texture2D UnloadIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/UnloadArrow", true);
  public static readonly Texture2D StashVehicle = ContentFinder<Texture2D>.Get("UI/Gizmos/StashVehicle", true);
  public static readonly Texture2D FishingIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/FishingGizmo", true);
  public static readonly Texture2D HaulPawnToVehicle = ContentFinder<Texture2D>.Get("UI/Gizmos/HaulPawnToVehicle", true);
  public static readonly Texture2D DeployVehicle = ContentFinder<Texture2D>.Get("UI/Gizmos/Gizmo_DeployVehicle", true);
  public static readonly Texture2D UndeployVehicle = ContentFinder<Texture2D>.Get("UI/Gizmos/Gizmo_UndeployVehicle", true);
  public static readonly Texture2D GearForward = ContentFinder<Texture2D>.Get("UI/Gizmos/GearDrive", true);
  public static readonly Texture2D GearReverse = ContentFinder<Texture2D>.Get("UI/Gizmos/GearReverse", true);
  public static readonly Texture2D[] PackCargoIcon = new Texture2D[4]
  {
    ContentFinder<Texture2D>.Get("UI/Gizmos/StartLoadBoat", true),
    ContentFinder<Texture2D>.Get("UI/Gizmos/StartLoadAerial", true),
    ContentFinder<Texture2D>.Get("UI/Gizmos/StartLoadVehicle", true),
    BaseContent.BadTex
  };
  public static readonly Texture2D[] CancelPackCargoIcon = new Texture2D[4]
  {
    ContentFinder<Texture2D>.Get("UI/Gizmos/CancelLoadBoat", true),
    ContentFinder<Texture2D>.Get("UI/Gizmos/CancelLoadAerial", true),
    ContentFinder<Texture2D>.Get("UI/Gizmos/CancelLoadVehicle", true),
    BaseContent.BadTex
  };
  public static readonly Texture2D FormCaravanVehicle = ContentFinder<Texture2D>.Get("UI/Gizmos/FormCaravanVehicle", true);
  public static readonly Texture2D RepairVehicles = ContentFinder<Texture2D>.Get("UI/Gizmos/Gizmo_RepairVehicles", true);
  public static readonly Texture2D RefuelFromCargo = ContentFinder<Texture2D>.Get("UI/Gizmos/RefuelFromCargo", true);
  public static readonly Texture2D ReloadIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/Reload", true);
  public static readonly Texture2D AutoTargetIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/AutoTarget", true);
  public static readonly Texture2D HaltIcon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
  public static readonly Texture2D SwitchLeft = ContentFinder<Texture2D>.Get("UI/ColorTools/SwitchLeft", true);
  public static readonly Texture2D SwitchRight = ContentFinder<Texture2D>.Get("UI/ColorTools/SwitchRight", true);
  public static readonly Texture2D SwapColors = ContentFinder<Texture2D>.Get("UI/ColorTools/SwapColors", true);
  public static readonly Texture2D Recolor = ContentFinder<Texture2D>.Get("UI/ColorTools/Paintbrush", true);
  public static readonly Texture2D ColorPicker = ContentFinder<Texture2D>.Get("UI/ColorTools/ColorCog", true);
  public static readonly Texture2D ColorHue = ContentFinder<Texture2D>.Get("UI/ColorTools/ColorHue", true);
  public static readonly Texture2D Rotate = ContentFinder<Texture2D>.Get("UI/Icons/Rotate", true);
  public static readonly Texture2D LeftArrow = ContentFinder<Texture2D>.Get("UI/Icons/ArrowLeft", true);
  public static readonly Texture2D RightArrow = ContentFinder<Texture2D>.Get("UI/Icons/ArrowRight", true);
  public static readonly Texture2D ResetPage = ContentFinder<Texture2D>.Get("UI/Settings/ResetPage", true);
  public static readonly Texture2D Settings = ContentFinder<Texture2D>.Get("UI/Settings/Settings", true);
  public static readonly Texture2D RoutePlanner = ContentFinder<Texture2D>.Get("UI/Gizmos/VehicleRoutePlanner", true);
  public static readonly Dictionary<VehicleDef, Texture2D> CachedTextureIcons = new Dictionary<VehicleDef, Texture2D>();
  public static readonly Dictionary<VehicleDef, string> CachedTextureIconPaths = new Dictionary<VehicleDef, string>();
  private static readonly Dictionary<(VehicleDef, Rot4), Texture2D> CachedVehicleTextures = new Dictionary<(VehicleDef, Rot4), Texture2D>();
  private static readonly Dictionary<VehicleDef, Graphic_Vehicle> CachedGraphics = new Dictionary<VehicleDef, Graphic_Vehicle>();
  private static readonly Dictionary<string, Texture2D> CachedTextureFilepaths = new Dictionary<string, Texture2D>();

  static VehicleTex()
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      stringBuilder.Clear();
      stringBuilder.AppendLine("Generating TextureCache for " + ((Def) vehicleDef).defName);
      try
      {
        stringBuilder.Append("Creating icon...");
        string key = vehicleDef.properties.iconTexPath;
        if (GenText.NullOrEmpty(key))
        {
          switch (vehicleDef.type)
          {
            case VehicleType.Sea:
              key = "UI/Icons/DefaultBoatIcon";
              break;
            case VehicleType.Air:
              key = "UI/Icons/DefaultPlaneIcon";
              break;
            case VehicleType.Land:
              key = "UI/Icons/DefaultVehicleIcon";
              break;
            default:
              key = "UI/Icons/DefaultVehicleIcon";
              break;
          }
        }
        stringBuilder.AppendLine("Icon created");
        stringBuilder.AppendLine("Creating BodyGraphicData and cached graphics...");
        if (vehicleDef.graphicData != null)
        {
          GraphicDataRGB graphicData = vehicleDef.graphicData;
          Graphic_Vehicle graphic = graphicData.Graphic as Graphic_Vehicle;
          stringBuilder.AppendLine("Setting TextureCache...");
          VehicleTex.SetTextureCache(vehicleDef, graphicData);
          stringBuilder.AppendLine("Finalized TextureCache");
          Texture2D texture2D;
          if (!VehicleTex.CachedTextureFilepaths.TryGetValue(key, out texture2D))
          {
            texture2D = ContentFinder<Texture2D>.Get(key, true);
            VehicleTex.CachedTextureFilepaths[key] = texture2D;
          }
          stringBuilder.AppendLine("Finalizing caching");
          VehicleTex.CachedGraphics[vehicleDef] = graphic;
          VehicleTex.CachedTextureIcons[vehicleDef] = texture2D;
          VehicleTex.CachedTextureIconPaths[vehicleDef] = key;
        }
        else
        {
          GraphicDataRGB graphicData = vehicleDef.graphicData;
          SmashLog.Error($"Unable to create GraphicData of type <type>{(graphicData != null ? Gen.ToStringSafe<Type>(((object) graphicData).GetType()) : (string) null) ?? "Null"} for {((Def) vehicleDef).defName}.\n{stringBuilder}");
        }
      }
      catch (Exception ex)
      {
        Log.Error($"Exception thrown while trying to generate cached textures. Exception=\"{ex}\"\n-----------------Tasks-----------------\n{stringBuilder}");
      }
    }
  }

  public static Texture2D VehicleTexture(VehicleDef def, Rot4 rot, out float rotate)
  {
    rotate = 0.0f;
    Texture2D texture2D;
    if (VehicleTex.CachedVehicleTextures.TryGetValue((def, rot), out texture2D))
      return texture2D;
    rotate = ((Rot4) ref rot).AsAngle;
    return VehicleTex.CachedVehicleTextures[(def, Rot4.North)];
  }

  private static void SetTextureCache(VehicleDef vehicleDef, GraphicDataRGB graphicData)
  {
    Texture2D texture2D1 = ContentFinder<Texture2D>.Get(graphicData.texPath + "_north", false) ?? ContentFinder<Texture2D>.Get(graphicData.texPath, true);
    if (!Object.op_Implicit((Object) texture2D1))
      throw new Exception($"Unable to locate north texture for {vehicleDef}");
    Texture2D texture2D2 = ContentFinder<Texture2D>.Get(graphicData.texPath + "_east", false);
    Texture2D texture2D3 = ContentFinder<Texture2D>.Get(graphicData.texPath + "_south", false);
    Texture2D texture2D4 = ContentFinder<Texture2D>.Get(graphicData.texPath + "_west", false);
    VehicleTex.CachedVehicleTextures[(vehicleDef, Rot4.North)] = texture2D1;
    if (Object.op_Implicit((Object) texture2D2))
      VehicleTex.CachedVehicleTextures[(vehicleDef, Rot4.East)] = texture2D2;
    if (Object.op_Implicit((Object) texture2D3))
      VehicleTex.CachedVehicleTextures[(vehicleDef, Rot4.South)] = texture2D3;
    if (!Object.op_Implicit((Object) texture2D4))
      return;
    VehicleTex.CachedVehicleTextures[(vehicleDef, Rot4.West)] = texture2D4;
  }
}
