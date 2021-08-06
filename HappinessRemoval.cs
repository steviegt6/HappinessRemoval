using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using TomatoLib;

namespace HappinessRemoval
{
    public class HappinessRemoval : TomatoMod
    {
        public override void Load()
        {
            IL.Terraria.Main.DrawNPCChatButtons += PatchOutButton;
            IL.Terraria.Chest.SetupShop += OverridePylonConditions;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DrawNPCChatButtons -= PatchOutButton;
            IL.Terraria.Chest.SetupShop -= OverridePylonConditions;
        }

        private void PatchOutButton(ILContext il)
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.NPCCheckHappiness")))
            {
                ModLogger.PatchFailure("Terraria.Main", "DrawNPCChatButtons", "ldstr", "UI.NPCCheckHappiness");
                return;
            }

            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldstr, string.Empty);
        }

        private void OverridePylonConditions(ILContext il)
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchStloc(0)))
            {
                ModLogger.PatchFailure("Terraria.Chest", "SetupShop", "stloc", "0");
                return;
            }

            c.EmitDelegate<Func<bool>>(() =>
            {
                if (ModContent.GetInstance<HappinessConfig>().OverridePylon)
                    return true;

                return Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.85000002384185791; // incredibly stupid number
            });

            c.Emit(OpCodes.Stloc_0); // flag
        }
    }

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

    public class HappinessPlayer : ModPlayer
    {
        public override void PreUpdate()
        {
            Player.currentShoppingSettings.PriceAdjustment = ModContent.GetInstance<HappinessConfig>().NpcHappiness;

            if (!Main.npcChatFocus4)
                return;

            Main.instance.MouseText("NPC happiness disabled by NPC Happiness Removal!");
            Main.mouseText = true;
        }
    }
}