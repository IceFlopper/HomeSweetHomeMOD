using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace HomeSweetHome
{
    /// <summary>
    /// All the fiddly memory bookkeeping lives here. The separation and companionship thoughts
    /// are ongoing states rather than one-off events, so they get refreshed on every sweep and
    /// wiped the moment the situation ends.
    /// </summary>
    public static class ThoughtOps
    {
        private static readonly List<Thought_Memory> scratch = new List<Thought_Memory>();

        private static ThoughtDef[] separationThoughts;
        private static ThoughtDef[] companionThoughts;

        public static ThoughtDef[] SeparationThoughts => separationThoughts ?? (separationThoughts = new[]
        {
            HSHThoughtDefOf.HSH_AwayPartner,
            HSHThoughtDefOf.HSH_AwayChild,
            HSHThoughtDefOf.HSH_AwayParent,
            HSHThoughtDefOf.HSH_AwaySibling,
            HSHThoughtDefOf.HSH_AwayRelative,
            HSHThoughtDefOf.HSH_AwayCloseFriend,
            HSHThoughtDefOf.HSH_AwayFriend,
            HSHThoughtDefOf.HSH_AwayRival,
            HSHThoughtDefOf.HSH_AwayEnemy
        });

        public static ThoughtDef[] CompanionThoughts => companionThoughts ?? (companionThoughts = new[]
        {
            HSHThoughtDefOf.HSH_RoadPartner,
            HSHThoughtDefOf.HSH_RoadFamily,
            HSHThoughtDefOf.HSH_RoadFriend,
            HSHThoughtDefOf.HSH_RoadRival,
            HSHThoughtDefOf.HSH_RoadEnemy
        });

        public static MemoryThoughtHandler MemoriesOf(Pawn pawn)
        {
            return pawn?.needs?.mood?.thoughts?.memories;
        }

        // ---------------------------------------------------------------- separation

        public static ThoughtDef SeparationThoughtFor(Bond bond)
        {
            switch (bond)
            {
                case Bond.Partner: return HSHThoughtDefOf.HSH_AwayPartner;
                case Bond.Child: return HSHThoughtDefOf.HSH_AwayChild;
                case Bond.Parent: return HSHThoughtDefOf.HSH_AwayParent;
                case Bond.Sibling: return HSHThoughtDefOf.HSH_AwaySibling;
                case Bond.Relative: return HSHThoughtDefOf.HSH_AwayRelative;
                case Bond.CloseFriend: return HSHThoughtDefOf.HSH_AwayCloseFriend;
                case Bond.Friend: return HSHThoughtDefOf.HSH_AwayFriend;
                case Bond.Rival: return HSHThoughtDefOf.HSH_AwayRival;
                case Bond.Enemy: return HSHThoughtDefOf.HSH_AwayEnemy;
                default: return null;
            }
        }

        public static ThoughtDef ReunionThoughtFor(Bond bond)
        {
            switch (bond)
            {
                case Bond.Partner: return HSHThoughtDefOf.HSH_ReturnedPartner;
                case Bond.Child:
                case Bond.Parent:
                case Bond.Sibling:
                case Bond.Relative: return HSHThoughtDefOf.HSH_ReturnedFamily;
                case Bond.CloseFriend:
                case Bond.Friend: return HSHThoughtDefOf.HSH_ReturnedFriend;
                case Bond.Rival: return HSHThoughtDefOf.HSH_ReturnedRival;
                case Bond.Enemy: return HSHThoughtDefOf.HSH_ReturnedEnemy;
                default: return null;
            }
        }

        public static ThoughtDef CompanionThoughtFor(Bond bond)
        {
            switch (bond)
            {
                case Bond.Partner: return HSHThoughtDefOf.HSH_RoadPartner;
                case Bond.Child:
                case Bond.Parent:
                case Bond.Sibling:
                case Bond.Relative: return HSHThoughtDefOf.HSH_RoadFamily;
                case Bond.CloseFriend:
                case Bond.Friend: return HSHThoughtDefOf.HSH_RoadFriend;
                case Bond.Rival: return HSHThoughtDefOf.HSH_RoadRival;
                case Bond.Enemy: return HSHThoughtDefOf.HSH_RoadEnemy;
                default: return null;
            }
        }

        // ---------------------------------------------------------------- applying

        /// <summary>
        /// Keeps exactly one memory of the given family pointed at <paramref name="subject"/>, at the
        /// requested stage and strength. Swaps the def out if the relationship changed while they were gone.
        /// </summary>
        public static void SetOngoing(Pawn observer, Pawn subject, ThoughtDef[] family, ThoughtDef def, int stage, float power)
        {
            MemoryThoughtHandler memories = MemoriesOf(observer);
            if (memories == null || def == null)
            {
                return;
            }

            Thought_Memory existing = FindMemory(memories, family, subject);
            if (existing != null)
            {
                if (existing.def == def)
                {
                    existing.SetForcedStage(stage);
                    existing.moodPowerFactor = power;
                    existing.age = 0;
                    return;
                }
                memories.RemoveMemory(existing);
            }

            Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(def);
            thought.SetForcedStage(stage);
            thought.moodPowerFactor = power;
            memories.TryGainMemory(thought, subject);
        }

        public static void SetOngoing(Pawn pawn, ThoughtDef def, int stage, float power)
        {
            MemoryThoughtHandler memories = MemoriesOf(pawn);
            if (memories == null || def == null)
            {
                return;
            }

            Thought_Memory existing = memories.GetFirstMemoryOfDef(def);
            if (existing != null)
            {
                existing.SetForcedStage(stage);
                existing.moodPowerFactor = power;
                existing.age = 0;
                return;
            }

            Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(def);
            thought.SetForcedStage(stage);
            thought.moodPowerFactor = power;
            memories.TryGainMemory(thought, null);
        }

        public static void GiveOneShot(Pawn pawn, ThoughtDef def, int stage, float power, Pawn about = null)
        {
            MemoryThoughtHandler memories = MemoriesOf(pawn);
            if (memories == null || def == null || power <= 0.01f)
            {
                return;
            }

            Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(def);
            thought.SetForcedStage(stage);
            thought.moodPowerFactor = power;
            memories.TryGainMemory(thought, about);
        }

        // ---------------------------------------------------------------- clearing

        public static void ClearSeparation(Pawn observer, Pawn subject)
        {
            Remove(MemoriesOf(observer), SeparationThoughts, subject);
        }

        public static void ClearCompanionship(Pawn pawn, Pawn companion)
        {
            Remove(MemoriesOf(pawn), CompanionThoughts, companion);
        }

        public static void ClearAllCompanionship(Pawn pawn)
        {
            Remove(MemoriesOf(pawn), CompanionThoughts, null);
        }

        public static void ClearTravelState(Pawn pawn)
        {
            MemoryThoughtHandler memories = MemoriesOf(pawn);
            if (memories == null)
            {
                return;
            }
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_Homesick);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_MadeCamp);
            Remove(memories, CompanionThoughts, null);
        }

        /// <summary>Drops every trace of this mod from a pawn. Used when the player switches it off mid-game.</summary>
        public static void ClearEverything(Pawn pawn)
        {
            MemoryThoughtHandler memories = MemoriesOf(pawn);
            if (memories == null)
            {
                return;
            }

            Remove(memories, SeparationThoughts, null);
            Remove(memories, CompanionThoughts, null);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_Homesick);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_MadeCamp);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_HomeAtLast);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_ReachedDestination);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_ReturnedPartner);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_ReturnedFamily);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_ReturnedFriend);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_ReturnedRival);
            memories.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_ReturnedEnemy);
        }

        private static Thought_Memory FindMemory(MemoryThoughtHandler memories, ThoughtDef[] family, Pawn subject)
        {
            List<Thought_Memory> all = memories.Memories;
            for (int i = 0; i < all.Count; i++)
            {
                Thought_Memory memory = all[i];
                if (memory.otherPawn != subject)
                {
                    continue;
                }
                for (int j = 0; j < family.Length; j++)
                {
                    if (memory.def == family[j])
                    {
                        return memory;
                    }
                }
            }
            return null;
        }

        /// <summary>Removes every memory in <paramref name="family"/>, or only those about <paramref name="subject"/> when one is given.</summary>
        private static void Remove(MemoryThoughtHandler memories, ThoughtDef[] family, Pawn subject)
        {
            if (memories == null)
            {
                return;
            }

            scratch.Clear();
            List<Thought_Memory> all = memories.Memories;
            for (int i = 0; i < all.Count; i++)
            {
                Thought_Memory memory = all[i];
                if (subject != null && memory.otherPawn != subject)
                {
                    continue;
                }
                for (int j = 0; j < family.Length; j++)
                {
                    if (memory.def == family[j])
                    {
                        scratch.Add(memory);
                        break;
                    }
                }
            }

            for (int i = 0; i < scratch.Count; i++)
            {
                memories.RemoveMemory(scratch[i]);
            }
            scratch.Clear();
        }

        // ---------------------------------------------------------------- staging

        /// <summary>Four bands: just left, a few days, over a week, long enough to worry.</summary>
        public static int StageForAbsence(float days)
        {
            if (days >= 15f) return 3;
            if (days >= 7f) return 2;
            if (days >= 3f) return 1;
            return 0;
        }

        /// <summary>Three bands for anything that fires once, keyed off how long the trip ran.</summary>
        public static int StageForTrip(float days)
        {
            if (days >= 12f) return 2;
            if (days >= 5f) return 1;
            return 0;
        }

        public static float ClampPower(float power)
        {
            return Mathf.Clamp(power, 0f, 4f);
        }
    }
}
