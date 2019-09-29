using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PathInfo
{
    public List<ANode> path;
    public int cost;

    public PathInfo(List<ANode> path)
    {
        this.path = new List<ANode>(path);
        this.cost = path.Count;
    }

}

public class EdgeGenerator
{

    public PathInfo[,] M;
    private List<Vector3> turret_list;
    public Node[] turret_arr;
    public int numTurrets;
    private NodeGrid grid;

    public EdgeGenerator(List<Vector3> turret_list, NodeGrid grid)
    {
        this.turret_list = turret_list;
        numTurrets = turret_list.Count;
        turret_arr = new Node[numTurrets];
        int counter = 0;
        foreach(Vector3 turretPos in turret_list){
            turret_arr[counter] = grid.getClosestNode((int)turretPos.x, (int)turretPos.z);
          //  Debug.Log("TEST: " + turret_arr[counter].x + " _ " + turret_arr[counter].y + " counter: " + (counter+1));
            counter++;
            
        }
        this.grid = grid;
        M = new PathInfo[numTurrets, numTurrets];

        for(int i = 0; i < numTurrets; i++)
        {
            for(int j = i+1; j < numTurrets; j++)
            {
                AStar astar = new AStar(grid);
                astar.init(turret_arr[i].index.x, turret_arr[i].index.y, turret_arr[j].index.x, turret_arr[j].index.y, 0);
                astar.findPath();
                M[i, j] = new PathInfo(astar.result);//it should be reversed
                M[j, i] = new PathInfo(astar.result);
                M[i, j].path.Reverse();
                //Debug.Log("PATH: " + astar.result.Count);
            }
            M[i, i] = new PathInfo(new List<ANode>());
        }



    }

}
