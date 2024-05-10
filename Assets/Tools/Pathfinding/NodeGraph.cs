using System.Collections.Generic;
using UnityEngine;

namespace Tools.Pathfinding
{
    public class NodeGraph
    {
        public readonly List<Vector3> Vertices;
        public readonly List<TriangleNode> Nodes;

        public NodeGraph(List<Vector3> vertices, List<TriangleNode> nodes)
        {
            Vertices = vertices;
            Nodes = nodes;
        }

        public float EstimateCost(TriangleNode from, TriangleNode to)
        {
            return to.LocationCost;
        }
    }
}