using Assets.Scripts;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using UnityEngine;

namespace ColorCycler
{

    internal static class ColorCyclerModHelpers
    {
        public static int GetPaintColorIndex(UnityEngine.Material paintMaterial)
        {
            for (int i = 0; i < GameManager.Instance.CustomColors.Count; i++)
            {
                if (GameManager.Instance.CustomColors[i].Normal == paintMaterial)
                {
                    return i;
                }
            }
            return -1;
        }

        public static UnityEngine.Material GetPaintColor(int colorIndex)
        {
            return GameManager.Instance.CustomColors[colorIndex].Normal;
        }

        public static void UpdateSprayCan(SprayCan sprayCan, UnityEngine.Material paintMaterial)
        {
            ColorCyclerBep.Logger.LogMessage($"{nameof(ColorCyclerBep.Settings.ShouldCreatePollution)}: {ColorCyclerBep.Settings.ShouldCreatePollution}");
            ColorCyclerBep.Logger.LogMessage($"{nameof(ColorCyclerBep.Settings.InfinitePaint)}: {ColorCyclerBep.Settings.InfinitePaint}");
            sprayCan.PaintableMaterial = paintMaterial;
            sprayCan.PaintMaterial = sprayCan.PaintableMaterial;
            foreach (Thing thing in Prefab.AllPrefabs)
            {
                if (thing is SprayCan sprayCan2)
                {
                    if (sprayCan2.PaintMaterial == sprayCan.PaintMaterial)
                    {
                        sprayCan.SetPrefab(sprayCan2);
                        // Update Hash so that sorting and logic will remain
                        sprayCan.PrefabHash = sprayCan2.PrefabHash;
                        // Update "real" name
                        sprayCan.PrefabName = sprayCan2.PrefabName;
                        sprayCan.Thumbnail = sprayCan2.Thumbnail;
                        // Update Mesh such that thrown model appears correctly
                        if (sprayCan.GetComponent<MeshRenderer>() is MeshRenderer mr)
                        {
                            mr.sharedMaterial = sprayCan.PaintMaterial;
                        }
                        ColorCyclerBep.Logger.LogDebug($"Previous SprayCan Quantity: {sprayCan.Quantity}");
                        if (ColorCyclerBep.Settings.InfinitePaint)
                        {
                            ColorCyclerBep.Logger.LogDebug($"Setting Quantity to MaxQuantity.");
                            sprayCan.Quantity = sprayCan.MaxQuantity;
                        }
                        break;
                    }
                }
            }

            if (NetworkManager.IsServer)
            {
                sprayCan.NetworkUpdateFlags |= 4096;
            }
        }
    }
}
