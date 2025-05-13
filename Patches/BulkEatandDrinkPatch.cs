using HarmonyLib;
using StardewValley;
using BulkEatingAndDrinking;
using System.Collections.Generic;

namespace BulkEatingAndDrinking.Patches
{
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.eatHeldObject))]
    public static class BulkEatandDrinkPatch
    {
        public static bool Prefix()
        {
            StardewValley.Item food = Game1.player.CurrentItem;
            if (food is not StardewValley.Object obj || obj.Edibility <= 0)
                return true;

            // Let Stardrop use the original code
            if (obj?.QualifiedItemId == "(O)434")
                return true;

            EatBulk(Game1.player, obj);
            return false;
        }

        private static void EatBulk(Farmer who, StardewValley.Object food)
        {
            int actualAmount = 1;
            int availableAmount = food.Stack;

            if (Game1.objectData.TryGetValue(food.ItemId, out var data) && data.IsDrink)
            {
                actualAmount = System.Math.Min(ModEntry.Instance.Config.BulkDrinkAmount, availableAmount);
            }
            else
            {
                string eatConfig = ModEntry.Instance.Config.BulkEatAmount;
                if (eatConfig == "Until Full Health & Stamina")
                {
                    int neededHealth = who.maxHealth - who.health;
                    int neededStamina = (int)(who.MaxStamina - who.stamina);

                    int perHealth = food.healthRecoveredOnConsumption();
                    int perStamina = food.staminaRecoveredOnConsumption();

                    int healthAmount = perHealth > 0 ? (neededHealth + perHealth - 1) / perHealth : 0;
                    int staminaAmount = perStamina > 0 ? (neededStamina + perStamina - 1) / perStamina : 0;

                    int targetAmount = System.Math.Max(1, System.Math.Max(healthAmount, staminaAmount));
                    actualAmount = System.Math.Min(targetAmount, availableAmount);
                }
                else if (int.TryParse(eatConfig, out int parsed))
                {
                    actualAmount = System.Math.Min(parsed, availableAmount);
                }
                else
                {
                    actualAmount = 1; 
                }
            }

            if (actualAmount <= 0)
                return;

            int initialHealth = who.health;
            float initialStamina = who.stamina;

            int totalHealth = food.healthRecoveredOnConsumption() * actualAmount;
            int totalStamina = food.staminaRecoveredOnConsumption() * actualAmount;

            who.health = System.Math.Min(who.maxHealth, who.health + totalHealth);
            who.stamina = System.Math.Min(who.MaxStamina, who.stamina + totalStamina);

            who.removeFirstOfThisItemFromInventory(food.ItemId, actualAmount);

            IEnumerable<Buff> buffs = food.GetFoodOrDrinkBuffs();
            if (buffs != null)
            {
                foreach (Buff buff in buffs)
                {
                    Buff cloned = new Buff(
                        id: buff.id,
                        source: buff.source,
                        displaySource: buff.displaySource,
                        duration: buff.millisecondsDuration * actualAmount,
                        iconTexture: buff.iconTexture,
                        iconSheetIndex: buff.iconSheetIndex,
                        effects: buff.effects,
                        isDebuff: null,
                        displayName: buff.displayName,
                        description: buff.description
                    );
                    who.applyBuff(cloned);
                }
            }

            Game1.player.Halt();
            Game1.player.CanMove = false;
            Game1.player.FarmerSprite.StopAnimation();
            Game1.player.completelyStopAnimatingOrDoingAction();

            if (Game1.objectData.TryGetValue(food.ItemId, out var data2) && data2.IsDrink)
            {
                Game1.player.itemToEat = food;
                Game1.player.FarmerSprite.animateOnce(294, 1f, 8);
                Game1.player.canMove = false;
                ModEntry.Instance.isPlayingDrinkAnimation = true;
            }
            else if (food.Edibility != -300)
            {
                Game1.player.itemToEat = food;
                Game1.player.FarmerSprite.animateOnce(216, 1f, 8);
                Game1.player.canMove = false;
                ModEntry.Instance.isPlayingEatAnimation = true;
            }

            if (initialStamina < who.stamina)
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(who.stamina - initialStamina)), 4));
            }
            if (initialHealth < who.health)
            {
                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", who.health - initialHealth), 5));
            }
        }
    }
}
