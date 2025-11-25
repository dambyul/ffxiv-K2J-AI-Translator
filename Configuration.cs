using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace DambyulK2J
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool IsFirstRun { get; set; } = true; 
        public bool IsEnabled { get; set; } = true; 
        public string GeminiApiKey { get; set; } = string.Empty;
        public int InputChannelId { get; set; } = 2057; 
        public int OutputChannelId { get; set; } = 56; 

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}