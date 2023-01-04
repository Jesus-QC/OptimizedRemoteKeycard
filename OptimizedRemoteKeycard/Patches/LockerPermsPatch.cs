using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using MapGeneration.Distributors;
using NorthwoodLib.Pools;

namespace OptimizedRemoteKeyCard.Patches;

[HarmonyPatch(typeof(Locker), nameof(Locker.ServerInteract))]
public class LockerPermsPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        CodeInstruction c = newInstructions[newInstructions.FindIndex(x => x.operand is MethodInfo m && m == AccessTools.Method(typeof(Locker), nameof(Locker.CheckPerms)))];
        c.opcode = OpCodes.Call;
        c.operand = AccessTools.Method(typeof(LockerPermsPatch), nameof(CheckPerms));
        
        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;
        
        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static bool CheckPerms(Locker _, KeycardPermissions permissions, ReferenceHub ply)
    {
        if (permissions <= KeycardPermissions.None) 
            return true;
        
        if (!Player.TryGet(ply, out Player player))
            return true;
        
        foreach (Item i in player.Items)
        {
            ItemBase item = i.Base;
            
            if (item is not KeycardItem keycardItem)
                continue;

            if (keycardItem.Permissions.HasFlagFast(permissions))
                return true;
        }
        return false;
    }
}