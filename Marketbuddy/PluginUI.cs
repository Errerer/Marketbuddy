﻿using System;
using ImGuiNET;

namespace Marketbuddy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private readonly Configuration configuration;
        private Plugin plugin;

        private bool settingsVisible;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible;

        // passing in the image here just for simplicity
        public PluginUI(Plugin plugin)
        {
            this.plugin = plugin;
            configuration = plugin.Configuration;
        }

        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible) return;

            if (ImGui.Begin("Marketbuddy", ref visible,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Nothing to show here at this time.");
                ImGui.End();
            }
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            if (ImGui.Begin("Marketbuddy config", ref settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                var AutoOpenComparePrices = configuration.AutoOpenComparePrices;
                if (ImGui.Checkbox("Open current prices list when adjusting a price", ref AutoOpenComparePrices))
                {
                    configuration.AutoOpenComparePrices = AutoOpenComparePrices;
                    configuration.Save();
                }

                var HoldShiftToStop = configuration.HoldShiftToStop;
                if (ImGui.Checkbox("Holding SHIFT prevents the above", ref HoldShiftToStop))
                {
                    configuration.HoldShiftToStop = HoldShiftToStop;
                    configuration.Save();
                }

                var HoldCtrlToPaste = configuration.HoldCtrlToPaste;
                if (ImGui.Checkbox("Holding CTRL pastes a price from the clipboard and confirms it",
                    ref HoldCtrlToPaste))
                {
                    configuration.HoldCtrlToPaste = HoldCtrlToPaste;
                    configuration.Save();
                }

                var AutoOpenHistory = configuration.AutoOpenHistory;
                if (ImGui.Checkbox("Open price history together with current prices list", ref AutoOpenHistory))
                {
                    configuration.AutoOpenHistory = AutoOpenHistory;
                    configuration.Save();
                }

                var AutoInputNewPrice = configuration.AutoInputNewPrice;
                if (ImGui.Checkbox("Clicking a price sets your price as that price with a 1gil undercut.",
                    ref AutoInputNewPrice))
                {
                    configuration.AutoInputNewPrice = AutoInputNewPrice;
                    configuration.Save();
                }

                var SaveToClipboard = configuration.SaveToClipboard;
                if (ImGui.Checkbox("Clicking a price copies that price with a 1gil undercut to the clipboard.",
                    ref SaveToClipboard))
                {
                    configuration.SaveToClipboard = SaveToClipboard;
                    configuration.Save();
                }

                var AutoConfirmNewPrice = configuration.AutoConfirmNewPrice;
                if (ImGui.Checkbox(
                    "Closes the list (if open) and confirms the new price after selecting it from the list (or if holding CTRL).",
                    ref AutoConfirmNewPrice))
                {
                    configuration.AutoConfirmNewPrice = AutoConfirmNewPrice;
                    configuration.Save();
                }
            }

            ImGui.End();
        }
    }
}