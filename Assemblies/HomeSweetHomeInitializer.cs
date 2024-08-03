using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace HomeSweetHome
{
    [StaticConstructorOnStartup]
    public static class HomeSweetHomeInitializer
    {
        static HomeSweetHomeInitializer()
        {
            var harmony = new Harmony("com.Lewkah0.rimworld.homesweethome");

            var classesToPatch = new List<Type>
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

            foreach (var cls in classesToPatch)
            {
                harmony.Patch(AccessTools.Method(cls, "Arrived"), postfix: new HarmonyMethod(typeof(Caravan_Arrive_Patch), nameof(Caravan_Arrive_Patch.Postfix)));
            }

            // Apply patches for caravan disappearance handling
            harmony.Patch(
                AccessTools.Method(typeof(Caravan), "RemovePawn"),
                postfix: new HarmonyMethod(typeof(Caravan_RemovePawn_Patch), nameof(Caravan_RemovePawn_Patch.Postfix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(WorldObject), "Destroy"),
                postfix: new HarmonyMethod(typeof(Caravan_Destroy_Patch), nameof(Caravan_Destroy_Patch.Postfix))
            );
        }
    }

    [HarmonyPatch(typeof(Caravan), "RemovePawn")]
    public static class Caravan_RemovePawn_Patch
    {
        public static void Postfix(Caravan __instance, Pawn p)
        {
            if (__instance?.Faction == Faction.OfPlayer && __instance.pawns.Count == 0) // Caravan is empty
            {
                var tracker = Find.World.GetComponent<CaravanDepartureTracker>();
                if (tracker != null)
                {
                    tracker.HandleCaravanDisappearance(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(WorldObject), "Destroy")]
    public static class Caravan_Destroy_Patch
    {
        public static void Postfix(WorldObject __instance)
        {
            if (__instance is Caravan caravan && caravan.Faction == Faction.OfPlayer)
            {
                var tracker = Find.World.GetComponent<CaravanDepartureTracker>();
                if (tracker != null)
                {
                    tracker.HandleCaravanDisappearance(caravan);
                }
            }
        }
    }
}
