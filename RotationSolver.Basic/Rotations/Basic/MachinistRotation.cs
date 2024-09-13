namespace RotationSolver.Basic.Rotations.Basic;

partial class MachinistRotation
{
    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Dexterity;

    #region Job Gauge
    /// <summary>
    /// 
    /// </summary>
    public static bool IsOverheated => JobGauge.IsOverheated;

    /// <summary>
    /// 
    /// </summary>
    public static byte Heat => JobGauge.Heat;

    /// <summary>
    /// 
    /// </summary>
    public static byte Battery => JobGauge.Battery;

    /// <summary>
    /// 
    /// </summary>
    public static byte LastSummonBatteryPower => JobGauge.LastSummonBatteryPower;

    static float OverheatTimeRemainingRaw => JobGauge.OverheatTimeRemaining / 1000f;

    /// <summary>
    /// 
    /// </summary>
    public static float OverheatTime => OverheatTimeRemainingRaw - DataCenter.DefaultGCDRemain;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    protected static bool OverheatedEndAfter(float time) => OverheatTime <= time;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gctCount"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    protected static bool OverheatedEndAfterGCD(uint gctCount = 0, float offset = 0)
        => OverheatedEndAfter(GCDTime(gctCount, offset));

    /// <inheritdoc/>
    public override void DisplayStatus()
    {
        ImGui.Text("IsOverheated: " + IsOverheated.ToString());
        ImGui.Text("Heat: " + Heat.ToString());
        ImGui.Text("Battery: " + Battery.ToString());
        ImGui.Text("LastSummonBatteryPower: " + LastSummonBatteryPower.ToString());
        ImGui.Text("OverheatTimeRemainingRaw: " + OverheatTimeRemainingRaw.ToString());
        ImGui.Text("OverheatTime: " + OverheatTime.ToString());
    }
    #endregion

    static partial void ModifySplitShotPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifySlugShotPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.HeatedSplitShotPvE, ActionID.SplitShotPvE];
    }

    static partial void ModifyHotShotPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyReassemblePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Reassembled];
        setting.ActionCheck = () => HasHostilesInRange;
    }

    static partial void ModifyGaussRoundPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifySpreadShotPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyCleanShotPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.SlugShotPvE, ActionID.HeatedSlugShotPvE];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyHyperchargePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !IsOverheated && (Heat >= 50 || Player.HasStatus(true, StatusID.Hypercharged));
        setting.StatusProvide = [StatusID.Overheated];
        setting.UnlockedByQuestID = 67233;
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyHeatBlastPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => IsOverheated && !OverheatedEndAfterGCD();
        setting.UnlockedByQuestID = 67234;
    }

    static partial void ModifyRookAutoturretPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Battery >= 50 && !JobGauge.IsRobotActive;
        setting.UnlockedByQuestID = 67235;
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyRookOverdrivePvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyRookOverloadPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyWildfirePvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Wildfire];
        setting.StatusProvide = [StatusID.Wildfire_1946];
        setting.ActionCheck = () => Heat >= 50 || Player.HasStatus(true, StatusID.Hypercharged);
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyRicochetPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67240;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyAutoCrossbowPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67242;
        setting.ActionCheck = () => IsOverheated && !OverheatedEndAfterGCD();
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyHeatedSplitShotPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67243;
    }

    static partial void ModifyTacticianPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67244;
        setting.StatusProvide = StatusHelper.PhysicalRangedResistance;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyDrillPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = StatusHelper.PhysicalRangedResistance;
        setting.UnlockedByQuestID = 67246;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHeatedSlugShotPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.HeatedSplitShotPvE];
        setting.UnlockedByQuestID = 67248;
    }

    static partial void ModifyDismantlePvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Dismantled];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHeatedCleanShotPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.HeatedSlugShotPvE];
    }

    static partial void ModifyBarrelStabilizerPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Hypercharged, StatusID.FullMetalMachinist];
        setting.ActionCheck = () => InCombat;
    }

    static partial void ModifyBlazingShotPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => IsOverheated && !OverheatedEndAfterGCD();
    }

    static partial void ModifyFlamethrowerPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !IsMoving;
        setting.UnlockedByQuestID = 68445;
        setting.IsFriendly = false;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 6,
        };
    }

    static partial void ModifyBioblasterPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Bioblaster, StatusID.Bioblaster_2019];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyAirAnchorPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyAutomatonQueenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Battery >= 50 && !JobGauge.IsRobotActive;
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyQueenOverdrivePvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyArmPunchPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyRollerDashPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyPileBunkerPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyScattergunPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyCrownedColliderPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 16,
        };
    }

    static partial void ModifyChainSawPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ExcavatorReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyDoubleCheckPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyCheckmatePvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyExcavatorPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Player.HasStatus(true, StatusID.ExcavatorReady);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFullMetalFieldPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !Player.HasStatus(true, StatusID.Reassembled) && Player.HasStatus(true, StatusID.FullMetalMachinist);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }


    /// <inheritdoc/>
    [RotationDesc(ActionID.TacticianPvE, ActionID.DismantlePvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction act)
    {
        if (TacticianPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (DismantlePvE.CanUse(out act, skipAoeCheck: true)) return true;
        return false;
    }
}
