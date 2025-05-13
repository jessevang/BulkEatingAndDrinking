using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.ComponentModel.DataAnnotations;

namespace BulkEatingAndDrinking
{
    public class Config
    {
        public string BulkEatAmount { get; set; } = "Until Full Health & Stamina"; 
        public int BulkDrinkAmount { get; set; } = 1;


    }

    internal sealed class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }
        public Config Config { get; private set; }
        public bool isPlayingEatAnimation { get; set; } = false;
        public bool isPlayingDrinkAnimation { get; set; } = false;
        public int eatTimerTick { get; set; } = 0;
        public int drinkTimerTick { get; set; } = 0;
        public bool PlayEatAnimation { get; set; } = true;
        public bool PlayDrinkAnimation { get; set; } = true;


        public override void Entry(IModHelper helper)
        {
            Instance = this; 
            Config = helper.ReadConfig<Config>() ?? new Config();

            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (isPlayingEatAnimation)
             {
                Game1.player.Halt();
                Game1.player.canMove = false;
                eatTimerTick++;
            }

            if (isPlayingDrinkAnimation)
            {
                Game1.player.Halt();
                Game1.player.canMove = false;
                drinkTimerTick++;
            }



            if (isPlayingEatAnimation)
            {
                // The animation finishes when CurrentAnimation is null
                if (eatTimerTick >= 130)
                {
                    Game1.player.canMove = true;
                    isPlayingEatAnimation = false;
                    //Console.WriteLine("isPlayingEatAnimation:" + isPlayingEatAnimation);
                    eatTimerTick = 0;
                }
            }

            if (isPlayingDrinkAnimation)
            {
                // The animation finishes when CurrentAnimation is null
                if (drinkTimerTick >= 110)
                {
                    Game1.player.canMove = true;
                    isPlayingDrinkAnimation = false;
                    //Console.WriteLine("isPlayingEatAnimation:" + ModEntry.Instance.isPlayingDrinkAnimation);
                    drinkTimerTick = 0;
                }
            }
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
            gmcm.AddTextOption(
                mod: ModManifest,
                getValue: () => Config.BulkEatAmount,
                setValue: value => Config.BulkEatAmount = value,
                name: () => "Eat Amount",
                tooltip: () => "Set how many foods to eat, or use 'Until Full Health & Stamina'",
                allowedValues: new[] { "1", "2", "3", "5", "10", "Until Full Health & Stamina" },
                formatAllowedValue: value => value
            );

            gmcm.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.BulkDrinkAmount,
                setValue: value => Config.BulkDrinkAmount = value,
                name: () => "Drink Amount",
                tooltip: () => "How many drinks to consume at once",
                min: 1,
                max: 20,
                interval: 1
            );
        }
    
    
    }
}