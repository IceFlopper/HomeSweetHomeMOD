using RimWorld;
using Verse;

namespace HomeSweetHome
{
    /// <summary>One colonist's current spell away from the colony.</summary>
    public class Journey : IExposable
    {
        public Pawn pawn;
        public int departedTick;
        public int lastSeenTick;
        public int campTicks;
        public Whereabouts place = Whereabouts.Away;

        /// <summary>Set once they've walked onto whatever map they set out for, so it only fires the once.</summary>
        public bool arrived;

        public Journey()
        {
        }

        public Journey(Pawn pawn, int tick)
        {
            this.pawn = pawn;
            departedTick = tick;
            lastSeenTick = tick;
        }

        public float DaysGone => (Find.TickManager.TicksGame - departedTick) / (float)GenDate.TicksPerDay;

        public float DaysCamped => campTicks / (float)GenDate.TicksPerDay;

        /// <summary>
        /// Days that count toward homesickness. Sitting in a camp is still time away, but it's a
        /// roof and a fire rather than another day of walking, so it weighs less.
        /// </summary>
        public float WearingDays
        {
            get
            {
                float camped = DaysCamped;
                return DaysGone - camped * (1f - HomeSweetHomeMod.Settings.campWearFactor);
            }
        }

        public bool Counts => DaysGone >= HomeSweetHomeMod.Settings.minAbsenceDays;

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref departedTick, "departedTick");
            Scribe_Values.Look(ref lastSeenTick, "lastSeenTick");
            Scribe_Values.Look(ref campTicks, "campTicks");
            Scribe_Values.Look(ref place, "place", Whereabouts.Away);
            Scribe_Values.Look(ref arrived, "arrived");
        }
    }
}
