using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using Verse;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace HomeSweetHome
{
    [HarmonyPatch(typeof(CaravanExitMapUtility))]
    [HarmonyPatch("ExitMapAndCreateCaravan")]
    [HarmonyPatch(new Type[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(int), typeof(int), typeof(bool) })] // Adjust the parameter types as necessary
    public static class CaravanExitMapUtility_ExitMapAndCreateCaravan_Patch
    {
        public static void Postfix(Caravan __result)
        {
            if (__result.Faction == Faction.OfPlayer)
            {
                __result.GetComponent<CaravanDepartureTimeTracker>().SetDepartureTime(Find.TickManager.TicksGame);
            }
        }
    }

    [HarmonyPatch(typeof(CaravanArrivalAction_Enter), "Arrived")]
    public static class Caravan_ArrivalAction_Enter_Arrived_Patch
    {
        public static void Postfix(CaravanArrivalAction_Enter __instance, Caravan caravan)
        {
            Log.Message("HomeSweetHome: Caravan arrived. Checking faction...");

            if (caravan.Faction == Faction.OfPlayer)
            {
                int departureTime = caravan.GetComponent<CaravanDepartureTimeTracker>().GetDepartureTime();
                int currentTime = Find.TickManager.TicksGame;
                int timeGone = currentTime - departureTime;
                int threeDaysInTicks = 3 * 60000; // 3 days * 60000 ticks/day

                if (timeGone >= threeDaysInTicks)
                {
                    Log.Message("HomeSweetHome: Caravan was gone for at least 3 days. Applying Home Sweet Home thought to caravan members.");
                    ApplyHomeSweetHomeThought(caravan.PawnsListForReading);

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
                    Log.Message("HomeSweetHome: Caravan was not gone for at least 3 days. Skipping thought application.");
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

    public class CaravanDepartureTimeTracker : WorldObjectComp
    {
        private int departureTime;

        public void SetDepartureTime(int time)
        {
            departureTime = time;
        }

        public int GetDepartureTime()
        {
            return departureTime;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref departureTime, "departureTime", 0);
        }
    }

    public class CaravanDepartureTimeTrackerCompProperties : WorldObjectCompProperties
    {
        public CaravanDepartureTimeTrackerCompProperties()
        {
            this.compClass = typeof(CaravanDepartureTimeTracker);
        }
    }
}
