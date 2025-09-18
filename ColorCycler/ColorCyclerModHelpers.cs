using Assets.Scripts;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using UnityEngine;

namespace ColorCycler
{

    internal static class ColorCyclerModHelpers
    {
        internal const ushort PaintableMaterialNetworkFlag = 0x1000; //NetworkUpdateType.Thing.GenericFlag2

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
            if (colorIndex < 0 || colorIndex > GameManager.Instance.CustomColors.Count)
            {
                ColorCyclerBep.Logger.LogError($"{nameof(GetPaintColor)}: {nameof(colorIndex)} was {colorIndex:D} which is out of range.");
                colorIndex = 0;
            }
            return GameManager.Instance.CustomColors[colorIndex].Normal;
        }

        public static void UpdateSprayCan(SprayCan sprayCan, UnityEngine.Material paintMaterial)
        {
            ColorCyclerBep.Logger.LogDebug($"{nameof(ColorCyclerBep.Settings.ShouldCreatePollution)}: {ColorCyclerBep.Settings.ShouldCreatePollution}");
            ColorCyclerBep.Logger.LogDebug($"{nameof(ColorCyclerBep.Settings.InfinitePaint)}: {ColorCyclerBep.Settings.InfinitePaint}");
            ColorCyclerBep.Logger.LogDebug($"Updating {sprayCan.DisplayName} - {sprayCan.ReferenceId} to {paintMaterial.name} on {(NetworkManager.IsServer ? "Server" : "Client")}");
            sprayCan.PaintableMaterial = paintMaterial;
            sprayCan.PaintMaterial = sprayCan.PaintableMaterial;
            //TODO: Look up prefab hashes and cache for faster lookup
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
                sprayCan.NetworkUpdateFlags |= PaintableMaterialNetworkFlag;
            }
        }
    }
}
