using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;
//using UnityEditor.Experimental.GraphView;

public class NMFlowFieldManager : MonoBehaviour
{
    public NavMeshSurface navMesh;
    void Awake()
    {
        navMesh = GetComponent<NavMeshSurface>();
    }
}
public class NavMeshFlowField
{
    public List<NavMeshNode> nodes = new List<NavMeshNode>();
    public NavMeshNode targetNode;
    NavMeshTriangulation triangulation;
    public void GenerateField()
    {
        GenerateNodes();
        SetNeighbors();
    }
    public void GenerateNodes()
    {
        triangulation = NavMesh.CalculateTriangulation();
        for (int i = 0; i < triangulation.vertices.Length; i += 3)
        {
            Vector3 vert1 = triangulation.vertices[i];
            Vector3 vert2 = triangulation.vertices[i + 1];
            Vector3 vert3 = triangulation.vertices[i + 2];
            Vector3 center = (vert1 + vert2 + vert3) / 3f;
            //nodes.Add(new NavMeshNode { index = i / 3, center = center });
            nodes.Add(new NavMeshNode(i / 3, center));
        }
    }
    public void SetNeighbors()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            NavMeshNode nodeA = nodes[i];
            for (int j = i + 1; j < nodes.Count; j++)
            {
                NavMeshNode nodeB = nodes[j];
                if (CheckNeighborhood(nodeA, nodeB, triangulation)) // Adjust threshold as needed
                {
                    nodeA.neighbors.Add(nodeB);
                    nodeB.neighbors.Add(nodeA);
                }
            }
        }
    }
    public static bool CheckNeighborhood(NavMeshNode node1, NavMeshNode node2, NavMeshTriangulation tri)
    {
        int neighborsNum = 0;
        for (int i = node1.index * 3; i <= (node1.index * 3) + 2; i++)
        {
            for (int j = node2.index * 3; j <= (node2.index * 3) + 2; j++)
            {
                if (tri.vertices[i] == tri.vertices[j])
                {
                    neighborsNum++;
                }
            }
        }
        return neighborsNum == 2;

    }
}
public class NavMeshNode
{
    public NavMeshNode(int index, Vector3 center)
    {
        this.index = index;
        this.center = center;
    }
    public int index;
    public Vector3 center;
    public List<NavMeshNode> neighbors = new List<NavMeshNode>();

    public float cost = float.MaxValue;
    public Vector3 flowDirection;
}
