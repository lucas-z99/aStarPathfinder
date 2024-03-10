using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AStarPathFinder
{

     public static class PathFinder
     {
          //return empty path if
          //1) pos not exist
          //2) no path can be found
          //3) start pos is end pos

          public static bool GetPath(PFGrid grid, Vector2Int start, Vector2Int end, int maxDist, int mask, 
          out List<Vector2Int> result, 
          List<Vector2Int> blocked, SearchOption[] options = null)
          {
               if (start == end)
               {
                    result = new List<Vector2Int>();
                    result.Add(start);

                    return true;
               }

               PFNode startNode = grid.GetNode(start.x, start.y);
               PFNode endNode = grid.GetNode(end.x, end.y);
               PFNode curNode = null;

               startNode.moveCost = 0;
               startNode.heuristic = 0;
               startNode.LastNode = null;

               if (startNode == null || endNode == null)
               {
                    result = null;
                    return false;
               }

               var openList = new List<PFNode>();//node we plan to check
               var closeList = new List<PFNode>();//node we have checked
               var badList = new List<PFNode>();//node we do not want to check

               if (blocked != null)
               {
                    foreach (Vector2Int pos in blocked)
                    {
                         PFNode node = grid.GetNode(pos);

                         if (node != null && badList.Contains(node) == false)
                         {
                              badList.Add(node);
                         }
                    }
               }

               //ReadyResultList(option);//other special options


               //start go through the list
               openList.Add(startNode);//we will start from here

               while (openList.Count > 0)
               {
                    //find node with lowest cost
                    curNode = openList[0];

                    for (int i = 1; i < openList.Count; i++)
                    {
                         PFNode node = openList[i];

                         if (node.mCost < curNode.mCost)
                         {
                              curNode = node;
                         }
                         else if (node.mCost == curNode.mCost && node.heuristic < curNode.heuristic)
                         {
                              curNode = node;
                         }
                    }


                    if (options != null)//---------------------------------------------------------- options
                    {
                         foreach (var option in options)
                         {
                              var mark = (MarkTarget)option;
                              if (mark != null)
                              {
                                   if (mark.list.Contains(curNode.pos))
                                   {
                                        mark.result.Add(curNode.pos);
                                   }
                              }
                         }
                    }


                    if (curNode == endNode)//---------------------------------------------------------- path found
                    {
                         result = new List<Vector2Int>();

                         //generate path
                         while (true)
                         {
                              result.Add(new Vector2Int(curNode.pos.x, curNode.pos.y));

                              if (curNode == startNode)
                              {
                                   break;
                              }

                              curNode = curNode.LastNode;
                         }

                         result.Reverse();
                         return true;
                    }


                    //finish with current node
                    openList.Remove(curNode);
                    closeList.Add(curNode);

                    //look through neighbour
                    foreach (var pair in curNode.Neighbour)
                    {
                         PFNode neighbour = pair.Key;
                         PFPassage passage = pair.Value;

                         //check: already encountered
                         if (closeList.Contains(neighbour) || badList.Contains(neighbour))
                         {
                              continue;
                         }

                         //check move mask
                         if ((mask & passage.mask) == 0)
                         {
                              continue;
                         }

                         if ((mask & neighbour.mask) == 0)
                         {
                              if (badList.Contains(neighbour) == false)
                              {
                                   badList.Add(neighbour);
                              }

                              continue;
                         }

                         //get move cost to this neighbour from current node
                         float _moveCost = curNode.moveCost + passage.distance;

                         //if (options != null)//---------------------------------------------------------- options
                         //{
                         //     foreach (var option in options)
                         //     {
                         //          var moveCost = (MoveCost)option;
                         //          if (moveCost != null)
                         //          {
                         //               if (moveCost.list.ContainsKey(neighbour.pos))
                         //               {
                         //                    _moveCost += moveCost.list[neighbour.pos];
                         //               }
                         //          }
                         //     }
                         //}

                         //check: distance
                         if (_moveCost > maxDist)
                         {
                              if (badList.Contains(neighbour) == false)
                              {
                                   badList.Add(neighbour);
                                   continue;
                              }
                         }

                         //pass all the check
                         //this node is now available
                         if (openList.Contains(neighbour) == false)
                         {
                              //first time encounter
                              openList.Add(neighbour);

                              neighbour.heuristic = Vector2Int.Distance(neighbour.pos, endNode.pos);
                              neighbour.moveCost = _moveCost;
                              neighbour.LastNode = curNode;


                              if (options != null)//---------------------------------------------------------- options
                              {
                                   foreach (var option in options)
                                   {
                                        var prefer = (Preference)option;
                                        if (prefer != null)
                                        {
                                             if (prefer.list.ContainsKey(neighbour.pos))
                                             {
                                                  neighbour.heuristic += prefer.list[neighbour.pos];
                                             }
                                        }
                                   }
                              }

                         }
                         else
                         {
                              //is in open list, but cost WAS not low enough to be choosed
                              //see if cost can be improved
                              if (_moveCost < neighbour.moveCost)
                              {
                                   neighbour.moveCost = _moveCost;
                                   neighbour.LastNode = curNode;
                              }
                         }
                    }
               }


               result = null;
               return false;
          }
          public static bool GetPath(PFGrid grid, Vector2Int start, Vector2Int end, int maxDist, int mask, out List<Vector2Int> result, List<Vector2Int> blocked, SearchOption option)
          {
               var options = new SearchOption[1];
               options[0] = option;

               return GetPath(grid, start, end, maxDist, mask, out result, blocked, options);
          }



          public static List<Vector2Int> Expand(PFGrid grid, Vector2Int start, int minDist, int maxDist, int mask, List<Vector2Int> blocked, SearchOption[] options = null)
          {

               //initial
               var result = new List<Vector2Int>();

               var openList = new List<PFNode>();//node plan to check
               var closeList = new List<PFNode>();//node covered
               var badList = new List<PFNode>();//node does not meet requirement


               PFNode startNode = grid.GetNode(start.x, start.y);
               PFNode curNode = null;

               startNode.moveCost = 0;

               //ReadyResultList(option);

               //blocked
               if (blocked != null)
               {
                    foreach (Vector2Int pos in blocked)
                    {
                         PFNode node = grid.GetNode(pos);

                         if (node != null && badList.Contains(node) == false)
                         {
                              badList.Add(node);
                         }
                    }
               }


               //start go through the list
               openList.Add(startNode);

               while (openList.Count > 0)
               {

                    //find node with lowest cost
                    curNode = openList[0];

                    for (int i = 1; i < openList.Count; i++)
                    {
                         PFNode node = openList[i];

                         if (node.mCost < curNode.mCost)
                         {
                              curNode = node;
                         }
                         else if (node.mCost == curNode.mCost && node.heuristic < curNode.heuristic)
                         {
                              curNode = node;
                         }
                    }

                    if (options != null)//---------------------------------------------------------- options
                    {
                         foreach (var option in options)
                         {
                              var mark = (MarkTarget)option;
                              if (mark != null)
                              {
                                   if (mark.list.Contains(curNode.pos))
                                   {
                                        mark.result.Add(curNode.pos);
                                   }
                              }
                         }
                    }

                    //result
                    if (curNode.moveCost >= minDist && curNode.moveCost <= maxDist)
                    {
                         result.Add(curNode.pos);
                    }


                    //finish with current node
                    openList.Remove(curNode);
                    closeList.Add(curNode);


                    //check neighbour
                    foreach (var pair in curNode.Neighbour)
                    {

                         PFNode neighbour = pair.Key;
                         PFPassage passage = pair.Value;


                         //check
                         if (closeList.Contains(neighbour) || badList.Contains(neighbour))
                         {
                              continue;
                         }

                         //check mask
                         if (mask > 0)
                         {
                              if ((mask & passage.mask) == 0)
                              {
                                   continue;
                              }

                              if ((mask & neighbour.mask) == 0)
                              {
                                   continue;
                              }
                         }


                         //move cost of this neighbour
                         float _moveCost = curNode.moveCost + passage.distance;


                         //////Option: extra cost
                         //if (options != null)//---------------------------------------------------------- options
                         //{
                         //     foreach (var option in options)
                         //     {
                         //          var moveCost = (MoveCost)option;
                         //          if (moveCost != null)
                         //          {
                         //               if (moveCost.list.ContainsKey(neighbour.pos))
                         //               {
                         //                    _moveCost += moveCost.list[neighbour.pos];
                         //               }
                         //          }
                         //     }
                         //}


                         //check distance
                         if (_moveCost > maxDist)
                         {
                              badList.Add(neighbour);
                              continue;
                         }


                         //all check passed

                         //finally
                         if (openList.Contains(neighbour) == false)
                         {
                              openList.Add(neighbour);
                              neighbour.heuristic = Vector2Int.Distance(neighbour.pos, startNode.pos);

                              neighbour.moveCost = _moveCost;
                         }
                         else
                         {
                              //only assign lowest move cost
                              //this can prevent backtrack to disturb minDist check
                              //like A-B-C-D-A will make A cost=4
                              if (_moveCost < neighbour.moveCost)
                              {
                                   neighbour.moveCost = _moveCost;
                              }
                         }
                    }
               }

               return result;
          }

          public static List<Vector2Int> Expand(PFGrid grid, Vector2Int start, int minDist, int maxDist, int mask, List<Vector2Int> blocked, SearchOption option)
          {
               var options = new SearchOption[1];
               options[0] = option;

               return Expand(grid, start, minDist, maxDist, mask, blocked, options);
          }



          public static List<Vector2Int> ExpandRange(PFGrid grid, List<Vector2Int> originalRange, int minDist, int maxDist, int mask, List<Vector2Int> blocked, SearchOption[] options)
          {
               var result = new List<Vector2Int>();

               //if (option != null)
               //{
               //     option.ClearResult();
               //     option.DoNotClearResult = true;
               //}

               foreach (Vector2Int origin in originalRange)
               {
                    //expand every node
                    List<Vector2Int> expanded = Expand(grid, origin, minDist, maxDist, mask, blocked, options);

                    //save result
                    foreach (Vector2Int pos in expanded)
                    {
                         if (result.Contains(pos) == false)
                         {
                              result.Add(pos);
                         }
                    }
               }

               return result;
          }



          public static bool ValidatePath(PFGrid grid, List<Vector2Int> path, int maxDist, int mask, List<Vector2Int> blocked)
          {
               //basic
               if (path.Count == 0)
               {
                    return false;
               }

               //dist
               if (maxDist < path.Count - 1)
               {
                    return false;
               }

               //blocked
               if (blocked != null)
               {
                    foreach (var pos in path)
                    {
                         if (blocked.Contains(pos))
                         {
                              return false;
                         }
                    }
               }

               //check the path itself
               for (int i = 0; i < path.Count - 1; i++)
               {
                    if (ValidateMove(grid, path[i], path[i + 1], mask) == false)
                    {
                         return false;
                    }
               }

               return true;
          }


          static bool ValidateMove(PFGrid grid, Vector2Int posA, Vector2Int posB, int moveMask)
          {
               PFNode nodeA = grid.GetNode(posA);
               PFNode nodeB = grid.GetNode(posB);

               if (nodeA == null) return false;
               if (nodeB == null) return false;

               //move mask
               if ((nodeA.mask & moveMask) == 0) return false;
               if ((nodeB.mask & moveMask) == 0) return false;

               //are neighbours
               if (nodeA.Neighbour.ContainsKey(nodeB) == false) return false;

               //passage move mask
               PFPassage passage = nodeA.Neighbour[nodeB];
               if ((passage.mask & moveMask) == 0) return false;

               return true;
          }

          public static bool ValidateNeighbour(PFGrid grid, Vector2Int posA, Vector2Int posB)
          {
               PFNode nodeA = grid.GetNode(posA);
               PFNode nodeB = grid.GetNode(posB);

               if (nodeA == null) return false;
               if (nodeB == null) return false;

               return nodeA.Neighbour.ContainsKey(nodeB);
          }


          public static bool GetClosestPointInRange(PFGrid grid, List<Vector2Int> range, Vector2Int target, int mask, out Vector2Int result)
          {

               //initial
               var openList = new List<PFNode>();
               var closeList = new List<PFNode>();


               PFNode targetNode = grid.GetNode(target.x, target.y);
               PFNode curNode = null;

               targetNode.moveCost = 0;
               targetNode.heuristic = 0;
               targetNode.LastNode = null;


               //start go through the list
               openList.Add(targetNode);

               while (openList.Count > 0)
               {
                    //find node closest to target pos, aka smallest move cost
                    curNode = openList[0];

                    for (int i = 1; i < openList.Count; i++)
                    {
                         PFNode node = openList[i];

                         if (node.moveCost < curNode.moveCost)
                         {
                              curNode = node;
                         }
                    }


                    //if reach moverange
                    if (range.Contains(curNode.pos) && (mask & curNode.mask) != 0)//the very first node might be inhibitble
                    {
                         result = curNode.pos;
                         return true;
                    }
                    else
                    {
                         //finish with current node
                         openList.Remove(curNode);
                         closeList.Add(curNode);
                    }


                    //look through neighbour
                    foreach (var pair in curNode.Neighbour)
                    {

                         PFNode neighbour = pair.Key;
                         PFPassage passage = pair.Value;

                         //check: already encountered
                         if (closeList.Contains(neighbour))
                         {
                              continue;
                         }

                         //get move cost to this neighbour from current node
                         float _moveCost = curNode.moveCost + passage.distance;


                         //pass all the check
                         //this node is now available
                         if (openList.Contains(neighbour) == false)
                         {
                              //first time encounter
                              openList.Add(neighbour);

                              neighbour.moveCost = _moveCost;
                              neighbour.LastNode = curNode;
                         }
                         else
                         {
                              //is in open list, but cost WAS not low enough to be choosed
                              //see if cost can be improved
                              if (_moveCost < neighbour.moveCost)
                              {
                                   neighbour.moveCost = _moveCost;
                                   neighbour.LastNode = curNode;
                              }
                         }

                    }
               }

               result = new Vector2Int();
               return false;
          }


     }



}













