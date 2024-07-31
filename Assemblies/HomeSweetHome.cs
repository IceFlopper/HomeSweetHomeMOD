using System;
using RimWorld;
using Verse;
using HarmonyLib;

namespace HomeSweetHome
{
    public class HomeSweetHome : Mod
    {
        public HomeSweetHome(ModContentPack content) : base(content)
        {
            Log.Message("[HomeSweetHome] HomeSweetHome loaded");
            var harmony = new Harmony("com.funstab.homesweethome");
            harmony.PatchAll();
            Log.Message("[HomeSweetHome] Harmony patches applied");
            GetSettings<HomeSweetHomeSettings>();
        }

        public override string SettingsCategory() => "Home Sweet Home";
    }

    public class HomeSweetHomeSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
