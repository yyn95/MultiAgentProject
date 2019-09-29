using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scrips.HELPERS
{
    public class Point
    {
        public Vector3 position;
        public Point(float x, float y)
        {
            this.position = new Vector3(x, 0.1f, y);
        }

        public Point(Vector3 pos)
        {
            this.position = pos;
        }

        public override bool Equals(object obj)
        {
            var item = obj as Point;
            if (item == null)
            {
                return false;
            }
            return position.x == item.position.x && position.z == item.position.z;
        }
        public override int GetHashCode()
        {
            return (position.x.GetHashCode() * 7) ^ (position.y.GetHashCode() * 11);
        }
    }

    public class Node
    {
        public int ID;
        public Vector3 position; //"Real position" value
        public Vector2Int index; //2D array index
        public Node UP, DOWN, LEFT, RIGHT;

        public void setMapXY(int mapX, int mapY)
        {
            index.x = mapX;
            index.y = mapY;
        }

        public Node(float x, float y, int ID, int mapX, int mapY)
        {
            this.position = new Vector3(x,0.1f,y);
            this.ID = ID;
            UP = DOWN = LEFT = RIGHT = null;
            this.index = new Vector2Int(mapX, mapY);
        }

        public Node(float x, float y, int ID)
        {
            this.position = new Vector3(x,0.1f,y);
            this.ID = ID;
            UP = DOWN = LEFT = RIGHT = null;
        }
        

        public override bool Equals(object obj)
        {
            var item = obj as Node;
            if (item == null)
            {
                return false;
            }
            return position.x == item.position.x && position.y == item.position.y && ID == item.ID;
        }
        public override int GetHashCode()
        {
            return (position.x.GetHashCode() * 7) ^ (position.y.GetHashCode() * 3) ^ ID;
        }
    }

}
