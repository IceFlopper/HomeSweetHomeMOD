using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace HomeSweetHome
{
    public static class Caravan_Arrive_Patch
    {
        public static void Postfix(object __instance, Caravan caravan)
        {
            if (caravan?.Faction == Faction.OfPlayer)
            {
                CaravanDepartureTracker tracker = Find.World.GetComponent<CaravanDepartureTracker>();
                if (tracker != null)
                {
                    int departureTicks = tracker.GetDepartureTicks(caravan);
                    int ticksGone = Find.TickManager.TicksGame - departureTicks;
                    if (ticksGone >= 120000)
                    {
                        FieldInfo mapParentField = __instance.GetType().GetField("mapParent", BindingFlags.NonPublic | BindingFlags.Instance);
                        MapParent mapParent = mapParentField != null ? (MapParent)mapParentField.GetValue(__instance) : null;
                        Map map = mapParent?.Map;

                        string thoughtDefName = GetThoughtDefName(ticksGone, map);

                        ApplyThought(caravan.PawnsListForReading, thoughtDefName);

                        if (map != null && map.IsPlayerHome)
                        {
                            ApplySocialThought(caravan.PawnsListForReading, map.mapPawns.FreeColonists, thoughtDefName);
                            RemoveWorriedThought(map.mapPawns.FreeColonists);
                        }

                        // Ensure worried thoughts are removed from all returning pawns
                        RemoveWorriedThought(caravan.PawnsListForReading);

                        // Mark the caravan as returned
                        tracker.MarkCaravanAsReturned(caravan);

                        // Mark the caravan as arrived successfully
                        tracker.MarkCaravanAsArrived(caravan);
                    }
                }
            }
        }

        private static string GetThoughtDefName(int ticksGone, Map map)
        {
            if (ticksGone >= 480000) // 8 in-game days
            {
                return map != null && map.IsPlayerHome ? "HomeSweetHome_Thought_Great" : "HomeSweetHome_DestinationArrival_Thought_Great";
            }
            else
            {
                return map != null && map.IsPlayerHome ? "HomeSweetHome_Thought" : "HomeSweetHome_DestinationArrival_Thought";
            }
        }

        private static void ApplyThought(IEnumerable<Pawn> pawns, string thoughtDefName)
        {
            foreach (Pawn pawn in pawns)
            {
                if (pawn.IsColonist)
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                    {
                        thoughtDefName += "_Psychopath";
                    }

                    var thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named(thoughtDefName));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                }
            }
        }

        private static void ApplySocialThought(IEnumerable<Pawn> caravanPawns, IEnumerable<Pawn> homePawns, string baseThoughtDefName)
        {
            foreach (Pawn homePawn in homePawns)
            {
                if (homePawn.IsColonist)
                {
                    bool likesAll = caravanPawns.All(caravanPawn => homePawn.relations.OpinionOf(caravanPawn) >= 0);
                    bool likesAny = caravanPawns.Any(caravanPawn => homePawn.relations.OpinionOf(caravanPawn) >= 0);

                    string thoughtDefName = likesAll ? baseThoughtDefName :
                                             likesAny ? "HomeSweetHome_Thought_Neutral" : "HomeSweetHome_Thought_Dislike";

                    var thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named(thoughtDefName));
                    homePawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                }
            }
        }

        private static void RemoveWorriedThought(IEnumerable<Pawn> pawns)
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
    }
}
