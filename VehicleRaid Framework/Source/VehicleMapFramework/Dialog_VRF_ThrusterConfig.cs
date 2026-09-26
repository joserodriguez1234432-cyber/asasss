using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VehicleRaidFramework.VehicleMapFramework
{
    public class Dialog_VRF_ThrusterConfig : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Dictionary<string, string> textBuffers = new Dictionary<string, string>();
        private List<ThingDef> cachedThrusters;

        public override Vector2 InitialSize => new Vector2(650f, 500f);

        public Dialog_VRF_ThrusterConfig()
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            cachedThrusters = VRF_GravshipSpeedUtility.GetAllThrusterDefs();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "VRF_ThrusterConfig_Title".Translate());
            Text.Font = GameFont.Small;

            Rect descRect = new Rect(0f, 40f, inRect.width, 30f);
            Widgets.Label(descRect, "VRF_ThrusterConfig_Desc".Translate());

            float topY = 75f;
            float bottomY = inRect.height - 45f;
            Rect outRect = new Rect(0f, topY, inRect.width, bottomY - topY);

            float rowHeight = 44f;
            float viewHeight = Mathf.Max(outRect.height, cachedThrusters.Count * rowHeight + 10f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float curY = 5f;

            for (int i = 0; i < cachedThrusters.Count; i++)
            {
                ThingDef def = cachedThrusters[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, rowHeight - 4f);
                Widgets.DrawHighlightIfMouseover(rowRect);

                // Icon
                Rect iconRect = new Rect(rowRect.x + 4f, rowRect.y + 4f, 32f, 32f);
                Widgets.DefIcon(iconRect, def);

                // Label & Range
                float range = VRF_GravshipSpeedUtility.GetRangeForThrusterDef(def);
                string labelText = $"{def.LabelCap} ({range:F0} casillas / cells)";
                Rect labelRect = new Rect(rowRect.x + 44f, rowRect.y + 6f, 260f, 28f);
                Widgets.Label(labelRect, labelText);

                // Speed setting
                float curSpeed = VRF_GravshipSpeedUtility.GetConfiguredSpeedForThruster(def);
                if (!textBuffers.ContainsKey(def.defName))
                {
                    textBuffers[def.defName] = curSpeed.ToString("F2");
                }

                Rect speedLabelRect = new Rect(rowRect.x + 310f, rowRect.y + 6f, 100f, 28f);
                Widgets.Label(speedLabelRect, "VRF_ThrusterConfig_Speed".Translate() + ":");

                Rect inputRect = new Rect(rowRect.x + 415f, rowRect.y + 6f, 65f, 26f);
                string buf = textBuffers[def.defName];
                buf = Widgets.TextField(inputRect, buf);
                textBuffers[def.defName] = buf;

                if (float.TryParse(buf, out float parsed) && parsed > 0f)
                {
                    if (Mathf.Abs(parsed - curSpeed) > 0.001f)
                    {
                        VRF_GravshipSpeedUtility.SetConfiguredSpeedForThruster(def, parsed);
                    }
                }

                // Reset button
                Rect resetBtnRect = new Rect(rowRect.x + 490f, rowRect.y + 6f, 110f, 26f);
                if (Widgets.ButtonText(resetBtnRect, "Reset".Translate()))
                {
                    float defSpeed = VRF_GravshipSpeedUtility.GetDefaultSpeedForThruster(def);
                    VRF_GravshipSpeedUtility.SetConfiguredSpeedForThruster(def, defSpeed);
                    textBuffers[def.defName] = defSpeed.ToString("F2");
                }

                curY += rowHeight;
            }

            Widgets.EndScrollView();
        }
    }
}
