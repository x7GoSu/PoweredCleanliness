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
        private bool initialSetupDone = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!initialSetupDone)
            {
                // Save the original cleanliness value
                StatModifier cleanlinessModifier = parent.def.statBases.Find(s => s.stat == StatDefOf.Cleanliness);
                originalCleanlinessValue = cleanlinessModifier != null ? cleanlinessModifier.value : 0.0f;
                Log.Message($"Initial Cleanliness Value for {parent.Label}: {originalCleanlinessValue}");
                initialSetupDone = true;
            }
            UpdateCleanliness();
        }

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

            // Notify the game to update the stats
            if (parent.Spawned)
            {
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Buildings);
                Log.Message($"Building {parent.Label} updated buildings");

                // Notify the room to update its stats
                Room room = parent.GetRoom();
                if (room != null)
                {
                    room.Notify_TerrainChanged();
                    Log.Message($"Notified room to update cleanliness");
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            // Revert cleanliness value to original when the building is despawned
            SetCleanliness(originalCleanlinessValue);
        }

        public override string CompInspectStringExtra()
        {
            StatModifier cleanlinessModifier = parent.def.statBases.Find(s => s.stat == StatDefOf.Cleanliness);
            float currentCleanliness = cleanlinessModifier != null ? cleanlinessModifier.value : 0.0f;
            return $"Cleanliness: {currentCleanliness}";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref originalCleanlinessValue, "originalCleanlinessValue", 0.0f);
        }
    }
}
