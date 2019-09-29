using Assets.Scrips.EXTRAS.STRUCTURES;
using Assets.Scrips.HELPERS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class chromosomes: IComparable<chromosomes>
{
    public int[] sequence;
    public List<LinkedList<int>> trips;
    public double fitness; //total cost of all trips
    
    public chromosomes(int[] seq)
    {
        this.sequence = seq;
        //seq.CopyTo(sequence,0);
    }

    private int[] CalBestSplit()
    {
        double[,] distances = GAGenerator.distances;
        int lowbound = 1, upbound = sequence.Length - 2; 
        int splitPos1 = 1, splitPos2 = sequence.Length - 2;

        double trip2_cost = distances[1, sequence[1]] + distances[1, sequence[upbound]];
        for (int i = 1; i < sequence.Length - 2; i++)
        {
            trip2_cost += distances[i, i + 1];
        }
        
        //at the begining, the second path must be the longest
        double[] best_cost = {distances[0, sequence[0]] * 2, trip2_cost, distances[2, sequence[sequence.Length - 1]] * 2};

        bool split1_found = false, split2_found = false;
        
        while (!split1_found && !split2_found)
        {
            int next1 = splitPos1 + 1;
            if (next1 < upbound && !split1_found)
            {
                double curr_cost1 = best_cost[0] + distances[splitPos1, next1] + 
                                      distances[next1, 0] - distances[0,splitPos1];
                double curr_cost2 = best_cost[1] + distances[1, next1] - distances[1, splitPos1] - distances[splitPos1, next1];
                if (curr_cost2 - curr_cost1 > 0)
                {
                    best_cost[0] = curr_cost1;
                    best_cost[1] = curr_cost2;
                    splitPos1++;
                }
                else
                {
                    split1_found = true;
                }
            }

            int next2 = splitPos2 - 1;
            if (next2 > lowbound && !split2_found)
            {
                double curr_cost2 = best_cost[1] + distances[next2, 1] - distances[next2, splitPos2] - 
                                      distances[splitPos2, 1];
                double curr_cost3 = best_cost[2] + distances[splitPos2, next2] + distances[next2, 2] -
                                      distances[splitPos2, 2];
                if (curr_cost2 - curr_cost3 > 0)
                {
                    best_cost[1] = curr_cost2;
                    best_cost[2] = curr_cost3;
                    splitPos2--;
                }
                else
                {
                    split2_found = true;
                }
            }
        }

        //get the fitness
        this.fitness = best_cost.Sum();
        
        return new int[2]{splitPos1, splitPos2};
        
    }

    public void getTrips()
    {
        trips = new List<LinkedList<int>>();
        int[] splitPos = CalBestSplit();

        LinkedList<int> trip1 = new LinkedList<int>();        
        for (int i = 0; i < splitPos[0]; i++)
        {
            trip1.AddLast(sequence[i]);
        }
        LinkedList<int> trip2 = new LinkedList<int>();
        for (int i = splitPos[0]; i < splitPos[1] + 1; i++)
        {
            trip2.AddLast(sequence[i]);
        }
        LinkedList<int> trip3 = new LinkedList<int>();
        for (int i = splitPos[1] + 1; i < sequence.Length; i++)
        {
            trip3.AddLast(sequence[i]);
        }
        trips.Add(trip1);
        trips.Add(trip2);
        trips.Add(trip3);
    }


    public int CompareTo(chromosomes other)
    {
        // Compares fitness
        if (other == null)
        {
            return 1;
        }
        else
        {
            return this.fitness.CompareTo(other.fitness);
        }
    }
      
}


public class GAGenerator
{
    
    private List<Robot> robots;
    private Node[,] Map2D;
    private PathInfo[,] pathInfoMatr;
    
    //for GAGenerator start
    private List<Vector3> turretPositions;
    private int turretNum;
    
    //indexes rules for all nodes: 0,1,2 for robots, turret index from 3 to turretNum + 2
    public static double[,] distances;
    private static Random rd = new Random(); //for get all random value
    
    //for population initialization
    private List<chromosomes> population;  
    private HashSet<int> spacedVal; //only when spacing value not in hashset, the chromosome can be added into population
    private double delta = 4; //make the population well spaced
    private int population_size = 30; //size for the population
    private int trysize = 50; //try times to find a spaced chromosome

    
    //for crossover
    private int[] N_array; //from 0 to turretNum - 1
    
