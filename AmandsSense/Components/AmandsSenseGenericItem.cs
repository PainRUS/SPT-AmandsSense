using AmandsSense.Helpers;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

namespace AmandsSense.Components
{
    /// <summary>
    /// Spoiler-free loose-loot marker. It intentionally does not classify the
    /// underlying item by category, quest state, wishlist state, rarity, flea
    /// availability, Kappa status, or EFT background color.
    /// </summary>
    public class AmandsSenseGenericItem : AmandsSenseConstructor
    {
        private ObservedLootItem observedLootItem;

        public override void SetSense(ObservedLootItem ObservedLootItem)
        {
            observedLootItem = ObservedLootItem;
            if (observedLootItem == null || !observedLootItem.gameObject.activeSelf || observedLootItem.Item == null)
            {
                amandsSenseWorld?.CancelSense();
                return;
            }

            // Preserve upstream behavior: spent/zero-durability repairable items
            // are not valid loose-loot Sense targets.
            if (observedLootItem.Item.TryGetItemComponent(out RepairableComponent repairableComponentCheck)
                && (int)repairableComponentCheck.Durability == 0)
            {
                amandsSenseWorld.CancelSense();
                return;
            }

            if (observedLootItem.ItemOwner != null)
            {
                observedLootItem.ItemOwner.RemoveItemEvent += RemoveLootItem;
            }

            color = Settings.ObservedLootItemColor.Value;
            if (AmandsSenseClass.LoadedSprites.ContainsKey("ObservedLootItem.png"))
            {
                sprite = AmandsSenseClass.LoadedSprites["ObservedLootItem.png"];
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = new Color(color.r, color.g, color.b, 0f);
            }

            if (light != null)
            {
                light.color = new Color(color.r, color.g, color.b, 1f);
                light.intensity = 0f;
                light.range = Settings.LightRange.Value;
            }

            // If the user switches the global Sense mode to OnText, keep this
            // policy spoiler-free instead of exposing type/name/resource data.
            ClearOptionalText();
            PlaySenseSound();
        }

        public override void UpdateSense()
        {
            if (observedLootItem == null || !observedLootItem.gameObject.activeSelf || observedLootItem.Item == null)
            {
                amandsSenseWorld?.CancelSense();
            }
        }

        public override void UpdateIntensity(float Intensity)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(color.r, color.g, color.b, color.a * Intensity);
            }

            if (light != null)
            {
                light.intensity = amandsSenseWorld.DisableGlow
                    ? 0f
                    : Settings.LightIntensity.Value * Intensity;
            }

            if (typeText != null)
            {
                typeText.color = new Color(color.r, color.g, color.b, 0f);
            }

            if (nameText != null)
            {
                nameText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            }

            if (descriptionText != null)
            {
                descriptionText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            }
        }

        public override void RemoveSense()
        {
            if (observedLootItem != null && observedLootItem.ItemOwner != null)
            {
                observedLootItem.ItemOwner.RemoveItemEvent -= RemoveLootItem;
            }
        }

        private void ClearOptionalText()
        {
            if (typeText != null)
            {
                typeText.text = string.Empty;
                typeText.color = new Color(color.r, color.g, color.b, 0f);
            }

            if (nameText != null)
            {
                nameText.text = string.Empty;
                nameText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            }

            if (descriptionText != null)
            {
                descriptionText.text = string.Empty;
                descriptionText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            }
        }

        private void PlaySenseSound()
        {
            if (Settings.SenseRareSound.Value && AmandsSenseClass.LoadedAudioClips.ContainsKey("SenseRare.wav"))
            {
                if (!Settings.SenseAlwaysOn.Value)
                {
                    Singleton<BetterAudio>.Instance.PlayAtPoint(
                        transform.position,
                        AmandsSenseClass.LoadedAudioClips["SenseRare.wav"],
                        Settings.AudioDistance.Value,
                        BetterAudio.AudioSourceGroupType.Environment,
                        Settings.AudioRolloff.Value,
                        Settings.AudioVolume.Value,
                        EOcclusionTest.Fast);
                }

                return;
            }

            if (!Settings.SenseAlwaysOn.Value)
            {
                AudioClip itemClip = Singleton<GUISounds>.Instance.GetItemClip(
                    observedLootItem.Item.ItemSound,
                    EInventorySoundType.pickup);

                if (itemClip != null)
                {
                    Singleton<BetterAudio>.Instance.PlayAtPoint(
                        transform.position,
                        itemClip,
                        Settings.AudioDistance.Value,
                        BetterAudio.AudioSourceGroupType.Environment,
                        Settings.AudioRolloff.Value,
                        Settings.AudioVolume.Value,
                        EOcclusionTest.Fast);
                }
            }
        }

        private void RemoveLootItem(RemoveItemEventArgs args)
        {
            if (args.Status != CommandStatus.Succeed)
            {
                return;
            }

            amandsSenseWorld.RemoveSense();
        }
    }
}
