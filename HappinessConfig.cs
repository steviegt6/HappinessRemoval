using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace HappinessRemoval
{
    [Label("Happiness Config")]
    public class HappinessConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Happiness")]
        [Label("NPC Happiness Level (Lower = Better)")]
        [Slider]
        [DefaultValue(0.75f)]
        [Range(0.5f, 2f)]
        public float NpcHappiness;

        [Label("Override Pylon Happiness")]
        [Tooltip("Forces an NPC to sell a pylon regardless of happiness if true.")]
        [DefaultValue(true)]
        public bool OverridePylon;

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
                if (Netplay.Clients[i].State == 10 && Main.player[i] == Main.player[whoAmI] && Netplay.Clients[i].Socket.GetRemoteAddress().IsLocalHost())
                    return true;

            message = "You are not the server host!";
            return false;
        }
    }
}