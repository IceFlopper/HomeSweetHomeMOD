using RimWorld.Planet;
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace HomeSweetHome
{
    public class CaravanDepartureTracker : WorldComponent
    {
        private Dictionary<Caravan, int> caravanDepartureTicks = new Dictionary<Caravan, int>();

        public CaravanDepartureTracker(World world) : base(world) { }

        public void SetDepartureTicks(Caravan caravan, int ticks)
        {
            if (!caravanDepartureTicks.ContainsKey(caravan))
            {
                caravanDepartureTicks[caravan] = ticks;
                //Log.Message($"HomeSweetHome: Set departure ticks for caravan: {ticks}");
            }
        }

        public int GetDepartureTicks(Caravan caravan)
        {
            if (caravanDepartureTicks.TryGetValue(caravan, out int ticks))
            {
                return ticks;
            }
            return 0;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            CheckForWorriedColonists();
        }

        private void CheckForWorriedColonists()
        {
            foreach (var kvp in caravanDepartureTicks.ToList())
            {
                Caravan caravan = kvp.Key;
                int departureTicks = kvp.Value;
                int ticksGone = Find.TickManager.TicksGame - departureTicks;

                if (ticksGone >= 360000) // 6 in-game days
                {
                    ApplyWorriedThoughtToHomeColonists(caravan);
                }
            }
        }

        private void ApplyWorriedThoughtToHomeColonists(Caravan caravan)
        {
            foreach (Map map in Find.Maps)
            {
                if (map.IsPlayerHome)
                {
                    foreach (Pawn pawn in map.mapPawns.FreeColonists)
                    {
                        if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                        {
                            continue; // Psychopaths don't get worried
                        }

                        var thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("HomeSweetHome_Thought_Worried"));
                        pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref caravanDepartureTicks, "caravanDepartureTicks", LookMode.Reference, LookMode.Value);
        }
    }
}
