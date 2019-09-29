using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class AStar
{
    Vector2Int start, end;
    NodeGrid grid;

    public static int getDistance(Vector2Int current, Vector2Int goal)
    {
        return Math.Max(Math.Abs(current.x - goal.x), Math.Abs(current.y - goal.y));
    }

    public AStar(NodeGrid grid)
    {
        this.grid = grid;
    }

    int carStartDir;

    public void init(int startX, int startY, int endX, int endY, float startAngle)
    {
        start = new Vector2Int(startX, startY);
        end = new Vector2Int(endX, endY);
        carStartDir = -1;
        /*if (startAngle >= 350 || startAngle <= 10)
            carStartDir = Node.DOWN;
        if (startAngle >= 80 && startAngle <= 110)
            carStartDir = Node.RIGHT;
        if (startAngle >= 170 && startAngle <= 190)
            carStartDir = Node.UP;
        if (startAngle >= 260 && startAngle <= 280)
            carStartDir = Node.LEFT;
        Debug.Log("Start angle test: " + carStartDir);*/
    }


    public Path<ANode> solution;
    public List<ANode> result;

    public void findPath()
    {
        ANode startNode = new ANode(start.x,start.y, 1, grid, carStartDir);
        ANode endNode = new ANode(end.x,end.y, 1, grid);

        solution = AStar.FindPath<ANode>(startNode, endNode, (p1, p2) => {
            return AStar.getDistance( p1.getLoc(), p2.getLoc()); }, (p1) => { return AStar.getDistance(p1.getLoc(), end); });

        result = new List<ANode>();
        if (solution != null)
        {
            IEnumerator<ANode> enumerator = solution.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ANode item = enumerator.Current;
                result.Add(item);
            }
        }

        //Console.WriteLine("RESULT: " + solution.TotalCost);
    }

    static public Path<Node> FindPath<Node>(
        Node start,
        Node destination,
        Func<Node, Node, int> distance,
        Func<Node, int> estimate)
        where Node : IHasNeighbours<Node>
    {
        var closed = new HashSet<Node>();
        var queue = new PriorityQueue<int, Path<Node>>();

        queue.Enqueue(0, new Path<Node>(start));
        while (!queue.IsEmpty)
        {
            var path = queue.Dequeue();
            if (closed.Contains(path.LastStep))
            {
                continue;
            }
            if (path.LastStep.sameSquare(destination))
                return path;
            closed.Add(path.LastStep);

            foreach (Node n in path.LastStep.Neighbours)
            {
                int d = distance(path.LastStep, n);
                var newPath = path.AddStep(n, d);
                queue.Enqueue(newPath.TotalCost + estimate(n) + n.GetNodeCost(), newPath);
            }
        }
        return null;
    }

}
