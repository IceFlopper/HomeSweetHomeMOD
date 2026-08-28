using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HomeSweetHome
{
    /// <summary>
    /// Sweeps the colony every so often, notices who left and who came back, and keeps the
    /// mood thoughts in step. Nothing here is patched into the game: it reads positions, which
    /// means caravans, vehicles, shuttles, outposts and camps all work without special cases.
    /// </summary>
    public class AbsenceTracker : WorldComponent
    {
        private const int SweepInterval = 500;

        // How long a colonist can drop off the roster before we assume they aren't coming back.
        // Long enough to cover map generation and pod flights, short enough that a captured
        // pawn stops haunting the colony forever.
        private const int MissingGraceTicks = 30000;

        private List<Journey> journeys = new List<Journey>();
        private Acquaintance acquaintance = new Acquaintance();

        private readonly Dictionary<Pawn, Whereabouts> placeNow = new Dictionary<Pawn, Whereabouts>();
        private readonly Dictionary<Pawn, object> groupNow = new Dictionary<Pawn, object>();
        private readonly List<Pawn> roster = new List<Pawn>();
        private readonly List<Journey> travelling = new List<Journey>();

        public AbsenceTracker(World world) : base(world)
        {
        }

        public IReadOnlyList<Journey> Journeys => journeys;

        public override void WorldComponentTick()
        {
            if (Find.TickManager.TicksGame % SweepInterval != 0)
            {
                return;
            }

            if (!HomeSweetHomeMod.Settings.enabled)
            {
                return;
            }

            Sweep();
        }

        private void Sweep()
        {
            HomeSweetHomeSettings settings = HomeSweetHomeMod.Settings;
            int now = Find.TickManager.TicksGame;

            TakeRoster(now);
            UpdateJourneys(now);
            ApplySeparation(settings, now);
            ApplyTravelState(settings, now);
            acquaintance.Prune();
        }

        /// <summary>Snapshot of every colonist we can still account for, and where they are.</summary>
        private void TakeRoster(int now)
        {
            roster.Clear();
            placeNow.Clear();
            groupNow.Clear();

            List<Pawn> colonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoCryptosleep;
            for (int i = 0; i < colonists.Count; i++)
            {
                Pawn pawn = colonists[i];
                if (pawn.needs?.mood == null)
                {
                    continue;
                }

                Whereabouts place = WhereaboutsUtility.Of(pawn);
                if (place == Whereabouts.Unknown)
                {
                    continue;
                }

                roster.Add(pawn);
                acquaintance.JoinedTickOf(pawn, now);
                placeNow[pawn] = place;
                groupNow[pawn] = WhereaboutsUtility.GroupKeyOf(pawn);
            }
        }

        private void UpdateJourneys(int now)
        {
            travelling.Clear();

            for (int i = journeys.Count - 1; i >= 0; i--)
            {
                Journey journey = journeys[i];

                if (journey.pawn == null || journey.pawn.Dead || journey.pawn.Destroyed)
                {
                    Abandon(journey);
                    journeys.RemoveAt(i);
                    continue;
                }

                if (!placeNow.TryGetValue(journey.pawn, out Whereabouts place))
                {
                    // Off the roster: mid-transition, kidnapped, or gone for good.
                    if (now - journey.lastSeenTick > MissingGraceTicks)
                    {
                        Abandon(journey);
                        journeys.RemoveAt(i);
                    }
                    continue;
                }

                journey.lastSeenTick = now;

                if (place == Whereabouts.Home)
                {
                    Welcome(journey, now);
                    journeys.RemoveAt(i);
                    continue;
                }

                if (place == Whereabouts.Camp)
                {
                    journey.campTicks += SweepInterval;
                }
                else if (!journey.arrived && journey.pawn.MapHeld != null && journey.Counts)
                {
                    // Off the world map and standing on whatever they set out for.
                    journey.arrived = true;
                    NoteArrival(journey);
                }

                journey.place = place;
                travelling.Add(journey);
            }

            // Anybody away who isn't already on the books has just set off.
            for (int i = 0; i < roster.Count; i++)
            {
                Pawn pawn = roster[i];
                if (placeNow[pawn] == Whereabouts.Home || HasJourney(pawn))
                {
                    continue;
                }

                Journey journey = new Journey(pawn, now)
                {
                    place = placeNow[pawn]
                };
                journeys.Add(journey);
                travelling.Add(journey);
            }
        }

        private bool HasJourney(Pawn pawn)
        {
            for (int i = 0; i < journeys.Count; i++)
            {
                if (journeys[i].pawn == pawn)
                {
                    return true;
                }
            }
            return false;
        }

        // ------------------------------------------------------------ separation

        /// <summary>
        /// For every colonist who has been away long enough to notice, tell everyone who isn't
        /// with them how they feel about it.
        /// </summary>
        private void ApplySeparation(HomeSweetHomeSettings settings, int now)
        {
            if (!settings.separationThoughts)
            {
                return;
            }

            for (int i = 0; i < travelling.Count; i++)
            {
                Journey journey = travelling[i];
                Pawn absent = journey.pawn;

                if (!journey.Counts)
                {
                    // Not long enough yet. Make sure nothing lingers from a previous trip.
                    ForgetAbout(absent);
                    continue;
                }

                if (journey.place == Whereabouts.Camp && !settings.campingCountsAsAway)
                {
                    ForgetAbout(absent);
                    continue;
                }

                acquaintance.NoLaterThan(absent, journey.departedTick, now);

                int stage = ThoughtOps.StageForAbsence(journey.DaysGone);
                object absentGroup = groupNow.TryGetValue(absent, out object key) ? key : null;

                for (int j = 0; j < roster.Count; j++)
                {
                    Pawn observer = roster[j];
                    if (observer == absent)
                    {
                        continue;
                    }

                    // Travelling together means nobody is missing anybody.
                    if (absentGroup != null && groupNow.TryGetValue(observer, out object observerGroup) && ReferenceEquals(observerGroup, absentGroup))
                    {
                        ThoughtOps.ClearSeparation(observer, absent);
                        continue;
                    }

                    ApplySeparationBetween(observer, absent, stage, settings, journey.departedTick, now);
                }
            }
        }

        private void ApplySeparationBetween(Pawn observer, Pawn absent, int stage, HomeSweetHomeSettings settings, int departedTick, int now)
        {
            // Somebody who joined after the caravan rolled out has never met these people, and
            // somebody recruited the day before it left barely has. Neither should be pining.
            float familiarity = acquaintance.Familiarity(observer, absent, departedTick, now);
            if (familiarity <= 0f)
            {
                ThoughtOps.ClearSeparation(observer, absent);
                return;
            }

            Bond bond = Bonds.Between(observer, absent);
            ThoughtDef def = ThoughtOps.SeparationThoughtFor(bond);
            if (def == null)
            {
                ThoughtOps.ClearSeparation(observer, absent);
                return;
            }

            if (Bonds.IsNegative(bond) && !settings.gloatingThoughts)
            {
                ThoughtOps.ClearSeparation(observer, absent);
                return;
            }

            Reaction reaction = TraitProfiles.For(observer);
            float traitFactor = Bonds.IsNegative(bond) ? reaction.gloating : reaction.missing;
            if (traitFactor <= 0.01f)
            {
                ThoughtOps.ClearSeparation(observer, absent);
                return;
            }

            int opinion = observer.relations.OpinionOf(absent);
            float power = ThoughtOps.ClampPower(Bonds.Strength(bond, opinion) * traitFactor * settings.intensity * familiarity);
            if (power <= 0.01f)
            {
                ThoughtOps.ClearSeparation(observer, absent);
                return;
            }

            ThoughtOps.SetOngoing(observer, absent, ThoughtOps.SeparationThoughts, def, stage, power);
        }

        private void ForgetAbout(Pawn absent)
        {
            for (int i = 0; i < roster.Count; i++)
            {
                ThoughtOps.ClearSeparation(roster[i], absent);
            }
        }

        // ------------------------------------------------------------ the road

        private void ApplyTravelState(HomeSweetHomeSettings settings, int now)
        {
            for (int i = 0; i < travelling.Count; i++)
            {
                Journey journey = travelling[i];
                Pawn traveller = journey.pawn;
                Reaction reaction = TraitProfiles.For(traveller);

                if (settings.homesickness && journey.Counts)
                {
                    float power = ThoughtOps.ClampPower(reaction.homesick * settings.intensity);
                    if (power > 0.01f)
                    {
                        ThoughtOps.SetOngoing(traveller, HSHThoughtDefOf.HSH_Homesick, ThoughtOps.StageForAbsence(journey.WearingDays), power);
                    }
                    else
                    {
                        ThoughtOps.MemoriesOf(traveller)?.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_Homesick);
                    }
                }

                if (settings.campThoughts && journey.place == Whereabouts.Camp)
                {
                    ThoughtOps.SetOngoing(traveller, HSHThoughtDefOf.HSH_MadeCamp, 0, settings.intensity);
                }
                else
                {
                    ThoughtOps.MemoriesOf(traveller)?.RemoveMemoriesOfDef(HSHThoughtDefOf.HSH_MadeCamp);
                }

                if (settings.companionThoughts)
                {
                    ApplyCompanionship(journey, reaction, settings, now);
                }
            }
        }

        private void ApplyCompanionship(Journey journey, Reaction reaction, HomeSweetHomeSettings settings, int now)
        {
            Pawn traveller = journey.pawn;
            List<Pawn> companions = WhereaboutsUtility.TravelCompanions(traveller);
            if (companions == null || companions.Count < 2)
            {
                ThoughtOps.ClearAllCompanionship(traveller);
                return;
            }

            if (!journey.Counts)
            {
                ThoughtOps.ClearAllCompanionship(traveller);
                return;
            }

            int stage = ThoughtOps.StageForTrip(journey.DaysGone);

            for (int i = 0; i < companions.Count; i++)
            {
                Pawn companion = companions[i];
                if (companion == traveller || companion.needs?.mood == null || !companion.IsFreeNonSlaveColonist)
                {
                    continue;
                }

                // Sharing a road is itself getting to know somebody, so this one measures up to
                // now rather than to the day they set off.
                float familiarity = acquaintance.Familiarity(traveller, companion, now, now);
                if (familiarity <= 0f)
                {
                    ThoughtOps.ClearCompanionship(traveller, companion);
                    continue;
                }

                Bond bond = Bonds.Between(traveller, companion);
                ThoughtDef def = ThoughtOps.CompanionThoughtFor(bond);
                if (def == null)
                {
                    ThoughtOps.ClearCompanionship(traveller, companion);
                    continue;
                }

                float traitFactor = Bonds.IsNegative(bond) ? reaction.friction : reaction.company;
                if (traitFactor <= 0.01f)
                {
                    ThoughtOps.ClearCompanionship(traveller, companion);
                    continue;
                }

                int opinion = traveller.relations.OpinionOf(companion);
                float power = ThoughtOps.ClampPower(Bonds.Strength(bond, opinion) * traitFactor * settings.intensity * familiarity);
                if (power <= 0.01f)
                {
                    ThoughtOps.ClearCompanionship(traveller, companion);
                    continue;
                }

                ThoughtOps.SetOngoing(traveller, companion, ThoughtOps.CompanionThoughts, def, stage, power);
            }
        }

        // ------------------------------------------------------------ endings

        /// <summary>Reaching the place they set out for is worth something on its own.</summary>
        private void NoteArrival(Journey journey)
        {
            HomeSweetHomeSettings settings = HomeSweetHomeMod.Settings;
            if (!settings.destinationThoughts)
            {
                return;
            }

            Reaction reaction = TraitProfiles.For(journey.pawn);
            float power = ThoughtOps.ClampPower(reaction.relief * settings.intensity);
            ThoughtOps.GiveOneShot(journey.pawn, HSHThoughtDefOf.HSH_ReachedDestination, ThoughtOps.StageForTrip(journey.DaysGone), power);
        }

        /// <summary>They made it back to a real colony.</summary>
        private void Welcome(Journey journey, int now)
        {
            HomeSweetHomeSettings settings = HomeSweetHomeMod.Settings;
            Pawn traveller = journey.pawn;
            float days = journey.DaysGone;

            List<Pawn> companions = CompanionsSeenDuring(journey);

            ThoughtOps.ClearTravelState(traveller);
            ForgetAbout(traveller);

            if (days < settings.minAbsenceDays)
            {
                return;
            }

            Reaction reaction = TraitProfiles.For(traveller);
            int tripStage = ThoughtOps.StageForTrip(days);

            if (settings.homecomingThoughts)
            {
                float relief = ThoughtOps.ClampPower(reaction.relief * settings.intensity);
                ThoughtOps.GiveOneShot(traveller, HSHThoughtDefOf.HSH_HomeAtLast, tripStage, relief);
            }

            if (settings.reunionThoughts)
            {
                for (int i = 0; i < roster.Count; i++)
                {
                    Pawn observer = roster[i];
                    if (observer == traveller || placeNow[observer] != Whereabouts.Home)
                    {
                        continue;
                    }
                    GiveReunion(observer, traveller, tripStage, settings, journey.departedTick, now);
                }
            }

            if (settings.travelBonds && companions != null)
            {
                AwardSharedRoad(traveller, companions, days, settings, now);
            }
        }

        private void GiveReunion(Pawn observer, Pawn traveller, int stage, HomeSweetHomeSettings settings, int departedTick, int now)
        {
            float familiarity = acquaintance.Familiarity(observer, traveller, departedTick, now);
            if (familiarity <= 0f)
            {
                return;
            }

            Bond bond = Bonds.Between(observer, traveller);
            ThoughtDef def = ThoughtOps.ReunionThoughtFor(bond);
            if (def == null)
            {
                return;
            }

            if (Bonds.IsNegative(bond) && !settings.gloatingThoughts)
            {
                return;
            }

            Reaction reaction = TraitProfiles.For(observer);
            float traitFactor = Bonds.IsNegative(bond) ? reaction.dread : reaction.relief;
            int opinion = observer.relations.OpinionOf(traveller);
            float power = ThoughtOps.ClampPower(Bonds.Strength(bond, opinion) * traitFactor * settings.intensity * familiarity);
            ThoughtOps.GiveOneShot(observer, def, stage, power, traveller);
        }

        /// <summary>A trip together leaves a mark on how two people see each other.</summary>
        private void AwardSharedRoad(Pawn traveller, List<Pawn> companions, float days, HomeSweetHomeSettings settings, int now)
        {
            int stage = ThoughtOps.StageForTrip(days);

            for (int i = 0; i < companions.Count; i++)
            {
                Pawn companion = companions[i];
                if (companion == traveller || companion.Dead || companion.needs?.mood == null)
                {
                    continue;
                }

                float familiarity = acquaintance.Familiarity(traveller, companion, now, now);
                if (familiarity <= 0f)
                {
                    continue;
                }

                float power = settings.intensity * familiarity;
                Bond bond = Bonds.Between(traveller, companion);
                if (Bonds.IsNegative(bond))
                {
                    ThoughtOps.GiveOneShot(traveller, HSHThoughtDefOf.HSH_EnduredEachOther, stage, power, companion);
                }
                else if (bond != Bond.Indifferent || days >= 5f)
                {
                    ThoughtOps.GiveOneShot(traveller, HSHThoughtDefOf.HSH_SharedTheRoad, stage, power, companion);
                }
            }
        }

        private List<Pawn> CompanionsSeenDuring(Journey journey)
        {
            List<Pawn> companions = WhereaboutsUtility.TravelCompanions(journey.pawn);
            if (companions != null)
            {
                return new List<Pawn>(companions);
            }

            // Already stepped onto the home map, so pull the people they shared thoughts with instead.
            MemoryThoughtHandler memories = ThoughtOps.MemoriesOf(journey.pawn);
            if (memories == null)
            {
                return null;
            }

            List<Pawn> found = new List<Pawn>();
            List<Thought_Memory> all = memories.Memories;
            for (int i = 0; i < all.Count; i++)
            {
                Thought_Memory memory = all[i];
                if (memory.otherPawn == null)
                {
                    continue;
                }
                for (int j = 0; j < ThoughtOps.CompanionThoughts.Length; j++)
                {
                    if (memory.def == ThoughtOps.CompanionThoughts[j] && !found.Contains(memory.otherPawn))
                    {
                        found.Add(memory.otherPawn);
                        break;
                    }
                }
            }
            return found;
        }

        /// <summary>They died, left the colony, or vanished. Vanilla handles the grief, we just tidy up.</summary>
        private void Abandon(Journey journey)
        {
            if (journey.pawn == null)
            {
                return;
            }

            ThoughtOps.ClearTravelState(journey.pawn);
            ForgetAbout(journey.pawn);
        }

        // ------------------------------------------------------------ housekeeping

        /// <summary>Wipes every thought this mod owns, colony wide. Used when the player turns it off.</summary>
        public void ScrubEverything()
        {
            journeys.Clear();

            List<Pawn> colonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
            for (int i = 0; i < colonists.Count; i++)
            {
                ThoughtOps.ClearEverything(colonists[i]);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref journeys, "journeys", LookMode.Deep);
            Scribe_Deep.Look(ref acquaintance, "acquaintance");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (journeys == null)
                {
                    journeys = new List<Journey>();
                }
                journeys.RemoveAll(j => j == null || j.pawn == null);

                if (acquaintance == null)
                {
                    acquaintance = new Acquaintance();
                }
            }
        }
    }
}

