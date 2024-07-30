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
        public static bool Prefix(ref bool __result)
        {
            // Modify __result and return false, so that pollution does not occur.
            __result = true;
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
                if (Thing.IsNetworkUpdateRequired(4096U, networkUpdateType))
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
                if (Thing.IsNetworkUpdateRequired(4096U, networkUpdateType))
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
            if (Thing.Find(__instance.ThingId) is SprayCan sprayCan)
            {
                var paintMaterial = ColorCyclerModHelpers.GetPaintColor(__instance.ColorIndex);
                ColorCyclerModHelpers.UpdateSprayCan(sprayCan, paintMaterial);
            }
        }
    }
}
