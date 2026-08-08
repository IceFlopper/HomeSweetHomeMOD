using System.Collections.Generic;
using Verse;

namespace HomeSweetHome
{
    /// <summary>
    /// How a single trait colours a pawn's reaction to people coming and going.
    /// Every field is a multiplier, so 1 means "this trait has no opinion on the matter".
    ///
    /// The trait is named as a plain string and resolved at runtime, so a profile for a mod
    /// that isn't installed is skipped quietly instead of spraying red errors at startup.
    /// Other mods can add their own by dropping another file of these in.
    /// </summary>
    public class TraitProfileDef : Def
    {
        public string trait;
        public int degree;

        /// <summary>Someone they care about is away.</summary>
        public float missing = 1f;

        /// <summary>...and has come back.</summary>
        public float relief = 1f;

        /// <summary>Someone they can't stand is away.</summary>
        public float gloating = 1f;

        /// <summary>...and has come back.</summary>
        public float dread = 1f;

        /// <summary>Being away from home themselves.</summary>
        public float homesick = 1f;

        /// <summary>Travelling alongside people they like.</summary>
        public float company = 1f;

        /// <summary>Travelling alongside people they don't.</summary>
        public float friction = 1f;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }

            if (trait.NullOrEmpty())
            {
                yield return "no trait named";
            }
        }
    }
}
