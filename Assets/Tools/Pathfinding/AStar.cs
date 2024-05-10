using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.Pathfinding
{
    public static class AStar
    {
        public static Vector2[] FindPath(Vector3 from, Vector3 to, NodeGraph graph)
        {
            var startTriangle = FindTriangleByPosition(new Vector2(from.x, from.z), graph.Nodes);
            var destinationTriangle = FindTriangleByPosition(new Vector2(to.x, to.z), graph.Nodes);
            
            if (startTriangle == null || destinationTriangle == null)
                return Array.Empty<Vector2>();
            
            startTriangle.CostFromOrigin = 0f;
            
            var reachable = new PriorityQueue<TriangleNode>();
            var explored = new HashSet<TriangleNode>();

            reachable.Enqueue(startTriangle, 0);

            int pathLength = 1;
            
            while (reachable.Count > 0)
            {
                var current = reachable.Dequeue();

                if (current == destinationTriangle)
                    return TracePath(current, new Vector2(from.x, from.y), new Vector2(to.x, to.y), pathLength, graph);

                foreach (var neighbour in current.Neighbours)
                {
                    var newCost = current.CostFromOrigin + graph.EstimateCost(current, neighbour);

                    if (newCost < neighbour.CostFromOrigin)
                    {
                        neighbour.CostFromOrigin = newCost;

                        var priority = newCost + HeuristicFunction(neighbour, new Vector2(to.x, to.y));
                        neighbour.Previous = current;
                        reachable.Enqueue(neighbour, priority);

                        neighbour.SharedEdge = FindSharedEdge(neighbour, current);

                        pathLength++;
                    }
                }
            }

            return Array.Empty<Vector2>();
        }
        
        private static TriangleNode FindTriangleByPosition(Vector2 point, List<TriangleNode> graph)
        {
            foreach (var node in graph)
            {
                var v1 = new Vector2(node.Triangle.Vertex1.Position.x, node.Triangle.Vertex1.Position.z);
                var v2 = new Vector2(node.Triangle.Vertex2.Position.x, node.Triangle.Vertex2.Position.z);
                var v3 = new Vector2(node.Triangle.Vertex3.Position.x, node.Triangle.Vertex3.Position.z);

                if (GeometryUtils.IsPointInTriangle(v1, v2, v3, point))
                {
                    return node;
                }
            }

            return null;
        }

        private static Vector2[] TracePath(
            TriangleNode node, 
            Vector2 from, 
            Vector2 to, 
            int pathLength, 
            NodeGraph nodeGraph)
        {
            var path = new Vector2[pathLength];

            var nodePath = new List<TriangleNode>();
            int remain = pathLength;
            
            path[--remain] = to;
            
            while (node.Previous != null)
            {
                nodePath.Add(node);
                node = node.Previous;
            }

            for (var i = 0; i < nodePath.Count; i++)
            {
                var triangleNode = nodePath[i];
                var edgeOrigin = nodeGraph.Vertices[triangleNode.SharedEdge.V1];
                var edgeEnd = nodeGraph.Vertices[triangleNode.SharedEdge.V2];

                var point = GeometryUtils.FindNearestPointOnLine(new Vector2(edgeOrigin.x, edgeOrigin.z),
                    new Vector2(edgeEnd.x, edgeEnd.z), path[pathLength - 1 - i]);

                path[--remain] = point;
            }

            return path;
        }
        
        

        private static Edge FindSharedEdge(TriangleNode a, TriangleNode b)
        {
            int matches = 0;

            for (var i = 0; i < a.Triangle.VertIndices.Count; i++)
            {
                var edgeA = a.Triangle.Edges[i];
                
                for (var j = 0; j < b.Triangle.VertIndices.Count; j++)
                {
                    var edgeB = b.Triangle.Edges[j];

                    if ((edgeA.V1 == edgeB.V1 && edgeA.V2 == edgeB.V2) ||
                        (edgeA.V1 == edgeB.V2 && edgeA.V2 == edgeB.V1) ||
                        (edgeA.V1 == edgeB.V2 && edgeA.V2 == edgeB.V2) ||
                        (edgeA.V1 == edgeB.V1 && edgeA.V2 == edgeB.V1))
                    {
                        matches++;

                        if (matches == 2)
                            return edgeA;
                    }
                }
            }
            
            return null;
        }

        private static float HeuristicFunction(TriangleNode from, Vector2 target)
        {
            return (target - GeometryUtils.CalculateCenterOfTriangle(from.Triangle)).sqrMagnitude;
        }
    }
}