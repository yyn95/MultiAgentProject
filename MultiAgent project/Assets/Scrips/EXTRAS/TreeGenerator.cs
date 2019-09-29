using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


public class TreeGenerator
{
    private Node[,] Map2D;
    private List<Robot> robots;
    private NodeGrid grid;
    private Robot robot1, robot2, robot3;

    public TreeGenerator(Node[,] Map2D, NodeGrid grid, List<Robot> robots)
    {
        this.grid = grid;
        this.Map2D = Map2D;
        this.robots = robots;
        robot1 = robots.ElementAt(0);
        robot2 = robots.ElementAt(1);
        robot3 = robots.ElementAt(2);
    }

    public void GenerateTree()
    {
        bool done = true;
        while (done)
        {
            foreach (Robot robot in robots)
            {
                if (robot.noMoreMoves)
                    continue;
                var moves = getPossibleMoves(robot.subtree.Last.Value);
                var bestMove = maximizeMinMove(moves, robot.ID);
                if (bestMove == null)
                {
                    //Hilling
                    //hilling not possible? Branch out.
                    robot.noMoreMoves = Hilling(robot.subtree, robot.ID);
                }
                else
                {
                    robot.subtree.AddLast(bestMove);
                    bestMove.ID = robot.ID;
                }
            }
            done = false;
            foreach (Robot robot in robots)
                if (!robot.noMoreMoves)
                    done = true;
        }
    }

    private bool rightFree(Node n1, Node n2)
    {
        int x1, x2, y1, y2;
        x1 = n1.index.x; x2 = n2.index.y; y1 = n1.index.x; y2 = n2.index.y;
        return x1 + 1 < grid.width && Map2D[x1 + 1, y1].ID == 0 && Map2D[x2 + 1, y2].ID == 0;
    }

    private enum DIR
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    private bool canHill(int x1, int y1, int x2, int y2, DIR dir)
    {
        if (y1 != y2)
        {
            if (dir == DIR.RIGHT)
                if (x1 + 1 < grid.width && Map2D[x1 + 1, y1] != null && Map2D[x2 + 1, y2] != null &&
                Map2D[x1 + 1, y1].ID == 0 && Map2D[x2 + 1, y2].ID == 0) //RIGHT
                {
                    return true;
                }
            if (dir == DIR.LEFT)
                if (x1 - 1 >= 0 && Map2D[x1 - 1, y1] != null && Map2D[x2 - 1, y2] != null &&
                Map2D[x1 - 1, y1].ID == 0 && Map2D[x2 - 1, y2].ID == 0) //LEFT
                {
                    return true;
                }
        }
        if (x1 != x2)
        {
            if (dir == DIR.UP)
                if (y1 - 1 >= 0 && Map2D[x1, y1 - 1] != null && Map2D[x2, y2 - 1] != null &&
                Map2D[x1, y1 - 1].ID == 0 && Map2D[x2, y2 - 1].ID == 0)
                {
                    return true;
                }
            if (dir == DIR.DOWN)
                if (y1 + 1 < grid.height && Map2D[x1, y1 + 1] != null && Map2D[x2, y2 + 1] != null &&
                Map2D[x1, y1 + 1].ID == 0 && Map2D[x2, y2 + 1].ID == 0)
                {
                    return true;
                }
        }

        return false;
    }

