using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace HappinessRemoval
{
    public class HappinessRemoval : Mod
    {
        public enum HappinessType
        {
            Unhappy,
            Dislikes,
            Neutral,
            Likes,
            Happy
        }

        public static double GetPriceAdjustment(HappinessType happinessType)
        {
            switch (happinessType)
            {
                case HappinessType.Unhappy:
                    return 1.2;

                case HappinessType.Dislikes:
                    return 1.1;

                case HappinessType.Neutral:
                default:
                    return 1.0;

                case HappinessType.Likes:
                    return 0.9;

                case HappinessType.Happy:
                    return 0.8;
            }
        }

        public override void Load()
        {
            IL.Terraria.Main.DrawNPCChatButtons += Main_DrawNPCChatButtons;
            IL.Terraria.Main.GUIChatDrawInner += Main_GUIChatDrawInner;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DrawNPCChatButtons -= Main_DrawNPCChatButtons;
            IL.Terraria.Main.GUIChatDrawInner -= Main_GUIChatDrawInner;
        }

        private void Main_DrawNPCChatButtons(ILContext il)
        {
            FieldInfo happinessText = typeof(HappinessRemoval).GetField("HappinessText", BindingFlags.Public | BindingFlags.Static);
            MethodInfo get_Black = typeof(Color).GetMethod("get_Black", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(i => i.MatchLdstr("UI.NPCCheckHappiness")))
            {
                Logger.Error("UI.NPCCheckHappiness");
                return;
            }

            if (!c.TryGotoNext(i => i.MatchLdloca(9)))
            {
                Logger.Error("Ldloca 9");
                return;
            }

            c.Index++;

            c.RemoveRange(9);

            c.Emit(OpCodes.Ldc_I4, 100);
            c.Emit(OpCodes.Ldc_I4, 100);
            c.Emit(OpCodes.Ldc_I4, 100);

            if (!c.TryGotoNext(i => i.MatchLdloc(11)))
            {
                Logger.Error("Ldloc 11");
                return;
            }

            if (!c.TryGotoNext(i => i.MatchLdloc(15)))
            {
                Logger.Error("Ldloc 15");
                return;
            }

            c.Index++;

            c.Emit(OpCodes.Pop);

            c.Emit(OpCodes.Call, get_Black);
        }

        private void Main_GUIChatDrawInner(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.Main", "npcChatFocus4")))
            {
                Logger.Error("Terraria.Main npcChatFocus4");
                return;
            }

            if (!c.TryGotoNext(i => i.MatchPop()))
            {
                Logger.Error("Pop");
                return;
            }

            c.Index++;

            c.RemoveRange(6);
        }
    }

    [Label("Happiness Config")]
    public class HappinessConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Happiness")]
        [Label("NPC Happiness Level")]
        [Tooltip("All NPCs will share the same level of happiness!" +
            "\nPrice modifications depending on happiness type:" +
            "\nUnhappy: 120% modifier" +
            "\nDislikes: 110% modifier" +
            "\nNeutral: 100% modifier (no change)" +
            "\nLikes: 90% modifier" +
            "\nHappy: 80% modifier (Pylon!)")]
        [Slider]
        [DrawTicks]
        public HappinessRemoval.HappinessType npcHappiness;

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
            player.currentShoppingSettings.PriceAdjustment = HappinessRemoval.GetPriceAdjustment(ModContent.GetInstance<HappinessConfig>().npcHappiness);

            if (Main.npcChatFocus4)
            {
                Main.instance.MouseText("NPC happiness disabled by NPC Happiness Removal!");
                Main.mouseText = true;
            }
        }
    }
}