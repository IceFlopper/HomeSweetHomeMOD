using RimWorld;
using UnityEngine;
using Verse;

namespace HomeSweetHome
{
    public class HomeSweetHomeMod : Mod
    {
        private static HomeSweetHomeSettings settings;

        private Vector2 scrollPosition;

        public HomeSweetHomeMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<HomeSweetHomeSettings>();
        }

        public static HomeSweetHomeSettings Settings => settings ?? (settings = new HomeSweetHomeSettings());

        public override string SettingsCategory()
        {
            return "HSH.ModTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            HomeSweetHomeSettings s = Settings;

            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, 660f);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);

            list.CheckboxLabeled("HSH.Settings.Enabled".Translate(), ref s.enabled, "HSH.Settings.Enabled.Desc".Translate());
            list.GapLine();

            list.Label("HSH.Settings.WhoFeelsWhat".Translate());
            list.Gap(4f);
            list.CheckboxLabeled("HSH.Settings.Separation".Translate(), ref s.separationThoughts, "HSH.Settings.Separation.Desc".Translate());
            list.CheckboxLabeled("HSH.Settings.Reunion".Translate(), ref s.reunionThoughts, "HSH.Settings.Reunion.Desc".Translate());
            list.CheckboxLabeled("HSH.Settings.Gloating".Translate(), ref s.gloatingThoughts, "HSH.Settings.Gloating.Desc".Translate());
            list.GapLine();

            list.Label("HSH.Settings.OnTheRoad".Translate());
            list.Gap(4f);
            list.CheckboxLabeled("HSH.Settings.Homesickness".Translate(), ref s.homesickness, "HSH.Settings.Homesickness.Desc".Translate());
            list.CheckboxLabeled("HSH.Settings.Homecoming".Translate(), ref s.homecomingThoughts, "HSH.Settings.Homecoming.Desc".Translate());
            list.CheckboxLabeled("HSH.Settings.Destination".Translate(), ref s.destinationThoughts, "HSH.Settings.Destination.Desc".Translate());
            list.CheckboxLabeled("HSH.Settings.Companions".Translate(), ref s.companionThoughts, "HSH.Settings.Companions.Desc".Translate());
            list.CheckboxLabeled("HSH.Settings.TravelBonds".Translate(), ref s.travelBonds, "HSH.Settings.TravelBonds.Desc".Translate());
            list.GapLine();

            list.Label("HSH.Settings.Camping".Translate());
            list.Gap(4f);
            list.CheckboxLabeled("HSH.Settings.CampCountsAsAway".Translate(), ref s.campingCountsAsAway, "HSH.Settings.CampCountsAsAway.Desc".Translate());
            list.CheckboxLabeled("HSH.Settings.CampThoughts".Translate(), ref s.campThoughts, "HSH.Settings.CampThoughts.Desc".Translate());
            list.Label("HSH.Settings.CampWear".Translate(s.campWearFactor.ToStringPercent()), -1f, "HSH.Settings.CampWear.Desc".Translate());
            s.campWearFactor = list.Slider(s.campWearFactor, 0f, 1f);
            list.GapLine();

            list.Label("HSH.Settings.Tuning".Translate());
            list.Gap(4f);
            list.Label("HSH.Settings.Intensity".Translate(s.intensity.ToStringPercent()), -1f, "HSH.Settings.Intensity.Desc".Translate());
            s.intensity = Widgets.HorizontalSlider(list.GetRect(22f), s.intensity, 0f, 2f, false, null, null, null, 0.05f);
            list.Gap(6f);
            list.Label("HSH.Settings.MinAbsence".Translate(s.minAbsenceDays.ToString("0.0")), -1f, "HSH.Settings.MinAbsence.Desc".Translate());
            s.minAbsenceDays = Widgets.HorizontalSlider(list.GetRect(22f), s.minAbsenceDays, 0.25f, 6f, false, null, null, null, 0.25f);
            list.Gap(10f);

            if (list.ButtonText("HSH.Settings.ResetDefaults".Translate()))
            {
                s.Reset();
            }

            if (Current.ProgramState == ProgramState.Playing && list.ButtonText("HSH.Settings.ClearThoughts".Translate()))
            {
                Find.World?.GetComponent<AbsenceTracker>()?.ScrubEverything();
                Messages.Message("HSH.Settings.ClearThoughts.Done".Translate(), MessageTypeDefOf.TaskCompletion, false);
            }

            list.End();
            Widgets.EndScrollView();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            // Switching the mod off shouldn't leave everyone stewing over a caravan forever.
            if (!Settings.enabled && Current.ProgramState == ProgramState.Playing)
            {
                Find.World?.GetComponent<AbsenceTracker>()?.ScrubEverything();
            }
        }
    }
}
