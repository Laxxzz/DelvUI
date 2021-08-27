﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface
{
    public class WarriorHudWindow : HudWindow
    {
        public override uint JobId => 21;

        protected int StormsEyeHeight => PluginConfiguration.WARStormsEyeHeight;
        protected int StormsEyeWidth => PluginConfiguration.WARStormsEyeWidth;
        protected new int XOffset => PluginConfiguration.WARBaseXOffset;
        protected new int YOffset => PluginConfiguration.WARBaseYOffset;
        protected int BeastGaugeHeight => PluginConfiguration.WARBeastGaugeHeight;
        protected int BeastGaugeWidth => PluginConfiguration.WARBeastGaugeWidth;
        protected int BeastGaugePadding => PluginConfiguration.WARBeastGaugePadding;
        protected int BeastGaugeXOffset => PluginConfiguration.WARBeastGaugeXOffset;
        protected int BeastGaugeYOffset => PluginConfiguration.WARBeastGaugeYOffset;
        protected int InterBarOffset => PluginConfiguration.WARInterBarOffset;
        protected Dictionary<string, uint> InnerReleaseColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000];
        protected Dictionary<string, uint> StormsEyeColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 1];
        protected Dictionary<string, uint> FellCleaveColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 2];
        protected Dictionary<string, uint> NascentChaosColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 3];
        protected Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.WAR * 1000 + 4];

        public WarriorHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            var nextHeight = DrawStormsEyeBar(0);
            nextHeight = DrawBeastGauge(nextHeight);
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private int DrawStormsEyeBar(int initialHeight)
        {
            var innerReleaseBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1177);
            var stormsEyeBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 90);

            var barWidth = StormsEyeWidth;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + initialHeight;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(barWidth, StormsEyeHeight);
            
            var drawList = ImGui.GetWindowDrawList();

            var duration = 0f;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if (innerReleaseBuff.Any())
            {
                duration = Math.Abs(innerReleaseBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 10) * duration, barSize.Y),
                    InnerReleaseColor["gradientLeft"], InnerReleaseColor["gradientRight"], InnerReleaseColor["gradientRight"], InnerReleaseColor["gradientLeft"]
                );
            }
            else if (stormsEyeBuff.Any())
            {
                duration = Math.Abs(stormsEyeBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 60) * duration, barSize.Y),
                    StormsEyeColor["gradientLeft"], StormsEyeColor["gradientRight"], StormsEyeColor["gradientRight"], StormsEyeColor["gradientLeft"]
                );
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            var durationText = duration != 0 ? Math.Round(duration).ToString() : "";
            var textSize = ImGui.CalcTextSize(durationText);
            DrawOutlinedText(durationText, new Vector2(cursorPos.X + StormsEyeWidth / 2f - textSize.X / 2f, cursorPos.Y-2));

            return StormsEyeHeight + initialHeight;
        }

        private int DrawBeastGauge(int initialHeight) {
            var gauge = PluginInterface.ClientState.JobGauges.Get<WARGauge>();
            var nascentChaosBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1897);
            var nascentChaosDisplayed = nascentChaosBuff.Any();
            
            var barWidth = (BeastGaugeWidth - BeastGaugePadding) / 2;
            var xPos = CenterX - XOffset + BeastGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + InterBarOffset + BeastGaugeYOffset;
            var cursorPos = new Vector2(xPos + barWidth + BeastGaugePadding, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, BeastGaugeHeight);
            
            var drawList = ImGui.GetWindowDrawList();

            for (var i = 2; i >= 1; i--)
            {
                var beast = Math.Max(Math.Min(gauge.BeastGaugeAmount, chunkSize * i) - chunkSize * (i - 1), 0);
                var scale = (float) beast / chunkSize;
                
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

                if (scale >= 1.0f)
                {
                    var color = FellCleaveColor;
                    if (nascentChaosDisplayed)
                    {
                        color = NascentChaosColor;
                        nascentChaosDisplayed = false;
                    }
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BeastGaugeHeight),
                        color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                    );
                }
                else 
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BeastGaugeHeight), 
                        EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                    );
                }
            
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos = new Vector2(cursorPos.X - barWidth - BeastGaugePadding, cursorPos.Y);
            }

            return BeastGaugeHeight + initialHeight;
        }
    }
}