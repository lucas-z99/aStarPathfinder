using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AStarPathFinder
{

    public class PFGrid
    {

        public static float defaultMoveCost = 1;//move cost per tile, aka the distance between two neighbour tiles   

        //About move cost per tile, for AStar this should almost always be 1 or higher
        //Cost of 1 reflect the geomatic relationship of two neighbour tiles
        //Cost higher then 1 represent a harsh terrain that will cost extra move points

        //Not suggest to use a value lower than 1
        //A value lower then 1 represents a non-realistic terrain,
        //such as a teleporter tile, will have a moveCost = 1 / distance, or a move range expander tile with a negative cost

        //Because AStar will settle for a path as soon as it founds one, so all distant tiles will be skipped, even with lower cost
        //This results in a somewhat realistic, but short-sighted AI behavier, who is able to utilize non-realistic special terrain, but will ignore them anyway when it is not conveniently 'heuristically' closer to target
        //DO USE this feature, if such behavier is intended


        //private

        public int sizeX { get; private set; }
        public int sizeY { get; private set; }
        PFNode[,] nodeList;


        public PFGrid(int sizeX, int sizeY)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;

            //create node
            nodeList = new PFNode[sizeX, sizeY];
        }


        //add node
        public PFNode AddNode(int x, int y)
        {
            if (x >= sizeX && y >= sizeY && x < 0 && y < 0) { Debug.LogError(""); return null; }
            if (nodeList[x, y] != null) { Debug.LogError(""); return null; }

            var node = new PFNode(x, y);
            nodeList[x, y] = node;

            return node;

        }
        public PFNode AddNode(Vector2Int pos)
        {
            return AddNode(pos.x, pos.y);
        }


        //get node
        public PFNode GetNode(int x, int y)
        {
            if (x < sizeX && y < sizeY && x >= 0 && y >= 0)
            {
                return nodeList[x, y];
            }

            return null;
        }
        public PFNode GetNode(Vector2Int pos)
        {
            return GetNode(pos.x, pos.y);
        }


        //connect nodes
        public PFPassage Connect(Vector2Int posA, Vector2Int posB, float moveCost, int moveMask)
        {
            PFNode nodeA = GetNode(posA);
            PFNode nodeB = GetNode(posB);

            return Connect(nodeA, nodeB, moveCost, moveMask);
        }
        public PFPassage Connect(PFNode nodeA, PFNode nodeB, float distDefault, int moveMask)
        {
            if (nodeA != null && nodeB != null)
            {
                if (nodeA.Neighbour.ContainsKey(nodeB) == false)
                {
                    PFPassage pass = new PFPassage(distDefault, moveMask);

                    nodeA.Neighbour.Add(nodeB, pass);
                    nodeB.Neighbour.Add(nodeA, pass);

                    return pass;
                }
            }

            return null;
        }

    }



    public class PFNode
    {

        public Vector2Int pos { get; private set; }
        public int mask { get; private set; }
        public Dictionary<PFNode, PFPassage> Neighbour = new Dictionary<PFNode, PFPassage>();


        //runtime var
        public float moveCost;//total move cost from start node
        public float heuristic;//'direct distance' move cost to end node
        public float mCost { get { return moveCost + heuristic; } }
        public PFNode LastNode;



        public PFNode(int x, int y)
        {
            pos = new Vector2Int(x, y);
        }

        public void SetMask(int mask)
        {
            this.mask = mask;
        }
    }



    public class PFPassage
    {
        //a passage between two nodes
        public float distance;
        public int mask;

        public PFPassage(float distance, int mask)
        {
            this.distance = distance;
            this.mask = mask;
        }
    }





}

