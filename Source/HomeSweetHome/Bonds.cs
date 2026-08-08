using RimWorld;
using UnityEngine;
using Verse;

namespace HomeSweetHome
{
    /// <summary>How one colonist feels about another, coarse enough to pick a thought from.</summary>
    public enum Bond
    {
        Partner,
        Child,
        Parent,
        Sibling,
        Relative,
        CloseFriend,
        Friend,
        Indifferent,
        Rival,
        Enemy
    }

    public static class Bonds
    {
        // Kept in step with vanilla's own friend/rival cutoffs.
        public const int FriendOpinion = 20;
        public const int CloseFriendOpinion = 55;
        public const int RivalOpinion = -20;
        public const int EnemyOpinion = -55;

        // Past this much bad blood the family tie stops counting for anything.
        private const int FamilyFallingOutOpinion = -45;

        private static readonly SimpleCurve StrengthByOpinion = new SimpleCurve
        {
            new CurvePoint(0f, 0.5f),
            new CurvePoint(20f, 0.7f),
            new CurvePoint(50f, 1f),
            new CurvePoint(100f, 1.35f)
        };

        public static Bond Between(Pawn observer, Pawn other)
        {
            int opinion = observer.relations.OpinionOf(other);

            if (opinion > FamilyFallingOutOpinion)
            {
                PawnRelationDef relation = PawnRelationUtility.GetMostImportantRelation(observer, other);
                if (relation != null)
                {
                    if (relation == PawnRelationDefOf.Spouse || relation == PawnRelationDefOf.Fiance || relation == PawnRelationDefOf.Lover)
                    {
                        return Bond.Partner;
                    }
                    if (relation == PawnRelationDefOf.Child)
                    {
                        return Bond.Child;
                    }
                    if (relation == PawnRelationDefOf.Parent || relation == PawnRelationDefOf.ParentBirth)
                    {
                        return Bond.Parent;
                    }
                    if (relation == PawnRelationDefOf.Sibling || relation == PawnRelationDefOf.HalfSibling)
                    {
                        return Bond.Sibling;
                    }
                    if (IsExtendedFamily(relation))
                    {
                        return Bond.Relative;
                    }
                }
            }

            if (opinion >= CloseFriendOpinion)
            {
                return Bond.CloseFriend;
            }
            if (opinion >= FriendOpinion)
            {
                return Bond.Friend;
            }
            if (opinion <= EnemyOpinion)
            {
                return Bond.Enemy;
            }
            if (opinion <= RivalOpinion)
            {
                return Bond.Rival;
            }
            return Bond.Indifferent;
        }

        private static bool IsExtendedFamily(PawnRelationDef relation)
        {
            return relation == PawnRelationDefOf.Grandparent
                || relation == PawnRelationDefOf.Grandchild
                || relation == PawnRelationDefOf.GreatGrandparent
                || relation == PawnRelationDefOf.GreatGrandchild
                || relation == PawnRelationDefOf.UncleOrAunt
                || relation == PawnRelationDefOf.NephewOrNiece
                || relation == PawnRelationDefOf.GranduncleOrGrandaunt
                || relation == PawnRelationDefOf.Cousin
                || relation == PawnRelationDefOf.Kin;
        }

        public static bool IsPositive(Bond bond)
        {
            return bond <= Bond.Friend;
        }

        public static bool IsNegative(Bond bond)
        {
            return bond >= Bond.Rival;
        }

        /// <summary>
        /// Scales the mood hit by how much the pair actually care. Family gets a floor so a
        /// husband you merely tolerate still registers when he disappears for a fortnight.
        /// </summary>
        public static float Strength(Bond bond, int opinion)
        {
            float byOpinion = StrengthByOpinion.Evaluate(Mathf.Abs(opinion));
            switch (bond)
            {
                case Bond.Partner:
                    return Mathf.Max(byOpinion, 1f);
                case Bond.Child:
                    return Mathf.Max(byOpinion, 0.95f);
                case Bond.Sibling:
                    return Mathf.Max(byOpinion, 0.75f);
                case Bond.Parent:
                    return Mathf.Max(byOpinion, 0.7f);
                case Bond.Relative:
                    return Mathf.Max(byOpinion, 0.55f);
                default:
                    return byOpinion;
            }
        }
    }
}
