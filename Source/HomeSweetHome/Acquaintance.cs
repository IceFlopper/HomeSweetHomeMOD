using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace HomeSweetHome
{
    /// <summary>
    /// Remembers when each colonist turned up, so the mod can tell the difference between
    /// somebody you have lived alongside for a season and somebody who was recruited the day
    /// before yesterday and has never once laid eyes on the caravan that left last month.
    ///
    /// Opinion on its own is no use here: a pretty face, a transhumanist's approval of an
    /// implant or a shared ideoligion will push a stranger past vanilla's friend threshold
    /// without the two of them ever having spoken.
    /// </summary>
    public class Acquaintance : IExposable
    {
        private Dictionary<Pawn, int> joinedTick = new Dictionary<Pawn, int>();

        private List<Pawn> scribePawns;
        private List<int> scribeTicks;
        private readonly List<Pawn> stale = new List<Pawn>();

        /// <summary>The tick this pawn first showed up as one of ours, seeded from their records the first time we see them.</summary>
        public int JoinedTickOf(Pawn pawn, int now)
        {
            if (joinedTick.TryGetValue(pawn, out int tick))
            {
                return tick;
            }

            tick = EstimateJoinedTick(pawn, now);
            joinedTick[pawn] = tick;
            return tick;
        }

        /// <summary>
        /// Nobody can have joined the colony after they set off from it. A pawn who was already
        /// out when the mod first laid eyes on them gets guessed at from their records, and that
        /// guess can land too late; this pulls it back to something possible.
        /// </summary>
        public void NoLaterThan(Pawn pawn, int tick, int now)
        {
            if (JoinedTickOf(pawn, now) > tick)
            {
                joinedTick[pawn] = tick;
            }
        }

        /// <summary>How long the two of them had both been in the colony as of <paramref name="asOfTick"/>.</summary>
        public float SharedDays(Pawn a, Pawn b, int asOfTick, int now)
        {
            int together = asOfTick - Mathf.Max(JoinedTickOf(a, now), JoinedTickOf(b, now));
            return together <= 0 ? 0f : together / (float)GenDate.TicksPerDay;
        }

        /// <summary>
        /// 0 when they had never met, 1 once they have been around each other long enough for it
        /// to mean something, ramping in between. Everything the mod hands out about one colonist
        /// to another is scaled by this.
        /// </summary>
        public float Familiarity(Pawn a, Pawn b, int asOfTick, int now)
        {
            float shared = SharedDays(a, b, asOfTick, now);
            if (shared <= 0f)
            {
                return 0f;
            }

            float needed = HomeSweetHomeMod.Settings.familiarDays;
            if (needed <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(shared / needed);
        }

        /// <summary>Drops people who are gone for good so the table doesn't grow forever.</summary>
        public void Prune()
        {
            if (joinedTick.Count < 64)
            {
                return;
            }

            stale.Clear();
            foreach (KeyValuePair<Pawn, int> entry in joinedTick)
            {
                if (IsGone(entry.Key))
                {
                    stale.Add(entry.Key);
                }
            }

            for (int i = 0; i < stale.Count; i++)
            {
                joinedTick.Remove(stale[i]);
            }
            stale.Clear();
        }

        /// <summary>
        /// First sighting of somebody who was already here — on an old save, or on the sweep after
        /// the mod was added. Their records know how long they've been a colonist, so use that
        /// rather than pretending everybody arrived this morning.
        /// </summary>
        private static int EstimateJoinedTick(Pawn pawn, int now)
        {
            float ticksAsColonist = pawn.records?.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal) ?? 0f;
            return Mathf.Clamp(now - Mathf.RoundToInt(ticksAsColonist), 0, now);
        }

        /// <summary>True once a reference to this pawn would no longer survive a save.</summary>
        private static bool IsGone(Pawn pawn)
        {
            return pawn == null || pawn.Discarded || pawn.Destroyed;
        }

        // Saved as two parallel lists rather than a dictionary: a pawn who no longer exists comes
        // back as a null key, which a dictionary handles a lot less gracefully than a list does.
        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                scribePawns = new List<Pawn>(joinedTick.Count);
                scribeTicks = new List<int>(joinedTick.Count);
                foreach (KeyValuePair<Pawn, int> entry in joinedTick)
                {
                    if (IsGone(entry.Key))
                    {
                        continue;
                    }
                    scribePawns.Add(entry.Key);
                    scribeTicks.Add(entry.Value);
                }
            }

            Scribe_Collections.Look(ref scribePawns, "pawns", LookMode.Reference);
            Scribe_Collections.Look(ref scribeTicks, "ticks", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                joinedTick.Clear();
                if (scribePawns != null && scribeTicks != null)
                {
                    int count = Mathf.Min(scribePawns.Count, scribeTicks.Count);
                    for (int i = 0; i < count; i++)
                    {
                        if (scribePawns[i] != null)
                        {
                            joinedTick[scribePawns[i]] = scribeTicks[i];
                        }
                    }
                }
                scribePawns = null;
                scribeTicks = null;
            }
        }
    }
}
