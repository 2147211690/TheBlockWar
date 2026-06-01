using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

[JsonDerivedType(typeof(ApplyAnimationStep), nameof(ApplyAnimationStep))]
[JsonDerivedType(typeof(ApplyBuffTurnStep), nameof(ApplyBuffTurnStep))]
[JsonDerivedType(typeof(ApplyCureStep), nameof(ApplyCureStep))]
[JsonDerivedType(typeof(ApplyDamageStep), nameof(ApplyDamageStep))]
[JsonDerivedType(typeof(ApplyDeathStep), nameof(ApplyDeathStep))]
[JsonDerivedType(typeof(ApplyDelayStep), nameof(ApplyDelayStep))]
[JsonDerivedType(typeof(ApplyDropTileStep), nameof(ApplyDropTileStep))]
[JsonDerivedType(typeof(ApplyDropUnitStep), nameof(ApplyDropUnitStep))]
[JsonDerivedType(typeof(ApplyModifyTileRegionStep), nameof(ApplyModifyTileRegionStep))]
[JsonDerivedType(typeof(ApplyModifyUnitBuffStep), nameof(ApplyModifyUnitBuffStep))]
[JsonDerivedType(typeof(ApplyMoveUnitStep), nameof(ApplyMoveUnitStep))]
[JsonDerivedType(typeof(ApplyMoveUnitPathStep), nameof(ApplyMoveUnitPathStep))]
[JsonDerivedType(typeof(ApplyPlaceTileStep), nameof(ApplyPlaceTileStep))]
[JsonDerivedType(typeof(ApplyPlaceUnitStep), nameof(ApplyPlaceUnitStep))]
[JsonDerivedType(typeof(ApplyPushUnitStep), nameof(ApplyPushUnitStep))]
[JsonDerivedType(typeof(ApplyRectWindStep), nameof(ApplyRectWindStep))]
[JsonDerivedType(typeof(ApplyRegionTurnStep), nameof(ApplyRegionTurnStep))]
[JsonDerivedType(typeof(ApplyStepsStep), nameof(ApplyStepsStep))]
public abstract class ApplyStep
{
    public abstract void Apply(SimObject obj);
}

[JsonDerivedType(typeof(AnimationStep), nameof(AnimationStep))]
[JsonDerivedType(typeof(BuffTurnStep), nameof(BuffTurnStep))]
[JsonDerivedType(typeof(CureStep), nameof(CureStep))]
[JsonDerivedType(typeof(DamageStep), nameof(DamageStep))]
[JsonDerivedType(typeof(DeathStep), nameof(DeathStep))]
[JsonDerivedType(typeof(DelayStep), nameof(DelayStep))]
[JsonDerivedType(typeof(DropTileStep), nameof(DropTileStep))]
[JsonDerivedType(typeof(DropUnitStep), nameof(DropUnitStep))]
[JsonDerivedType(typeof(ModifyTileRegionStep), nameof(ModifyTileRegionStep))]
[JsonDerivedType(typeof(ModifyUnitBuffStep), nameof(ModifyUnitBuffStep))]
[JsonDerivedType(typeof(MoveUnitStep), nameof(MoveUnitStep))]
[JsonDerivedType(typeof(MoveUnitPathStep), nameof(MoveUnitPathStep))]
[JsonDerivedType(typeof(PlaceTileStep), nameof(PlaceTileStep))]
[JsonDerivedType(typeof(PlaceUnitStep), nameof(PlaceUnitStep))]
[JsonDerivedType(typeof(PushUnitStep), nameof(PushUnitStep))]
[JsonDerivedType(typeof(RectWindStep), nameof(RectWindStep))]
[JsonDerivedType(typeof(RegionTurnStep), nameof(RegionTurnStep))]
[JsonDerivedType(typeof(StepsStep), nameof(StepsStep))]
public class SimStep
{
    public ApplyStep Apply { get; set; }
    public int Deep { get; set; }
    public SimStep(ApplyStep apply, int deep)
    {
        Apply = apply;
        Deep = deep;
    }
}

public class ApplyAnimationStep : ApplyStep
{
    public AnimationType Animation { get; set; }

    public ApplyAnimationStep(AnimationType animation)
    {
        Animation = animation;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyAnimation(Animation);
    }
}

