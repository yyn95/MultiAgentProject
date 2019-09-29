using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;

public class IOstuff
{

    GameObject terrain_manager_game_object;
    private NodeGrid nodegrid;

    int width, height;

    public void storeText(string filename, TerrainManager manager, NodeGrid nodegrid)
    {
        string path = Application.dataPath + "/" + filename + ".txt";
        width = nodegrid.width;
        height = nodegrid.height;

        string gridinfo = "width: " + width + " height: " + height + "\n";


        gridinfo += getMazeStr(nodegrid.grid);

        File.WriteAllText(path, gridinfo);
    }

    public void storeTextEdge(PathInfo[,] M, int V , string filename)
    {
        string path = Application.dataPath + "/" + filename + ".txt";
        width = V;
        height = V;
        string gridinfo = "width: " + V + " height: " + V + "\n";
        gridinfo += getMazeStr2(M);
        File.WriteAllText(path, gridinfo);
    }

    private string getMazeStr2(PathInfo[,] M)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (M[j, i] != null)
                {
                    sb.Append("|"+ M[i, j].cost.ToString()+"|");
                }
                else
                    sb.Append(M[i,j].cost.ToString());

            }
            sb.Append("\n");
        }
        return sb.ToString();
    }

    private string getMazeStr(Node[,] grid)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[j, i] != null)
                {
                    sb.Append(' ');
                }
                else
                    sb.Append('M');

            }
            sb.Append("\n");
        }
        return sb.ToString();
    }
}

