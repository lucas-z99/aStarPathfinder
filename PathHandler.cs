using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStarPathFinder;


public class PathHandler
{

     //use the basic algarithm of PathFinder
     //to handle all kinds of actual game request


     static PFGrid grid { get { return BattleField.Inst.pfGrid; } }
     static List<Chara> charaList { get { return BattleField.Inst.charaList; } }
     static bool testLog;


     //find path ============================================================================================================

     public static bool GetMovePath(Chara chara, Vector2Int pos, out List<Vector2Int> path)
     {
          var endChara = BoardMgr.GetChara(pos);

          if (endChara == null)
          {
               var enemyTiles = GetEnemyOccupiedTile(chara);

               return PathFinder.GetPath(grid, chara.pos, pos, chara.mov, (int)chara.moveMask, out path, enemyTiles);
          }
          else if (endChara == chara)
          {
               path = new List<Vector2Int>();
               path.Add(pos);

               return true;
          }

          path = null;
          return false;
     }


     //path end on a dash-spell target
     public static bool GetDashPath(Chara chara, Spell spell, Chara target, out List<Vector2Int> path)
     {
          if (CombatUtility.FitRequiredTargetMask(chara, spell, target))
          {
               //blocked
               var enemy = GetEnemyOccupiedTile(chara);
               if (enemy.Contains(target.pos))
               {
                    enemy.Remove(target.pos);//but dash target is exception 
               }

               //longer range
               int range = chara.mov + spell.rangeMax;

               //path finder
               return PathFinder.GetPath(grid, chara.pos, target.pos, range, (int)chara.moveMask, out path, enemy);
          }

          path = null;
          return false;
     }


     //path end right next to a melee-spell target
     public static bool GetMeleePath(Chara chara, Spell spell, Chara target, out List<Vector2Int> path)
     {
          if (CombatUtility.FitRequiredTargetMask(chara, spell, target))
          {
               List<Vector2Int> options;
               if (GetCastPosition(chara, spell, target.pos, chara.moveRange, out options))
               {
                    var attackPos = CombatUtility.GetClosest(chara.pos, options);
                    var enemy = GetEnemyOccupiedTile(chara);

                    return PathFinder.GetPath(grid, chara.pos, attackPos, chara.mov, (int)chara.moveMask, out path, enemy);
               }
          }

          path = null;
          return false;
     }


     //path end on a friend
     public static bool GetPathToFriend(Chara chara, Chara target, out List<Vector2Int> path)
     {
          if (target.team == chara.team && target != chara)
          {
               var enemy = GetEnemyOccupiedTile(chara);

               return PathFinder.GetPath(grid, chara.pos, target.pos, chara.mov, (int)chara.moveMask, out path, enemy);
          }

          path = null;
          return false;
     }


     //move as close as possible
     public static bool GetMovePath_TargetNotInMoveRange(Chara chara, Vector2Int targetPos, MoveRange moveRange, out List<Vector2Int> path)
     {
          //a path from self to a target pos outside move range
          //if path is not available, go to the closest spot

          //find out if path to target is open
          List<Vector2Int> _enemyTiles = GetEnemyOccupiedTile(chara);
          if (_enemyTiles.Contains(targetPos))
          {
               _enemyTiles.Remove(targetPos);
          }

          ////if path open, find how far we can move
          //List<Vector2Int> _fullPath;

          //if (PathFinder.GetPath(grid, chara.pos, targetPos, int.MaxValue, (int)chara.moveMask, out _fullPath, _enemyTiles))
          //{
          //     int maxLength = Mathf.Min(_fullPath.Count - 1, chara.movePoint);

          //     for (int i = maxLength; i >= 0; i--)
          //     {
          //          Vector2Int pos = _fullPath[i];

          //          return GetMovePath(chara, pos, out path);
          //     }
          //}
          //else
          //{
          //     //else within move range, find the closest point
          //     Vector2Int _closestPos;

          //     if (PathFinder.GetClosestPointInRange(grid, moveRange.list, targetPos, (int)chara.moveMask, out _closestPos))
          //     {
          //          return GetMovePath(chara, _closestPos, out path);
          //     }
          //}

          //within move range, find the closest point
          Vector2Int _closestPos;

          if (PathFinder.GetClosestPointInRange(grid, moveRange.list, targetPos, (int)chara.moveMask, out _closestPos))
          {
               return GetMovePath(chara, _closestPos, out path);
          }

          path = null;
          return false;
     }



