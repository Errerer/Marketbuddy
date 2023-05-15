using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

namespace Marketbuddy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private Configuration conf => Configuration.GetOrLoad();

        private Marketbuddy marketbuddy;

        private bool _settingsVisible;

        // passing in the image here just for simplicity
        public PluginUI(Marketbuddy plugin)
        {
            marketbuddy = plugin;
            SettingsVisible = false;
        }

        public bool SettingsVisible
        {
            get => _settingsVisible;
            set => _settingsVisible = value;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            DrawSettingsWindow();
            DrawOverlayWindow();
        }

        private void DrawOverlayWindow()
        {
            if (!conf.AdjustMaxStackSizeInSellList ||
                !marketbuddy.MarketGuiEventHandler.AddonRetainerSellList_Position(out Vector2 position)) return;

            var windowVisible = true;
            ImGui.SetNextWindowPos(position);

            var hSpace = new Vector2(1, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, hSpace);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, hSpace);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, hSpace);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.One);
            if (ImGui.Begin("Marketbuddy_stacklimit", ref windowVisible,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse |
                    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground))
            {
                if (ImGui.Checkbox("将物品堆叠大小限制为 ", ref conf.UseMaxStackSize))
                    conf.Save();

                ImGui.SameLine();
                ImGui.SetNextItemWidth(30);
                if (ImGui.InputInt("物品", ref conf.MaximumStackSize, 0))
                    MaximumStackSizeChanged();

                ImGui.SameLine();
                ImGui.Dummy(new(20, 1));

                ImGui.SameLine();
                ImGui.SetNextItemWidth(30);
                if (conf.UndercutUsePercent)
                {
                    if (ImGui.InputInt("##percundercut", ref conf.UndercutPercent, 0))
                        UndercutPriceChanged();
                }
                else
                {
                    if (ImGui.InputInt("##gilundercut", ref conf.UndercutPrice, 0))
                        UndercutPriceChanged();
                }
                ImGui.SameLine();
                ImGui.SetNextItemWidth(40);
                DrawUndercutTypeSelector();
                ImGui.SameLine();
                ImGui.Text("低于最低价格");
            }

            ImGui.PopStyleVar(5);
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            if (!ImGui.Begin("Marketbuddy设置", ref _settingsVisible,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.End();
                return;
            }

            if (ImGui.Checkbox("自动查看价格并修改价格", ref conf.AutoOpenComparePrices))
            {
                if (!conf.AutoOpenComparePrices)
                    conf.HoldShiftToStop = false;
                conf.Save();
            }


            DrawNestIndicator(1);
            if (ImGui.Checkbox(
                    $"按住SHIFT {(conf.AutoOpenComparePrices ? "阻止上述操作" : "阻止上述操作")}",
                    ref conf.HoldShiftToStop))
                conf.Save();


            ImGui.Spacing();
            if (ImGui.Checkbox("按住 CTRL 从剪切板粘贴价格并确认出售。",
                    ref conf.HoldCtrlToPaste))
                conf.Save();

            ImGui.Spacing();
            if (ImGui.Checkbox("查看当前价格时，打开市场中最近的交易履历。", ref conf.AutoOpenHistory))
                conf.Save();


            DrawNestIndicator(1);
            if (ImGui.Checkbox($"按住 ALT {(conf.AutoOpenHistory ? "阻止上述操作" : "阻止上述操作")}",
                    ref conf.HoldAltHistoryHandling))
                conf.Save();

            ImGui.Spacing();
            ImGui.SetNextItemWidth(45);
            if (conf.UndercutUsePercent)
            {
                if (ImGui.InputInt("##percundercut", ref conf.UndercutPercent, 0))
                    UndercutPriceChanged();
            }
            else
            {
                if (ImGui.InputInt("##gilundercut", ref conf.UndercutPrice, 0))
                    UndercutPriceChanged();
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(55);
            DrawUndercutTypeSelector();
            ImGui.SameLine();
            ImGui.TextUnformatted("设定压价金额");

            DrawNestIndicator(1);
            if (ImGui.Checkbox(
                    $"点击市场中的时价价格,将价格-{GetUndercutText()} 后复制到剪切板。",
                    ref conf.SaveToClipboard))
                conf.Save();

            DrawNestIndicator(1);
            if (ImGui.Checkbox(
                    $"点击市场中的时价价格,将价格-{GetUndercutText()}后设定为你的价格",
                    ref conf.AutoInputNewPrice))
            {
                if (!conf.AutoInputNewPrice)
                    conf.AutoConfirmNewPrice = false;
                conf.Save();
            }

            DrawNestIndicator(2);
            if (!conf.AutoInputNewPrice) PushStyleDisabled();
            if (ImGui.Checkbox(
                    "选择市场中的时价价格后,关闭市场窗口并确认修改价格",
                    ref conf.AutoConfirmNewPrice))
            {
                if (!conf.AutoInputNewPrice)
                    conf.AutoConfirmNewPrice = false;
                conf.Save();
            }

            if (!conf.AutoInputNewPrice) PopStyleDisabled();

            ImGui.Spacing();
            if (ImGui.Checkbox("限制上架物品最大数量", ref conf.UseMaxStackSize))
                conf.Save();

            ImGui.SameLine();
            ImGui.SetNextItemWidth(45);
            if (ImGui.InputInt("个物品", ref conf.MaximumStackSize, 0))
                MaximumStackSizeChanged();

            DrawNestIndicator(1);
            if (ImGui.Checkbox("调整雇员的出售品列表(上架)最大堆叠数量的UI位置",
                    ref conf.AdjustMaxStackSizeInSellList))
                conf.Save();

            if (conf.AdjustMaxStackSizeInSellList)
            {
                DrawNestIndicator(2);
                if (ImGui.DragFloat2("位置（相对于左上角）", ref conf.AdjustMaxStackSizeInSellListOffset,
                        1f, 1, float.MaxValue, "%.0f"))
                    conf.Save();
            }

            ImGui.End();
        }

        private void DrawUndercutTypeSelector()
        {
            if (ImGui.BeginCombo("##undercuttype", conf.UndercutUsePercent ? "%" : "gil"))
            {
                if (ImGui.Selectable("固定以低于多少金币的价格出售")) conf.UndercutUsePercent = false;
                if (ImGui.Selectable("固定以低于多少百分比的价格出售")) conf.UndercutUsePercent = true;
                ImGui.EndCombo();
            }
        }

        private string GetUndercutText(bool escape = false)
        {
            if (conf.UndercutUsePercent)
            {
                return $"{conf.UndercutPercent}%" + (escape?"%":"");
            }
            else
            {
                return $"{conf.UndercutPrice}gil";
            }
        }

        private void MaximumStackSizeChanged()
        {
            conf.MaximumStackSize = conf.MaximumStackSize <= 9999
                ? conf.MaximumStackSize >= 1 ? conf.MaximumStackSize : 1
                : 9999;
            conf.Save();
        }

        private void UndercutPriceChanged()
        {
            if (conf.UndercutPrice < 0)
                conf.UndercutPrice = 0;
            if (conf.UndercutPercent > 99) conf.UndercutPercent = 99;
            if (conf.UndercutPercent < 0) conf.UndercutPercent = 0;
            conf.Save();
        }

        private static void DrawNestIndicator(int depth)
        {
            // https://github.com/DelvUI/DelvUI/blob/62b28ce1901f374ec167c26ce9fcf3afaf2adb13/DelvUI/Config/Tree/FieldNode.cs#L58

            // This draws the L shaped symbols and padding to the left of config items collapsible under a checkbox.
            // Shift cursor to the right to pad for children with depth more than 1.
            // 26 is an arbitrary value I found to be around half the width of a checkbox
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(26, 0) * Math.Max((depth - 1), 0));

            var color = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];

            ImGui.TextColored(new Vector4(color.X, color.Y, color.Z, 0.9f), "\u2002\u2514");
            //ImGui.TextColored(new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f), "\u2002\u2514");
            ImGui.SameLine();
        }

        private static void PushStyleDisabled()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
        }

        private static void PopStyleDisabled()
        {
            ImGui.PopStyleVar();
        }
    }
}
