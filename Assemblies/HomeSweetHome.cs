using HarmonyLib;
using Verse;

namespace HomeSweetHome
{
    public class HomeSweetHome : Mod
    {

        public HomeSweetHome(ModContentPack content) : base(content)
        {
            Log.Message("HomeSweetHome: Initializing Harmony");
            var harmony = new Harmony("com.Lewkah0.homesweethome");
            harmony.PatchAll();
            Log.Message("HomeSweetHome: Harmony patches applied");
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
