using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public class PositiveConnectionsModSettingsWindow : Window
    {
        private Vector2 scrollPosition;

        private ModSettings modSettings;

        public PositiveConnectionsModSettingsWindow(ModSettings settings)
        {
            // Set the window properties
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            // Store the ModSettings instance
            modSettings = settings;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Define the rect for the content area
            Rect contentRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 30f);

            // Begin a scroll view if needed
            Widgets.BeginScrollView(contentRect, ref scrollPosition, contentRect);

            // Create the mod settings GUI elements
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(contentRect);

            // Add a checkbox for the gender adjustment setting
            listing.CheckboxLabeled("Enable Gender Adjustment", ref modSettings.EnableGenderAdjustment);

            listing.End();
            Widgets.EndScrollView();

            // Save the mod settings when the window is closed
            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 25f, inRect.width, 25f), "Save"))
            {
                modSettings.Write();
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            // Set the window size and position
            windowRect = new Rect(0f, 0f, 400f, 300f);
            windowRect.center = new Vector2(UI.screenWidth / 2f, UI.screenHeight / 2f);
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            // Save the mod settings when the window is closed
            modSettings.Write();
        }
    }
}
