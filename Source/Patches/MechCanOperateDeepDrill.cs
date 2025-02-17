using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BetterGameLife.Source.Patches
{
    /// <summary>
    /// TEMPORARY 1.5 Allow Tunnelers To Drill
    /// </summary>
    [Harmony]
    internal class MechCanOperateDeepDrill
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.Inner(typeof(JobDriver_OperateDeepDrill), "<>c__DisplayClass1_0"), "<MakeNewToils>b__1");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            int insertTarget = 0;
            var codes = new List<CodeInstruction>(instructions);
            Label tagetJump = il.DefineLabel();

            //loop trought the list to find the line of code to change
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4)
                {
                    var strOperand = (float)codes[i].operand;

                    if (strOperand == 0.065f)
                    {
                        insertTarget = i - 3;
                        //label the return to allow jump to it later
                        codes[i + 4].labels.Add(tagetJump);
                        break;
                    }
                }
                //use 0.065 as an anchor as it is the only time it is use in the IL
            }

            //insert a conditional to the IL to prevent it for giving xp if the target does not have axp bar to begin with
            var instructionsToInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.skills))),
                new CodeInstruction(OpCodes.Brfalse_S, tagetJump)
            };
            //check if the code was able to find the target, if yes inject the code else send an error
            if (insertTarget != 0)
                codes.InsertRange(insertTarget, instructionsToInsert);
            else
                Log.Error("TunnulerFix: Could not find the target code to modify");

            return codes;
        }
    }
}