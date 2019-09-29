using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;


//*NB! This class must be added as a component on Unity for Awake() to be called.*//
public class PathGenerator : MonoBehaviour
{
    public GameObject terrain_manager_game_object;
    private TerrainManager terrain_manager;
    public NodeGrid grid;

    //for problem 3 to get the turrets positions
    public GameObject game_manager_object;
    private GameManager game_manager;

    private Vector3[] start_pos;

    public Robot robot1, robot2, robot3;
    public List<Robot> robots;

    public int ProblemIndex = 1;

    //for problem 1
    public TreeGenerator treeGenerator;
    private Node[,] Map2D;

    //for problem 3
    public EdgeGenerator edgegenerator;
    public GAGenerator gaGenerator;

    //for problem 5
    //private static bool runOnce = false;
    public List<Targ> targets;
    public List<LinkedList<Node>> listpaths1, listpaths2, listpaths3;
    public CostGrid costGrid;
    private static bool runOnce = false;

    private void Start()
    {
        terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
        game_manager = game_manager_object.GetComponent<GameManager>();

        Initialize();

        //must be called after initialize done
        switch (ProblemIndex)
        {
            case 1:
                Problem1();
                break;
            case 2:
                Problem2();
                break;
            case 3:
                Problem3();
                break;
            case 5:
                Problem5();
                break;
            default:
                Console.WriteLine("there is no such problem!");
                break;
        }

    }

    //initialize must be called before any problem func start
    public void Initialize()
    {  
        
        //initialize the map based on problems
        if (ProblemIndex == 5)
        {
            grid = new NodeGrid(terrain_manager, 5);
        }
        else
        {
            grid = new NodeGrid(terrain_manager, 2);
        }

        //initialize robots
        robots = new List<Robot>();
        start_pos = new Vector3[3]
        {
            new Vector3(220, 0.1f, 230),
            new Vector3(209.5f, 0.1f, 230), new Vector3(201.3f, 0.1f, 230)
        };

        for (int i = 0; i < 3; i++)
        {
            Vector3 startPosition = start_pos[i];
            Node start_node = grid.getNode(startPosition.x, startPosition.z);
            start_node.ID = -1;
            Robot robot = new Robot(startPosition, i + 1);
            robot.subtree.AddFirst(start_node);
            //robot.path.AddFirst(new Point(startPosition));
            robots.Add(robot);
        }

        robot1 = robots[0];
        robot2 = robots[1];
        robot3 = robots[2];

        //if (startnode == null)
        //{
        //    startnode = new Node(startX, startY, -1, grid.get_i_index(startX,true),grid.get_j_index(startY,true));
        //}
    }

    private void Problem1()
    {
        Map2D = grid.grid;
        treeGenerator = new TreeGenerator(Map2D, grid, robots);
        treeGenerator.GenerateTree();
    }

    private void Problem2()
    {
        RoomGenerator roomGenerator = new RoomGenerator(grid);
        roomGenerator.init();
        Debug.Log("Amount of rooms: " + roomGenerator.rooms.Count);
        List<Vector3> targets_list = roomGenerator.getPoints();
        List<Vector3> all_list = new List<Vector3>();
        all_list.AddRange(start_pos);
        all_list.AddRange(targets_list);

        edgegenerator = new EdgeGenerator(all_list, grid);
        gaGenerator = new GAGenerator(targets_list, robots, grid, edgegenerator);
        gaGenerator.GeneratePath();

    }

    private void Problem3()
    {
        List<GameObject> turretObj_list = game_manager.turret_list;
        List<Vector3> turret_list = new List<Vector3>();
        foreach (GameObject turret in turretObj_list)
        {
            turret_list.Add(turret.transform.position);
        }

        List<Vector3> all_list = new List<Vector3>();
        all_list.AddRange(start_pos);
        all_list.AddRange(turret_list);

        //build path
        edgegenerator = new EdgeGenerator(all_list, grid);

        //build GA
        gaGenerator = new GAGenerator(turret_list, robots, grid, edgegenerator);
        gaGenerator.GeneratePath();

    }

    private void Problem5()
    {
        listpaths1 = new List<LinkedList<Node>>();
        listpaths2 = new List<LinkedList<Node>>();
        listpaths3 = new List<LinkedList<Node>>();
        Map2D = grid.grid;
        costGrid = new CostGrid(game_manager, grid);
        int startX;
        int startY;
        robot1.startPos = terrain_manager.myInfo.start_pos;

        startX = grid.get_i_index(robot1.startPos.x, true);
        startY = grid.get_j_index(robot1.startPos.z, true);

        costGrid.init(startX, startY);
        //IOstuff iostuff = new IOstuff();
        //iostuff.storeTextCosts("testOkokok", terrain_manager, costGrid);

        targets = costGrid.targets;

        grid.setCostGrid(costGrid.costMaps[0]);
        AStar astar = new AStar(grid);
        int Sx1 = startX;
        int Sy1 = startY;
        int Sx2 = startX;
        int Sy2 = startY;
        int Sx3 = startX;
        int Sy3 = startY;
        int counter = 0;
        foreach (Targ t in targets)
        {
            counter++;
            int targX = t.target1.mapX;
            int targY = t.target1.mapY;
            astar.init(Sx1, Sy1, targX, targY, -1);
            astar.findPath();
            List<ANode> ares = astar.result;
            LinkedList<Node> realResult = new LinkedList<Node>();
            foreach (ANode an in ares)
            {
                Node tmpNode = grid.grid[an.x, an.y];
                realResult.AddFirst(tmpNode);
            }

            listpaths1.Add(realResult);
            Sx1 = targX;
            Sy1 = targY;

            //////////////////
            targX = t.target2.mapX;
            targY = t.target2.mapY;
            astar.init(Sx2, Sy2, targX, targY, -1);
            astar.findPath();
            ares = astar.result;
            realResult = new LinkedList<Node>();
            foreach (ANode an in ares)
            {
                Node tmpNode = null;
                try
                {
                    tmpNode = grid.grid[an.x, an.y];
                }
                catch (Exception e)
                {
                    Debug.Log("X: " + an.x + " y " + an.y);
                    Debug.Log("X: " + Sx2 + " y " + Sy2 + " _ " + targX + "," + targY + " counting: " + counter);
                }

                realResult.AddFirst(tmpNode);
            }

            listpaths2.Add(realResult);
            Sx2 = targX;
            Sy2 = targY;


            //////////////////////

            targX = t.target3.mapX;
            targY = t.target3.mapY;
            astar.init(Sx3, Sy3, targX, targY, -1);
            astar.findPath();
            ares = astar.result;
            realResult = new LinkedList<Node>();
            foreach (ANode an in ares)
            {
                if (an.x == -1)
                {
                    Debug.Log("WAT: " + Sx3 + " " + Sy3 + " tx " + targX + " ty " + targY);
                    continue;
                }

                Node tmpNode = grid.grid[an.x, an.y];
                realResult.AddFirst(tmpNode);
            }

            listpaths3.Add(realResult);
            Sx3 = targX;
            Sy3 = targY;

        }
    }

}