public class ApplyBuffTurnStep : ApplyStep
{
    public int Turn { get; set; }

    public ApplyBuffTurnStep(int turn)
    {
        Turn = turn;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyBuffTurn(Turn);
    }
}

public class ApplyCureStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public float Cure { get; set; }

    public ApplyCureStep(Vector2I coords, float cure)
    {
        Coords = coords;
        Cure = cure;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyCure(Coords, Cure);
    }
}

public class ApplyDamageStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public float Damage { get; set; }

    public ApplyDamageStep(Vector2I coords, float damage)
    {
        Coords = coords;
        Damage = damage;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyDamage(Coords, Damage);
    }
}

public class ApplyDeathStep : ApplyStep
{
    public Vector2I Coords { get; set; }

    public ApplyDeathStep(Vector2I coords)
    {
        Coords = coords;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyDeath(Coords);
    }
}

public class ApplyDelayStep : ApplyStep
{
    public int Delay { get; set; }

    public ApplyDelayStep(int delay)
    {
        Delay = delay;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyDelay(Delay);
    }
}

public class ApplyDropTileStep : ApplyStep
{
    public Vector2I Coords { get; set; }

    public ApplyDropTileStep(Vector2I coords)
    {
        Coords = coords;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyDropTile(Coords);
    }
}

public class ApplyDropUnitStep : ApplyStep
{
    public Vector2I Coords { get; set; }

    public ApplyDropUnitStep(Vector2I coords)
    {
        Coords = coords;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyDropUnit(Coords);
    }
}

public class ApplyModifyTileRegionStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public string RegionName { get; set; }
    public RegionOperation Operation { get; set; }
    public int Duration { get; set; }

    public ApplyModifyTileRegionStep(Vector2I coords, string regionName, RegionOperation operation, int duration)
    {
        Coords = coords;
        RegionName = regionName;
        Operation = operation;
        Duration = duration;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyModifyTileRegion(Coords, RegionName, Operation, Duration);
    }
}

public class ApplyModifyUnitBuffStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public string BuffName { get; set; }
    public BuffOperation Operation { get; set; }
    public int Duration { get; set; }

    public ApplyModifyUnitBuffStep(Vector2I coords, string buffName, BuffOperation operation, int duration)
    {
        Coords = coords;
        BuffName = buffName;
        Operation = operation;
        Duration = duration;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyModifyUnitBuff(Coords, BuffName, Operation, Duration);
    }
}

public class ApplyMoveUnitStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public Vector2I To { get; set; }

    public ApplyMoveUnitStep(Vector2I coords, Vector2I to)
    {
        Coords = coords;
        To = to;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyMoveUnit(Coords, To);
    }
}

public class ApplyMoveUnitPathStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public List<Vector2I>? Path { get; set; }

    public ApplyMoveUnitPathStep(Vector2I coords, List<Vector2I>? path)
    {
        Coords = coords;
        Path = path;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyMoveUnitPath(Coords, Path);
    }
}

public class ApplyPlaceTileStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public string TileName { get; set; }

    public ApplyPlaceTileStep(Vector2I coords, string tileName)
    {
        Coords = coords;
        TileName = tileName;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyPlaceTile(Coords, TileName);
    }
}

public class ApplyPlaceUnitStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public string UnitName { get; set; }
    public int Team { get; set; }

    public ApplyPlaceUnitStep(Vector2I coords, string unitName, int team)
    {
        Coords = coords;
        UnitName = unitName;
        Team = team;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyPlaceUnit(Coords, UnitName, Team);
    }
}

public class ApplyPushUnitStep : ApplyStep
{
    public Vector2I Coords { get; set; }
    public DirMask Dir { get; set; }

    public ApplyPushUnitStep(Vector2I coords, DirMask dir)
    {
        Coords = coords;
        Dir = dir;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyPushUnit(Coords, Dir);
    }
}

public class ApplyRectWindStep : ApplyStep
{
    public Rect2I Rect { get; set; }
    public DirMask Dir { get; set; }

    public ApplyRectWindStep(Rect2I rect, DirMask dir)
    {
        Rect = rect;
        Dir = dir;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyRectWind(Rect, Dir);
    }
}

