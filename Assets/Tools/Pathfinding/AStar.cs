using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Tools.Pathfinding
{
    public static class AStar
    {
        public static Vector2[] FindPath(Vector3 from, Vector3 to, NodeGraph graph, List<HalfEdge> halfEdges, out List<TriangleNode> nodes)
        {
            //var halfEdges = GeometryUtils.TransformFromTriangleToHalfEdge(triangles);
            
            var blocking = GetBlockingHalfEdges(from, to, halfEdges);
            nodes = null;
            if (blocking.Count == 0)
                return new[] { new Vector2(from.x, from.z), new Vector2(to.x, to.z) };
            
            var startTriangle = FindTriangleByPosition(new Vector2(from.x, from.z), graph.Nodes);
            
            // Handles.DrawLine(startTriangle.Triangle.Vertex1.Position, startTriangle.Triangle.Vertex2.Position, 1f);
            // Handles.DrawLine(startTriangle.Triangle.Vertex2.Position, startTriangle.Triangle.Vertex3.Position, 1f);
            // Handles.DrawLine(startTriangle.Triangle.Vertex3.Position, startTriangle.Triangle.Vertex1.Position, 1f);
            
            // Debug.DrawLine(startTriangle.Triangle.Vertex1.Position, startTriangle.Triangle.Vertex2.Position, Color.cyan, 10000);
            // Debug.DrawLine(startTriangle.Triangle.Vertex2.Position, startTriangle.Triangle.Vertex3.Position, Color.cyan, 10000);
            // Debug.DrawLine(startTriangle.Triangle.Vertex3.Position, startTriangle.Triangle.Vertex1.Position, Color.cyan, 10000);

          
            var destinationTriangle = FindTriangleByPosition(new Vector2(to.x, to.z), graph.Nodes);
            
            if (startTriangle == null || destinationTriangle == null)
                return Array.Empty<Vector2>();
            
            startTriangle.CostFromOrigin = 0f;
            
            var reachable = new PriorityQueue<TriangleNode>();
            var explored = new HashSet<TriangleNode>();

            reachable.Enqueue(startTriangle, 0);

           // int pathLength = 1;
            
            while (reachable.Count > 0)
            {
                var current = reachable.Dequeue();

                if (current == destinationTriangle)
                    return TracePath(current, new Vector2(from.x, from.z), new Vector2(to.x, to.z), graph, out nodes);

                foreach (var neighbour in current.Neighbours)
                {
                   
                    var newCost = current.CostFromOrigin + graph.EstimateCost(current, neighbour);

                    if (newCost < neighbour.CostFromOrigin)
                    {
                        neighbour.SharedEdge = FindSharedEdge(neighbour, current);
                        
                        if (neighbour.SharedEdge == null)
                            continue;
                        
                        neighbour.CostFromOrigin = newCost;

                        var priority = newCost + HeuristicFunction(neighbour, new Vector2(to.x, to.y));
                        neighbour.Previous = current;
                        reachable.Enqueue(neighbour, priority);

                        

                        //pathLength++;
                    }
                }
            }

            return Array.Empty<Vector2>();
        }

        private static List<HalfEdge> GetBlockingHalfEdges(Vector3 start, Vector3 end, List<HalfEdge> triangles)
        {
            var origin = new Vector2(start.x, start.z);
            var target = new Vector2(end.x, end.z);
            
            //Debug.DrawLine(Start.position, End.position, Color.blue, 0.01f);
            //
            // var HalfEdges = new HashSet<HalfEdge>();
            //
            // foreach (var triangle in triangles)
            // {
            //     HalfEdges.Add(triangle.Triangle.Vertex1.halfEdge);
            //     HalfEdges.Add(triangle.Triangle.Vertex2.halfEdge);
            //     HalfEdges.Add(triangle.Triangle.Vertex3.halfEdge);
            // }
            var blockingEdges = new List<HalfEdge>();
            
            foreach (var halfEdge in triangles)
            {
                var p1 = new Vector2(halfEdge.V.Position.x, halfEdge.V.Position.z);
                var p2 = new Vector2(halfEdge.nextEdge.V.Position.x, halfEdge.nextEdge.V.Position.z);
                
                //Debug.DrawLine(new Vector3(p1.x, 0f, p1.y), new Vector3(p2.x, 0f, p2.y), Color.cyan, 10000000);
                
                if (GeometryUtils.AreLinesIntersecting(origin, target,
                        p1, p2, false))
                {
                    var pI1 = new Vector3(halfEdge.V.Position.x, 0f, halfEdge.V.Position.z);
                    var pI2 = new Vector3(halfEdge.nextEdge.V.Position.x, 0f, halfEdge.nextEdge.V.Position.z);
                    
                    if (halfEdge.nextEdge.oppositeEdge == null)
                    {
                        Debug.Log("cant walk, Half edge: ");
                        Debug.DrawLine(pI1, pI2, Color.cyan, 00.1f);
                        blockingEdges.Add(halfEdge);
                    }
         
                }
            }

            return blockingEdges;
        }
        
        public static TriangleNode FindTriangleByPosition(Vector2 point, List<TriangleNode> graph)
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
            NodeGraph nodeGraph, out List<TriangleNode> pathNodes)
        {
           

            var nodePath = new List<TriangleNode>();
            pathNodes = nodePath;
           
            int pathLength = 2;
            
            while (node.Previous != null)
            {
                nodePath.Add(node);
                node = node.Previous;
                pathLength++;
            }
            
            var path = new Vector2[pathLength];
            int remain = pathLength;
            path[pathLength - 1] = to;
            path[0] = from;
            remain -= 1;

            for (var i = 0; i < nodePath.Count; i++)
            {
                var triangleNode = nodePath[i];

                DrawTriangle(triangleNode);
                
                // var edgeOrigin = nodeGraph.Vertices[triangleNode.SharedEdge.V2];
                // var edgeEnd = nodeGraph.Vertices[triangleNode.SharedEdge.V1];

                var edgeOrigin = triangleNode.SharedEdge.V2;
                var edgeEnd = triangleNode.SharedEdge.V1;
                var fromPoint = path[pathLength - 1 - i];
                var point = GeometryUtils.FindNearestPointOnLine(new Vector2(edgeOrigin.Position.x, edgeOrigin.Position.z),
                    new Vector2(edgeEnd.Position.x, edgeEnd.Position.z), fromPoint);

                path[--remain] = point;
            }

            return path;
        }


        private static void DrawTriangle(TriangleNode triangleNode)
        {
            Debug.DrawLine(triangleNode.Triangle.Vertex1.Position, triangleNode.Triangle.Vertex2.Position, Color.magenta, 10000);
            Debug.DrawLine(triangleNode.Triangle.Vertex2.Position, triangleNode.Triangle.Vertex3.Position, Color.magenta, 10000);
            Debug.DrawLine(triangleNode.Triangle.Vertex3.Position, triangleNode.Triangle.Vertex1.Position, Color.magenta, 10000);
            
            Debug.DrawLine(triangleNode.SharedEdge.V1.Position, triangleNode.SharedEdge.V2.Position, Color.blue, 10000);
        }
        
        

        private static Edge FindSharedEdge(TriangleNode a, TriangleNode b)
        {
             int matches = 0;
            
            // for (var i = 0; i < a.Triangle.Edges.Count; i++)
            // {
            //     var edgeA = a.Triangle.Edges[i];
            //     
            //     for (var j = 0; j < b.Triangle.Edges.Count; j++)
            //     {
            //         var edgeB = b.Triangle.Edges[j];
            //
            //         if ((edgeA.V1 == edgeB.V1 && edgeA.V2 == edgeB.V2) ||
            //             (edgeA.V1 == edgeB.V2 && edgeA.V2 == edgeB.V1) ||
            //             (edgeA.V1 == edgeB.V2 && edgeA.V2 == edgeB.V2) ||
            //             (edgeA.V1 == edgeB.V1 && edgeA.V2 == edgeB.V1))
            //         {
            //             // matches++;
            //             //
            //             // if (matches == 1)
            //             //     return edgeA;
            //             
            //             return edgeA;
            //         }
            //     }
            // }
            //
            // return null;

            Vertex first = null;
            Vertex second = null;
            if (IsTriangleHasVertex(a.Triangle.Vertex1, b.Triangle))
            {
                first = a.Triangle.Vertex1;
                matches++;
            }
            
            if (IsTriangleHasVertex(a.Triangle.Vertex2, b.Triangle))
            {
                if (matches == 0)
                    first = a.Triangle.Vertex2;
                else second = a.Triangle.Vertex2;
            
                matches++;
            }
            
            if (matches == 0)
                return null;
            
            if (IsTriangleHasVertex(a.Triangle.Vertex3, b.Triangle))
            {
                if (matches == 1)
                    second = a.Triangle.Vertex3;
            }

            return new Edge(first, second);
        }

        private static bool IsTriangleHasVertex(Vertex a, Triangle triangle)
        {
            if (a.Position == triangle.Vertex1.Position || a.Position == triangle.Vertex2.Position ||
                a.Position == triangle.Vertex3.Position)
                return true;

            return false;
        }

        private static float HeuristicFunction(TriangleNode from, Vector2 target)
        {
            return (target - GeometryUtils.CalculateCenterOfTriangle(from.Triangle)).sqrMagnitude;
        }
    }
}