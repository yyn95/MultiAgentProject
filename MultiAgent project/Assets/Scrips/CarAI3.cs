using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Assets.Scrips.HELPERS;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI3 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        //public GameObject terrain_manager_game_object;
        public GameObject path_generator_game_object;
        private Rigidbody rigidbody;
        private Collider collider;
        //private TerrainManager terrain_manager;
        
        public GameObject[] friends;
        public GameObject[] enemies;
        
        //path(tree)
        private PathGenerator pathgen;
        private LinkedList<Node> subtree;
        private bool treeComplete;
        private bool ifCollided;
        private Node currNode;
        
        //car move
        private int counter_obstacle;
        private float maxSteerAngle;
        private float footBrake;
        private float steerAngle;
        private float acceleration;
        public float vMax = 10f;
        private Vector3 collision_point;
        
        private void Start()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            maxSteerAngle = m_Car.m_MaximumSteerAngle;
            //terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            rigidbody = GetComponent<Rigidbody>();
            pathgen = path_generator_game_object.GetComponent<PathGenerator>();

            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            if (m_Car.name == "ArmedCar1")
            {
                //draw only called once
                drawInfo();
                subtree = pathgen.robot1.subtree;
                Debug.Log("Car" + pathgen.robot1.ID + "begins!");
            }
            else if (m_Car.name == "ArmedCar2")
            {
                subtree = pathgen.robot2.subtree;
                Debug.Log("Car" + pathgen.robot2.ID + "begins!");
            }
            else if(m_Car.name == "ArmedCar3")
            {
                subtree = pathgen.robot3.subtree;
                Debug.Log("Car" + pathgen.robot3.ID + "begins!");
            }
            subtree.RemoveFirst();   
            
        }


        private void FixedUpdate()
        {
            if (subtree.Count > 0)
            {
                if (!ifCollided)
                {
                    currNode = subtree.First();
                    
                    Debug.DrawLine(transform.position, currNode.position);
                    
                    //set the speed and footbrake
                    SetAcceleration(transform.position, currNode.position);
                    //set the steer angle
                    steerAngle = GetSteerAngle(transform.position, currNode.position);

                    m_Car.Move(steerAngle, acceleration, footBrake, 0);
                    
                    //check if the car has got to the current node
                    //Vector3 direction = (currNode.position - transform.position).normalized;
                    //bool in_front = Vector3.Dot(direction, transform.forward) > 0f;
                    if ((currNode.position - transform.position).magnitude < 10f)
                    {
                        //print("Current Position: " + transform.position + "Current Node: " + currNode.position);
                        subtree.RemoveFirst();
                        //Debug.Log("total number of nodes " + subtree.Count);
                    }
                    
                }
                else
                {
                    //Debug.Log("Collision:" + counter_obstacle);
                    
                    steerAngle = GetSteerAngle(transform.position, currNode.position);
                    m_Car.Move(-steerAngle, 0, -1, 0);
                    
                    //AvoidCollision();
                    //m_Car.Move(steerAngle, acceleration, footBrake, 0);
                  
                    counter_obstacle++;
                    if (counter_obstacle > 60)
                    {             
                        counter_obstacle = 0;
                        ifCollided = false;
                    }
                }
            }
            else
            {
                if (rigidbody.velocity.magnitude > 0.1f)
                    m_Car.Move(0f, 0f, -1f, 0f);
                else
                {
                    print("path of " + m_Car.name + "complete!");
                    enabled = false;
                }
                //print("path of " + m_Car.name + "complete!");
                //enabled = false;
            }
        }
        
        void OnCollisionEnter(Collision collisionInfo)
        {
            if (collisionInfo.collider.GetType() == typeof(BoxCollider))
            {
                ifCollided = true;
                collision_point = collisionInfo.contacts[0].point;
                //print("Collided:" + collision_point);
            }
        }
        
        void SetAcceleration(Vector3 currentCarPosition, Vector3 destinationPosition)
        {
            float distance = (destinationPosition - currentCarPosition).magnitude;
            float vel = rigidbody.velocity.magnitude;
            if (distance > 10f && vel <= vMax)
            {
                acceleration = 1f;
                footBrake = 0;
            }
            else if (distance > 5f && vel < vMax)
            {
                acceleration = 0.5f;
                footBrake = 0f;
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
        
        private void drawInfo()
        {
            //BEGIN DRAWING
            LinkedList<Node> subtree1 = pathgen.robot1.subtree;
            LinkedList<Node> subtree2 = pathgen.robot2.subtree;
            LinkedList<Node> subtree3 = pathgen.robot3.subtree;
            drawTree(subtree1, Color.magenta);
            drawTree(subtree2, Color.red);
            drawTree(subtree3, Color.black);
        }
        
        void drawTree(LinkedList<Node> subtree, Color color)
        {
            Debug.Log("Size of Path: " + subtree.Count);
            GameObject treeDraw = new GameObject();
            LineRenderer lineRenderer = treeDraw.AddComponent<LineRenderer>();
            lineRenderer.material.color = color;
            lineRenderer.widthMultiplier = 1f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = subtree.Count;
            var points = new Vector3[subtree.Count];
  
            int i = 0;
            foreach (Node n in subtree)
            {
                points[i] = n.position;
                i++;
            }

            lineRenderer.SetPositions(points);
        }
                
        
    }
}
