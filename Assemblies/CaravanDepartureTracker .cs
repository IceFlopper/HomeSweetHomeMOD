using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HomeSweetHome
{
    public class CaravanDepartureTracker : WorldComponent
    {
        private Dictionary<Caravan, int> caravanDepartureTicks = new Dictionary<Caravan, int>();
        private HashSet<Caravan> playerCaravans = new HashSet<Caravan>();
        private HashSet<Caravan> arrivedCaravans = new HashSet<Caravan>();

        public CaravanDepartureTracker(World world) : base(world) { }

        public void SetDepartureTicks(Caravan caravan, int ticks)
        {
            if (!caravanDepartureTicks.ContainsKey(caravan))
            {
                caravanDepartureTicks[caravan] = ticks;
                if (caravan.Faction == Faction.OfPlayer)
                {
                    playerCaravans.Add(caravan);
                }
            }
        }

        public int GetDepartureTicks(Caravan caravan)
        {
            return caravanDepartureTicks.TryGetValue(caravan, out int ticks) ? ticks : 0;
        }

        public void MarkCaravanAsReturned(Caravan caravan)
        {
            if (playerCaravans.Contains(caravan))
            {
                playerCaravans.Remove(caravan);
            }
        }

        public void MarkCaravanAsArrived(Caravan caravan)
        {
            if (!arrivedCaravans.Contains(caravan))
            {
                arrivedCaravans.Add(caravan);
            }
        }

        public void HandleCaravanDisappearance(Caravan caravan)
        {
            if (playerCaravans.Contains(caravan) && !arrivedCaravans.Contains(caravan))
            {
                if (HomeSweetHome.settings.enableWorriedThought)
                {
                    RemoveWorriedThought(Find.Maps.Where(map => map.IsPlayerHome).SelectMany(map => map.mapPawns.FreeColonists));
                }
                playerCaravans.Remove(caravan);
                caravanDepartureTicks.Remove(caravan);
            }
        }

        private void RemoveWorriedThought(IEnumerable<Pawn> pawns)
        {
            foreach (Pawn pawn in pawns)
            {
                var worriedThoughts = pawn.needs.mood.thoughts.memories.Memories
                    .Where(m => m.def == ThoughtDef.Named("HomeSweetHome_Thought_Worried")).ToList();
                foreach (var worriedThought in worriedThoughts)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemory(worriedThought);
                }
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            CheckForWorriedColonists();
        }

        private void CheckForWorriedColonists()
        {
            if (!HomeSweetHome.settings.enableWorriedThought)
                return;

            foreach (var kvp in caravanDepartureTicks.ToList())
            {
                Caravan caravan = kvp.Key;
                int departureTicks = kvp.Value;
                int ticksGone = Find.TickManager.TicksGame - departureTicks;

                if (ticksGone >= 360000 && playerCaravans.Contains(caravan)) // 6 in-game days
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
                        if (!pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                        {
                            var thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("HomeSweetHome_Thought_Worried"));
                            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref caravanDepartureTicks, "caravanDepartureTicks", LookMode.Reference, LookMode.Value);
            Scribe_Collections.Look(ref playerCaravans, "playerCaravans", LookMode.Reference);
            Scribe_Collections.Look(ref arrivedCaravans, "arrivedCaravans", LookMode.Reference);
        }
    }
}
