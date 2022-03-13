using HarmonyLib;
using System.Reflection;

#pragma warning disable IDE0051 // Remove unused private members

public class StopFuelWaste : IModApi
{

    public void InitMod(Mod mod)
    {
        Log.Out(" Loading Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(TileEntityWorkstation))]
    [HarmonyPatch("HandleFuel")]
    public class TileEntityWorkstation_HandleFuel
    {
        static float GetAllRecipeTimes(TileEntityWorkstation __instance)
        {
            float crafting = 0;
            foreach (RecipeQueueItem recipe in __instance.Queue)
            {
                if (recipe.Multiplier > 1)
                {
                    crafting += recipe.OneItemCraftTime * (recipe.Multiplier - 1f);
                }
                crafting += recipe.CraftingTimeLeft;
            }
            return crafting;
        }

        static bool CanSmeltStackItemHere(
            TileEntityWorkstation __instance, 
            ItemClass item)
        {
            if (item.MadeOfMaterial.ForgeCategory is string category)
            {
                foreach (string name in __instance.MaterialNames)
                {
                    if (category.EqualsCaseInsensitive(name)) return true;
                }
            }
            return false;
        }

        static float GetItemStackSmeltingTime(
            TileEntityWorkstation __instance, 
            bool[] ___isModuleUsed,
            float timeLeft,
            ItemStack stack)
        {
            if (ItemClass.GetForId(stack.itemValue.type) is ItemClass item)
            {
                if (CanSmeltStackItemHere(__instance, item))
                {
                    float smeltOneTime = item.GetWeight() * (item.MeltTimePerUnit > 0.0 ? item.MeltTimePerUnit : 1f);
                    // Do we support tools?
                    if (___isModuleUsed[0])
                    {
                        for (int n = 0; n < __instance.Tools.Length; ++n)
                        {
                            float modifier = 1f;
                            __instance.Tools[n].itemValue.ModifyValue(null, null,
                                PassiveEffects.CraftingSmeltTime,
                                ref smeltOneTime, ref modifier,
                                FastTags.Parse(item.Name));
                            smeltOneTime *= modifier;
                        }
                    }
                    if (timeLeft == int.MinValue)
                    {
                        return smeltOneTime * stack.count;
                    }
                    else if (stack.count > 1)
                    {
                        return smeltOneTime * (stack.count - 1f) + timeLeft;
                    }
                    else if (stack.count == 1)
                    {
                        return timeLeft;
                    }
                }
            }
            return 0;
        }

        static void Prefix(
            TileEntityWorkstation __instance,
            float[] ___currentMeltTimesLeft,
            bool[] ___isModuleUsed,
            ref float _timePassed)
        {
            float smelting = 0f;
            // Only check irregular deltas
            if (_timePassed < 10f) return;
            // Check if smelting is used
            if (___isModuleUsed[4])
            {
                for (int i = 0; i < __instance.InputSlotCount; i += 1)
                {
                    smelting = System.Math.Max(
                        GetItemStackSmeltingTime(
                            __instance, ___isModuleUsed,
                            ___currentMeltTimesLeft[i],
                            __instance.Input[i]),
                        smelting);
                }
            }
            // Get summed up crafting time for all recipes
            float crafting = GetAllRecipeTimes(__instance);
            // Get maximum time for either crafting or smelting
            float timeNeeded = System.Math.Max(crafting, smelting);
            // If nothing to be done, we keep burning (feature)
            if (timeNeeded == 0) return;
            // Do not burn more fuel than needed for crafting or smelting
            _timePassed = System.Math.Min(_timePassed, timeNeeded);
        }
    }

    [HarmonyPatch(typeof(TileEntityWorkstation))]
    [HarmonyPatch("UpdateTick")]
    public class TileEntityWorkstation_UpdateTick
    {

        static bool HasWork(TileEntityWorkstation station,
            RecipeQueueItem[] queue,
            ItemStack[] input)
        {
            foreach (var item in queue)
            {
                if (item.IsCrafting) return true;
            }
            for (int i = 0; i < station.InputSlotCount; i += 1)
            {
                if (!input[i].IsEmpty()) return true;
            }
            return false;
        }

        static void Prefix(TileEntityWorkstation __instance,
            RecipeQueueItem[] ___queue,
            ItemStack[] ___input,
            out bool __state)
        {
            __state = false;
            if (!__instance.IsBurning) return;
            if (!HasWork(__instance, ___queue, ___input)) return;
            __state = true;
        }

        static void Postfix(TileEntityWorkstation __instance,
            RecipeQueueItem[] ___queue,
            ItemStack[] ___input,
            bool __state)
        {
            if (__state == false) return;
            if (!__instance.IsBurning) return;
            if (HasWork(__instance, ___queue, ___input)) return;
            __instance.IsBurning = false;
        }

    }

    [HarmonyPatch(typeof(XUiC_WorkstationWindowGroup))]
    [HarmonyPatch("Update")]
    public class XUiC_WorkstationFuelGrid_Update
    {

        static bool HasWork(XUiC_CraftingQueue craftingQueue,
            XUiC_WorkstationInputGrid inputWindow)
        {
            foreach (var item in craftingQueue.GetRecipesToCraft())
            {
                if (item.IsCrafting) return true;
            }
            // Not every station has input
            if (inputWindow == null) return false;
            XUiC_WorkstationMaterialInputGrid matInput = inputWindow.WindowGroup
                .Controller.GetChildByType<XUiC_WorkstationMaterialInputGrid>();
            int count = matInput.GetItemStackControllers().Length;
            var slots = inputWindow.GetSlots();
            for (int i = 0; i < count; i += 1)
            {
                if (!slots[i].IsEmpty()) return true;
            }
            return false;
        }

        static void Prefix(out bool __state,
            XUiC_CraftingQueue ___craftingQueue,
            XUiC_WorkstationInputGrid ___inputWindow)
        {
            __state = HasWork(___craftingQueue, ___inputWindow);
        }

        static void Postfix(bool __state,
            XUiC_CraftingQueue ___craftingQueue,
            XUiC_WorkstationInputGrid ___inputWindow,
            XUiC_WorkstationFuelGrid ___fuelWindow)
        {
            if (__state == false) return;
            if (HasWork(___craftingQueue, ___inputWindow)) return;
            if (___fuelWindow != null)
            {
                ___fuelWindow.TurnOff();
            }
        }

    }

}