using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RoomGenerator
{
    NodeGrid grid;
    int width, height;
    bool[,] visited;
    Node[,] Map2D;

    public List<Rectangle> rooms;

    public RoomGenerator(NodeGrid grid)
    {
        this.grid = grid;
        Map2D = grid.grid;
        width = grid.width;
        height = grid.height;

        visited = new bool[width,height];
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if (Map2D[i, j] == null)
                    visited[i, j] = true;
            }
        }
        rooms = new List<Rectangle>();
    }
  
    public void init()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!visited[i, j])
                {
                    Rectangle rect = new Rectangle(i,j);
                    increaseRect(rect, j, i);
                    rect.setMid();
                    setVisited(rect);
                    rooms.Add(rect);
                   // Debug.Log("ROOM: x: " + rect.xmin + " - " + rect.xmax + "  y: " + rect.ymin + " - " + rect.ymax);
                   // Debug.Log("midx: " + rect.xmid + " ymid: " + rect.ymid);
                }
            }
        }
    }

    public List<Vector3> getPoints()
    {
        List<Vector3> result = new List<Vector3>();
        foreach(Rectangle room in rooms)
        {
            int x = (int)Map2D[room.xmid, room.ymid].position.x;
            int y = (int)Map2D[room.xmid, room.ymid].position.z;
            result.Add(new Vector3(x,2.0f,y));
        }

        return result;
    }

    private void setVisited(Rectangle rect)
    {
        for(int i = rect.xmin; i <= rect.xmax; i++)
        {
            for (int j = rect.ymin; j <= rect.ymax; j++)
            {
                visited[i, j] = true;
            }
        }
    }

    private bool canDownRight(Rectangle rect)
    {
        int x = rect.xmax;
        int y = rect.ymax;
        for(int i = rect.xmin; i <= rect.xmax; i++)
        {
            if (y+1 >= height || Map2D[i, y + 1] == null)
                return false;
        }
        for (int i = rect.ymin; i <= rect.ymax; i++)
        {
            if (x+1 >= width || Map2D[x+1, i] == null)
                return false;
        }
        return true;
    }

    private bool canUpLeft(Rectangle rect)
    {
        int x = rect.xmin;
        int y = rect.ymin;
        for (int i = rect.xmin; i <= rect.xmax; i++)
        {
            if (y-1 < 0 || Map2D[i, y - 1] == null)
                return false;
        }
        for (int i = rect.ymin; i <= rect.ymax; i++)
        {
            if ( x-1 < 0 || Map2D[x - 1, i] == null)
                return false;
        }
        return true;
    }

    private bool canUp(Rectangle rect)
    {
        int y = rect.ymin;
        for (int i = rect.xmin; i <= rect.xmax; i++)
        {
            if (y - 1 < 0 || Map2D[i, y - 1] == null)
                return false;
        }
        return true;
    }

    private bool canDown(Rectangle rect)
    {
        int y = rect.ymax;
        for (int i = rect.xmin; i <= rect.xmax; i++)
        {
            if (y + 1 >= height || Map2D[i, y + 1] == null)
                return false;
        }
        return true;
    }

    private bool canRight(Rectangle rect)
    {
        int x = rect.xmax;
        for (int i = rect.ymin; i <= rect.ymax; i++)
        {
            if (x + 1 >= width || Map2D[x + 1, i] == null)
                return false;
        }
        return true;
    }

    private bool canLeft(Rectangle rect)
    {
        int x= rect.xmin;
        for (int i = rect.ymin; i <= rect.ymax; i++)
        {
            if (x - 1 < 0 || Map2D[x - 1, i] == null)
                return false;
        }
        return true;
    }

    private void increaseRect(Rectangle rect, int xStart, int yStart)
    {
        while (canDownRight(rect))
        {
            rect.xmax++;
            rect.ymax++;
        }
        while (canUpLeft(rect))
        {
            rect.xmin--;
            rect.ymin--;
        }

        while (canUp(rect))
        {
            rect.ymin--;
        }

        while (canDown(rect))
        {
            rect.ymax++;
        }

        while (canRight(rect))
        {
            rect.xmax++;
        }

        while (canLeft(rect))
        {
            rect.xmin--;
        }
    }


}