    private bool Hilling(LinkedList<Node> subtree, int robotID)
    {

        for (LinkedListNode<Node> it = subtree.First; it != null && it.Next != null;)
        {
            LinkedListNode<Node> next = it.Next;

            Node n1 = it.Value;
            Node n2 = next.Value;

            int x1, x2, y1, y2;
            x1 = n1.index.x; x2 = n2.index.x; y1 = n1.index.y; y2 = n2.index.y;
            //Horizontal difference, inspect up, down

            if (canHill(x1, y1, x2, y2, DIR.UP))
            {
                Node newn1 = Map2D[x1, y1 - 1];
                Node newn2 = Map2D[x2, y2 - 1];
                newn1.ID = robotID;
                newn2.ID = robotID;
                subtree.AddAfter(it, newn1);
                it = it.Next;
                subtree.AddAfter(it, newn2);
                /*if (x1 < x2)*/
                return false;
            }
            if (canHill(x1, y1, x2, y2, DIR.DOWN))
            {
                Node newn1 = Map2D[x1, y1 + 1];
                Node newn2 = Map2D[x2, y2 + 1];
                newn1.ID = robotID;
                newn2.ID = robotID;
                subtree.AddAfter(it, newn1);
                it = it.Next;
                subtree.AddAfter(it, newn2);
                return false;

            }
            if (canHill(x1, y1, x2, y2, DIR.RIGHT))
            {
                Node newn1 = Map2D[x1+1, y1];
                Node newn2 = Map2D[x2+1, y2];
                newn1.ID = robotID;
                newn2.ID = robotID;
                subtree.AddAfter(it, newn1);
                it = it.Next;
                subtree.AddAfter(it, newn2);
                return false;
            }
            if (canHill(x1, y1, x2, y2, DIR.LEFT))
            {
                Node newn1 = Map2D[x1 - 1, y1];
                Node newn2 = Map2D[x2 - 1, y2];
                newn1.ID = robotID;
                newn2.ID = robotID;
                subtree.AddAfter(it, newn1);
                it = it.Next;
                subtree.AddAfter(it, newn2);
                return false;
            }
            //Vertical difference, inspect left, right
           /* if (n1.mapY > n2.mapY)
            {
            }
            if (n1.mapY < n2.mapY)
            {
            }*/

            //subtree.Remove(it);
            it = it.Next;
        }

        return true;
    }

    //calculate Manhanttan distance between two nodes
    private float Manhattan(Node n1, Node n2) { return Math.Abs(n1.position.x - n2.position.x) + Math.Abs(n1.position.z - n2.position.z); }

    List<Node> getPossibleMoves(Node nowNode)
    {
        List<Node> result = new List<Node>();
        int nX = nowNode.index.x;
        int nY = nowNode.index.y;
        //Debug.Log("WAT " + nX + " , " + nY + " TEST: "  + Map2D[0, 0].ID + " "+ grid.width);
        if (nX - 1 >= 0 && Map2D[nX - 1, nY] != null && Map2D[nX - 1, nY].ID == 0)
        {
            result.Add(Map2D[nX - 1, nY]);
        }
        if (nY - 1 >= 0 && Map2D[nX, nY - 1] != null && Map2D[nX, nY - 1].ID == 0)
        {
            result.Add(Map2D[nX, nY - 1]);
        }
        if (nX + 1 < grid.width && Map2D[nX + 1, nY] != null && Map2D[nX + 1, nY].ID == 0)
        {
            result.Add(Map2D[nX + 1, nY]);
        }
        if (nY + 1 < grid.height && Map2D[nX, nY + 1] != null && Map2D[nX, nY + 1].ID == 0)
        {
            result.Add(Map2D[nX, nY + 1]);
        }

        return result;
    }

    private Node maximizeMinMove(List<Node> movesPossible, int cur_robotID)
    {
        float bestDistance = Int32.MinValue;
        Node chosenNode = null;
        foreach (Node n in movesPossible)
        {
            float dist = MinDist(n, cur_robotID);
            if (dist > bestDistance)
            {
                bestDistance = dist;
                chosenNode = n;
            }
        }
        return chosenNode;
    }

    //calculate the minimum distance between the potential node and the other cars
    private float MinDist(Node potential_move, int cur_robotID)
    {
        float min_distance = Int32.MaxValue;
        Node latest1 = null; Node latest2 = null;
        if (robot1.subtree.Count == 0 || robot2.subtree.Count == 0 || robot3.subtree.Count == 0)
            return 0;

        if (cur_robotID == 1)
        {
            latest1 = robot2.subtree.Last.Value;
            latest2 = robot3.subtree.Last.Value;
        }
        if (cur_robotID == 2)
        {
            latest1 = robot1.subtree.Last.Value;
            latest2 = robot3.subtree.Last.Value;
        }
        if (cur_robotID == 3)
        {
            latest1 = robot1.subtree.Last.Value;
            latest2 = robot2.subtree.Last.Value;
        }
        min_distance = Math.Min(min_distance, Manhattan(potential_move, latest1));
        min_distance = Math.Min(min_distance, Manhattan(potential_move, latest2));
        return min_distance;
    }

}
