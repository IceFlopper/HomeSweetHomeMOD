using Verse;

namespace HomeSweetHome
{
    public class HomeSweetHomeSettings : ModSettings
    {
        public bool enableWorriedThought = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableWorriedThought, "enableWorriedThought", true);
            base.ExposeData();
        }
    }
}
