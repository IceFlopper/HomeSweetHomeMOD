using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace HomeSweetHome
{
    public class HomeSweetHome : Mod
    {
        public static HomeSweetHomeSettings settings;

        public HomeSweetHome(ModContentPack content) : base(content)
        {
            settings = GetSettings<HomeSweetHomeSettings>();
            var harmony = new HarmonyLib.Harmony("com.Lewkah0.homesweethome");
            harmony.PatchAll();
        }

        public override string SettingsCategory() => "Home Sweet Home";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            bool previousSetting = settings.enableWorriedThought;
            listingStandard.CheckboxLabeled("Enable Worried Thought", ref settings.enableWorriedThought, "Toggle to enable or disable the worried thought for caravans.");

            if (previousSetting != settings.enableWorriedThought && !settings.enableWorriedThought)
            {
                RemoveWorriedThoughtFromAllPawns();
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        private void RemoveWorriedThoughtFromAllPawns()
        {
            foreach (Map map in Find.Maps)
            {
                foreach (Pawn pawn in map.mapPawns.FreeColonists)
                {
                    var worriedThoughts = pawn.needs.mood.thoughts.memories.Memories
                        .Where(m => m.def == ThoughtDef.Named("HomeSweetHome_Thought_Worried")).ToList();
                    foreach (var worriedThought in worriedThoughts)
                    {
                        pawn.needs.mood.thoughts.memories.RemoveMemory(worriedThought);
                    }
                }
            }
        }
    }
}
