using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MuzzyUtils;

[HarmonyPatch(typeof(ColonistBarColonistDrawer), "DrawColonist")]
public class ColonistBarColonistDrawer_DrawColonist_Patch
{
    private static float ApplyEntryInAnotherMapAlphaFactor(Map map, float alpha)
    {
        if (map == null)
        {
            if (!WorldRendererUtility.WorldRenderedNow)
            {
                alpha = Mathf.Min(alpha, 0.4f);
            }
        }
        else if (map != Find.CurrentMap || WorldRendererUtility.WorldRenderedNow)
        {
            alpha = Mathf.Min(alpha, 0.4f);
        }

        return alpha;
    }

    public static bool Prefix(ColonistBarColonistDrawer __instance,
        ref Rect rect, ref Pawn colonist, ref Map pawnMap,
        ref bool highlight, ref bool reordering)
    {
        var colonistBar = Find.ColonistBar;
        var entryRectAlpha = colonistBar.GetEntryRectAlpha(rect);
        entryRectAlpha = ApplyEntryInAnotherMapAlphaFactor(pawnMap, entryRectAlpha);

        if (reordering)
        {
            entryRectAlpha *= 0.5f;
        }

        var mood = !colonist.Dead ? colonist.needs.mood : null;
        var mentalBreaker = !colonist.Dead ? colonist.mindState.mentalBreaker : null;

        var color = new Color(1f, 1f, 1f, entryRectAlpha);
        GUI.color = color;

        if (mood != null && mentalBreaker != null)
        {
            var moodBorderRect = rect.ContractedBy(-1f);
            var currentMood = mood.CurLevelPercentage;
            if (currentMood <= mentalBreaker.BreakThresholdExtreme)
            {
                GUI.DrawTexture(moodBorderRect, Main.MoodExtremeCrossedTex);
            }
            else if (currentMood <= mentalBreaker.BreakThresholdMajor)
            {
                GUI.DrawTexture(moodBorderRect, Main.MoodMajorCrossedTex);
            }
            else if (currentMood <= mentalBreaker.BreakThresholdMinor)
            {
                GUI.DrawTexture(moodBorderRect, Main.MoodMinorCrossedTex);
            }

            GUI.DrawTexture(rect, ColonistBar.BGTex);
            var moodRect = rect.ContractedBy(2f);
            var position = moodRect;
            var num = position.height * currentMood;
            position.yMin = position.yMax - num;
            position.height = num;

            var currentMoodLevel = colonist.needs.mood.CurLevel;

            // Extreme break threshold
            if (currentMoodLevel <= mentalBreaker.BreakThresholdExtreme)
            {
                GUI.DrawTexture(moodRect, Main.MoodExtremeCrossedBGTex);
                GUI.DrawTexture(position, Main.MoodExtremeCrossedTex);
            }
            // Major break threshold
            else if (currentMoodLevel <= mentalBreaker.BreakThresholdMajor)
            {
                GUI.DrawTexture(position, Main.MoodMajorCrossedTex);
            }
            // Minor break threshold
            else if (currentMoodLevel <= mentalBreaker.BreakThresholdMinor)
            {
                GUI.DrawTexture(position, Main.MoodMinorCrossedTex);
            }
            // Neutral
            else if (currentMoodLevel <= 0.65f)
            {
                GUI.DrawTexture(position, Main.NeutralMoodTex);
            }
            // Content
            else if (currentMoodLevel <= 0.9f)
            {
                GUI.DrawTexture(position, Main.ContentMoodTex);
            }
            // Happy
            else
            {
                GUI.DrawTexture(position, Main.HappyMoodTex);
            }

            foreach (var threshold in new List<float>
                     {
                         mentalBreaker.BreakThresholdExtreme, mentalBreaker.BreakThresholdMajor,
                         mentalBreaker.BreakThresholdMinor
                     })
            {
                var lineColor = Main.MoodBreakTex;
                if (currentMoodLevel <= threshold)
                {
                    lineColor = Main.MoodBreakInvertedTex;
                }

                GUI.DrawTexture(new Rect(moodRect.x,
                        moodRect.yMax - (moodRect.height * threshold),
                        moodRect.width, 1),
                    lineColor);
            }

            GUI.DrawTexture(
                new Rect(moodRect.x, moodRect.yMax - (moodRect.height * mood.CurInstantLevelPercentage),
                    moodRect.width, 1), Main.MoodTargetTex);
            GUI.DrawTexture(
                new Rect(moodRect.xMax + 1, moodRect.yMax - (moodRect.height * mood.CurInstantLevelPercentage) - 1,
                    2, 3), Main.MoodTargetTex);
        }
        else
        {
            GUI.DrawTexture(rect, ColonistBar.BGTex);
        }

        if (highlight)
        {
            var thickness = rect.width > 22f ? 3 : 2;
            GUI.color = Color.white;
            Widgets.DrawBox(rect, thickness);
            GUI.color = color;
        }

        var rect2 = rect.ContractedBy(-2f * colonistBar.Scale);

        var isColonistSelected = colonist.Dead
            ? Find.Selector.SelectedObjects.Contains(colonist.Corpse)
            : Find.Selector.SelectedObjects.Contains(colonist);
        if (isColonistSelected && !WorldRendererUtility.WorldRenderedNow)
        {
            Main.drawSelectionOverlayOnGUIMethod.Invoke(__instance, [colonist, rect2]);
        }
        else if (WorldRendererUtility.WorldRenderedNow && colonist.IsCaravanMember() &&
                 Find.WorldSelector.IsSelected(colonist.GetCaravan()))
        {
            Main.drawCaravanSelectionOverlayOnGUIMethod.Invoke(__instance,
                [colonist.GetCaravan(), rect2]);
        }

        var pawnTexturePosition = __instance.GetPawnTextureRect(new Vector2(rect.x, rect.y));

        GUI.DrawTexture(pawnTexturePosition, PortraitsCache.Get(colonist, ColonistBarColonistDrawer.PawnTextureSize,
            Rot4.South,
            ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f));
        GUI.color = new Color(1f, 1f, 1f, entryRectAlpha * 0.8f);
        Main.drawIconsMethod.Invoke(__instance, [rect, colonist]);
        GUI.color = color;

        if (colonist.Dead)
        {
            GUI.DrawTexture(rect, (Texture)Main.deadColonistTexField.GetValue(null));
        }

        var num2 = 4f * colonistBar.Scale;
        var pos = new Vector2(rect.center.x, rect.yMax - num2);
        GenMapUI.DrawPawnLabel(colonist, pos, entryRectAlpha,
            rect.width + colonistBar.SpaceBetweenColonistsHorizontal - 2f,
            (Dictionary<string, string>)Main.pawnLabelsCacheField.GetValue(__instance));
        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        return false;
    }
}