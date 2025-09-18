using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using HarmonyLib;
using JetBrains.Annotations;
using Objects.Items;

namespace ColorCycler
{
    [HarmonyPatch(typeof(InventoryManager), "NormalMode")]
    public class ColorCyclerMod
    {
        [UsedImplicitly]
        public static void Prefix(InventoryManager __instance)
        {
            bool keyUp = __instance.newScrollData > 0f;
            bool keyDown = __instance.newScrollData < 0f;
            if ((bool)__instance.ActiveHand.Slot.Get() && (keyUp || keyDown))
            {
                if (__instance.ActiveHand.Slot.Get() is SprayCan sprayCan)
                {
                    int current = ColorCyclerModHelpers.GetPaintColorIndex(sprayCan.PaintMaterial);
                    if (keyUp)
                    {
                        current++;
                    }
                    else if (keyDown)
                    {
                        current--;
                    }
                    if (current >= GameManager.Instance.CustomColors.Count)
                    {
                        current = 0;
                    }
                    if (current < 0)
                    {
                        current = GameManager.Instance.CustomColors.Count - 1;
                    }
                    var paintMaterial = ColorCyclerModHelpers.GetPaintColor(current);
                    ColorCyclerModHelpers.UpdateSprayCan(sprayCan, paintMaterial);
                    __instance.ActiveHand.Slot.RefreshSlotDisplay();

                    if (NetworkManager.IsClient)
                    {
                        ColorCyclerBep.Logger.LogDebug($"Sending {nameof(ThingColorMessage)} to Server. ColorIndex = {current}, ThingId = {sprayCan.ReferenceId}");
                        NetworkClient.SendToServer(new ThingColorMessage
                        {
                            ThingId = sprayCan.ReferenceId,
                            ColorIndex = current
                        }, NetworkChannel.GeneralTraffic);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SprayCan), "OnUseItem")]
    public class SprayCanOnUseItemPatch
    {
        [UsedImplicitly]
        public static bool Prefix(SprayCan __instance, ref bool __result, ref float quantity)
        {
            __result = true;

            // Make Paint infinite.
            if (ColorCyclerBep.Settings.InfinitePaint)
            {
                ColorCyclerBep.Logger.LogDebug($"Setting Quantity to 0 from {quantity}");
                quantity = 0.0f;
            }
            ColorCyclerBep.Logger.LogDebug($"Using Quantity {quantity}");

            // Return true, so that original code is executed, if pollution should occur.
            if (ColorCyclerBep.Settings.ShouldCreatePollution)
            {
                return true;
            }

            // No pollution, skip original, but apply Quantity in case of non-infinite paint
            __instance.Quantity -= quantity;
            return false;
        }
    }

    // We have to patch Consumable because SprayCan does not implement the method
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.GetQuantityText))]
    public class ConsumableGetQuantityTextPatch
    {
        [UsedImplicitly]
        public static bool Prefix(Consumable __instance, ref string __result)
        {
            if (__instance is not SprayCan sprayCan || !ColorCyclerBep.Settings.InfinitePaint)
            {
                return true;
            }
            __result = "Infinite";
            return false;
        }

    }

    // We have to patch Consumable because SprayCan does not implement the method
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.BuildUpdate))]
    public class ConsumableBuildUpdatePatch
    {
        [UsedImplicitly]
        public static void Postfix(Consumable __instance, RocketBinaryWriter writer, ushort networkUpdateType)
        {
            if (__instance is SprayCan sprayCan)
            {
                if (Thing.IsNetworkUpdateRequired(ColorCyclerModHelpers.PaintableMaterialNetworkFlag, networkUpdateType))
                {
                    writer.WriteInt32(ColorCyclerModHelpers.GetPaintColorIndex(sprayCan.PaintMaterial));
                }
            }
        }
    }

    // We have to patch Consumable because SprayCan does not implement the method
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ProcessUpdate))]
    public class ConsumableProcessUpdatePatch
    {
        [UsedImplicitly]
        public static void Postfix(Consumable __instance, RocketBinaryReader reader, ushort networkUpdateType)
        {
            if (__instance is SprayCan sprayCan)
            {
                if (Thing.IsNetworkUpdateRequired(ColorCyclerModHelpers.PaintableMaterialNetworkFlag, networkUpdateType))
                {
                    int index = reader.ReadInt32();
                    var paintMaterial = ColorCyclerModHelpers.GetPaintColor(index);
                    ColorCyclerModHelpers.UpdateSprayCan(sprayCan, paintMaterial);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ThingColorMessage), nameof(ThingColorMessage.Process))]
    public class ThingColorMessageProcessPatch
    {
        [UsedImplicitly]
        public static void Postfix(ThingColorMessage __instance, long hostId)
        {
            ColorCyclerBep.Logger.LogDebug($"Received {nameof(ThingColorMessage)}. ColorIndex = {__instance.ColorIndex}, ThingId = {__instance.ThingId}");
            if (Thing.Find(__instance.ThingId) is SprayCan sprayCan)
            {
                var paintMaterial = ColorCyclerModHelpers.GetPaintColor(__instance.ColorIndex);
                ColorCyclerModHelpers.UpdateSprayCan(sprayCan, paintMaterial);
            }
        }
    }
}