public class ApplyRegionTurnStep : ApplyStep
{
    public int Turn { get; set; }

    public ApplyRegionTurnStep(int turn)
    {
        Turn = turn;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplyRegionTurn(Turn);
    }
}

public class ApplyStepsStep : ApplyStep
{
    public ApplyStep[] Applys { get; set; }
    public int StartDelay { get; set; }
    public int EndDelay { get; set; }
    public StepsRunningMode RunningMode { get; set; }

    public ApplyStepsStep(ApplyStep[] applys, int startDelay, int endDelay, StepsRunningMode runningMode)
    {
        Applys = applys;
        StartDelay = startDelay;
        EndDelay = endDelay;
        RunningMode = runningMode;
    }

    public override void Apply(SimObject obj)
    {
        obj.ApplySteps(Applys, StartDelay, EndDelay, RunningMode);
    }
}

public class AnimationStep : SimStep
{
    public ApplyAnimationStep Apply => (ApplyAnimationStep)base.Apply;
    public AnimationStep(int deep, AnimationType applyAnimation)
        : base(new ApplyAnimationStep(applyAnimation), deep)
    {
    }
}

public class BuffTurnStep : SimStep
{
    public ApplyBuffTurnStep Apply => (ApplyBuffTurnStep)base.Apply;
    public BuffTurnStep(int deep, int applyTurn)
        : base(new ApplyBuffTurnStep(applyTurn), deep)
    {
    }
}

public class CureStep : SimStep
{
    public ApplyCureStep Apply => (ApplyCureStep)base.Apply;
    public long UnitId { get; set; }
    public float ValidCure { get; set; }

    public CureStep(int deep, Vector2I applyCoords, float applyCure, long unitId, float validCure)
        : base(new ApplyCureStep(applyCoords, applyCure), deep)
    {
        UnitId = unitId;
        ValidCure = validCure;
    }
}

public class DamageStep : SimStep
{
    public ApplyDamageStep Apply => (ApplyDamageStep)base.Apply;
    public long UnitId { get; set; }
    public float ValidDamage { get; set; }

    public DamageStep(int deep, Vector2I applyCoords, float applyDamage, long unitId, float validDamage)
        : base(new ApplyDamageStep(applyCoords, applyDamage), deep)
    {
        UnitId = unitId;
        ValidDamage = validDamage;
    }
}

public class DeathStep : SimStep
{
    public ApplyDeathStep Apply => (ApplyDeathStep)base.Apply;
    public long UnitId { get; set; }
    public bool Valid { get; set; }

    public DeathStep(int deep, Vector2I applyCoords, long unitId, bool valid)
        : base(new ApplyDeathStep(applyCoords), deep)
    {
        UnitId = unitId;
        Valid = valid;
    }
}

public class DelayStep : SimStep
{
    public ApplyDelayStep Apply => (ApplyDelayStep)base.Apply;
    public DelayStep(int deep, int applyDelay)
        : base(new ApplyDelayStep(applyDelay), deep)
    {
    }
}

public class DropTileStep : SimStep
{
    public ApplyDropTileStep Apply => (ApplyDropTileStep)base.Apply;
    public DropTileStep(int deep, Vector2I applyCoords)
        : base(new ApplyDropTileStep(applyCoords), deep)
    {
    }
}

public class DropUnitStep : SimStep
{
    public ApplyDropUnitStep Apply => (ApplyDropUnitStep)base.Apply;
    public long UnitId { get; set; }
    public bool Valid { get; set; }

    public DropUnitStep(int deep, Vector2I applyCoords, long unitId, bool valid)
        : base(new ApplyDropUnitStep(applyCoords), deep)
    {
        UnitId = unitId;
        Valid = valid;
    }
}

public class ModifyTileRegionStep : SimStep
{
    public ApplyModifyTileRegionStep Apply => (ApplyModifyTileRegionStep)base.Apply;
    public RegionOperation Operation { get; set; }
    public int Duration { get; set; }

    public ModifyTileRegionStep(int deep, Vector2I applyCoords, string applyRegionName, RegionOperation applyOperation, int applyDuration, RegionOperation operation, int duration)
        : base(new ApplyModifyTileRegionStep(applyCoords, applyRegionName, applyOperation, applyDuration), deep)
    {
        Operation = operation;
        Duration = duration;
    }
}

