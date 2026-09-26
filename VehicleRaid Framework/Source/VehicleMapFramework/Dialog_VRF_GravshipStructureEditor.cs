using System.IO;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using VehicleRaidFramework.VehicleMapFramework;

namespace VehicleRaidFramework
{
    public class Dialog_VRF_GravshipStructureEditor : Window
    {
        private readonly VRF_GravshipRaidEntry _entry;
        private readonly Vehicles.VehicleDef _vDef;
        private readonly string _filePath;
        private string _cpBuf;
        private string _minBuf;
        private string _maxBuf;

        public override Vector2 InitialSize => new Vector2(760f, 440f);

        public Dialog_VRF_GravshipStructureEditor(VRF_GravshipRaidEntry entry, Vehicles.VehicleDef vDef = null, string filePath = null)
        {
            _entry = entry;
            _vDef = vDef;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                _filePath = filePath;
            }
            else
            {
                try
                {
                    var all = VRF_GravshipPresetUtility.FindAllGravshipPresetFiles();
                    _filePath = all.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == entry.presetName);
                }
                catch
                {
                    _filePath = null;
                }
            }

            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            _cpBuf = ((int)_entry.combatPower).ToString();
            _minBuf = ((int)_entry.minRaidPoints).ToString();
            _maxBuf = ((int)_entry.maxRaidPoints).ToString();
        }

        public override void DoWindowContents(Rect inRect)
        {
            float contentHeight = inRect.height - 45f; // Reserve space for default close button

            // Left column: Large preview
            float previewSize = Mathf.Min(320f, contentHeight);
            Rect leftArea = new Rect(inRect.x, inRect.y, previewSize, contentHeight);

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(leftArea.x, leftArea.y, leftArea.width, 28f), "Vista previa");
            Text.Font = GameFont.Small;

            Rect previewBox = new Rect(leftArea.x, leftArea.y + 32f, previewSize, previewSize - 32f);
            Widgets.DrawBoxSolid(previewBox, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            Widgets.DrawBox(previewBox, 1);

            if (!string.IsNullOrEmpty(_filePath))
            {
                VRF_SettingsUI.DrawGravshipPresetStructureThumbnail(previewBox.ContractedBy(6f), _filePath, _vDef);
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.gray;
                Widgets.Label(previewBox, "Sin datos de estructura");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }

            // Right column: Configuration controls
            float rightX = leftArea.xMax + 24f;
            float rightW = inRect.xMax - rightX;
            Rect rightArea = new Rect(rightX, inRect.y, rightW, contentHeight);

            float curY = rightArea.y;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rightArea.x, curY, rightArea.width, 32f), "Estructura: " + _entry.presetName);
            curY += 38f;
            Text.Font = GameFont.Small;

            bool wasEnabled = _entry.enabled;
            Widgets.CheckboxLabeled(new Rect(rightArea.x, curY, rightArea.width, 28f), "Habilitar en raids naturales", ref _entry.enabled);
            if (_entry.enabled != wasEnabled) VRF_Mod.Instance.WriteSettings();
            curY += 34f;

            int totalStrats = DefDatabase<RaidStrategyDef>.DefCount;
            int allowedCount = _entry.allowedRaidStrategies.Count == 0
                ? totalStrats
                : totalStrats - _entry.allowedRaidStrategies.Count;

            string stratBtn = "Estrategias de raid (" + allowedCount + "/" + totalStrats + ")";
            if (Widgets.ButtonText(new Rect(rightArea.x, curY, Mathf.Min(rightArea.width, 240f), 28f), stratBtn))
            {
                Find.WindowStack.Add(new Dialog_VRF_RaidStrategies(_entry, _entry.presetName));
            }
            curY += 38f;

            Widgets.DrawLineHorizontal(rightArea.x, curY, rightArea.width);
            curY += 12f;

            Widgets.Label(new Rect(rightArea.x, curY, rightArea.width, 22f), "Poder de combate:");
            curY += 24f;
            float prevCp = _entry.combatPower;
            Widgets.TextFieldNumeric(new Rect(rightArea.x, curY, 140f, 24f), ref _entry.combatPower, ref _cpBuf, 0f, 999999f);
            if (_entry.combatPower != prevCp) VRF_Mod.Instance.WriteSettings();
            curY += 32f;

            float halfW = (rightArea.width - 20f) * 0.5f;
            Widgets.Label(new Rect(rightArea.x, curY, halfW, 22f), "Puntos mínimos de raid:");
            Widgets.Label(new Rect(rightArea.x + halfW + 20f, curY, halfW, 22f), "Puntos máximos de raid:");
            curY += 24f;

            float prevMin = _entry.minRaidPoints;
            Widgets.TextFieldNumeric(new Rect(rightArea.x, curY, 120f, 24f), ref _entry.minRaidPoints, ref _minBuf, 0f, 999999f);
            if (_entry.minRaidPoints != prevMin) VRF_Mod.Instance.WriteSettings();

            float prevMax = _entry.maxRaidPoints;
            Widgets.TextFieldNumeric(new Rect(rightArea.x + halfW + 20f, curY, 120f, 24f), ref _entry.maxRaidPoints, ref _maxBuf, 0f, 999999f);
            if (_entry.maxRaidPoints != prevMax) VRF_Mod.Instance.WriteSettings();
            curY += 36f;

            string hoverLabel = _entry.hoverMode
                ? "Modo hover Gravship: Activado"
                : "Modo hover Gravship: Desactivado";
            if (Widgets.ButtonText(new Rect(rightArea.x, curY, Mathf.Min(rightArea.width, 280f), 28f), hoverLabel))
            {
                _entry.hoverMode = !_entry.hoverMode;
                VRF_Mod.Instance.WriteSettings();
            }
            curY += 34f;
        }
    }
}
