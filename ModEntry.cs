using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.ComponentModel.DataAnnotations;

namespace BulkEatingAndDrinking
{
    public class Config
    {
        public int BulkEatAmount { get; set; } = 3;
        public int BulkDrinkAmount { get; set; } = 1;

    }

    internal sealed class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }
        public Config Config { get; private set; }

        
        public override void Entry(IModHelper helper)
        {
            Instance = this; 
            Config = helper.ReadConfig<Config>() ?? new Config();

            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }


        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {

            // Uses Generic Mod Config Menu API to build a config UI.
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm == null)
                return;

            // Register the mod.
            gmcm.Register(ModManifest, () => Config = new Config(), () => Helper.WriteConfig(Config));



            gmcm.AddParagraph(
                ModManifest,
                text: () => "Set the amount your farmer will drink or eat"
            );
            gmcm.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.BulkEatAmount,
                setValue: value => Config.BulkEatAmount = value,
                name: () => "Eat this Amount",
                tooltip: () => "The amount your farmer will eat from your stack of food",
                min: 1,
                max: 20,
                interval: 1
            );
            gmcm.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.BulkDrinkAmount,
                setValue: value => Config.BulkDrinkAmount = value,
                name: () => "Drink this Amount",
                tooltip: () => "The amount your farmer will drink from your stack of drinks",
                min: 1,
                max: 20,
                interval: 1
            );
        }
    
    
    }
}