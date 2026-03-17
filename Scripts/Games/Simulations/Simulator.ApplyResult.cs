using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyResult(SimulationResult result)
        {
            //运行站位
            var runningResult = new SimulationResult(0, result.TeamType, SimulationLayer.Running, false);
            foreach (var step in result.MasterSteps)
            {
                switch (step)
                {
                    case DamageStep damageStep:
                        //GD.Print($"伤害 {damageStep.Coords} {damageStep.Damage} {damageStep.ResultValidDamage}");
                        ApplyDamage(state, damageStep.Coords, damageStep.Damage, runningResult);
                        break;
                    case DeathStep deathStep:
                        ApplyDeath(state, deathStep.Coords, runningResult);
                        break;
                    case PlaceTileStep placeTileStep:
                        ApplyPlaceTile(state, placeTileStep.Coords, placeTileStep.Tile, runningResult);
                        break;
                    case PlaceUnitStep placeUnitStep:
                        ApplyPlaceUnit(state, placeUnitStep.Coords, placeUnitStep.UnitName, placeUnitStep.Team, placeUnitStep.ResultUnit, runningResult);
                        break;
                    case MoveUnitStep moveStep:
                        ApplyMoveUnit(state, moveStep.Coords, moveStep.To, runningResult);
                        break;
                    case MoveUnitPathStep movePathStep:
                        ApplyMoveUnitPath(state, movePathStep.Coords, movePathStep.Path, runningResult);
                        break;
                    case DropUnitStep dropStep:
                        ApplyDropUnit(state, dropStep.Coords, runningResult);
                        break;
                    case PushUnitStep pushStep:
                        ApplyPushUnit(state, pushStep.Coords, pushStep.PushDir, runningResult);
                        break;
                    case DropTileStep dropTileStep:
                        ApplyDropTile(state, dropTileStep.Coords, runningResult);
                        break;
                    case ModifyUnitBuffStep modifyUnitBuffStep:
                        ApplyModifyUnitBuff(state, modifyUnitBuffStep.Coords, modifyUnitBuffStep.BuffType,
                            modifyUnitBuffStep.Operation, modifyUnitBuffStep.Duration, runningResult);
                        break;
                    case ModifyTileRegionStep modifyTileRegionStep:
                        ApplyModifyTileRegion(state, modifyTileRegionStep.Coords, modifyTileRegionStep.RegionType,
                            modifyTileRegionStep.Operation, modifyTileRegionStep.Duration, runningResult);
                        break;
                    case null:
                        break;
                }
            }
        }
    }
}