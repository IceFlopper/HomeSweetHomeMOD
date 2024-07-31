using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using System.Reflection;

namespace HomeSweetHome
{
    // This patch records the departure time of a caravan.
    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan")]
    public static class Patch_ExitMapAndCreateCaravan
    {
        public static Dictionary<Caravan, int> CaravanDepartureTimes = new Dictionary<Caravan, int>();

        [HarmonyPostfix]
        public static void RecordDepartureTime(Caravan __result)
        {
            if (__result != null)
            {
                // Record the departure time in ticks.
                CaravanDepartureTimes[__result] = Find.TickManager.TicksGame;
            }
        }
    }

    // This patch applies the "Home Sweet Home" thought when a caravan arrives.
    [HarmonyPatch(typeof(CaravanArrivalAction_Enter), "Arrived")]
    public static class Caravan_ArrivalAction_Enter_Arrived_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(CaravanArrivalAction_Enter __instance, Caravan caravan)
        {
            Log.Message("HomeSweetHome: Caravan arrived. Checking faction...");

            if (caravan.Faction == Faction.OfPlayer)
            {
                Log.Message("HomeSweetHome: Caravan belongs to player.");

                if (Patch_ExitMapAndCreateCaravan.CaravanDepartureTimes.TryGetValue(caravan, out int departureTicks))
                {
                    int currentTicks = Find.TickManager.TicksGame;
                    int timeAway = currentTicks - departureTicks;
                    int threeDaysTicks = 60000 * 3; // 3 days in ticks

                    Log.Message($"HomeSweetHome: Caravan was away for {timeAway} ticks.");

                    if (timeAway >= threeDaysTicks)
                    {
                        Log.Message("HomeSweetHome: Caravan was away for 3 or more days. Applying Home Sweet Home thought to caravan members.");
                        ApplyHomeSweetHomeThought(caravan.PawnsListForReading);
                    }

                    // Remove the record as it's no longer needed.
                    Patch_ExitMapAndCreateCaravan.CaravanDepartureTimes.Remove(caravan);
                }
                else
                {
                    Log.Message("HomeSweetHome: No departure record found for this caravan.");
                }

                // Reflectively access and process the map for home colonists.
                FieldInfo mapParentField = typeof(CaravanArrivalAction_Enter).GetField("mapParent", BindingFlags.NonPublic | BindingFlags.Instance);
                if (mapParentField != null)
                {
                    MapParent mapParent = (MapParent)mapParentField.GetValue(__instance);
                    if (mapParent != null)
                    {
                        Map map = mapParent.Map;
                        if (map != null)
                        {
                            Log.Message("HomeSweetHome: Applying Home Sweet Home thought to all colonists at home map.");
                            ApplyHomeSweetHomeThought(map.mapPawns.FreeColonists);
                        }
                    }
                }
                else
                {
                    Log.Error("HomeSweetHome: Failed to reflectively access 'mapParent' field.");
                }
            }
            else
            {
                Log.Message("HomeSweetHome: Caravan does not belong to player. Skipping thought application.");
            }
        }

        private static void ApplyHomeSweetHomeThought(IEnumerable<Pawn> pawns)
        {
            foreach (Pawn pawn in pawns)
            {
                Log.Message($"HomeSweetHome: Checking pawn {pawn.Name.ToStringShort}...");

                if (pawn.IsColonist)
                {
                    Log.Message($"HomeSweetHome: Pawn {pawn.Name.ToStringShort} is a colonist. Applying thought.");

                    var thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("HomeSweetHome_Thought"));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought);

                    Log.Message($"HomeSweetHome: Thought applied to {pawn.Name.ToStringShort}.");
                }
                else
                {
                    Log.Message($"HomeSweetHome: Pawn {pawn.Name.ToStringShort} is not a colonist. Skipping thought application.");
                }
            }
        }
    }
}
