using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.InteropServices;
using System;
using FFXIVClientStructs.FFXIV.Client.UI; 
using FFXIVClientStructs.FFXIV.Client.System.String;
using Dalamud.Utility.Signatures;
using DambyulK2J.Services;

namespace DambyulK2J
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "dambyul K2J AI Translator";

        internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        internal static IChatGui ChatGui { get; private set; } = null!;
        internal static ICommandManager CommandManager { get; private set; } = null!;
        internal static IFramework Framework { get; private set; } = null!;
        internal static IClientState ClientState { get; private set; } = null!;
        internal static IPartyList PartyList { get; private set; } = null!;
        internal static IObjectTable ObjectTable { get; private set; } = null!;
        internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }
        private TranslationService TranslationService { get; init; }

        private unsafe delegate void ProcessChatBoxDelegate(nint uiModule, nint message, nint unused, byte a4);

        // 7.x 버전용 시그니처
        [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9")]
        private readonly ProcessChatBoxDelegate? ProcessChatBox = null;

        public Plugin(
            IDalamudPluginInterface pluginInterface,
            IChatGui chatGui,
            ICommandManager commandManager,
            IFramework framework,
            IClientState clientState,
            IPartyList partyList,
            IObjectTable objectTable,
            IGameInteropProvider gameInteropProvider)
        {
            PluginInterface = pluginInterface;
            ChatGui = chatGui;
            CommandManager = commandManager;
            Framework = framework;
            ClientState = clientState;
            PartyList = partyList;
            ObjectTable = objectTable;
            GameInteropProvider = gameInteropProvider;
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            TranslationService = new TranslationService();
            PluginUi = new PluginUI(Configuration, TranslationService);

            GameInteropProvider.InitializeFromAttributes(this);

            CommandManager.AddHandler("/byul", new CommandInfo(OnCommand)
            {
                HelpMessage = "설정 창을 엽니다."
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            ChatGui.ChatMessage += OnChatMessage;

            _ = Task.Run(() => TranslationService.LoadDictionaryAsync());

            if (Configuration.IsFirstRun)
            {
                PluginUi.Visible = true;
                Configuration.IsFirstRun = false;
                Configuration.Save();
            }
        }

        private string ExtractPartyMembers()
        {
            if (PartyList.Length == 0) return "";
            StringBuilder sb = new StringBuilder();
            foreach (var member in PartyList)
            {
                sb.AppendLine($"- {member.Name.TextValue}");
            }
            return sb.ToString();
        }

        private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Configuration.IsEnabled) return;
            if ((int)type != Configuration.InputChannelId) return;
            
            var localPlayer = ObjectTable[0];
            if (localPlayer == null) return;
            
            if (!sender.TextValue.Equals(localPlayer.Name.TextValue, System.StringComparison.Ordinal)) return;

            string text = message.TextValue;
            if (string.IsNullOrWhiteSpace(text)) return;

            string partyInfo = ExtractPartyMembers();

            _ = Task.Run(async () => 
            {
                string translated = await TranslationService.TranslateAsync(text, Configuration.GeminiApiKey, partyInfo);
                
                if (translated.StartsWith("Error:"))
                {
                    ChatGui.Print(new XivChatEntry { Message = $"[K2J System] {translated}", Type = XivChatType.Echo });
                    return; 
                }

                lock(PluginUi.History)
                {
                    PluginUi.History.Insert(0, new HistoryItem 
                    { 
                        Original = text, 
                        Translated = translated, 
                        Time = System.DateTime.Now 
                    });
                }

                _ = Framework.RunOnTick(() => 
                {
                    SendTranslationOutput(translated);
                });
            });
        }

        private unsafe void SendTranslationOutput(string text)
        {
            int outId = Configuration.OutputChannelId;

            if (outId == 56)
            {
                ChatGui.Print(new XivChatEntry { Message = $"[K2J] {text}", Type = XivChatType.Echo });
            }
            else
            {
                string command = "";
                if (outId == (int)XivChatType.Party) command = $"/p {text}";
                else if (outId == (int)XivChatType.Ls1) command = $"/l1 {text}";
                else if (outId == (int)XivChatType.CrossLinkShell1) command = $"/cwl1 {text}";
                else command = $"/p {text}";

                try
                {
                    if (ProcessChatBox == null)
                    {
                        ChatGui.Print("[K2J Error] 채팅 함수 연결 실패. 업데이트가 필요합니다.");
                        return;
                    }

                    var uiModule = UIModule.Instance();
                    if (uiModule == null) return;

                    var utf8String = Utf8String.FromString(command);
                    ProcessChatBox((nint)uiModule, (nint)utf8String, nint.Zero, 0);
                    utf8String->Dtor();
                    
                    // 디버깅 출력
                    //ChatGui.Print($"[전송됨] {command}");
                }
                catch (Exception ex)
                {
                    ChatGui.Print($"[Error] 채팅 전송 실패: {ex.Message}");
                }
            }
        }

        private void OnCommand(string command, string args) => PluginUi.Visible = !PluginUi.Visible;
        private void DrawUI() => PluginUi.Draw();
        private void DrawConfigUI() => PluginUi.Visible = true;

        public void Dispose()
        {
            PluginUi.Dispose();
            TranslationService.Dispose();
            ChatGui.ChatMessage -= OnChatMessage;
            CommandManager.RemoveHandler("/byul");
        }
    }
}