using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using Verse;
using System.Reflection;
using System.Collections.Generic;

namespace HomeSweetHome
{
    // Static constructor for initializing Harmony patches
    [StaticConstructorOnStartup]
    public static class HomeSweetHomeInitializer
    {
        static HomeSweetHomeInitializer()
        {
            // Log.Message("HomeSweetHome: Initializing Harmony");
            var harmony = new Harmony("com.Lewkah0.rimworld.homesweethome");

            // List all classes that need patching
            var classesToPatch = new List<System.Type>
            {
                typeof(CaravanArrivalAction_Enter),
                typeof(CaravanArrivalAction_AttackSettlement),
                typeof(CaravanArrivalAction_OfferGifts),
                typeof(CaravanArrivalAction_VisitSettlement),
                typeof(CaravanArrivalAction_Trade),
                typeof(CaravanArrivalAction_VisitEscapeShip),
                typeof(CaravanArrivalAction_VisitPeaceTalks),
                typeof(CaravanArrivalAction_VisitSite)
            };

            // Patch each class's Arrived method
            foreach (var cls in classesToPatch)
            {
                harmony.Patch(AccessTools.Method(cls, "Arrived"), postfix: new HarmonyMethod(typeof(Caravan_Arrive_Patch), nameof(Caravan_Arrive_Patch.Postfix)));
            }
        }
    }

    public static class Caravan_Arrive_Patch
    {
        public static void Postfix(object __instance, Caravan caravan)
        {
            // Log.Message("HomeSweetHome: Arrived - Patch invoked.");

            if (caravan.Faction == Faction.OfPlayer)
            {
                // Log.Message("HomeSweetHome: Caravan belongs to the player.");

                // Check if the caravan has been gone for at least 120,000 ticks (2 in-game days)
                CaravanDepartureTracker tracker = Find.World.GetComponent<CaravanDepartureTracker>();
                if (tracker != null)
                {
                    int departureTicks = tracker.GetDepartureTicks(caravan);
                    // Log.Message($"HomeSweetHome: Departure ticks: {departureTicks}, Current ticks: {Find.TickManager.TicksGame}");

                    int ticksGone = Find.TickManager.TicksGame - departureTicks;
                    if (ticksGone >= 120000)
                    {
                        // Log.Message("HomeSweetHome: Caravan has been gone for at least 2 in-game days. Applying appropriate thought.");

                        // Using reflection to get mapParent from the __instance
                        FieldInfo mapParentField = __instance.GetType().GetField("mapParent", BindingFlags.NonPublic | BindingFlags.Instance);
                        MapParent mapParent = mapParentField != null ? (MapParent)mapParentField.GetValue(__instance) : null;

                        Map map = mapParent?.Map;

                        string thoughtDefName;
                        if (ticksGone >= 480000) // 8 in-game days
                        {
                            thoughtDefName = map != null && map.IsPlayerHome ? "HomeSweetHome_Thought_Great" : "HomeSweetHome_DestinationArrival_Thought_Great";
                        }
                        else
                        {
                            thoughtDefName = map != null && map.IsPlayerHome ? "HomeSweetHome_Thought" : "HomeSweetHome_DestinationArrival_Thought";
                        }

                        // Log.Message($"HomeSweetHome: Applying '{thoughtDefName}' thought.");
                        ApplyThought(caravan.PawnsListForReading, thoughtDefName);

                        if (map != null && map.IsPlayerHome)
                        {
                            ApplySocialThought(caravan.PawnsListForReading, map.mapPawns.FreeColonists, thoughtDefName);

                            // Remove the worried thought from home colonists
                            foreach (Pawn homePawn in map.mapPawns.FreeColonists)
                            {
                                Thought_Memory worriedThought = homePawn.needs.mood.thoughts.memories.Memories.FirstOrDefault(m => m.def == ThoughtDef.Named("HomeSweetHome_Thought_Worried")) as Thought_Memory;
                                if (worriedThought != null)
                                {
                                    homePawn.needs.mood.thoughts.memories.RemoveMemory(worriedThought);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Log.Message("HomeSweetHome: Caravan has not been gone for at least 2 in-game days. Skipping thought application.");
                    }
                }
                else
                {
                    // Log.Error("HomeSweetHome: Failed to get CaravanDepartureTracker component.");
                }
            }
            else
            {
                // Log.Message("HomeSweetHome: Caravan does not belong to player. Skipping thought application.");
            }
        }

        private static void ApplyThought(IEnumerable<Pawn> pawns, string thoughtDefName)
        {
            foreach (Pawn pawn in pawns)
            {
                // Log.Message($"HomeSweetHome: Checking pawn {pawn.Name.ToStringShort}...");

                if (pawn.IsColonist)
                {
                    // Log.Message($"HomeSweetHome: Pawn {pawn.Name.ToStringShort} is a colonist. Applying thought.");

                    // Check if the pawn has the Psychopath trait
                    if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
                    {
                        thoughtDefName += "_Psychopath";
                    }

                    var thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named(thoughtDefName));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought);

                    // Log.Message($"HomeSweetHome: Thought applied to {pawn.Name.ToStringShort}.");
                }
                else
                {
                    // Log.Message($"HomeSweetHome: Pawn {pawn.Name.ToStringShort} is not a colonist. Skipping thought application.");
                }
            }
        }

        private static void ApplySocialThought(IEnumerable<Pawn> caravanPawns, IEnumerable<Pawn> homePawns, string baseThoughtDefName)
        {
            foreach (Pawn homePawn in homePawns)
            {
                if (homePawn.IsColonist)
                {
                    bool likesAll = true;
                    bool likesAny = false;

                    foreach (Pawn caravanPawn in caravanPawns)
                    {
                        int opinion = homePawn.relations.OpinionOf(caravanPawn);

                        if (opinion >= 0)
                        {
                            likesAny = true;
                        }
                        else
                        {
                            likesAll = false;
                        }
                    }

                    string thoughtDefName;

                    if (likesAll)
                    {
                        thoughtDefName = baseThoughtDefName; // Positive mood already applied
                        // Log.Message($"HomeSweetHome: {homePawn.Name.ToStringShort} likes all returning members. Keeping positive mood.");
                    }
                    else if (likesAny)
                    {
                        thoughtDefName = "HomeSweetHome_Thought_Neutral"; // Mixed feelings
                        // Log.Message($"HomeSweetHome: {homePawn.Name.ToStringShort} has mixed feelings. Applying neutral mood.");
                    }
                    else
                    {
                        thoughtDefName = "HomeSweetHome_Thought_Dislike"; // Negative mood
                        // Log.Message($"HomeSweetHome: {homePawn.Name.ToStringShort} dislikes all returning members. Applying negative mood.");
                    }

                    var thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named(thoughtDefName));
                    homePawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                }
            }
        }
    }
}
