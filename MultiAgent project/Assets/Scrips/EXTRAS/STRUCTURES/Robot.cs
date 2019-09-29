
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scrips.HELPERS
{
    public class Robot
    {
        public int ID; //1,2,3
        public Vector3 startPos;
        //public LinkedList<Point> path;
        public LinkedList<Node> subtree;

        //public Dictionary<Point, Node> nodemap;
        //public Dictionary<Point, bool> used;

        public bool noMoreMoves;

        public Robot(Vector3 startposition, int ID)
        {
            this.startPos = startposition;
            this.ID = ID;
            subtree = new LinkedList<Node>();
            //path = new LinkedList<Point>();
            noMoreMoves = false;
        }

    }
}