public class ModifyUnitBuffStep : SimStep
{
    public ApplyModifyUnitBuffStep Apply => (ApplyModifyUnitBuffStep)base.Apply;
    public long UnitId { get; set; }
    public BuffOperation Operation { get; set; }
    public int Duration { get; set; }

    public ModifyUnitBuffStep(int deep, Vector2I applyCoords, string applyBuffName, BuffOperation applyOperation, int applyDuration, long unitId, BuffOperation operation, int duration)
        : base(new ApplyModifyUnitBuffStep(applyCoords, applyBuffName, applyOperation, applyDuration), deep)
    {
        UnitId = unitId;
        Operation = operation;
        Duration = duration;
    }
}

public class MoveUnitStep : SimStep
{
    public ApplyMoveUnitStep Apply => (ApplyMoveUnitStep)base.Apply;
    public long UnitId { get; set; }
    public bool Valid { get; set; }

    public MoveUnitStep(int deep, Vector2I applyCoords, Vector2I applyTo, long unitId, bool valid)
        : base(new ApplyMoveUnitStep(applyCoords, applyTo), deep)
    {
        UnitId = unitId;
        Valid = valid;
    }
}

public class MoveUnitPathStep : SimStep
{
    public ApplyMoveUnitPathStep Apply => (ApplyMoveUnitPathStep)base.Apply;
    public long UnitId { get; set; }
    public int TargetIndex { get; set; }
    public bool Valid { get; set; }

    public MoveUnitPathStep(int deep, Vector2I applyCoords, List<Vector2I>? applyPath, long unitId, int targetIndex, bool valid)
        : base(new ApplyMoveUnitPathStep(applyCoords, applyPath), deep)
    {
        UnitId = unitId;
        TargetIndex = targetIndex;
        Valid = valid;
    }
}

public class PlaceTileStep : SimStep
{
    public ApplyPlaceTileStep Apply => (ApplyPlaceTileStep)base.Apply;
    public PlaceTileStep(int deep, Vector2I applyCoords, string applyTileName)
        : base(new ApplyPlaceTileStep(applyCoords, applyTileName), deep)
    {
    }
}

public class PlaceUnitStep : SimStep
{
    public ApplyPlaceUnitStep Apply => (ApplyPlaceUnitStep)base.Apply;
    public UnitState UnitState { get; set; }
    public bool Valid { get; set; }

    public PlaceUnitStep(int deep, Vector2I applyCoords, string applyUnitName, int applyTeam, UnitState unitState, bool valid)
        : base(new ApplyPlaceUnitStep(applyCoords, applyUnitName, applyTeam), deep)
    {
        UnitState = unitState;
        Valid = valid;
    }
}

public class PushUnitStep : SimStep
{
    public ApplyPushUnitStep Apply => (ApplyPushUnitStep)base.Apply;
    public long UnitId { get; set; }
    public bool Valid { get; set; }

    public PushUnitStep(int deep, Vector2I applyCoords, DirMask applyDir, long unitId, bool valid)
        : base(new ApplyPushUnitStep(applyCoords, applyDir), deep)
    {
        UnitId = unitId;
        Valid = valid;
    }
}

public class RectWindStep : SimStep
{
    public ApplyRectWindStep Apply => (ApplyRectWindStep)base.Apply;
    public RectWindStep(int deep, Rect2I applyRect, DirMask applyDir)
        : base(new ApplyRectWindStep(applyRect, applyDir), deep)
    {
    }
}

public class RegionTurnStep : SimStep
{
    public ApplyRegionTurnStep Apply => (ApplyRegionTurnStep)base.Apply;
    public RegionTurnStep(int deep, int applyTurn)
        : base(new ApplyRegionTurnStep(applyTurn), deep)
    {
    }
}

public class StepsStep : SimStep
{
    public ApplyStepsStep Apply => (ApplyStepsStep)base.Apply;
    public StepsStep(int deep, ApplyStep[] applyApplys, int applyStartDelay, int applyEndDelay, StepsRunningMode applyRunningMode)
        : base(new ApplyStepsStep(applyApplys, applyStartDelay, applyEndDelay, applyRunningMode), deep)
    {
    }
}
