using HarmonyLib;
using Kitchen;
using KitchenData;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace KitchenBakeryPatch
{
    [HarmonyPatch]
    static class AssignMenuRequests_Patch
    {
        static readonly HashSet<int> AFFECTED_ORDERS = new HashSet<int>()
        {
            -1303191076,    // CHOCOLATE FLAVOUR
            -19982058,      // COFFEE FLAVOUR
            51761947        // LEMON FLAVOUR
        };

        [HarmonyPatch(typeof(AssignMenuRequests), "AttemptOrderSpecific")]
        [HarmonyPrefix]
        static bool AttemptOrderSpecific_Prefix(
            ref string __state,
            ref AssignMenuRequests __instance,
            ref HashSet<int> ___TempIngredients,
            ref bool __result,
            EntityContext ctx,
            CItem data,
            NativeArray<Entity> menu_entities,
            NativeArray<CMenuItem> menu_items,
            MenuPhase phase,
            Entity group,
            ref float bonus_time,
            int member_index)
        {
            if (menu_entities.Length == 0)
            {
                return true;
            }
            if (data.IsPartial)
            {
                return true;
            }
            for (int i = 0; i < menu_entities.Length; i++)
            {
                CMenuItem cMenuItem = menu_items[i];
                if (cMenuItem.Phase != phase || !AFFECTED_ORDERS.Contains(cMenuItem.Item) ||
                    !GameData.Main.TryGet<Item>(cMenuItem.Item, out var output, warn_if_fail: true) ||
                    !output.SatisfiedBy.Select(x => x.ID).Contains(data.ID))
                {
                    continue;
                }
                bool allIngredientsSatisfied = true;
                foreach (Item needsIngredient in output.NeedsIngredients)
                {
                    bool ingredientSatisfied = false;
                    for (int j = 0; j < data.Items.Count; j++)
                    {
                        if (data.Items[j] == needsIngredient.ID)
                        {
                            ingredientSatisfied = true;
                            break;
                        }
                    }
                    if (!ingredientSatisfied)
                    {
                        allIngredientsSatisfied = false;
                        break;
                    }
                }
                if (!allIngredientsSatisfied)
                    continue;
                ItemList item_components = default(ItemList);
                __instance.OrderItem(output, ctx, item_components, group, ref bonus_time, member_index, phase, cMenuItem.SourceDish);
                if (output.AlwaysOrderAdditionalItem != 0 && GameData.Main.TryGet<Item>(output.AlwaysOrderAdditionalItem, out var output3, warn_if_fail: true))
                {
                    __instance.OrderItem(output3, ctx, (output3 is ItemGroup) ? GameData.Main.ItemSetView.GetRandomConfiguration(output.AlwaysOrderAdditionalItem, ___TempIngredients) : new ItemList(output.AlwaysOrderAdditionalItem), group, ref bonus_time, member_index, phase, cMenuItem.SourceDish);
                }
                __result = true;
                __state = output.name;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AssignMenuRequests), "AttemptOrderSpecific")]
        [HarmonyPostfix]
        static void AttemptOrderSpecific_Postfix(ref string __state, ref bool __result)
        {
            if (__result && __state != null)
            {
                Main.LogWarning($"{__state} encouraged!");
            }
        }
    }
}
