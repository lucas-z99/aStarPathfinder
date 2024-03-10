using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace AStarPathFinder
{

     //dummy base class
     public abstract class SearchOption
     {
          //
     }


     //For Path and Range  ================================================================================

     //mark those in result, if search point ever encounter them
     public class MarkTarget : SearchOption
     {
          public List<Vector2Int> list;
          public List<Vector2Int> result;

          public MarkTarget(List<Vector2Int> list)
          {
               this.list = list;
               result = new List<Vector2Int>();
          }
     }


     ////for terrain-like move cost
     ////note that this will not change search priority
     //public class MoveCost : SearchOption
     //{
     //     public Dictionary<Vector2Int, float> list { get; private set; }

     //     public MoveCost(Dictionary<Vector2Int, float> list)
     //     {
     //          this.list = list;
     //     }
     //}



     //For Path only  ================================================================================

     //decrease heuristic cost, so that the higher the value, the more priority algorithm will give to these tiles
     //note :not tested
     public class Preference : SearchOption
     {
          public Dictionary<Vector2Int, float> list { get; private set; }

          public Preference(Dictionary<Vector2Int, float> list)
          {
               this.list = list;
          }
     }



     ////For Range only  ================================================================================

     //public class Preference : SearchOption
     //{
     //     public Dictionary<Vector2Int, float> list { get; private set; }

     //     public Preference(Dictionary<Vector2Int, float> list)
     //     {
     //          this.list = list;
     //     }
     //}

}