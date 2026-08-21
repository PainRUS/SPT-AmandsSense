using AmandsSense.Helpers;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;

namespace AmandsSense.Components
{
    /// <summary>
    /// Content-blind container marker. It never reads ItemOwner.RootItem grids,
    /// contained items, item count, wishlist state, rarity, flea availability,
    /// or Kappa status.
    /// </summary>
    public class AmandsSenseBlindContainer : AmandsSenseConstructor
    {
        private LootableContainer lootableContainer;
        private bool drawer;

        public override void SetSense(LootableContainer LootableContainer)
        {
            lootableContainer = LootableContainer;
            if (lootableContainer == null || !lootableContainer.gameObject.activeSelf)
            {
                amandsSenseWorld?.CancelSense();
                return;
            }

            drawer = amandsSenseWorld.SenseWorldType == Enums.SenseWorldType.Drawer;
            color = Settings.LootableContainerColor.Value;

            if (AmandsSenseClass.LoadedSprites.ContainsKey("LootableContainer.png"))
            {
                sprite = AmandsSenseClass.LoadedSprites["LootableContainer.png"];
            }

            ApplyVisuals(initial: true);
            ClearOptionalText();
            PlaySenseSound();
        }

        public override void UpdateSense()
        {
            if (lootableContainer == null || !lootableContainer.gameObject.activeSelf)
            {
                amandsSenseWorld?.CancelSense();
                return;
            }

            // Re-read only user-facing marker configuration, never container data.
            color = Settings.LootableContainerColor.Value;
            ApplyVisuals(initial: false);
            ClearOptionalText();
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
                    : Settings.LightIntensity.Value * Intensity * (drawer ? 0.25f : 1f);
            }

            // Text is intentionally suppressed even when global Sense is OnText.
            ClearOptionalText();
        }

        private void ApplyVisuals(bool initial)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                float alpha = initial ? 0f : spriteRenderer.color.a;
                spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
            }

            if (light != null)
            {
                light.color = new Color(color.r, color.g, color.b, 1f);
                if (initial)
                {
                    light.intensity = 0f;
                }
                light.range = Settings.LightRange.Value;
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
                        Settings.ContainerAudioVolume.Value,
                        EOcclusionTest.Fast);
                }

                return;
            }

            if (!Settings.SenseAlwaysOn.Value && !drawer && lootableContainer.OpenSound.Length > 0)
            {
                AudioClip openSound = lootableContainer.OpenSound[0];
                if (openSound != null)
                {
                    Singleton<BetterAudio>.Instance.PlayAtPoint(
                        transform.position,
                        openSound,
                        Settings.AudioDistance.Value,
                        BetterAudio.AudioSourceGroupType.Environment,
                        Settings.AudioRolloff.Value,
                        Settings.ContainerAudioVolume.Value,
                        EOcclusionTest.Fast);
                }
            }
        }
    }
}
