using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoardComponent;
using AStarPathFinder;


public static class BoardToPFGrid
{

    public static PFGrid CreatePFGrid(Board board)
    {

        PFGrid grid = new PFGrid(board.sizeX, board.sizeY);

        //add node
        foreach (Tile tile in board.TileList)
        {
            PFNode node = grid.AddNode(tile.Pos);
            node.SetMask((int)tile.moveMask);
        }


        //connect nodes
        foreach (Tile tile in board.TileList)
        {
            Vector2Int pos = tile.Pos;

            //connect to top
            Tile tTop = board.GetTile(pos.x, pos.y + 1);

            if (tTop != null)
            {
                PFPassage passage = grid.Connect(pos, pos + Vector2Int.up, PFGrid.defaultMoveCost, 0);

                if (tile.topWalled)
                {
                    passage.mask = (int)PassageMoveMask.Wall;
                }
                else
                {
                    passage.mask = (int)PassageMoveMask.NoWall;
                }
            }

            //connect to right
            Tile tRight = board.GetTile(pos.x + 1, pos.y);

            if (tRight != null)
            {
                PFPassage passage = grid.Connect(pos, pos + Vector2Int.right, PFGrid.defaultMoveCost, 0);

                if (tile.rightWalled)
                {
                    passage.mask = (int)PassageMoveMask.Wall;
                }
                else
                {
                    passage.mask = (int)PassageMoveMask.NoWall;
                }
            }
        }

        return grid;

    }




}
