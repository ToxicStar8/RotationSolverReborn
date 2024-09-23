﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Ipc;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver;

public static class Watcher
{
    private static ICallGateSubscriber<object, object>? IpcSubscriber;

    public static void Enable()
    {
        IpcSubscriber = Svc.PluginInterface.GetIpcSubscriber<object, object>("PingPlugin.Ipc");

        ActionEffect.ActionEffectEvent += ActionFromEnemy;
        ActionEffect.ActionEffectEvent += ActionFromSelf;
    }

    public static void Disable()
    {
        ActionEffect.ActionEffectEvent -= ActionFromEnemy;
        ActionEffect.ActionEffectEvent -= ActionFromSelf;
    }

    public static string ShowStrSelf { get; private set; } = string.Empty;
    public static string ShowStrEnemy { get; private set; } = string.Empty;

    private static void ActionFromEnemy(ActionEffectSet set)
    {
        try
        {
            // Check Source.
            var source = set.Source;
            if (source == null) return;
            if (source is not IBattleChara battle) return;
            if (battle is IPlayerCharacter) return;
            const int FriendSubKind = 9;
            if (battle.SubKind == FriendSubKind) return; // Friend!
            if (Svc.Objects.SearchById(battle.GameObjectId) is IPlayerCharacter) return;

            var playerObject = Player.Object;
            if (playerObject == null) return;

            float damageRatio = 0;
            foreach (var effect in set.TargetEffects)
            {
                if (effect.TargetID == playerObject.GameObjectId)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var entry = effect[i];
                        if (entry.type == ActionEffectType.Damage)
                        {
                            damageRatio += (float)entry.value / playerObject.MaxHp;
                        }
                    }
                }
            }

            DataCenter.AddDamageRec(damageRatio);

            ShowStrEnemy = $"Damage Ratio: {damageRatio}\n{set}";

            foreach (var effect in set.TargetEffects)
            {
                if (effect.TargetID != playerObject.GameObjectId) continue;
                if (effect.GetSpecificTypeEffect(ActionEffectType.Knockback, out var entry))
                {
                    var knock = Svc.Data.GetExcelSheet<Knockback>()?.GetRow(entry.value);
                    if (knock != null)
                    {
                        DataCenter.KnockbackStart = DateTime.UtcNow;
                        DataCenter.KnockbackFinished = DateTime.UtcNow + TimeSpan.FromSeconds(knock.Distance / (float)knock.Speed);
                        if (set.Action != null && !OtherConfiguration.HostileCastingKnockback.Contains(set.Action.RowId) && Service.Config.RecordKnockbackies)
                        {
                            OtherConfiguration.HostileCastingKnockback.Add(set.Action.RowId);
                            OtherConfiguration.Save();
                        }
                    }
                    break;
                }
            }

            if (set.Header.ActionType == ActionType.Action && DataCenter.PartyMembers.Length >= 4 && set.Action?.Cast100ms > 0)
            {
                var type = set.Action.GetActionCate();

                if (type is ActionCate.Spell or ActionCate.Weaponskill or ActionCate.Ability)
                {
                    int partyMemberCount = DataCenter.PartyMembers.Length;
                    int damageEffectCount = 0;

                    foreach (var effect in set.TargetEffects)
                    {
                        if (DataCenter.PartyMembers.Any(p => p.GameObjectId == effect.TargetID) &&
                            effect.GetSpecificTypeEffect(ActionEffectType.Damage, out var damageEffect) &&
                            (damageEffect.value > 0 || (damageEffect.param0 & 6) == 6))
                        {
                            damageEffectCount++;
                        }
                    }

                    if (damageEffectCount == partyMemberCount)
                    {
                        if (Service.Config.RecordCastingArea)
                        {
                            OtherConfiguration.HostileCastingArea.Add(set.Action.RowId);
                            OtherConfiguration.SaveHostileCastingArea();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in ActionFromEnemy: {ex}");
        }
    }


    private static void ActionFromSelf(ActionEffectSet set)
    {
        try
        {
            var playerObject = Player.Object;
            if (set.Source == null || playerObject == null) return;
            if (set.Source.GameObjectId != playerObject.GameObjectId) return;
            if (set.Header.ActionType != ActionType.Action && set.Header.ActionType != ActionType.Item) return;
            if (set.Action == null) return;
            if ((ActionCate)set.Action.ActionCategory.Value!.RowId == ActionCate.Autoattack) return;

            var id = set.Action.RowId;
            if (!set.Action.IsRealGCD() && (set.Action.ClassJob.Row > 0 || Enum.IsDefined((ActionID)id)))
            {
                OtherConfiguration.AnimationLockTime[id] = set.Header.AnimationLockTime;
            }

            if (!set.TargetEffects.Any()) return;

            var action = set.Action;
            var tar = set.Target;

            // Record
            DataCenter.AddActionRec(action);
            ShowStrSelf = set.ToString();

            DataCenter.HealHP = set.GetSpecificTypeEffect(ActionEffectType.Heal);
            DataCenter.ApplyStatus = set.GetSpecificTypeEffect(ActionEffectType.ApplyStatusEffectTarget);
            foreach (var effect in set.GetSpecificTypeEffect(ActionEffectType.ApplyStatusEffectSource))
            {
                DataCenter.ApplyStatus[effect.Key] = effect.Value;
            }
            DataCenter.MPGain = (uint)set.GetSpecificTypeEffect(ActionEffectType.MpGain)
                .Where(i => i.Key == playerObject.GameObjectId)
                .Sum(i => i.Value);
            DataCenter.EffectTime = DateTime.UtcNow;
            DataCenter.EffectEndTime = DateTime.UtcNow.AddSeconds(set.Header.AnimationLockTime + 1);

            var attackedTargets = DataCenter.AttackedTargets;
            var attackedTargetsCount = DataCenter.AttackedTargetsCount;

            foreach (var effect in set.TargetEffects)
            {
                if (!effect.GetSpecificTypeEffect(ActionEffectType.Damage, out _)) continue;

                // Check if the target is already in the attacked targets list
                bool targetExists = false;
                foreach (var target in attackedTargets)
                {
                    if (target.id == effect.TargetID)
                    {
                        targetExists = true;
                        break;
                    }
                }
                if (targetExists) continue;

                // Ensure the current target is not dequeued
                while (attackedTargets.Count >= attackedTargetsCount)
                {
                    var oldestTarget = attackedTargets.Peek();
                    if (oldestTarget.id == effect.TargetID)
                    {
                        // If the oldest target is the current target, break the loop to avoid dequeuing it
                        break;
                    }
                    attackedTargets.Dequeue();
                }

                // Enqueue the new target
                attackedTargets.Enqueue((effect.TargetID, DateTime.UtcNow));
            }

            // Macro
            var regexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            var events = Service.Config.Events;
            foreach (var item in events)
            {
                if (!Regex.IsMatch(action.Name, item.Name, regexOptions)) continue;
                if (item.AddMacro(tar)) break;
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in ActionFromSelf: {ex}");
        }
    }

}