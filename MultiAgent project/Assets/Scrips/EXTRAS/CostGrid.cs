using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CostGrid
{

    List<Vector3> turret_list;
    List<GameObject> turretObj_list;
    private RaycastHit hit;
    private NodeGrid nodegrid;
    public int width, height;
    public CostPoint[,] costMap;
    private Node[,] Map2D;
    public int[][,] costMaps;

    private void copyGrid(int index)
    {
        if (index >= turret_list.Count)
            return;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                costMaps[index][i, j] = costMap[i, j].numTurrets;
            }
        }
    }

    public CostGrid(GameManager gameManager, NodeGrid nodegrid)
    {
        width = nodegrid.width;
        height = nodegrid.height;
        Map2D = nodegrid.grid;
        costMap = new CostPoint[width, height];
        targets = new List<Targ>();
        turretObj_list = gameManager.turret_list;
        turret_list = new List<Vector3>();
        this.nodegrid = nodegrid;
        foreach (GameObject turret in turretObj_list)
        {
            turret_list.Add(turret.transform.position);
        }

        costMaps = new int[turret_list.Count][,];
        for (int i = 0; i < turret_list.Count; i++)
        {
            targets.Add(new Targ());
            costMaps[i] = new int[width, height];
        }

    }

    private bool checkHit(Vector3 nodePos, Vector3 tmpDirection, Vector3 turrPos)
    {
        int countHits = 0;
        Vector3 tmpPos = new Vector3(nodePos.x, nodePos.y, nodePos.z);
        if (Physics.Raycast(tmpPos, tmpDirection, out hit, Mathf.Infinity))
        {
            String hitname = hit.collider.gameObject.name;
            if (hitname.Equals("Cylinder"))
            {
                countHits++;
            }
        }
        tmpPos.x += 0f;
        tmpPos.y += 1f;
        tmpDirection = (turrPos - tmpPos);
        if (Physics.Raycast(tmpPos, tmpDirection, out hit, Mathf.Infinity))
        {
            String hitname = hit.collider.gameObject.name;
            if (hitname.Equals("Cylinder"))
            {
                countHits++;
            }
        }

        tmpPos.x += 1f;
        tmpPos.y -= 1f;
        tmpDirection = (turrPos - tmpPos);
        if (Physics.Raycast(tmpPos, tmpDirection, out hit, Mathf.Infinity))
        {
            String hitname = hit.collider.gameObject.name;
            if (hitname.Equals("Cylinder"))
            {
                countHits++;
            }
            else
            {
            }
        }

        return countHits == 3;
    }

    public CostPoint countHits(Vector3 nodePos)
    {
        int res = 0;
        nodePos.y = 3f;
        int counter = 0;
        CostPoint costpoint = new CostPoint();

        foreach (Vector3 turrPosTmp in turret_list)
        {
            var turrPos = turrPosTmp;
            turrPos.y = 3f;
            Vector3 tmpDirection = (turrPos - nodePos);

            if (checkHit(nodePos, tmpDirection, turrPos))
            {
                costpoint.turretsAtPoint.Add(counter);
                res++;
            }

            counter++;
        }

        costpoint.numTurrets = res;
        return costpoint;
    }

    public void removePoint(int x, int y)
    {
        CostPoint costTmp = costMap[x, y];
        List<int> turrRmv = new List<int>(costTmp.turretsAtPoint);
        //costTmp.numTurrets = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //scostMap[i, j].turretsAtPoint.Except(turrRmv).ToList();
                foreach (int id in turrRmv)
                {
                    costMap[i, j].turretsAtPoint.Remove(id);
                }
                costMap[i, j].numTurrets = costMap[i, j].turretsAtPoint.Count;
            }
        }

    }

    public void createTargets()
    {
        //targets = new List<TargetPoint>();
        Targ target = null;

        int counter = 0;
        while (true)
        {
            if (counter == targets.Count)
                return;
            
            copyGrid(counter);
            counter++;
            // IOstuff iostuff = new IOstuff();
            // iostuff.storeTextCosts("testGridShape"+counter.ToString(), this);

            target = getTarget();
            if (target == null)
                return;
            else
                targets[counter-1] = target;
        }

    }

    TargetPoint res = null;
    int prevTarg = -1;
    bool firstDone = false;
    public Targ getTarget()
    {
        TargetPoint res = new TargetPoint();
        Targ targ = new Targ();
        bool[,] visited = new bool[width, height];
        bestDistance = Int32.MaxValue;
        targ.target1 = targ.target2 = targ.target3 = new TargetPoint();

        prevTarg = -1;
        firstDone = false;
        bfs(res, -1, visited, startX1, startY1, targ);
        firstDone = true;

        targ.target1 = res;
        startX1 = res.mapX;
        startY1 = res.mapY;

        res = new TargetPoint();
        visited = new bool[width, height];
        bfs(res, -1, visited, startX2, startY2,targ);
        targ.target2 = res;
        startX2 = res.mapX;
        startY2 = res.mapY;

        res = new TargetPoint();
        visited = new bool[width, height];
        bfs(res, -1, visited, startX3, startY3, targ);
        targ.target3 = res;
        startX3 = res.mapX;
        startY3 = res.mapY;
        // if (res.numEnemies == Int32.MaxValue)
        //     return null;
        removePoint(startX1, startY1);
        // Debug.Log("TARGET INFO: " + res.x + " , " + res.y + " dir: " + res.dir);
        return targ;
    }

    
    private bool hasTurrets(CostPoint from, CostPoint to)
    {
        int counter = from.turretsAtPoint.Count;
        foreach (int id in to.turretsAtPoint)
        {
            if (from.turretsAtPoint.Contains(id))
                counter--;
        }

        return counter == 0;
    }

    private int calcNeighbors(int nx, int ny, int dir)
    {
        var nowP = costMap[nx, ny];
        int totalN = 0;
        if (dir == TargetPoint.UP || dir == TargetPoint.DOWN)
        {
            var upper = costMap[nx, ny + 1];
            var lower = costMap[nx, ny - 1];

            if (nowP.shareTurret(lower))
                totalN++;
            if (nowP.shareTurret(upper))
                totalN++;
        }
        else
        {
            var lefti = costMap[nx - 1, ny];
            var righti = costMap[nx + 1, ny];

            if (nowP.shareTurret(lefti))
                totalN++;
            if (nowP.shareTurret(righti))
                totalN++;
        }
        return totalN;
    }


    int bestDistance;
    
    private bool isTaken(int x, int y, Targ others)
    {
        if (others.target1.mapX == x && others.target1.mapY == y)
            return true;
        if (others.target2.mapX == x && others.target2.mapY == y)
            return true;
        if (others.target3.mapX == x && others.target3.mapY == y)
            return true;
        return false;
    }
    
    bool hasSquare(int x, int y, int target)
    {
        if (costMap[x + 1, y].has(target) && costMap[x, y+1].has(target) && costMap[x + 1, y+1].has(target))
        {
            return true;
        }
        if (costMap[x - 1, y].has(target) && costMap[x, y + 1].has(target) && costMap[x - 1, y-1].has(target))
        {
            return true;
        }
        if (costMap[x + 1, y].has(target) && costMap[x, y - 1].has(target) && costMap[x + 1, y-1].has(target))
        {
            return true;
        }
        if (costMap[x - 1, y].has(target) && costMap[x, y - 1].has(target) && costMap[x - 1, y-1].has(target))
        {
            return true;
        }

        return false;
    }

    private void bfs(TargetPoint t, int dir, bool[,] visited, int nx, int ny, Targ others)
    {
        if (nx <= 0 || ny <= 0 || nx >= width-1 || ny >= height-1 || Map2D[nx, ny] == null || visited[nx, ny])
            return;
        visited[nx, ny] = true;
        var nowP = costMap[nx, ny];
        int turrets = costMap[nx, ny].numTurrets;
        List<int> turList = costMap[nx, ny].turretsAtPoint;

        if (costMap[nx + 1, ny].numTurrets + costMap[nx - 1, ny].numTurrets == 0 &&
            costMap[nx, ny + 1].numTurrets + costMap[nx, ny - 1].numTurrets == 0)
        {

        }
        else
        {
            if (turrets > 0 && turrets < t.numEnemies && !isTaken(nx,ny,others))
            {
                if (firstDone && !costMap[nx, ny].has(prevTarg))
                    return;
                int arbitrary = costMap[nx, ny].turretsAtPoint[0];
                if (!hasSquare(nx, ny, arbitrary))
                    return;

                //  if(others.target1.numEnemies != 0 && !costMap[nx,ny].has())
                if(!firstDone)
                    prevTarg = costMap[nx, ny].turretsAtPoint[0];
                t.numEnemies = turrets;
                t.mapX = nx;
                t.mapY = ny;
                t.x = nodegrid.get_x_pos(nx);
                t.y = nodegrid.get_z_pos(ny);
                t.dir = dir;
                // bestDistance = getDistance(nx, ny);
                // t.neighbors = calcNeighbors(nx,ny,dir);

            }
            /* else if(turrets > 0 && turrets == t.numEnemies && getDistance(nx,ny) < bestDistance)
             {
                 t.numEnemies = turrets;
                 t.mapX = nx;
                 t.mapY = ny;
                 t.x = nodegrid.get_x_pos(nx);
                 t.y = nodegrid.get_z_pos(ny);
                 t.dir = dir;
                 bestDistance = getDistance(nx, ny);
             }*/
            else if (turrets > 0)
                return;
        }

        if (dir == TargetPoint.UP)
            bfs(t, TargetPoint.UP, visited, nx, ny + 1, others);
        if (dir == TargetPoint.DOWN)
            bfs(t, TargetPoint.DOWN, visited, nx, ny - 1, others);
        if (dir == TargetPoint.LEFT)
            bfs(t, TargetPoint.LEFT, visited, nx - 1, ny, others);
        if (dir == TargetPoint.RIGHT)
            bfs(t, TargetPoint.RIGHT, visited, nx + 1, ny, others);

        bfs(t, TargetPoint.UP, visited, nx, ny + 1, others);
        bfs(t, TargetPoint.RIGHT, visited, nx + 1, ny, others);
        bfs(t, TargetPoint.DOWN, visited, nx, ny - 1, others);
        bfs(t, TargetPoint.LEFT, visited, nx - 1, ny, others);
    }

    public List<Targ> targets;
    private int startX1, startY1, startX2, startY2, startX3, startY3;

    public void init(int startX, int startY)
    {
        this.startX1 = startX; this.startY1 = startY;
        startX2 = startX3 = startX;
        startY2 = startY3 = startY;

        Debug.Log(" POS: " + Map2D[startX, startY].position);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Node n = Map2D[i, j];
                if (n == null)
                {
                    // float x = get_x_pos(i);
                    // float y = get_z_pos(j);
                    costMap[i, j] = new CostPoint();
                }
                else
                {
                    Vector3 nodePos = n.position;
                    //  Debug.Log("x " + i + " y " + j + " POS: " + nodePos);
                    costMap[i, j] = countHits(nodePos);
                }
            }

        }

        //IOstuff iostuff = new IOstuff();
        //iostuff.storeTextCosts("testOkokok", this);

        createTargets();

        //Debug.Log("IT worked, " + targets.Count);

    }

}

