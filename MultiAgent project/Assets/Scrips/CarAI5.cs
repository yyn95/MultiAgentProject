using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scrips.HELPERS;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI5 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use
        public GameObject path_generator_game_object;
        private PathGenerator pathgen;
        private Rigidbody rigidbody;

        public GameObject[] friends;
        public GameObject[] enemies;
        
        
        //car move
        private float maxSteerAngle;
        private float footBrake;
        private float steerAngle;
        private float acceleration;
        private float vMax = 7f;

        //formation
        private Vector3 prePos;
        private Vector3 CurPos;
        private Quaternion preRotation;

        //path
        private Node currNode;
        private bool ifCollided;
        private bool cantCollide;
        private int counter_obstacle = 0;
        private int crashCounter = 0;
        private Vector3 lastPosInPath;

        //state
        private static int car1 = 1, car2 = 2, car3 = 3;
        static int ATTACK = 1, SAFETY = 2, MOVING = 3, WAIT = 4;
        int state;
        int targetIndex;
        private float start_time;
        float totalTime, checkTime;
        bool timerStarted = false;
       
        public static bool ready1 = false;
        public static bool ready2 = false;
        public static bool ready3 = false;

        List<LinkedList<Node>> allPaths1, allPaths2, allPaths3;
        LinkedList<Node> currentPath1, currentPath2, currentPath3;


        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            pathgen = path_generator_game_object.GetComponent<PathGenerator>();
            rigidbody = GetComponent<Rigidbody>();
            maxSteerAngle = m_Car.m_MaximumSteerAngle;
            
            //car name: 1, 3, 2 
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
                        
            //initialize state and path
            state = MOVING;
            targetIndex = 0;
            cantCollide = false;
            start_time = Time.time;
            
            allPaths1 = new List<LinkedList<Node>>();
            foreach (LinkedList<Node> lln in pathgen.listpaths1)
            {
                allPaths1.Add(new LinkedList<Node>(lln));
            }
            allPaths2 = new List<LinkedList<Node>>();
            foreach (LinkedList<Node> lln in pathgen.listpaths2)
            {
                allPaths2.Add(new LinkedList<Node>(lln));
            }
            allPaths3 = new List<LinkedList<Node>>();
            foreach (LinkedList<Node> lln in pathgen.listpaths3)
            {
                allPaths3.Add(new LinkedList<Node>(lln));
            }
            currentPath1 = new LinkedList<Node>(allPaths1[0]);
            currentPath2 = new LinkedList<Node>(allPaths2[0]);
            currentPath3 = new LinkedList<Node>(allPaths3[0]);
            //Debug.Log("SIZE: " + allPaths.Count);
            //allPaths = new List<LinkedList<Node>>(allPaths);
        }

        void repath(ref LinkedList<Node> nowPath, Node dest)
        {
            pathgen.grid.setCostGrid(pathgen.costGrid.costMaps[targetIndex]);
            AStar astar = new AStar(pathgen.grid);
            int tmpX = pathgen.grid.get_i_index(transform.position.x, true);
            int tmpY = pathgen.grid.get_j_index(transform.position.z, true);
            Node n = pathgen.grid.getClosestNode((int)transform.position.x, (int)transform.position.z);
            tmpX = n.index.x;
            tmpY = n.index.y;
            int tmpEndX = pathgen.grid.get_i_index(lastPosInPath.x, true);
            int tmpEndY = pathgen.grid.get_j_index(lastPosInPath.z, true);
            astar.init(tmpX, tmpY, tmpEndX, tmpEndY, -1);
            astar.findPath();
            List<ANode> ares = astar.result;
            LinkedList<Node> realResult = new LinkedList<Node>();
            foreach (ANode an in ares)
            {
                Node tmpNode = pathgen.grid.grid[an.x, an.y];
                realResult.AddFirst(tmpNode);
            }
            nowPath = realResult;
        }

        void makePath(ref LinkedList<Node> nowPath, int carID)
        {
            LinkedList<Node> oldPath = null;
            if(carID == car1)
                oldPath = new LinkedList<Node>(allPaths1[targetIndex]);
            if (carID == car2)
                oldPath = new LinkedList<Node>(allPaths2[targetIndex]);
            if (carID == car3)
                oldPath = new LinkedList<Node>(allPaths3[targetIndex]);

            lastPosInPath = oldPath.Last.Value.position;
            pathgen.grid.setCostGrid(pathgen.costGrid.costMaps[targetIndex]);
            AStar astar = new AStar(pathgen.grid);
            int tmpX = pathgen.grid.get_i_index(transform.position.x, true);
            int tmpY = pathgen.grid.get_j_index(transform.position.z, true);

            Node n = pathgen.grid.getClosestNode((int)transform.position.x, (int)transform.position.z);
            tmpX = n.index.x;
            tmpY = n.index.y;

            int tmpEndX = pathgen.grid.get_i_index(lastPosInPath.x, true);
            int tmpEndY = pathgen.grid.get_j_index(lastPosInPath.z, true);
            
            astar.init(tmpX, tmpY, tmpEndX, tmpEndY, -1);
            astar.findPath();
            List<ANode> ares = astar.result;
            LinkedList<Node> realResult = new LinkedList<Node>();
            foreach (ANode an in ares)
            {
                Node tmpNode = pathgen.grid.grid[an.x, an.y];
                realResult.AddFirst(tmpNode);
            }
            nowPath = realResult;
        }

        void OnCollisionEnter(Collision collisionInfo)
        {
            if (collisionInfo.collider.GetType() == typeof(BoxCollider))
            {
                if (!cantCollide)
                {
                    crashCounter++;
                    ifCollided = true;
                }
                    
            }
        }


        void avoidObstacles(Vector3 targetPos)
        {
            steerAngle = GetSteerAngle(transform.position, targetPos);
            m_Car.Move(-steerAngle, 0, -1, 0);

            counter_obstacle++;
            if (counter_obstacle > 70)
            {
                counter_obstacle = 0;
                ifCollided = false;
            }
        }


        void moveNormally(Vector3 targetPos, ref LinkedList<Node> nowPath)
        {
            if (lastPosInPath == targetPos)
            {
                breakCar();
                nowPath.RemoveFirst();
                return;
            }
            lastPosInPath = nowPath.Last.Value.position;
            Debug.DrawLine(transform.position, targetPos);
            SetAccCruise(transform.position, targetPos);
            steerAngle = GetSteerAngle(transform.position, targetPos);
            m_Car.Move(steerAngle, acceleration, footBrake, 0);
            //bool in_front = Vector3.Dot(direction, transform.forward) > 0f;
            if ((targetPos - transform.position).magnitude < 7f)
            {
                nowPath.RemoveFirst();
            }else if (countReady() == 2 && (targetPos - transform.position).magnitude < 15f && crashCounter > 5)
            {
                nowPath.RemoveFirst();
            }
        }


        void startTimer()
        {
            if (!timerStarted)
            {
                timerStarted = true;
                start_time = totalTime;
            }
        }

        bool hasTimeElapsed(float seconds)
        {
            // Debug.Log("INFO " + (totalTime - start_time - seconds));
            if (timerStarted)
            {
                return totalTime - start_time - seconds > 0f;
            }
            return false;
        }
        
        /*void stopTimer()
        {
            timerStarted = false;
        }*/


        void changeState(ref LinkedList<Node> nowPath, int carID)
        {
            if (hasTimeElapsed(2.5f) && state == MOVING)
            {
                timerStarted = false;
                state = WAIT;
                if (carID == car1)
                    ready1 = true;
                if (carID == car2)
                    ready2 = true;
                if (carID == car3)
                    ready3 = true;

            }

            if(state == WAIT)
            {
                if (ready1 && ready2 && ready3)
                    state = ATTACK;
            }

            if (hasTimeElapsed(7f) && state == ATTACK)
            {
                ready1 = ready2 = ready3 = false;
                timerStarted = false;
                state = SAFETY;
            }

            if (hasTimeElapsed(3f) && state == SAFETY)
            {
                targetIndex = (targetIndex + 1) % 8;
                timerStarted = false;
                state = MOVING;
                crashCounter = 0;
                makePath(ref nowPath, carID);
            }
        }


        void attack()
        {
            if (state != ATTACK)
                return;
            lastPosInPath.y = 0.1f;

            SetAccAttack(transform.position, lastPosInPath);
            steerAngle = GetSteerAngle(transform.position, lastPosInPath);
            m_Car.Move(steerAngle, acceleration, footBrake, 0);
            startTimer();

        }

        private void moveCar(ref LinkedList<Node> nowPath)
        {

            //Debug.Log("Time test: " + totalTime);
            //Debug.Log("Counter: " + allPaths[0].Count);
            //Debug.Log("Steer: " + steerAngle + " acc: " + acceleration + " brake " + footBrake);
            if (state == MOVING)
            {
                if (nowPath.Count > 0)
                {
                    cantCollide = false;
                    currNode = nowPath.First.Value;
                    Vector3 position = new Vector3(currNode.position.x, 0.1f, currNode.position.z);

                    if (!ifCollided)
                    {
                        moveNormally(position, ref nowPath);
                    }
                    else
                    {
                        avoidObstacles(position);
                        if(ifCollided == false && countReady() == 2)
                        {
                            repath(ref nowPath, nowPath.Last.Value);
                        }

                    }
                }
                else//Target reached
                {
                    startTimer();
                    cantCollide = true;
                    breakCar();
                    m_Car.Move(steerAngle, acceleration, footBrake, 0);
                }

            }
            if (state == SAFETY)
            {
                startTimer();
                cantCollide = true;
                m_Car.Move(0f, 0.7f, -0.5f, 0);

            }

        }

        private void breakCar()
        {
            float vel = rigidbody.velocity.magnitude;
            float diffVel = (rigidbody.velocity.normalized - rigidbody.transform.forward).magnitude;
            if (vel > 0.2f && diffVel < 0.8)
            {
                acceleration = 1f;
                footBrake = -1f;
            }
            else
            {
                acceleration = 0f;
                footBrake = 0f;
            }
        }

        private void FixedUpdate()
        {
            totalTime += Time.deltaTime;
            if (friends[0] == null)
                ready1 = true;
            if (friends[1] == null)
                ready2 = true;
            if (friends[2] == null)
                ready3 = true;

            if (m_Car.name == "ArmedCar1")
            {
            
                changeState(ref currentPath1, car1);
                moveCar(ref currentPath1);
                attack();
            }
            if (m_Car.name == "ArmedCar2")
            {
                changeState(ref currentPath3, car3);
                moveCar(ref currentPath3);
                attack();
            }
            if (m_Car.name == "ArmedCar3")
            {
                changeState(ref currentPath2, car2);
                moveCar(ref currentPath2);
                attack();
            }

            // m_Car.Move(steerAngle, acceleration, footBrake, 0);

        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            Gizmos.color = Color.yellow;
            //Gizmos.DrawSphere(tmpvec, 7);
            // Debug.Log("tmpvec: " + tmpvec);

            Gizmos.color = Color.red;
            int counting = 0;
            foreach (Targ t in pathgen.targets)
            {
                counting++;
                if (counting == 2)
                    Gizmos.color = Color.magenta;
                if (counting == 3)
                    Gizmos.color = Color.blue;
                if (counting == 4)
                    Gizmos.color = Color.white;
                if (counting == 5)
                    Gizmos.color = Color.yellow;
                if (counting == 6)
                    Gizmos.color = Color.cyan;
                if (counting == 7)
                    Gizmos.color = Color.black;
                if (counting == 8)
                    Gizmos.color = Color.green;

                Vector3 pos = new Vector3(t.target1.x, 2f, t.target1.y);
                Gizmos.DrawSphere(pos, 3);

                pos = new Vector3(t.target2.x, 2f, t.target2.y);
                Gizmos.DrawSphere(pos, 3);

                pos = new Vector3(t.target3.x, 2f, t.target3.y);
                Gizmos.DrawSphere(pos, 3);
            }

            Gizmos.color = Color.red;
            /*  List<LinkedList<Node>> listpaths = pathgen.listpaths;
              foreach (LinkedList<Node> l in listpaths)
              {
                  foreach (Node an in l)
                  {
                      Vector3 tmpPos = an.position;
                      Gizmos.DrawCube(tmpPos, new Vector3(1, 1, 1));
                  }

              }*/
            Gizmos.color = Color.cyan;
            foreach (Node an in currentPath1)
            {
                Vector3 tmpPos = an.position;
                Gizmos.DrawCube(tmpPos, new Vector3(1, 1, 1));
            }

            Gizmos.color = Color.white;
            foreach (Node an in currentPath2)
            {
                Vector3 tmpPos = an.position;
                Gizmos.DrawCube(tmpPos, new Vector3(1, 1, 1));
            }

            Gizmos.color = Color.black;
            foreach (Node an in currentPath3)
            {
                Vector3 tmpPos = an.position;
                Gizmos.DrawCube(tmpPos, new Vector3(1, 1, 1));
            }

            //   Gizmos.DrawSphere(new Vector3(target.x, 3f, target.y), 8);
            //Debug.Log("TESTAA: " + target.x + " _ " + target.y);

        }

        int countReady()
        {
            int counter = 0;
            if (ready1)
                counter++;
            if (ready2)
                counter++;
            if (ready3)
                counter++;
            return counter;
        }

        void SetAccAttack(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            float vel = rigidbody.velocity.magnitude;

            Vector3 direction = (destinationPosition - transform.position).normalized;
            bool in_front = Vector3.Dot(direction, transform.forward) > 0f;
           // Debug.Log("VEL: " + vel);
            if (!in_front)
            {
                breakCar();
                steerAngle = 0f;
                return;
            }

            if (in_front && distance >= 10f && vel < vMax)
            {
                acceleration = 1f;
                footBrake = 0;
            }
            else if (in_front && distance < 10 && vel < 5)
            {
                acceleration = 0.3f;
                footBrake = 0f;
            }
            else
            {
                breakCar();
                steerAngle = 0f;
            }
        }

        void SetAccCruise(Vector3 currentCarPosition, Vector3 destinationPosition)//slow but safe
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            float vel = rigidbody.velocity.magnitude;
            if (distance > 50 && vel < vMax)
            {
                acceleration = 1f;
                footBrake = 0;
            }
            else
            if (distance <= 50f && vel <= vMax)
            {
                acceleration = 0.5f;
                footBrake = 0;
            }
            else
            {
                acceleration = 0f;
                footBrake = 0f;
            }
        }

        float GetSteerAngle(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            Vector3 directionVector = destinationPosition - currentCarPosition;
            float turnAngle = Vector3.Angle(directionVector, transform.forward);
            if (Vector3.Cross(directionVector, transform.forward).y > 0)
            {
                turnAngle = -turnAngle;
            }
            turnAngle = Mathf.Clamp(turnAngle, (-1) * maxSteerAngle, maxSteerAngle) / maxSteerAngle;
            return turnAngle;
        }


    }
}