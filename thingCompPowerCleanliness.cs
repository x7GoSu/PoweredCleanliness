using RimWorld;
using Verse;

namespace PoweredCleanliness
{
    public class CompProperties_PowerCleanliness : CompProperties
    {
        public float secondaryCleanliness;

        public CompProperties_PowerCleanliness()
        {
            this.compClass = typeof(CompPowerCleanliness);
        }
    }

    public class CompPowerCleanliness : ThingComp
    {
        public CompProperties_PowerCleanliness Props => (CompProperties_PowerCleanliness)props;

        private float originalCleanlinessValue;

        public override void CompTick()
        {
            base.CompTick();
            UpdateCleanliness();
        }

        private void UpdateCleanliness()
        {
            var powerComp = parent.TryGetComp<CompPowerTrader>();
            if (powerComp != null && powerComp.PowerOn)
            {
                Log.Message($"Power is ON for {parent.Label}. Setting cleanliness to {Props.secondaryCleanliness}");
                SetCleanliness(Props.secondaryCleanliness); // Sets Cleanliness value when powered
            }
            else
            {
                Log.Message($"Power is OFF for {parent.Label}. Reverting cleanliness to original value: {originalCleanlinessValue}");
                SetCleanliness(originalCleanlinessValue); // Revert Cleanliness value to original when not powered
            }
        }

        private void SetCleanliness(float value)
        {
            Log.Message($"Setting cleanliness for {parent.Label} to {value}");
            StatModifier cleanlinessModifier = parent.def.statBases.Find(s => s.stat == StatDefOf.Cleanliness);
            StatModifier cleanlinessremove = parent.def.statBases.Find(s => s.stat == StatDefOf.Cleanliness);
            if (cleanlinessModifier == null)
            {
                cleanlinessModifier = new StatModifier
                {
                    stat = StatDefOf.Cleanliness,
                    value = value
                };
                parent.def.statBases.Add(cleanlinessModifier);
                Log.Message($"Added cleanliness modifier to {parent.Label} with value {value}");
            }
            else
            {
                cleanlinessModifier.value = value;
                Log.Message($"Updated cleanliness modifier for {parent.Label} to value {value}");
            }
        }

    }
}