//     public static bool GetFurthestPointInRange(PFGrid grid, List<Vector2Int> range, Vector2Int target, out Vector2Int result)
//     {

//          //initial
//          var openList = new List<PFNode>();

//          PFNode startNode = grid.GetNode(target.x, target.y);
//          PFNode curNode = null;

//          startNode.moveCost = 0;
//          startNode.heuristic = 0;
//          startNode.LastNode = null;

//          //start go through the list
//          openList.Add(startNode);

//          while (openList.Count > 0)
//          {
//               curNode = openList[0];

//               //look through neighbour
//               foreach (var pair in curNode.Neighbour)
//               {
//                    PFNode neighbour = pair.Key;
//                    PFPassage passage = pair.Value;

//                    //check: already encountered
//                    if (range.Contains(neighbour.pos))
//                    {
//                         //get move cost to this neighbour from current node
//                         float _moveCost = curNode.moveCost + passage.distance;

//                         //pass all the check
//                         //this node is now available
//                         if (openList.Contains(neighbour) == false)
//                         {
//                              //first time encounter
//                              openList.Add(neighbour);

//                              neighbour.moveCost = _moveCost;
//                              neighbour.LastNode = curNode;
//                         }
//                         else
//                         {
//                              //is in open list, but cost WAS not low enough to be choosed
//                              //see if cost can be improved
//                              if (_moveCost < neighbour.moveCost)
//                              {
//                                   neighbour.moveCost = _moveCost;
//                                   neighbour.LastNode = curNode;
//                              }
//                         }
//                    }
//               }
//          }

//          if (openList.Count == 0)
//          {
//               result = new Vector2Int();
//               return false;
//          }
//          else
//          {
//               PFNode furthestNode = null;

//               foreach (var node in openList)
//               {
//                    if (furthestNode == null || furthestNode.moveCost < node.moveCost)
//                    {
//                         furthestNode = node;
//                    }
//               }

//               result = furthestNode.pos;
//               return true;
//          }
//     }
//}

