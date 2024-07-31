using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;
using System.Collections.Generic;

namespace HomeSweetHome
{
    public class HomeSweetHome : Mod
    {
        public HomeSweetHome(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Initialize, "Initializing HomeSweetHome", false, null);
        }

        private void Initialize()
        {
            // Subscribe to the necessary game events
            CaravanFormedUtility.CaravanFormed += OnCaravanFormed;
            CaravanArrivalUtility.CaravanArrived += OnCaravanArrived;
        }

        private void OnCaravanFormed(Caravan caravan)
        {
            foreach (var pawn in caravan.PawnsListForReading)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("MissedCaravanMember"));
            }
        }

        private void OnCaravanArrived(Caravan caravan)
        {
            foreach (var pawn in caravan.PawnsListForReading)
            {
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("MissedCaravanMember"));
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("CaravanReturned"));
            }

            foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
            {
                if (!caravan.PawnsListForReading.Contains(pawn))
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("MissedCaravanMember"));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("CaravanReturned"));
                }
            }
        }
    }
}
