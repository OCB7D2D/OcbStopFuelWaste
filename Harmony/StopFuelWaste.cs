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
            if (___fuelWindow != null) ___fuelWindow.TurnOff();
        }

    }

}