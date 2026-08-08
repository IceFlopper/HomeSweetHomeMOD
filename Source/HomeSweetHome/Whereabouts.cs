using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HomeSweetHome
{
    public enum Whereabouts
    {
        Unknown,
        Home,
        Camp,
        Away
    }

    /// <summary>
    /// Works out where a colonist actually is. Everything else in the mod hangs off this,
    /// which is why it asks the game about positions instead of hooking caravan events:
    /// vehicles, shuttles, outposts and whatever else people have installed all end up
    /// somewhere the game can describe, even when they never touch a vanilla caravan.
    /// </summary>
    public static class WhereaboutsUtility
    {
        // World objects that mean "we pitched a tent here" rather than "we live here".
        // Vanilla's Camp is caught by type, these cover the popular camping mods.
        private static readonly HashSet<string> CampObjectDefNames = new HashSet<string>
        {
            "CaravanCamp",   // Set Up Camp
            "AbandonedCamp",
            "TempCamp",
            "VOE_Camp"
        };

        public static Whereabouts Of(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.Destroyed)
            {
                return Whereabouts.Unknown;
            }

            Map map = pawn.MapHeld;
            if (map != null)
            {
                // Checked before IsPlayerHome on purpose. A camp map counts as the player's
                // for raids and work orders, but sleeping in a tent is not being home.
                if (IsCamp(map.Parent))
                {
                    return Whereabouts.Camp;
                }
                return map.IsPlayerHome ? Whereabouts.Home : Whereabouts.Away;
            }

            // No map: riding in a caravan, a vehicle, a shuttle, an outpost, a pod in flight.
            return HoldingWorldObject(pawn) != null ? Whereabouts.Away : Whereabouts.Unknown;
        }

        public static bool IsCamp(WorldObject worldObject)
        {
            if (worldObject == null)
            {
                return false;
            }

            // Never mistake a real colony for a camp, whatever it happens to be called.
            if (worldObject is Settlement settlement && settlement.Faction == Faction.OfPlayer)
            {
                return false;
            }

            if (worldObject is Camp)
            {
                return true;
            }

            if (worldObject.def != null && CampObjectDefNames.Contains(worldObject.def.defName))
            {
                return true;
            }

            return worldObject.GetType().Name.IndexOf("Camp", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Walks up the holder chain until it finds the caravan / vehicle / outpost carrying this pawn.</summary>
        public static WorldObject HoldingWorldObject(Pawn pawn)
        {
            IThingHolder holder = pawn.ParentHolder;
            for (int i = 0; holder != null && i < 32; i++)
            {
                if (holder is WorldObject worldObject)
                {
                    return worldObject;
                }
                holder = holder.ParentHolder;
            }
            return null;
        }

        /// <summary>
        /// Whatever a pawn is currently sharing their situation with. Two pawns with the same
        /// key are together, so they miss each other only when the keys differ.
        /// </summary>
        public static object GroupKeyOf(Pawn pawn)
        {
            WorldObject worldObject = HoldingWorldObject(pawn);
            if (worldObject != null)
            {
                return worldObject;
            }
            return pawn.MapHeld;
        }

        /// <summary>The people sharing this pawn's journey: caravan mates, or whoever else is on the same away map.</summary>
        public static List<Pawn> TravelCompanions(Pawn pawn)
        {
            if (HoldingWorldObject(pawn) is Caravan caravan)
            {
                return caravan.PawnsListForReading;
            }

            Map map = pawn.MapHeld;
            if (map != null && (IsCamp(map.Parent) || !map.IsPlayerHome))
            {
                return map.mapPawns.FreeColonists;
            }

            return null;
        }
    }
}
