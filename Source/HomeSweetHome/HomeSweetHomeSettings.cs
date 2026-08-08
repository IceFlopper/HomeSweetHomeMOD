using Verse;

namespace HomeSweetHome
{
    public class HomeSweetHomeSettings : ModSettings
    {
        public bool enabled = true;

        public bool separationThoughts = true;
        public bool reunionThoughts = true;
        public bool gloatingThoughts = true;
        public bool homesickness = true;
        public bool homecomingThoughts = true;
        public bool destinationThoughts = true;
        public bool companionThoughts = true;
        public bool travelBonds = true;
        public bool campThoughts = true;

        /// <summary>Camping still counts as being away from home, but the player can turn that off.</summary>
        public bool campingCountsAsAway = true;

        /// <summary>Global dial on every mood effect the mod produces.</summary>
        public float intensity = 1f;

        /// <summary>Nothing fires until someone has been gone this long, so day trips stay quiet.</summary>
        public float minAbsenceDays = 1f;

        /// <summary>How heavily a day spent in camp counts toward wearing a traveller down, against a day on the road.</summary>
        public float campWearFactor = 0.5f;

        public void Reset()
        {
            enabled = true;
            separationThoughts = true;
            reunionThoughts = true;
            gloatingThoughts = true;
            homesickness = true;
            homecomingThoughts = true;
            destinationThoughts = true;
            companionThoughts = true;
            travelBonds = true;
            campThoughts = true;
            campingCountsAsAway = true;
            intensity = 1f;
            minAbsenceDays = 1f;
            campWearFactor = 0.5f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref separationThoughts, "separationThoughts", true);
            Scribe_Values.Look(ref reunionThoughts, "reunionThoughts", true);
            Scribe_Values.Look(ref gloatingThoughts, "gloatingThoughts", true);
            Scribe_Values.Look(ref homesickness, "homesickness", true);
            Scribe_Values.Look(ref homecomingThoughts, "homecomingThoughts", true);
            Scribe_Values.Look(ref destinationThoughts, "destinationThoughts", true);
            Scribe_Values.Look(ref companionThoughts, "companionThoughts", true);
            Scribe_Values.Look(ref travelBonds, "travelBonds", true);
            Scribe_Values.Look(ref campThoughts, "campThoughts", true);
            Scribe_Values.Look(ref campingCountsAsAway, "campingCountsAsAway", true);
            Scribe_Values.Look(ref intensity, "intensity", 1f);
            Scribe_Values.Look(ref minAbsenceDays, "minAbsenceDays", 1f);
            Scribe_Values.Look(ref campWearFactor, "campWearFactor", 0.5f);
        }
    }
}
