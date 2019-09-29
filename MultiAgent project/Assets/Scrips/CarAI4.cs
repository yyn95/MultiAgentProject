using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;
using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using UnityEngine.Analytics;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI4 : MonoBehaviour
    {
        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;
        public NodeGrid grid;
        public Node[,] Map2D;
        public GameObject[] friends;
        public GameObject[] enemies;
        private Rigidbody[] rigidbody;
        private CarController[] m_Car; // the car controller we want to use
        private int[] leaderIndex;
        private int[] relativeDir;
        private float[] waitTime;
        
        //car move
        private float maxSteerAngle;
        private float footBrake;
        private float steerAngle;
        private float acceleration;

        //replay car
        private GameObject replayCar;
        private Vector3 preRCPos;

        
        //formation parameter
        private float edgeLength;
        //private float start_time;


        private void Start()
        {
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            grid = new NodeGrid(terrain_manager, 1);
            Map2D = grid.grid;
            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
          
            // initialization
            // car name: replay, 3, 1, 4, 2 
            replayCar = friends[0];
            rigidbody = new Rigidbody[friends.Length];
            m_Car = new CarController[friends.Length];
            leaderIndex = new int[]{0, 2, 0, 0, 3};
            relativeDir = new int[]{0, -180, -150, 150, 180}; // -1: left to leader, 1: right to leader
            for (int i = 0; i < friends.Length; i++)
            {  
                rigidbody[i] = friends[i].GetComponent<Rigidbody>();
                if (i == 0)
                {
                    m_Car[i] = null;
                }
                else
                {
                    m_Car[i] = friends[i].GetComponent<CarController>();
                }
            }            
            waitTime = new float[]{0, 1.1f, 0.8f, 0.5f, 2.5f};
            
            maxSteerAngle = m_Car[1].m_MaximumSteerAngle;
            edgeLength = 10f;
            preRCPos = replayCar.transform.position;
            //start_time = Time.time;
            
        }


        private void FixedUpdate()
        {
            // update replay car velocity and previous pos
            Vector3 curRCPos = replayCar.transform.position;
            Vector3 replayVelocity = (curRCPos - preRCPos) / Time.fixedDeltaTime;
            rigidbody[0].velocity = replayVelocity;
            preRCPos = curRCPos;

            //Debug.Log("replay car velocity: " + rigidbody[0].velocity.magnitude);

            for (int i = 1; i < m_Car.Length; i++)
            {
                Transform leaderTrans = friends[leaderIndex[i]].transform;
                Vector3 leaderPos = leaderTrans.position;
                Vector3 followPos = m_Car[i].transform.position;
                Vector3 nextPos = getFormationPos(leaderTrans, relativeDir[i]);
                float followVel = rigidbody[i].velocity.magnitude;
                float leaderVel = rigidbody[leaderIndex[i]].velocity.magnitude;
                
                Debug.DrawLine(leaderPos, nextPos, Color.yellow);

                acceleration = 0f;
                footBrake = 0f;
                
                float distance = (nextPos - followPos).magnitude;
                //float distance2 = (leaderPos - followPos).magnitude;
                steerAngle = GetSteerAngle(followPos, nextPos, m_Car[i].transform.forward);
                
                // set acceleration
                if (followVel < leaderVel&& Time.time > 3f)
                {
                        acceleration = 1f;
                }

                if (followVel >= leaderVel && Time.time > 3f)
                {
                    if (distance < edgeLength * 2)
                    {
                        if (Math.Abs(steerAngle) * Mathf.Rad2Deg < 45)
                        {
                            if (followVel - leaderVel < 1f)
                            {
                                acceleration = 0.3f;
                            }
                            else
                            {
                                footBrake = -0.3f;
                            }
                        }
                        else
                        {
                            footBrake = -1f;
                        }
                    }
                    else
                    {
                        if (followVel - leaderVel < 1f)
                        {
                            acceleration = 1f;
                        }
                        else
                        {
                            acceleration = 0f;
                        }                        
                    }
                }
                
                
                if (Time.time > waitTime[i])
                {
                    m_Car[i].Move(steerAngle, acceleration, footBrake, 0f);
                }

            }

        }


        private Vector3 getFormationPos(Transform leaderTrans, int steerAngle)
        {
            Vector3 leaderPos = leaderTrans.position;
            Vector3 leaderForward = leaderTrans.forward;
            Vector3 leaderRight = leaderTrans.right;
                      
            Vector3 leaderCheckPos = leaderPos + leaderForward * edgeLength ;
            Node leaderNode = grid.getNode(leaderPos.x, leaderPos.z);
            Node leaderCheckNode = grid.getNode(leaderCheckPos.x, leaderCheckPos.z);

            if (leaderTrans.position == replayCar.transform.position)
            {
                Vector3 dir1 = Quaternion.AngleAxis(steerAngle, Vector3.up)* leaderForward;
                Vector3 Pos1 = dir1 * 2.5f * edgeLength + leaderPos;
                Vector3 dir2 = Quaternion.AngleAxis(-steerAngle, Vector3.up)* leaderForward;
                Vector3 Pos2 = dir2 * 2.5f * edgeLength + leaderPos;
                Node leaderCheckNode1 = grid.getNode(Pos1.x, Pos1.z);
                Node leaderCheckNode2 = grid.getNode(Pos2.x, Pos2.z);
                //Vector3 leaderCheckPos2 = leaderPos - leaderForward * edgeLength * 4f;
                //Vector3 leaderCheckPos3 = leaderPos - leaderForward * edgeLength * 3f;
                //Node leaderCheckNode1 = grid.getNode(leaderCheckPos2.x, leaderCheckPos2.z);
                //Node leaderCheckNode2 = grid.getNode(leaderCheckPos3.x, leaderCheckPos3.z);
                if (leaderCheckNode == null || leaderNode == null || leaderCheckNode1 == null || leaderCheckNode2 == null)
                {
                    leaderPos = new Vector3(leaderPos.x - 20, leaderPos.y, leaderPos.z);
                }
            }
            if (leaderCheckNode == null || leaderNode == null)
            {
                leaderPos = new Vector3(leaderPos.x - 20, leaderPos.y, leaderPos.z);
            }
        
            Vector3 dir = Quaternion.AngleAxis(steerAngle, Vector3.up)* leaderForward;
            Vector3 Pos = dir * edgeLength + leaderPos;
            Vector3 resPos = Pos;
//            Node PosNode = grid.getNode(Pos.x, Pos.z);
//            
//            Debug.DrawLine(leaderPos, Pos, Color.blue);
//            
//            // use two checker
//            Vector3 leftChecker = Pos + (leaderForward - 2*leaderRight).normalized * edgeLength * 0.5f;
//            Vector3 rightChecker = Pos + (leaderForward + 2*leaderRight).normalized * edgeLength * 0.5f;
//            Node leftNode = grid.getNode(leftChecker.x, leftChecker.z);
//            Node rightNode = grid.getNode(rightChecker.x, rightChecker.z);
//            
//            Debug.DrawLine(leftChecker, Pos, Color.red);
//            Debug.DrawLine(rightChecker, Pos, Color.red);
//
//            if ((leftNode == null && rightNode == null) || PosNode == null)
//            {
//                Vector3 leftPos = Pos - leaderRight * edgeLength * 1.5f;
//                Vector3 rightPos = Pos + leaderRight * edgeLength * 1.5f;
//                while (grid.ObstacleFound(leftPos.x, leftPos.z) && grid.ObstacleFound(rightPos.x, rightPos.z))
//                {
//                    leftPos = leftPos - leaderRight * edgeLength;
//                    if (!grid.ObstacleFound(leftPos.x, leftPos.z))
//                    {
//                        return leftPos;
//                    }
//                    rightPos = rightPos + leaderRight * edgeLength;
//                    //int i = grid.get_i_index(rightPos.x, false);
//                    //int j = grid.get_j_index(rightPos.z, false);
//                    //Debug.Log(i + ":" + j + ":" + terrain_manager.myInfo.traversability[i,j]);
//                    if (!grid.ObstacleFound(rightPos.x, rightPos.z))
//                    {
//                        return rightPos;
//                    }
//
//                    if (leftPos.x < grid.xlow || rightPos.x < grid.xlow || leftPos.z > grid.zhigh ||
//                        rightPos.z > grid.zhigh)
//                    {
//                        break;
//                    }
//                }
//            }
//            else if (leftNode == null)
//            {
//                resPos = Pos + leaderRight * edgeLength;
//            }
//            else if (rightNode == null)
//            {
//                resPos = Pos - leaderRight * edgeLength;
//            }
// 
            return resPos;
        }
             
        void SetAcceleration(Vector3 goalPos, float time)
        {
            //Vector3 goalPos = path.Peek();
            //Vector3 curleaderPos = leaderpath.Peek();
            Vector3 curPos = transform.position;
            //bool is_to_the_front = Vector3.Dot(transform.forward, goalPos - curPos) > 0f;
            float distance = (curPos - goalPos).magnitude;
            Vector3 velocity = rigidbody[0].velocity;
            //float distance1 = (curPos - centre).magnitude;
            //float distance2 = (curPos - goalPos).magnitude;
            
            //bool is_to_the_front1 = Vector3.Dot(curTrans.forward, curPos - centre) > 0f;
            //bool is_to_the_front2 = Vector3.Dot(curTrans.forward, curleaderPos - curPos) > 0f;
            
            acceleration = 0f;
            footBrake = 0f;

            if (distance > 5f)
            {
                acceleration = 1f;
            }
            else if (velocity.magnitude <= distance)
            {
                acceleration = 0.5f;
            }
            else if (velocity.magnitude > distance)
            {
                footBrake = -1f;
            }

        }
        
        
        float GetSteerAngle(Vector3 currentPos, Vector3 destination, Vector3 forwardDirection)
        {
            Vector3 directionVector = destination - currentPos;
            float turnAngle = Vector3.Angle(directionVector, forwardDirection);
            if (Vector3.Cross(directionVector, forwardDirection).y > 0)
            {
                turnAngle = -turnAngle;
            }
            turnAngle = Mathf.Clamp(turnAngle, (-1) * maxSteerAngle, maxSteerAngle) / maxSteerAngle;

            return turnAngle;
        }
    }
}