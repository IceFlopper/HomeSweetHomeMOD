using HarmonyLib;
using Verse;

namespace HomeSweetHome
{
    public class HomeSweetHome : Mod
    {
        public HomeSweetHome(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.funstab.homesweethome");
            harmony.PatchAll();
        }
    }

    public class HomeSweetHomeSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}