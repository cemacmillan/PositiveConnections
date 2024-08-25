using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public class PositiveConnectionsModSettingsWindow : Window
    {
        private Vector2 scrollPosition;
        public PositiveConnectionsModSettings modSettings;

        public PositiveConnectionsModSettingsWindow(PositiveConnectionsModSettings settings)
        {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            modSettings = settings;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect contentRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 30f);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, contentRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(contentRect);

            listing.CheckboxLabeled("Enable Gender Adjustment", ref modSettings.EnableGenderAdjustment);
            listing.CheckboxLabeled("Hardcore: Disable notifications and messages from this mod (but enable all other effects)", ref modSettings.DisableAllMessages);

            listing.Label("Base Interaction Frequency");

            float minValue = 0.25f;
            float maxValue = 10f;
            float sliderValue = Mathf.InverseLerp(minValue, maxValue, modSettings.BaseInteractionFrequency);

            float newSliderValue = listing.Slider(sliderValue, 0f, 1f);
            modSettings.BaseInteractionFrequency = Mathf.Lerp(minValue, maxValue, newSliderValue);

            listing.Label(modSettings.BaseInteractionFrequency.ToString("F2"));

            // Add the new checkboxes
            listing.CheckboxLabeled("Stop All Pawn Interactions From This Mod", ref modSettings.StopInteractions);
            listing.CheckboxLabeled("Enable Rare Interactions", ref modSettings.EnableRareInteractions);
            listing.CheckboxLabeled("Disable Certain Interactions", ref modSettings.DisableCertainInteractions);
            listing.CheckboxLabeled("Enable Debugging Logs", ref modSettings.EnableLogging);
            listing.End();
            Widgets.EndScrollView();

            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 25f, inRect.width, 25f), "Save"))
            {
                modSettings.Write();
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            windowRect = new Rect(0f, 0f, 400f, 300f);
            windowRect.center = new Vector2(UI.screenWidth / 2f, UI.screenHeight / 2f);
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            modSettings.Write();
        }
    }
}