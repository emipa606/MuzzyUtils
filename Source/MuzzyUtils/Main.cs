using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MuzzyUtils;

[StaticConstructorOnStartup]
public class Main
{
    public static readonly MethodInfo drawSelectionOverlayOnGUIMethod;
    public static readonly MethodInfo drawCaravanSelectionOverlayOnGUIMethod;
    public static readonly MethodInfo drawIconsMethod;
    public static readonly FieldInfo deadColonistTexField;
    public static readonly FieldInfo pawnLabelsCacheField;

    public static readonly Texture2D HappyMoodTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.75f, 0.2f, 0.5f));

    public static readonly Texture2D ContentMoodTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f, 0.5f));

    public static readonly Texture2D NeutralMoodTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.87f, 0.96f, 0.79f, 0.5f));

    public static readonly Texture2D MoodMinorCrossedTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.85f, 0.85f, 0.2f, 0.5f));

    public static readonly Texture2D MoodMajorCrossedTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.95f, 0.55f, 0.05f, 0.75f));

    public static readonly Texture2D MoodExtremeCrossedTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.95f, 0.15f, 0.00f, 0.8f));

    public static readonly Texture2D MoodExtremeCrossedBGTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.1f, 0.00f, 0.45f));

    public static readonly Texture2D MoodTargetTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.7f, 0.9f, 0.95f, 0.7f));

    public static readonly Texture2D MoodBreakTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.2f, 0.22f, 0.8f));

    public static readonly Texture2D MoodBreakInvertedTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.9f, 0.9f, 0.2f));


    static Main()
    {
        var harmony = new Harmony("Mlie.MuzzyUtils");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        drawSelectionOverlayOnGUIMethod = typeof(ColonistBarColonistDrawer).GetMethod("DrawSelectionOverlayOnGUI",
            BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(Pawn), typeof(Rect)], null);
        drawCaravanSelectionOverlayOnGUIMethod = typeof(ColonistBarColonistDrawer).GetMethod(
            "DrawCaravanSelectionOverlayOnGUI",
            BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(Caravan), typeof(Rect)], null);
        drawIconsMethod = typeof(ColonistBarColonistDrawer).GetMethod("DrawIcons",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null, [typeof(Rect), typeof(Pawn)], null);
        deadColonistTexField = typeof(ColonistBarColonistDrawer).GetField("DeadColonistTex",
            BindingFlags.Static | BindingFlags.NonPublic);
        pawnLabelsCacheField = typeof(ColonistBarColonistDrawer).GetField("pawnLabelsCache",
            BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private static void LogMessage(string text)
    {
        Log.Message("[ColorCodedMoodBar] " + text);
    }
}