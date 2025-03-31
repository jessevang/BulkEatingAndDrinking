using HarmonyLib;
using StardewValley;
using BulkEatingAndDrinking;



namespace BulkEatingAndDrinking.Patches
{
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.eatHeldObject))]
    public static class BulkEatandDrinkPatch
    {
        public static bool Prefix()
        {
            StardewValley.Item food = Game1.player.CurrentItem; 
            if (food is not StardewValley.Object obj || obj.Edibility <= 0)
            {
                return true;
            }

            //if i'm trying to eat a stardrop should run original code to spawn all the effect and bonus stats
            if (obj?.QualifiedItemId == "(O)434")
            {
                return true;
            }
                //Console.WriteLine("eating now");
                EatBulk(Game1.player, obj);
       
           return false;
        }

        private static void EatBulk(Farmer who, StardewValley.Object food)
        {
            int desiredAmount = 1;
            if (Game1.objectData.TryGetValue(food.ItemId, out var data) && data.IsDrink)
            {
                desiredAmount = ModEntry.Instance.Config.BulkDrinkAmount;
            }
            else 
            {
                desiredAmount = ModEntry.Instance.Config.BulkEatAmount;
            }
                

            int availableAmount = food.Stack;
            int actualAmount = System.Math.Min(desiredAmount, availableAmount);

            if (actualAmount <= 0)
                return;

            int initialHealth = Game1.player.health;
            float initialStamina = Game1.player.stamina;



            int totalHealth = food.healthRecoveredOnConsumption() * actualAmount;
            int totalStamina = food.staminaRecoveredOnConsumption() * actualAmount;

            who.health = System.Math.Min(who.maxHealth, who.health + totalHealth);
            who.stamina = System.Math.Min(who.MaxStamina, who.stamina + totalStamina);



            Console.WriteLine($"Total Amount Removed:{actualAmount} TotalHealth Gained: {totalHealth}  TotalStaminaGained: {totalStamina}");

            who.removeFirstOfThisItemFromInventory(food.ItemId, actualAmount);
            IEnumerable<Buff> buffs = food.GetFoodOrDrinkBuffs();
            if (buffs != null)
            {
                int remainingTime = 0;
                foreach (Buff buff in buffs)
                {

                    Buff cloned = new Buff(
                        id: buff.id,
                        source: buff.source,
                        displaySource: buff.displaySource,
                        duration: buff.millisecondsDuration,
                        iconTexture: buff.iconTexture,
                        iconSheetIndex: buff.iconSheetIndex,
                        effects: buff.effects,
                        isDebuff: null,
                        displayName: buff.displayName,
                        description: buff.description
                    );
                    cloned.millisecondsDuration = (cloned.millisecondsDuration * actualAmount);
                    who.applyBuff(cloned);
                }
            }


            Game1.player.forceCanMove();
            Game1.player.completelyStopAnimatingOrDoingAction();
            if (Game1.objectData.TryGetValue(food.ItemId, out var data2) && data2.IsDrink)
            {
                Game1.player.itemToEat = food;
                Game1.player.FarmerSprite.animateOnce(294, 80f, 8);


            }
            else if (food.Edibility != -300)
            {

                Game1.player.itemToEat = food;
                Game1.player.FarmerSprite.animateOnce(216, 80f, 8);
 
            }


            if (initialStamina < Game1.player.stamina)
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(Game1.player.stamina - initialStamina)), 4));
            }
            if (initialHealth < Game1.player.health)
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", Game1.player.health - initialHealth), 5));
            }




        }
    }
}
