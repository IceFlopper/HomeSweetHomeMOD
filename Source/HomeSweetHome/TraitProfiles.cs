using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HomeSweetHome
{
    /// <summary>The combined multipliers for one pawn, after every trait they have has had its say.</summary>
    public struct Reaction
    {
        public float missing;
        public float relief;
        public float gloating;
        public float dread;
        public float homesick;
        public float company;
        public float friction;

        public static Reaction Neutral
        {
            get
            {
                return new Reaction
                {
                    missing = 1f,
                    relief = 1f,
                    gloating = 1f,
                    dread = 1f,
                    homesick = 1f,
                    company = 1f,
                    friction = 1f
                };
            }
        }

        public void Apply(TraitProfileDef profile)
        {
            missing *= profile.missing;
            relief *= profile.relief;
            gloating *= profile.gloating;
            dread *= profile.dread;
            homesick *= profile.homesick;
            company *= profile.company;
            friction *= profile.friction;
        }
    }

    [StaticConstructorOnStartup]
    public static class TraitProfiles
    {
        private static readonly Dictionary<TraitDef, List<TraitProfileDef>> byTrait = new Dictionary<TraitDef, List<TraitProfileDef>>();

        static TraitProfiles()
        {
            Build();
        }

        private static void Build()
        {
            byTrait.Clear();

            int skipped = 0;
            foreach (TraitProfileDef profile in DefDatabase<TraitProfileDef>.AllDefsListForReading)
            {
                if (profile.trait.NullOrEmpty())
                {
                    continue;
                }

                TraitDef traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(profile.trait);
                if (traitDef == null)
                {
                    // Profile for a mod the player doesn't have. Nothing to do, and nothing to complain about.
                    skipped++;
                    continue;
                }

                if (!byTrait.TryGetValue(traitDef, out List<TraitProfileDef> list))
                {
                    list = new List<TraitProfileDef>();
                    byTrait[traitDef] = list;
                }
                list.Add(profile);
            }

            if (Prefs.DevMode)
            {
                Log.Message($"[Home Sweet Home] {byTrait.Count} traits with reaction profiles, {skipped} skipped for missing mods.");
            }
        }

        public static Reaction For(Pawn pawn)
        {
            Reaction reaction = Reaction.Neutral;

            List<Trait> traits = pawn.story?.traits?.allTraits;
            if (traits == null)
            {
                return reaction;
            }

            for (int i = 0; i < traits.Count; i++)
            {
                Trait trait = traits[i];
                if (trait.Suppressed)
                {
                    continue;
                }

                if (!byTrait.TryGetValue(trait.def, out List<TraitProfileDef> profiles))
                {
                    continue;
                }

                for (int j = 0; j < profiles.Count; j++)
                {
                    if (profiles[j].degree == trait.Degree)
                    {
                        reaction.Apply(profiles[j]);
                    }
                }
            }

            return reaction;
        }
    }
}