     //validate path  ============================================================================================================

     public static bool ValidateMovePath(Chara chara, Vector2Int target, List<Vector2Int> path)
     {
          return _ValidatePath(chara, target, path, false, 0);
     }

     public static bool ValidateDashPath(Chara chara, int spellMaxRange, Chara target, List<Vector2Int> path)
     {
          if (path.Count >= 2)
          {
               return _ValidatePath(chara, target.pos, path, true, spellMaxRange);
          }

          return false;
     }

     public static bool ValidatePathToFriend(Chara chara, Chara friend, List<Vector2Int> path)
     {
          if (path.Count >= 2)
          {
               var endChara = BoardMgr.GetChara(path[path.Count - 1]);

               if (endChara != null)
               {
                    if (endChara.team == chara.team && endChara != chara)//is friend
                    {
                         return _ValidatePath(chara, friend.pos, path, true, 0);
                    }
               }
          }

          return false;
     }

     static bool _ValidatePath(Chara chara, Vector2Int target, List<Vector2Int> path, bool canLandOnOthers, int extraMoveRange)
     {
          if (chara == null) { Debug.LogError(""); return false; }
          if (path == null) { Debug.LogError(chara.charaName); return false; }
          if (path.Count == 0) { Debug.LogError(chara.charaName); return false; }

          if (path[0] != chara.pos) return false;

          //can path end on someone else?
          var endPos = path[path.Count - 1];
          if (endPos != target) return false;

          if (canLandOnOthers == false && path.Count >= 2)
          {
               var endChara = BoardMgr.GetChara(endPos);

               if (endChara != null)
               {
                    return false;
               }
          }

          //blocked tile
          var _enemyTiles = GetEnemyOccupiedTile(chara);

          if (canLandOnOthers && _enemyTiles.Contains(endPos))
          {
               _enemyTiles.Remove(endPos);
          }

          //validate
          return PathFinder.ValidatePath(grid, path, chara.mov + extraMoveRange, (int)chara.moveMask, _enemyTiles);
     }


     public static bool AreNeighbour(Vector2Int posA, Vector2Int posB)
     {
          return PathFinder.ValidateNeighbour(grid, posA, posB);
     }


     //range related  ============================================================================================================

     //utility
     public static void RefreshRangeInfo(Chara chara)
     {
          //move range
          if (chara.canMove)
          {
               chara.moveRange = GetMoveRange(chara);
          }
          else
          {
               var list = new List<Vector2Int>();
               list.Add(chara.pos);
               chara.moveRange = new MoveRange(list, new List<Vector2Int>());
          }

          //spell range
          if (chara.canAttack)
          {
               foreach (Spell spell in chara.spellList)
               {
                    spell.range = GetSpellRange(chara, spell, chara.moveRange);
               }
          }
     }

     static MoveRange GetMoveRange(Chara chara)
     {
          //blocked
          List<Vector2Int> _enemyObstacle = GetEnemyOccupiedTile(chara);

          //mark friend for later
          List<Vector2Int> _tSame = BoardMgr.GetTeam(chara.team);
          _tSame.Remove(chara.pos);
          var markFriend = new MarkTarget(_tSame);


          //find range
          List<Vector2Int> moveRange = PathFinder.Expand(grid, chara.pos, 0, chara.mov, (int)chara.moveMask, _enemyObstacle, markFriend);

          var friendInRange = new List<Vector2Int>();

          foreach (var pos in markFriend.result)
          {
               moveRange.Remove(pos);
               friendInRange.Add(pos);
          }

          return new MoveRange(moveRange, friendInRange);
     }


     static SpellRange GetSpellRange(Chara chara, Spell spell, MoveRange moveRange)
     {
          //1
          List<Vector2Int> coverArea = null;

          if (spell.isDash)
          {
               coverArea = GetSpellCoverArea(chara, spell, moveRange.includeFriend);
          }
          else
          {
               coverArea = GetSpellCoverArea(chara, spell, moveRange.list);
          }

          //2
          List<Vector2Int> target = MarkIntendedTargetInArea(chara, spell, coverArea);

          //3
          var standStill = new List<Vector2Int>();
          standStill.Add(chara.pos);

          List<Vector2Int> notMovingCoverArea = GetSpellCoverArea(chara, spell, standStill);

          List<Vector2Int> immedTarget = MarkIntendedTargetInArea(chara, spell, notMovingCoverArea);


          //finally
          return new SpellRange(coverArea, target, immedTarget);
     }


