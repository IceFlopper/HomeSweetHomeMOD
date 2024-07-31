using Verse;
using RimWorld;
using HomeSweetHome;
using RimWorld.Planet;

public static class CaravanReturnHandler
{
    public static void HandleCaravanReturn(Caravan caravan)
    {
        if (Patch_ExitMapAndCreateCaravan.CaravanDepartureTimes.TryGetValue(caravan, out int departureTicks))
        {
            int currentTicks = GenDate.TicksGame;
            int timeAway = currentTicks - departureTicks;
            int threeDaysTicks = 60000 * 3; // 3 days in ticks (assuming 1 day = 60000 ticks)

            if (timeAway >= threeDaysTicks)
            {
                ApplyMoodBuff(caravan);
            }

            // Remove the record as it's no longer needed
            Patch_ExitMapAndCreateCaravan.CaravanDepartureTimes.Remove(caravan);
        }
    }

    private static void ApplyMoodBuff(Caravan caravan)
    {
        foreach (Pawn pawn in caravan.pawns)
        {
            if (pawn.needs?.mood != null)
            {
                // Example of adding a mood buff
                HediffDef moodBuffDef = DefDatabase<HediffDef>.GetNamed("MyMoodBuff");
                pawn.needs.mood.thoughts.memories.TryGainMemory(moodBuffDef);
            }
        }
    }
}
