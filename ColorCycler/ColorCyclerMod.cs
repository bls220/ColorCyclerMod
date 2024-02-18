using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using HarmonyLib;
using JetBrains.Annotations;
using System.Collections;

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
                    int current = -1;
                    for (int i = 0; i < GameManager.Instance.CustomColors.Count; i++)
                    {
                        if (GameManager.Instance.CustomColors[i].Normal == sprayCan.PaintMaterial)
                        {
                            current = i;
                            break;
                        }
                    }
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
                        current = GameManager.Instance.CustomColors.Count-1;
                    }
                    sprayCan.PaintableMaterial = GameManager.Instance.CustomColors[current].Normal;
                    sprayCan.PaintMaterial = GameManager.Instance.CustomColors[current].Normal;
                    foreach (Thing thing in Prefab.AllPrefabs)
                    {
                        if (thing is SprayCan sprayCan2)
                        {
                            if (sprayCan2.PaintMaterial == sprayCan.PaintMaterial)
                            {
                                sprayCan.SetPrefab(sprayCan2);
                                sprayCan.RenameThing(sprayCan2.DisplayName);
                                sprayCan.Thumbnail = sprayCan2.Thumbnail;
                                __instance.ActiveHand.Slot.RefreshSlotDisplay();
                            }
                        }
                    }
                    sprayCan.Quantity = sprayCan.MaxQuantity;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SprayCan), "OnUseItem")]
    public class SprayCanOnUseItemPAtch
    {
        [UsedImplicitly]
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}