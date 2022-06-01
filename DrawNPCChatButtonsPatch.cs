using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using TeaFramework.Features.Patching;
using TeaFramework.Features.Utility;
using Terraria;

namespace HappinessRemoval
{
    public class DrawNPCChatButtonsPatch : Patch<ILContext.Manipulator>
    {
        public override MethodInfo ModifiedMethod { get; } = typeof(Main).GetCachedMethod("DrawNPCChatButtons");

        protected override ILContext.Manipulator PatchMethod { get; } = il =>
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdstr("UI.NPCCheckHappiness"))) {
                ModLogger.PatchFailure("Terraria.Main", "DrawNPCChatButtons", "ldstr", "UI.NPCCheckHappiness");
                return;
            }

            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldstr, string.Empty);
        };
    }
}