    //for mutation
    private List<int> allIndex; //from 0 to turretNum + 2  
    private List<int[]> pairs; //combine all distinct two nodes u,v in pairs (u,v can be robot nodes)
    private int[] methodIndex; //from 1 to 9
    private double mutationProbability = 0.05;
    
    //for main GA 
    private int maxIteration = 100;
    private int max_alpha = 50; //maximum number of crossovers not yield a clone
    private int max_beta = 20; //maximal number of iterations without improving the best solution
    //private int low_bound = 1000;//rough estimation to stop GA iterations
       
    /*
     * for GAGenerator start
     */
   
    public GAGenerator(List<Vector3> turret_list, List<Robot> robots, NodeGrid grid, EdgeGenerator eg)
    {    
        //initialize       
        this.robots = robots;
        this.Map2D = grid.grid;
        this.pathInfoMatr = eg.M;
        this.turretPositions = turret_list;
        turretNum = turretPositions.Count;
        Debug.Log("turret number:" + turretNum);
        
        N_array = Enumerable.Range(0, turretNum).ToArray();
        allIndex = Enumerable.Range(0, turretNum + 3).ToList();
        methodIndex = Enumerable.Range(1, 9).ToArray();
        pairs = GetCombination(allIndex);
        
        //get the distances matrix
        CalDistances();
        
    }
    
    public void GeneratePath()
    {
        //run GA
        InitPopulation();
        mainGA();
        
        //set the path for each robot in pathGenerator
        getPath();
    }

    private void CalDistances()
    {
        distances = new double[allIndex.Count,allIndex.Count];
        for (int i = 0; i < allIndex.Count; i++)
        {
            distances[i, i] = 0;
            for (int j = i + 1; j < allIndex.Count; j++)
            {
                //distances[j, i] = distances[i, j] = EucDistance(all_nodes[i], all_nodes[j]);
                distances[j, i] = distances[i, j] = pathInfoMatr[j, i].cost;
            }
        }
    }

    private double EucDistance(Vector3 pos1, Vector3 pos2)
    {
        double dist = Math.Sqrt(Math.Pow(pos1.x - pos2.x,2) + Math.Pow(pos1.z - pos2.z,2));
        return dist;
    }

    private void getPath()
    {
        chromosomes bestSolution = population[0];
        for (int i = 0; i < 3; i++)
        {
            var currNode = bestSolution.trips[i].First;
            var currPath = pathInfoMatr[i, currNode.Value].path;
            for (int j = 0; j < currPath.Count; j++)
            {
                robots[i].subtree.AddLast(Map2D[currPath[j].x, currPath[j].y]);
            }
            
            while (currNode.Next != null)
            {
                currPath = pathInfoMatr[currNode.Value, currNode.Next.Value].path;
                for (int k = 1; k < currPath.Count; k++)
                {
                    robots[i].subtree.AddLast(Map2D[currPath[k].x, currPath[k].y]);
                }
                currNode = currNode.Next;
            }
        }
    }
     
    
    /*
     * Initialize population for GA
     */