     public static List<Vector2Int> GetSpellCoverArea(Chara chara, Spell spell, List<Vector2Int> moveRange)
     {
          if (spell.rangeType == SpellRangeType.Default)
          {
               return PathFinder.ExpandRange(grid, moveRange, spell.rangeMin, spell.rangeMax, spell.rangeMask, null, null);
          }
          else
          {
               var area = new List<Vector2Int>();

               //expand from move range
               foreach (Vector2Int movePos in moveRange)
               {
                    if (spell.rangeType == SpellRangeType.Cross)
                    {
                         area.AddRange_NonRepeated(GeoUtility.GetCross(movePos, spell.rangeMax, BoardMgr.bounds));
                    }
                    else if (spell.rangeType == SpellRangeType.Square)
                    {
                         area.AddRange_NonRepeated(GeoUtility.GetSquare(movePos, spell.rangeMax, BoardMgr.bounds));
                    }
                    else if (spell.rangeType == SpellRangeType.Star)
                    {
                         area.AddRange_NonRepeated(GeoUtility.Get8DirStar(movePos, spell.rangeMax, BoardMgr.bounds));
                    }
               }

               return area;
          }
     }


     static List<Vector2Int> MarkIntendedTargetInArea(Chara caster, Spell spell, List<Vector2Int> area)
     {
          var result = new List<Vector2Int>();

          foreach (Chara target in charaList)
          {
               if (area.Contains(target.pos))
               {
                    if (CombatUtility.FitRequiredTargetMask(caster, spell, target))
                    {
                         if (SpellCastChecker.CanBeAffectedBySpell(target, caster, spell))
                         {
                              result.Add(target.pos);
                         }
                    }
               }
          }

          return result;
     }

     public static bool GetCastPosition(Chara chara, Spell spell, Vector2Int targetPos, MoveRange moveRange, out List<Vector2Int> result)
     {
          result = PathFinder.Expand(grid, targetPos, spell.rangeMin, spell.rangeMax, (int)chara.moveMask, null);

          if (moveRange != null)
          {
               //if move range is provided, check if in move range
               for (int i = result.Count - 1; i >= 0; i--)
               {
                    if (moveRange.list.Contains(result[i]) == false && chara.pos != result[i])
                    {
                         result.RemoveAt(i);
                    }
               }
          }

          return result.Count > 0;
     }


     public static List<Vector2Int> GetAllEnemyInSight(Chara chara, int range, bool seeThroughWall)
     {
          var tDiff = new List<Vector2Int>();

          foreach (Chara target in charaList)
          {
               if (target.team != chara.team)
               {
                    tDiff.Add(target.pos);
               }
          }

          var markTarget = new MarkTarget(tDiff);

          //sight mask
          SightMask mask = seeThroughWall ? SightMask.Phantom : SightMask.Default;

          if (range < 0)
          {
               range = int.MaxValue;
          }

          PathFinder.Expand(grid, chara.pos, 0, range, (int)mask, null, markTarget);

          return markTarget.result;
     }


     //Other useful method ============================================================================================================

     //similar to enemy team, but will be empty if chara is flying unit
     static List<Vector2Int> GetEnemyOccupiedTile(Chara chara)
     {
          if (((int)chara.moveMask & (int)TileMoveMask.Air) == 0)
          {
               return BoardMgr.GetDiffTeam(chara.team);
          }
          else
          {
               return new List<Vector2Int>();
          }
     }

     public static bool IsInSpellRange(Spell spell, Vector2Int caster, Vector2Int target)
     {
          List<Vector2Int> path;

          if (PathFinder.GetPath(grid, caster, target, spell.rangeMax, spell.rangeMask, out path, null))
          {
               if (path.Count > spell.rangeMin)
               {
                    return true;
               }
          }

          return false;
     }


     public static List<Vector2Int> GetPlayerDeployArea()
     {
          var origin = BoardMgr.GetObjectsOfType(TilemapObjType.StartPoint);
          var result = new List<Vector2Int>();

          foreach (var pos in origin)
          {
               result.AddRange_NonRepeated(PathFinder.Expand(grid, pos, 0, 4, (int)CharaMoveMask.Air, null));
          }

          return result;
     }




}







