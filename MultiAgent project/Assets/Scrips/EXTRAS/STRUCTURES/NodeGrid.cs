using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scrips.EXTRAS.STRUCTURES
{
    public class NodeGrid
    {
        private TerrainManager manager;
        public Node[,] grid;

        public int width, height;
        public float xlow, zlow, xhigh, zhigh;
        public float xstep, zstep;
        
        //for problem 5
        private int[,] costgrid;
        public void setCostGrid(int[,] costgrid)
        {
            this.costgrid = costgrid;
        }

        public NodeGrid(TerrainManager manager, int minification)
        {
            this.manager = manager;
            xlow = manager.myInfo.x_low;
            zlow = manager.myInfo.z_low;
            xhigh = manager.myInfo.x_high;
            zhigh = manager.myInfo.z_high;
            
            width = manager.myInfo.x_N *  minification;
            height = manager.myInfo.z_N *  minification;
            xstep = (xhigh - xlow) / width;
            zstep = (zhigh - zlow) / height;
            grid = new Node[width, height];
            costgrid = new int[width,height]; //for problem 1,2,3, it will be 0
            
            //initiate all available nodes with ID = -1, obstacle node is null
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float x = get_x_pos(i);
                    float y = get_z_pos(j);
                    //check if the cell contain obstacles, obstacle node is null, elso ID = 0
                    if(ObstacleFound(x, y)){
                        grid[i, j] = null;
                    }
                    else{
                        grid[i, j] = new Node(x, y, 0,i,j);
                    }
                }
            }
        }

        public bool ObstacleFound(float x, float y)
        {
            int i = get_i_index(x, false);
            int j = get_j_index(y, false);
            if (manager.myInfo.traversability[i, j] > 0.5f){return true;}
            else{return false;}
        }
        
        public Node getNode(float x, float y)
        {       
            int i = get_i_index(x, true);
            int j = get_j_index(y,true);
            return grid[i, j];
        }
       
        
        public Node getClosestNode(int x, int y)
        {
            Node n = getNode(x, y);
            if (n != null)
                return n;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (grid[i, j] == null)
                        continue;

                    int off = 7;

                    float gx = grid[i, j].position.x;
                    float gy = grid[i, j].position.z;

                    if (
                        gx-off < x &&  x < gx + off &&
                        gy - off < y && y < gy + off)
                    {
                        return grid[i, j];
                    }
                }
            }

            return null;
        }
        
        public ANode GetNodeStar(ANode prev, int x, int y, int cost, int carDir = -1, int turns = 0)
        {
            if (IsOnGrid(x, y))
            {
                int extracost = costgrid[x, y]*10;
                cost += extracost;
                if (!IsOnGrid(x - 1, y) || !IsOnGrid(x + 1, y)|| !IsOnGrid(x , y+1) || !IsOnGrid(x, y-1))
                {
                    cost += 20;
                }

                ANode node = new ANode(x,y, cost + 1, this, carDir,turns);
                /*if(!IsOnGrid(x + 1, y+1) || !IsOnGrid(x - 1, y-1))
                {
                    node.cost += 150;
                }*/
                int px = prev.x;
                int py = prev.y;
                if(carDir == ANode.DOWNLEFT)
                {
                    if (!IsOnGrid(px - 1, py) || !IsOnGrid(px, py + 1))
                        return null;
                }
                if (carDir == ANode.DOWNRIGHT)
                {
                    if (!IsOnGrid(px + 1, py) || !IsOnGrid(px, py + 1))
                        return null;
                }
                if (carDir == ANode.UPLEFT)
                {
                    if (!IsOnGrid(px - 1, py) || !IsOnGrid(px, py - 1))
                        return null;
                }
                if (carDir == ANode.UPRIGHT)
                {
                    if (!IsOnGrid(px + 1, py) || !IsOnGrid(px, py - 1))
                        return null;
                }

                return node;
            }
            return null;
        }

        
        public bool IsOnGrid(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return false;
            if (grid[x, y] == null)
                return false;
            return true;
        }
        
        // get index of given coordinate
        //maptype == true: 2D map, false: terrain D map
        public int get_i_index(float x, bool maptype)
        {
            int x_N = width;            
            if (!maptype)
            {
                x_N = manager.myInfo.x_N;
            }
            int index = (int) Mathf.Floor(x_N * (x - xlow) / (xhigh - xlow));
            if (index < 0)
            {
                index = 0;
            }else if (index > x_N - 1)
            {
                index = x_N - 1;
            }
            return index;
        }
        
        public int get_j_index(float z, bool maptype) 
        {
            int z_N = height;            
            if (!maptype)
            {
                z_N = manager.myInfo.z_N;
            }
            int index = (int)Mathf.Floor(z_N * (z - zlow) / (zhigh - zlow));
            if (index < 0)
            {
                index = 0;
            }
            else if (index > z_N - 1)
            {
                index = z_N - 1;
            }
            return index;
        }

        public float get_x_pos(int i) {return (xlow + xstep / 2 + xstep * i);}

        public float get_z_pos(int j) {return (zlow + zstep / 2 + zstep * j);}
        

    }

}