    private void InitPopulation()
    {
        population = new List<chromosomes>();
        spacedVal = new HashSet<int>();
        
        chromosomes init1 = Gillett_Miller();
        
        int spacing = (int)(init1.fitness / delta);
        population.Add(init1);
        spacedVal.Add(spacing);

        for (int i = 0; i < population_size - 1; i++)
        {
            for (int j = 0; j < trysize; j++)
            {
                //for randomly get a new chromosome, from 3 to turretNum + 2
                int[] turretIndex = Enumerable.Range(3, turretNum).ToArray();
                Shuffle(turretIndex);
                chromosomes newChromo = new chromosomes(turretIndex);
                newChromo.getTrips();
                
                spacing = (int)(newChromo.fitness / delta);
                if (!spacedVal.Contains(spacing))
                {
                    population.Add(newChromo);
                    spacedVal.Add(spacing);
                    break;
                }
                //make sure the size of population
                if (j == 49)
                {
                    population.Add(newChromo);
                }
            }
        }
        population.Sort();
        Debug.Log("population size:" + population.Count);
        Debug.Log("min init fitness:" + population[0].fitness);
        Debug.Log("population initialization completed!");
    }
    
    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rd.Next(n + 1);  
            T value = list[k];
            list[k] = list[n];  
            list[n] = value;  
        }
    }
    
   
    
    /*
     * Initialize chromosome: Gillett Miller method
     */
    
    private chromosomes Gillett_Miller()
    {
        //store angles between robot1 and each client
        Vector3 depot = robots[0].startPos;
        Dictionary<int, double> angles = new Dictionary<int, double>();

        int first_node = rd.Next(0, turretNum);
        int[] otherNodes = allIndex.Take(first_node).Concat(allIndex.Skip(first_node + 1).Take(turretNum - first_node - 1)).ToArray();
        angles.Add(first_node, 0);
        foreach (int node in otherNodes)
        {
            double angle = vector_angle(turretPositions[first_node] - depot, turretPositions[node] - depot);
            angles.Add(node, angle);
        }
        //sort angles ascending 
        int[] newSeq = (from entry in angles orderby entry.Value ascending select entry.Key + 3).ToArray();
        chromosomes result = new chromosomes(newSeq);
        result.getTrips();
        return result;
    }
    
    
    /*
     * for main GA process
     */  
    
    public void mainGA()
    {
        int alpha = 0, beta = 0;
        for (int i = 0; i < maxIteration; i++)
        {
            List<chromosomes> parents = new List<chromosomes>();
            
            //binary tournament method
            for (int j = 0; j < 2; j++)
            {
                chromosomes parent0 = population[rd.Next(0, population_size)];
                chromosomes parent1 = population[rd.Next(0, population_size)];
                if (parent0.fitness < parent1.fitness)
                {
                    parents.Add(parent0);
                }
                else
                {
                    parents.Add(parent1);
                }
            }
            while (parents[0] == parents[1])
            {
                parents[1] = population[rd.Next(0, population_size)];
            }
            
            //cross over
            chromosomes child = crossover(parents[0], parents[1]);
                      
            int k = rd.Next((int) (population_size / 2), population_size - 1);
            int kspacing = (int)(population[k].fitness / delta);
            
            if (rd.NextDouble() < mutationProbability)
            {
                chromosomes mutateChild = mutate(child);
                int mut_spacing = (int)(mutateChild.fitness / delta);               
                if (!spacedVal.Contains(mut_spacing) || mut_spacing == kspacing)
                {
                    child = mutateChild;
                }
            }
            
            int child_spacing = (int)(child.fitness / delta);
            if (!spacedVal.Contains(child_spacing) || child_spacing == kspacing)
            {
                alpha = alpha + 1;
                if (child.fitness < population[0].fitness)
                {
                    beta = 0;
                    Debug.Log("best cost update:" + population[0].fitness);
                }
                else
                {
                    beta = beta + 1;
                }

                population[k] = child;
                spacedVal.Remove(kspacing);
                spacedVal.Add(child_spacing);
                population.Sort();
                
            }          
            //if (alpha == max_alpha || beta == max_beta || population[0].fitness <= low_bound)
            if (alpha == max_alpha || beta == max_beta)
            {
                break;
            }

        }
    }
    
    private double vector_angle(Vector3 vec1, Vector3 vec2)
    {
        double cos_sim = (vec1.x * vec2.x + vec1.z * vec2.z) / (EucDistance(vec1, Vector3.zero) * EucDistance(vec2, Vector3.zero));
        return Math.Acos(cos_sim);
    }


    /*
     * Crossover: randomly select two cross points, and randomly choose one child
     */
    
    private chromosomes crossover(chromosomes chromosomes1, chromosomes chromosomes2)
    {
        //Debug.Log("crossover begin!");
        int[] crossPos = new int[2];
        //Random rd = new Random();
        crossPos[0] = rd.Next(0, turretNum);
        crossPos[1] = rd.Next(0, turretNum);
        //make sure get two different positions
        while (crossPos[0] == crossPos[1])
        {
            crossPos[1] = rd.Next(0, turretNum);
        }
        Array.Sort(crossPos);
        
        int segment_count = crossPos[1] - crossPos[0] + 1; //the size of reserved middle segment
        int[] otherPos = N_array.Skip(crossPos[1] + 1).Take(turretNum - crossPos[1] - 1)
            .Concat(N_array.Take(crossPos[0])).ToArray(); //indexes of positions after and before the middle segment  
        
        int[] childSeq = new int[turretNum]; //new child seuqence
        
        int choose_child = rd.Next(1, 3); //choose one child from the crossover result
        
        if (choose_child == 1)
        {
            // use dict to fast check if one term in the middle segment
            Dictionary<int, bool> middle_segment = chromosomes1.sequence.Skip(crossPos[0]).Take(segment_count).ToDictionary(term => term, term => true);
            Array.Copy(chromosomes1.sequence, crossPos[0], childSeq, crossPos[0], segment_count);
 
            //items after and before the last item of middle segment in another chromosome
            int[] otherSeq = chromosomes2.sequence.Skip(crossPos[1] + 1).Take(turretNum - crossPos[1] - 1)
                .Concat(chromosomes2.sequence.Take(crossPos[1] + 1)).ToArray();

            int j = 0;
            foreach (int pos in otherPos)
            {
                while (middle_segment.ContainsKey(otherSeq[j]))
                {
                    j++;
                }
                childSeq[pos] = otherSeq[j];
                j++;
            }
   
        }
        else
        {
            Dictionary<int, bool> middle_segment = chromosomes2.sequence.Skip(crossPos[0]).Take(segment_count).ToDictionary(term => term, term => true);
            Array.Copy(chromosomes2.sequence, crossPos[0], childSeq, crossPos[0], segment_count);
            int[] otherSeq = chromosomes1.sequence.Skip(crossPos[1] + 1).Take(turretNum - crossPos[1] - 1)
                .Concat(chromosomes1.sequence.Take(crossPos[1] + 1)).ToArray();

            int j = 0;
            foreach (int pos in otherPos)
            {
                while (middle_segment.ContainsKey(otherSeq[j]))
                {
                    j++;
                }
                childSeq[pos] = otherSeq[j];
                j++;
            }
 
        }

        chromosomes child = new chromosomes(childSeq);
        child.getTrips();
        //Debug.Log("crossover completed!");
        
        return child;
    }

    
    /*
     * Mutate: 9 different methods for local searching
     */
    
    private chromosomes mutate(chromosomes chromo)
    {
        Debug.Log("mutate begin!");
        double least_cost = chromo.fitness; //current least cost
        double cost = chromo.fitness;
        List<LinkedList<int>> best_mutation = new List<LinkedList<int>>(deepcopy(chromo.trips));
        
        foreach (int[] pair in pairs)
        {
            
            //find u,v belongs to which robot's trip, -1: robot nodes 
            int index0 = -1, index1 = -1; 
            bool ufind = false;
            bool vfind = false;
            for (int i = 0; i < 3; i++)
            {
                if (pair[0] > 2 && !ufind && best_mutation[i].Contains(pair[0]))
                {
                    index0 = i;
                    ufind = true;
                }
                if (pair[1] > 2 && !vfind && best_mutation[i].Contains(pair[1]))
                {
                    index1 = i;
                    vfind = true;
                }
            }
         
            //Debug.Log("pair index:" + index0 + "," + index1);
            
            //each pair try totally 9 methods to mutate in order, if least cost update then continue to the next pair
            foreach (int method in methodIndex)
            {              
                //M1: called when u is client
                if (method == 1)
                {                   
                    if (pair[0] > 2)
                    {
                        //Debug.Log("method1 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        if (pair[1] > 2)
                        {
                            var v = trips[index1].Find(pair[1]); //v
                            trips[index0].Remove(pair[0]);
                            trips[index1].AddAfter(v, pair[0]);
                        }
                        else //if(pair[1] <= 2)
                        {
                            trips[pair[1]].AddFirst(pair[0]);
                            trips[index0].Remove(pair[0]);                      
                        }

                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M2: called when u, u.next are clients
                else if (method == 2)
                {
                    //if u and u.next are clients
                    if (pair[0] > 2 && pair[0] != best_mutation[index0].Last.Value)
                    {
                        //Debug.Log("method2 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var uNext = trips[index0].Find(pair[0]).Next; //u.next
                        int val_uNext = uNext.Value; //u.next.value
                        if (pair[1] > 2)
                        {
                            var v = trips[index1].Find(pair[1]); //v
                            //v is not u.next, else nothing need change
                            if (pair[1] != val_uNext)
                            {
                                trips[index0].Remove(uNext.Previous);
                                trips[index0].Remove(uNext);
                                trips[index1].AddAfter(v, pair[0]);
                                trips[index1].AddAfter(v.Next, val_uNext);
                            }
                        }
                        else //if(pair[1] <= 2)
                        {
                            //u is not the first node of robot v's trip, else nothing need change
                            if (trips[pair[1]].First.Value != pair[0])
                            {
                                trips[pair[1]].AddFirst(val_uNext);
                                trips[pair[1]].AddFirst(pair[0]);
                                trips[index0].Remove(uNext.Previous);
                                trips[index0].Remove(uNext);
                            }
                        }

                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M3: called when u, u.next are clients
                else if (method == 3)
                {
                    //if u and u.next are clients
                    if (pair[0] > 2 && pair[0] != best_mutation[index0].Last.Value)
                    {
                        //Debug.Log("method3 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var uNext = trips[index0].Find(pair[0]).Next; //u.next
                        int val_uNext = uNext.Value; //u.next.value
                        if (pair[1] > 2)
                        {
                            //v != u.next, else swap u and v
                            if (pair[1] != val_uNext)
                            {
                                var v = trips[index1].Find(pair[1]); //v
                                trips[index0].Remove(uNext.Previous);
                                trips[index0].Remove(uNext);
                                trips[index1].AddAfter(v, val_uNext);
                                trips[index1].AddAfter(v.Next, pair[0]);
                            }
                            else
                            {
                                trips[index1].AddAfter(uNext, pair[0]);
                                trips[index0].Remove(uNext.Previous);
                            }
                        }
                        else //if(pair[1] <= 2)
                        {
                            trips[pair[1]].AddFirst(pair[0]);
                            trips[pair[1]].AddFirst(val_uNext);
                            trips[index0].Remove(uNext.Previous);
                            trips[index0].Remove(uNext);
                        }

                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M4: called when u, v are clients
                else if (method == 4)
                {
                    //if u, v are clients 
                    if (pair[0] > 2 && pair[1] > 2)
                    {
                        //Debug.Log("method4 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var u = trips[index0].Find(pair[0]); //u
                        var v = trips[index1].Find(pair[1]); //v

                        trips[index0].AddAfter(u, pair[1]);
                        trips[index1].AddAfter(v, pair[0]);
                        trips[index0].Remove(u);
                        trips[index1].Remove(v);

                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M5: called when u, u.next, v are clients
                else if (method == 5)
                {
                    //if u, u.next, v are clients
                    if (pair[0] > 2 && pair[0] != best_mutation[index0].Last.Value && pair[1] > 2)
                    {
                        //Debug.Log("method5 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var u = trips[index0].Find(pair[0]); //u
                        var v = trips[index1].Find(pair[1]); //v
                        //v != u.next, else nothing need change
                        if (pair[1] != u.Next.Value)
                        {
                            trips[index1].AddAfter(v, pair[0]);
                            trips[index1].AddAfter(v.Next, u.Next.Value);
                            trips[index0].AddBefore(u, pair[1]);
                            trips[index0].Remove(u.Next);
                            trips[index0].Remove(u);
                            trips[index1].Remove(v);
                        }

                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M6: called when u, u.next, v, v.next are clients
                else if (method == 6)
                {
                    if (pair[0] > 2 && pair[0] != best_mutation[index0].Last.Value && pair[1] > 2 &&
                        pair[1] != best_mutation[index1].Last.Value)
                    {
                        //Debug.Log("method6 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var u = trips[index0].Find(pair[0]); //u
                        var v = trips[index1].Find(pair[1]); //v
                        var vNext = v.Next;
                        //u and v can't be adjacent, else nothing will change
                        if (u.Next.Value != pair[1] && vNext.Value != pair[0])
                        {
                            trips[index1].AddAfter(vNext, pair[0]);
                            trips[index1].AddAfter(vNext.Next, u.Next.Value);
                            trips[index0].AddBefore(u, vNext.Value);
                            trips[index0].AddBefore(u.Previous, pair[1]);
                            trips[index0].Remove(u.Next);
                            trips[index0].Remove(u);
                            trips[index1].Remove(vNext);
                            trips[index1].Remove(v);
                        }

                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M7: called when u, v in the same trip (and u.next are clients)
                else if (method == 7)
                {
                    if (pair[0] > 2 && pair[1] > 2 && index0 == index1 && pair[0] != best_mutation[index0].Last.Value)
                    {
                        //Debug.Log("method7 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var u = trips[index0].Find(pair[0]); //u
                        var v = trips[index1].Find(pair[1]); //v
                        int uNextVal = u.Next.Value;

                        if (pair[1] != uNextVal)
                        {
                            trips[index0].Remove(u.Next);
                            trips[index0].AddAfter(u, pair[1]);
                            trips[index1].AddAfter(v, uNextVal);
                            trips[index1].Remove(v);
                        }
            
                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M8: called when u, v in different trips (and u.next are clients)
                else if (method == 8)
                {
                    if (pair[0] > 2 && pair[1] > 2 && index0 != index1 && pair[0] != best_mutation[index0].Last.Value)
                    {
                        //Debug.Log("method8 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var u = trips[index0].Find(pair[0]); //u
                        var v = trips[index1].Find(pair[1]); //v
                        int uNextVal = u.Next.Value;

                        trips[index0].Remove(u.Next);
                        trips[index0].AddAfter(u, pair[1]);
                        trips[index1].AddAfter(v, uNextVal);                      
                        trips[index1].Remove(v);
                        
                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }

                //M9: called when u, v in different trips (and u.next, v.next are clients)
                else //if(method == 9)
                {
                    if (pair[0] > 2 && pair[1] > 2 && index0 != index1 && pair[0] != best_mutation[index0].Last.Value &&
                        pair[1] != best_mutation[index1].Last.Value)
                    {
                        //Debug.Log("method9 begin!");
                        List<LinkedList<int>> trips = new List<LinkedList<int>>(deepcopy(best_mutation));
                        
                        var u = trips[index0].Find(pair[0]); //u
                        var v = trips[index1].Find(pair[1]); //v

                        trips[index1].AddBefore(v, u.Next.Value);
                        trips[index0].Remove(u.Next);
                        trips[index0].AddAfter(u, v.Next.Value);
                        trips[index1].Remove(v.Next);
                        
                        cost = CalTripsDist(trips);
                        if (cost < least_cost)
                        {
                            least_cost = cost;
                            best_mutation = deepcopy(trips);
                            break;
                        }
                    }
                }     
                
            }  
            
        }


        if (best_mutation != chromo.trips)
        {
            List<int> newSeq = new List<int>();
            foreach (var trip in best_mutation)
            {
                foreach (var node in trip)
                {
                    newSeq.Add(node);
                }
            }
            chromosomes result = new chromosomes(newSeq.ToArray());
            result.trips = best_mutation;
            result.fitness = least_cost;
            Debug.Log("new mutation found!");
            return result;
        }
        else
        {
            Debug.Log("No new mutation!");
            return chromo;
        }
    }
    
    private List<int[]>  GetCombination(List<int> set)
    {
        List<int[]> result = new List<int[]>();
        for (int i = 0; i < set.Count - 1; i++)
        {
            for (int j = i + 1; j < set.Count; j++)
            {
                result.Add(new int[2]{i,j});
                result.Add(new int[2]{j,i});
            }
        }
        return result;
    }

    private List<LinkedList<T>> deepcopy<T>(List<LinkedList<T>> set)
    {
        List<LinkedList<T>> result = new List<LinkedList<T>>();
        foreach (LinkedList<T> list in set)
        {
            LinkedList<T> temp = new LinkedList<T>();
            foreach (T item in list)
            {
                temp.AddLast(item);
            }
            result.Add(temp);         
        }
        return result;
    }

    private double CalTripsDist(List<LinkedList<int>> trips)
    {
        double[] totalDistance = new double[3];
        for (int i = 0; i < 3; i++)
        {
            if (trips[i].Count > 0)
            {
                var currNode = trips[i].First;
                totalDistance[i] += distances[i, currNode.Value];
                while (currNode.Next != null)
                {
                    currNode = currNode.Next;
                    totalDistance[i] += distances[currNode.Previous.Value, currNode.Value];
                }
            }
        }

        return totalDistance.Max();
    }

   

}
