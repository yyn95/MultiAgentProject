using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IHasNeighbours<N>
{
    IEnumerable<N> Neighbours { get; }
    bool sameSquare(N other);
    int GetNodeCost();
}

public class ANode : IHasNeighbours<ANode>
{
    public NodeGrid grid;
    public int cost;
    public int ID;
    public int x, y;
    public int turns;
    public int carDir;

    private static readonly int maxTurns = 0;
    private static readonly int extraCost = 0;
    private static readonly int turnCost = 1;
    private static readonly int diagonalCost = 5;
    
    public static readonly int UP = 0, UPRIGHT = 1, RIGHT = 2, DOWNRIGHT = 4, DOWN = 5, DOWNLEFT = 6, LEFT = 7, UPLEFT = 8;

    public ANode() { }
    public ANode(int x, int y, int cost , NodeGrid grid, int carDir = -1, int turns = 0)
    {
        this.x = x;
        this.y = y;
        this.cost = cost;
        this.grid = grid;
        this.carDir = carDir;
        this.turns = turns;
    }

    public ANode Up
    {
        get
        {
            if (carDir != UP && turns > 0)
                return null;
            if (carDir == RIGHT || carDir == LEFT)
                return null;
            if (carDir == DOWNLEFT || carDir == DOWNRIGHT || carDir == DOWN)
                return grid.GetNodeStar(this, x, y - 1, cost + extraCost, UP, maxTurns);
            if (carDir != UP)
                return grid.GetNodeStar(this, x, y - 1, cost + turnCost, UP, maxTurns);
            return grid.GetNodeStar(this, x, y - 1, cost, UP, turns - 1);
        }
    }

    public ANode UpLeft
    {
        get
        {
            if (carDir != UPLEFT && turns > 0)
                return null;
            if (carDir == UPRIGHT || carDir == DOWNLEFT)
                return null;
            if (carDir == DOWN || carDir == RIGHT || carDir == DOWNRIGHT)
                return grid.GetNodeStar(this, x - 1, y - 1, cost + extraCost+ diagonalCost, UPLEFT, maxTurns);
            if (carDir != UPLEFT)
                return grid.GetNodeStar(this, x - 1, y - 1, cost + turnCost+ diagonalCost, UPLEFT, maxTurns);
            return grid.GetNodeStar(this, x - 1, y - 1, cost+ diagonalCost, UPLEFT, turns - 1);
        }
    }

    public ANode UpRight
    {
        get
        {
            if (carDir != UPRIGHT && turns > 0)
                return null;
            if (carDir == UPLEFT || carDir == DOWNRIGHT)
                return null;
            if (carDir == DOWN || carDir == LEFT || carDir == DOWNLEFT)
                return grid.GetNodeStar(this, x + 1, y - 1, cost + extraCost+ diagonalCost, UPRIGHT, maxTurns);
            if (carDir != UPRIGHT)
                return grid.GetNodeStar(this, x + 1, y - 1, cost + turnCost+ diagonalCost, UPRIGHT, maxTurns);
            return grid.GetNodeStar(this, x + 1, y - 1, cost+ diagonalCost, UPRIGHT, turns - 1);
        }
    }

    public ANode DownRight
    {
        get
        {
            if (carDir != DOWNRIGHT && turns > 0)
                return null;
            if (carDir == DOWNLEFT || carDir == UPRIGHT)
                return null;
            if (carDir == UP || carDir == LEFT || carDir == UPLEFT)
                return grid.GetNodeStar(this, x + 1, y + 1, cost + extraCost+ diagonalCost, DOWNRIGHT, maxTurns);
            if (carDir != DOWNRIGHT)
                return grid.GetNodeStar(this, x + 1, y + 1, cost + turnCost+ diagonalCost, DOWNRIGHT, maxTurns);
            return grid.GetNodeStar(this, x + 1, y + 1, cost+ diagonalCost, DOWNRIGHT, turns - 1);
        }
    }

    public ANode DownLeft
    {
        get
        {
            if (carDir != DOWNLEFT && turns > 0)
                return null;
            if (carDir == UPLEFT || carDir == DOWNRIGHT)
                return null;
            if (carDir == UP || carDir == RIGHT || carDir == UPRIGHT)
                return grid.GetNodeStar(this, x - 1, y + 1, cost + extraCost+ diagonalCost, DOWNLEFT, maxTurns);
            if (carDir != DOWNLEFT)
                return grid.GetNodeStar(this, x - 1, y + 1, cost + turnCost+ diagonalCost, DOWNLEFT, maxTurns);
            return grid.GetNodeStar(this, x - 1, y + 1, cost+ diagonalCost, DOWNLEFT, turns - 1);
        }
    }


    public ANode Down
    {
        get
        {
            if (carDir != DOWN && turns > 0)
                return null;
            if (carDir == RIGHT || carDir == LEFT)
                return null;
            if (carDir == UPLEFT || carDir == UPRIGHT || carDir == UP)
                return grid.GetNodeStar(this, x, y + 1, cost + extraCost, DOWN, maxTurns);
            if (carDir != DOWN)
                return grid.GetNodeStar(this, x, y + 1, cost + turnCost, DOWN, maxTurns);
            return grid.GetNodeStar(this, x, y + 1, cost, DOWN, turns - 1);
        }
    }

    public ANode Left
    {
        get
        {
            if (carDir != LEFT && turns > 0)
                return null;
            if (carDir == UP || carDir == DOWN)
                return null;
            if (carDir == DOWNRIGHT || carDir == UPRIGHT || carDir == RIGHT)
                return grid.GetNodeStar(this, x - 1, y, cost + extraCost, LEFT, maxTurns);
            if (carDir != LEFT)
                return grid.GetNodeStar(this, x - 1, y, cost + turnCost, LEFT, maxTurns);
            return grid.GetNodeStar(this, x - 1, y, cost, LEFT, turns - 1);
        }
    }

    public ANode Right
    {
        get
        {
            if (carDir != RIGHT && turns > 0)
                return null;
            if (carDir == UP || carDir == DOWN)
                return null;
            if (carDir == UPLEFT || carDir == DOWNLEFT || carDir == LEFT)
                return grid.GetNodeStar(this, x + 1, y, cost + extraCost, RIGHT, maxTurns);
            if (carDir != RIGHT)
                return grid.GetNodeStar(this, x + 1, y, cost + turnCost, RIGHT, maxTurns);
            return grid.GetNodeStar(this, x + 1, y, cost, RIGHT, turns - 1);
        }
    }

    public IEnumerable<ANode> Neighbours
    {
        get
        {
            ANode[] neighbors = new ANode[] { Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight };
            foreach (ANode neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    yield return neighbor;
                }
            }
        }
    }

    public Vector2Int getLoc()
    {
        return new Vector2Int(x,y);
    }

    public bool sameSquare(ANode other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        var item = obj as ANode;
        if (item == null)
        {
            return false;
        }
        return x == item.x && y == item.y && carDir == item.carDir;
    }

    public override int GetHashCode()
    {
        return (x.GetHashCode() * 7) ^ (y.GetHashCode() * 3) ^ carDir;
    }

    public int GetNodeCost()
    {
        return cost;
    }
}
