using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Bindings.ImGui; 
using DambyulK2J.Services;

namespace DambyulK2J
{
    public class HistoryItem
    {
        public string Original { get; set; } = "";
        public string Translated { get; set; } = "";
        public DateTime Time { get; set; }
        public bool Reported { get; set; }
    }

    public class PluginUI : IDisposable
    {
        private Configuration configuration;
        private TranslationService translationService;
        
        public bool Visible = false;
        
        public List<HistoryItem> History { get; } = new();

        private bool showReportPopup = false;
        private HistoryItem? currentReportItem = null;
        private string reportReason = "";

        private readonly List<(string Name, XivChatType Type)> channelList = new()
        {
            ("사용 안 함 (Disable)", XivChatType.None),
            ("테스트용: 에코 (Echo)", XivChatType.Echo),
            ("파티 (Party)", XivChatType.Party),
            ("링크셸 1 (Linkshell 1)", XivChatType.Ls1),
            ("링크셸 2 (Linkshell 2)", XivChatType.Ls2),
            ("초월 링크셸 1 (CWLS 1)", XivChatType.CrossLinkShell1),
            ("초월 링크셸 2 (CWLS 2)", XivChatType.CrossLinkShell2),
        };
        private string[] channelNames;

        public PluginUI(Configuration configuration, TranslationService service)
        {
            this.configuration = configuration;
            this.translationService = service;
            channelNames = channelList.Select(c => c.Name).ToArray();
        }

        public void Draw()
        {
            if (!Visible) return;

            ImGui.SetNextWindowSize(new Vector2(480, 550), ImGuiCond.FirstUseEver);
            
            if (ImGui.Begin("dambyul K2J Translator Config", ref this.Visible))
            {
                if (ImGui.BeginTabBar("Tabs"))
                {
                    if (ImGui.BeginTabItem("설정 (Settings)"))
                    {
                        DrawSettings();
                        ImGui.EndTabItem();
                    }
                    if (ImGui.BeginTabItem("번역 기록 (History)"))
                    {
                        DrawHistory();
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                }
                ImGui.End();
            }
            DrawReportPopup();
        }

        private void DrawSettings()
        {
            var enabled = configuration.IsEnabled;
            if (ImGui.Checkbox("플러그인 활성화 (Enable)", ref enabled))
            {
                configuration.IsEnabled = enabled;
                configuration.Save();
            }

            ImGui.Separator();
            ImGui.Text("1. Google Gemini API Key");
            var apiKey = configuration.GeminiApiKey;
            if (ImGui.InputText("API Key", ref apiKey, 100, ImGuiInputTextFlags.Password))
            {
                configuration.GeminiApiKey = apiKey;
                configuration.Save();
            }

            ImGui.Separator();
            ImGui.Text("2. 채널 설정 (Channel Settings)");

            // Input
            int currentInputId = configuration.InputChannelId;
            int selectedInputIndex = channelList.FindIndex(c => (int)c.Type == currentInputId);
            if (selectedInputIndex == -1) selectedInputIndex = 0;

            if (ImGui.Combo("입력 채널 (Input Source)", ref selectedInputIndex, channelNames, channelNames.Length))
            {
                int newInputId = (int)channelList[selectedInputIndex].Type;
                if (newInputId != 0 && newInputId == configuration.OutputChannelId)
                { /* Do nothing */ }
                else
                {
                    configuration.InputChannelId = newInputId;
                    configuration.Save();
                }
            }

            // Output
            int currentOutputId = configuration.OutputChannelId;
            int selectedOutputIndex = channelList.FindIndex(c => (int)c.Type == currentOutputId);
            if (selectedOutputIndex == -1) selectedOutputIndex = 1; 

            if (ImGui.Combo("출력 채널 (Output Target)", ref selectedOutputIndex, channelNames, channelNames.Length))
            {
                int newOutputId = (int)channelList[selectedOutputIndex].Type;
                if (newOutputId == configuration.InputChannelId && configuration.InputChannelId != 0)
                { /* Do nothing */ }
                else
                {
                    configuration.OutputChannelId = newOutputId;
                    configuration.Save();
                }
            }

            if (configuration.InputChannelId != 0 && configuration.InputChannelId == configuration.OutputChannelId)
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "⚠️ 오류: 입력과 출력이 같습니다.");
            }

            if ((int)channelList[selectedOutputIndex].Type != (int)XivChatType.Echo)
            {
                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.8f, 0.0f, 1.0f));
                ImGui.TextWrapped("⚠️ 주의: 번역 결과가 즉시 노출됩니다. Echo 모드 테스트 권장.");
                ImGui.PopStyleColor();
            }
        }

        private void DrawHistory()
        {
            ImGui.BeginChild("HistoryList");
            foreach (var item in History)
            {
                ImGui.TextDisabled($"[{item.Time:HH:mm:ss}]");
                ImGui.Text($"KOR: {item.Original}");
                ImGui.TextColored(new Vector4(0.6f, 1f, 0.6f, 1f), $"JPN: {item.Translated}");

                if (!item.Reported)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton($"신고##{item.GetHashCode()}"))
                    {
                        currentReportItem = item;
                        reportReason = "";
                        showReportPopup = true;
                    }
                }
                else
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled("(신고됨)");
                }
                ImGui.Separator();
            }
            ImGui.EndChild();
        }

        private void DrawReportPopup()
        {
            if (showReportPopup) ImGui.OpenPopup("오번역 신고");

            if (ImGui.BeginPopupModal("오번역 신고", ref showReportPopup, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (currentReportItem != null)
                {
                    ImGui.Text($"원문: {currentReportItem.Original}");
                    ImGui.InputTextMultiline("사유", ref reportReason, 200, new Vector2(300, 100));
                    if (ImGui.Button("전송"))
                    {
                        _ = translationService.SendReportAsync(currentReportItem.Original, currentReportItem.Translated, reportReason);
                        currentReportItem.Reported = true;
                        showReportPopup = false;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("취소")) showReportPopup = false;
                }
                ImGui.EndPopup();
            }
        }

        public void Dispose() { }
    }
}