using HarmonyLib;
using RimWorld;
using Verse;

namespace HomeSweetHome
{
    [HarmonyPatch(typeof(CaravanFormingUtility), "FormAndCreateCaravan")]
    public static class Patch_CaravanFormingUtility_FormAndCreateCaravan
    {
        public static void Postfix(Caravan __result)
        {
            if (__result.IsPlayerControlled)
            {
                foreach (var pawn in __result.PawnsListForReading)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("MissedCaravanMember"));
                    Log.Message($"[HomeSweetHome] Applied 'MissedCaravanMember' mood to pawn {pawn.Label}");
                }
            }
        }
    }

    [HarmonyPatch(typeof(CaravanEnterMapUtility), "Enter")]
    public static class Patch_CaravanEnterMapUtility_Enter
    {
        public static void Postfix(Caravan caravan)
        {
            if (caravan.IsPlayerControlled)
            {
                foreach (var pawn in caravan.PawnsListForReading)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("MissedCaravanMember"));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("CaravanReturned"));
                    Log.Message($"[HomeSweetHome] Applied 'CaravanReturned' mood to pawn {pawn.Label}");
                }

                foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
                {
                    if (!caravan.PawnsListForReading.Contains(pawn))
                    {
                        pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("MissedCaravanMember"));
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("CaravanReturned"));
                        Log.Message($"[HomeSweetHome] Applied 'CaravanReturned' mood to pawn {pawn.Label}");
                    }
                }
            }
        }
    }
}
