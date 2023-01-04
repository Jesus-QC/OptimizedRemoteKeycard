using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using NorthwoodLib.Pools;

namespace OptimizedRemoteKeyCard.Patches;

[HarmonyPatch("InteractingDoor", "Prefix")]
public class DoorPermsPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        CodeInstruction c = newInstructions[newInstructions.FindIndex(x => x.operand is MethodInfo m && m == AccessTools.Method(typeof(DoorPermissions), nameof(DoorPermissions.CheckPermissions)))];
        c.opcode = OpCodes.Call;
        c.operand = AccessTools.Method(typeof(DoorPermsPatch), nameof(CheckPerms));
        
        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;
        
        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static bool CheckPerms(DoorPermissions permissions, ItemBase _, ReferenceHub ply)
    {
        if (permissions.RequiredPermissions == KeycardPermissions.None)
            return true;

        if (ply is not null && ply.serverRoles.BypassMode)
            return true;

        if (!Player.TryGet(ply, out Player player))
            return true;

        foreach (Item i in player.Items)
        {
            ItemBase item = i.Base;
            
            if(item is not KeycardItem keycardItem)
                continue;

            if ((!permissions.RequireAll && (keycardItem.Permissions & permissions.RequiredPermissions) > KeycardPermissions.None) || (keycardItem.Permissions & permissions.RequiredPermissions) == permissions.RequiredPermissions)
                return true;
        }

        return player.IsScp && permissions.RequiredPermissions.HasFlagFast(KeycardPermissions.ScpOverride);
    }
}