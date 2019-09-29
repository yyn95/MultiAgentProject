using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CostPoint
{
    public int numTurrets;
    public List<int> turretsAtPoint;

    public CostPoint()
    {
        numTurrets = 0;
        turretsAtPoint = new List<int>();
    }

    public bool has(int target)
    {
        return turretsAtPoint.Contains(target);
    }

    public bool shareTurret(CostPoint other)
    {
        foreach(int a in other.turretsAtPoint)
        {
            foreach (int b in turretsAtPoint)
            {
                if (a == b)
                    return true;
            }
        }

        return false;
    }

}


public class Targ
{
    public TargetPoint target1, target2, target3;
}


public class TargetPoint
{
    public static readonly int UP = 0, UPRIGHT = 1, RIGHT = 2, DOWNRIGHT = 4, DOWN = 5, DOWNLEFT = 6, LEFT = 7, UPLEFT = 8;

    public int mapX, mapY;
    public float x, y;
    public int dir;
    public int numEnemies;
    public int neighbors = 0;

    public TargetPoint(int mapX, int mapY, int dir)
    {
        this.mapX = mapX;
        this.mapY = mapY;
        this.dir = dir;
    }
    public TargetPoint()
    {
        mapX = mapY = dir = -1;
        numEnemies = Int32.MaxValue;
    }

}