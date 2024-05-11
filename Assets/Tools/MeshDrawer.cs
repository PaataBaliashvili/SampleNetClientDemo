using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tools.Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class MeshDrawer : MonoBehaviour
    {
        public Transform Start;
        public Transform End;
        public MeshFilter MeshFilter;
        public List<HalfEdge> HalfEdges;
        private NodeGraph _nodeGraph;
        private TriangleNode StartTriangle;
        public Material Material;
        private List<Triangle> _navmeshTriangles;
        private List<TriangleNode> _nodes;

        private Vector2[] _path;


        private void OnDrawGizmos()
        {
            // if (StartTriangle != null)
            // {
            //     Handles.color = Color.green;
            //     Handles.DrawLine(StartTriangle.Triangle.Vertex1.Position, StartTriangle.Triangle.Vertex2.Position, 5);
            //     Handles.DrawLine(StartTriangle.Triangle.Vertex2.Position, StartTriangle.Triangle.Vertex3.Position, 5);
            //     Handles.DrawLine(StartTriangle.Triangle.Vertex3.Position, StartTriangle.Triangle.Vertex1.Position, 5);
            //     //
            //     Gizmos.color = Color.yellow;
            //     Gizmos.DrawSphere(StartTriangle.Triangle.Vertex1.Position, 0.2f);
            //     Gizmos.DrawSphere(StartTriangle.Triangle.Vertex2.Position, 0.2f);
            //     Gizmos.DrawSphere(StartTriangle.Triangle.Vertex3.Position, 0.2f);
            //     //
            //     // Handles.DrawAAConvexPolygon(StartTriangle.Triangle.Vertex1.Position, StartTriangle.Triangle.Vertex2.Position, StartTriangle.Triangle.Vertex3.Position);
            //     
            //     foreach (var triangle in StartTriangle.Neighbours)
            //     { 
            //         Handles.color = Color.cyan;
            //         Handles.DrawLine(triangle.Triangle.Vertex1.Position, triangle.Triangle.Vertex2.Position, 4);
            //         Handles.DrawLine(triangle.Triangle.Vertex2.Position, triangle.Triangle.Vertex3.Position, 4);
            //         Handles.DrawLine(triangle.Triangle.Vertex3.Position, triangle.Triangle.Vertex1.Position, 4);
            //         
            //         // Handles.DrawAAConvexPolygon(triangle.Triangle.Vertex1.Position, triangle.Triangle.Vertex2.Position, triangle.Triangle.Vertex3.Position);
            //         //
            //         // Gizmos.color = Color.cyan;
            //         // Gizmos.DrawSphere(triangle.Triangle.Vertex1.Position, 0.2f);
            //         // Gizmos.DrawSphere(triangle.Triangle.Vertex2.Position, 0.2f);
            //         // Gizmos.DrawSphere(triangle.Triangle.Vertex3.Position, 0.2f);
            //         
            //         
            //         // Handles.DrawLine(triangle.Triangle.Vertex2.Position, triangle.Triangle.Vertex3.Position, 4);
            //         // Handles.DrawLine(triangle.Triangle.Vertex3.Position, triangle.Triangle.Vertex1.Position, 4);
            //     }
            // }


            if (_nodes != null)
            {
                foreach (var vNode in _nodes)
                {
                    Handles.color = Color.green;
                    Handles.DrawLine(vNode.SharedEdge.V1.Position, vNode.SharedEdge.V2.Position, 6);
                }
            }
            


            if (_navmeshTriangles != null)
            {
                foreach (var triangle in _navmeshTriangles)
                {
                    // Gizmos.color = Color.yellow;
                    // Gizmos.DrawSphere(triangle.Vertex1.Position, 0.2f);
                    // Gizmos.DrawSphere(triangle.Vertex2.Position, 0.2f);
                    // Gizmos.DrawSphere(triangle.Vertex3.Position, 0.2f);
                }
            }
           
            
            
            if (_path == null)
                return;
            
            Debug.Log($"OnDrawGizmos, {_path.Length}");
            for (var i = 0; i < _path.Length - 1; i++)
            {
                Handles.color = Color.yellow;
                var point = _path[i];
                var next = _path[i + 1];
                Handles.DrawLine(new Vector3(point.x, 0f, point.y), new Vector3(next.x, 0f, next.y), 5f);
            }
            
        }

        private void Awake()
        {
            var pathToNavData = Path.GetFullPath(@"C:\Users\patat\Documents\UnityProjects\SampleNetClient\Assets\NavmeshExportNavMesh.obj");
            var geometry = GeometryLoader.LoadGeometry(pathToNavData);
            
            HalfEdges = GeometryUtils.TransformFromTriangleToHalfEdge(geometry.Item3);
            
            var distincted = RemoveDoubles(geometry.Item1, geometry.Item2);


            _navmeshTriangles = CreateTriangles(geometry.Item1, geometry.Item2, distincted.Item3);
            
            var graph = CreateTriangleNodeGraph(_navmeshTriangles);


            _nodeGraph = new NodeGraph(geometry.Item1, graph);
           
            _path = AStar.FindPath(Start.position, End.position, _nodeGraph, HalfEdges, out _nodes);

           StartCoroutine(Move(_path));

            StartTriangle = AStar.FindTriangleByPosition(new Vector2(Start.position.x, Start.position.z), _nodeGraph.Nodes);
            
            

            GameObject meshObj = new GameObject("mesh");
            //meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            MeshFilter = meshObj.AddComponent<MeshFilter>();
           
            var mesh = new Mesh();
            meshObj.AddComponent<MeshRenderer>().sharedMaterial = Material;
            mesh.vertices = geometry.Item1.ToArray();
            mesh.triangles = geometry.Item2.ToArray();

            MeshFilter.sharedMesh = mesh;
            mesh.RecalculateNormals();
            
            foreach (var triangle in geometry.Item3)
            {
                // Debug.DrawLine(triangle.Vertex1.Position, triangle.Vertex2.Position, Color.red, 10000000);
                // Debug.DrawLine(triangle.Vertex2.Position, triangle.Vertex3.Position, Color.red, 10000000);
                // Debug.DrawLine(triangle.Vertex3.Position, triangle.Vertex1.Position, Color.red, 10000000);
            }
        }

        private IEnumerator Move(Vector2[] path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                var target = new Vector3(_path[i].x, 0f, _path[i].y);
                var self = new Vector3(Start.position.x, 0f, Start.position.z);

                while ((target - new Vector3(Start.position.x, 0f, Start.position.z)).sqrMagnitude > 0.01f)
                {
                    var dir = target - new Vector3(Start.position.x, 0f, Start.position.z);

                    Start.position += dir.normalized * 2f * Time.deltaTime;

                    yield return null;
                }
            }
        }

        private static List<Triangle> CreateTriangles(List<Vector3> vertices, List<int> trianglesIds, Dictionary<int, int> vertMap)
        {
            var triangles = new List<Triangle>();
            
            for (int i = 0; i < trianglesIds.Count ; i+=3)
            {
                var vertex1Id = trianglesIds[i];
                var vertex2Id = trianglesIds[i + 1];
                var vertex3Id = trianglesIds[i + 2];
                
                var vertex1IdMapped = vertMap[vertex1Id];
                var vertex2IdMapped = vertMap[vertex2Id];
                var vertex3IdMapped = vertMap[vertex3Id];
                
                
                var v1 = vertices[vertex1Id];
                var v2 = vertices[vertex2Id];
                var v3 = vertices[vertex3Id];
            
                var triangle = new Triangle(new Vertex(v1), new Vertex(v2), new Vertex(v3), new List<int>(){vertex1IdMapped, vertex2IdMapped, vertex3IdMapped});
                triangles.Add(triangle);
            }

            return triangles;
        }

        private static (List<Vector3>, List<int>, Dictionary<int, int>) RemoveDoubles(List<Vector3> vertices, List<int> triangles)
        {
            var uniqueVertices = new List<Vector3>();
            var verticesMap = new Dictionary<int, int>();

            for (var i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];

                if (uniqueVertices.Contains(vertex))
                {
                    var index = uniqueVertices.IndexOf(vertex);
                    verticesMap.Add(i, index);
                }
                else
                {
                    uniqueVertices.Add(vertex);
                    verticesMap.Add(i, i);
                }
            }

            var resultTriangles = new List<int>();

            foreach (var vertId in triangles)
            {
                var realVert = verticesMap[vertId];
                resultTriangles.Add(realVert);
            }

            return (uniqueVertices, resultTriangles, verticesMap);
        }
        
        private static List<TriangleNode> CreateTriangleNodeGraph(List<Triangle> triangles)
        {
            //create nodes
            var nodes = new List<TriangleNode>();
            foreach (var triangle in triangles)
            {
                nodes.Add(new TriangleNode(triangle));
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var triangleNode1 = nodes[i];
                var vertices = new List<Vector3>();
                
                vertices.Add(triangleNode1.Triangle.Vertex1.Position);
                vertices.Add(triangleNode1.Triangle.Vertex2.Position);
                vertices.Add(triangleNode1.Triangle.Vertex3.Position);

                for (int j = 0; j < nodes.Count; j++)
                {
                    if (i == j) continue;
                    
                    
                    var triangleNode2 = nodes[j];
                    
                    

                    int passed = 0;
                    // foreach (var vertIndex in triangleNode1.Triangle.VertIndices)
                    // {
                    //     // if (triangleNode2.Triangle.VertIndices.Contains(vertIndex))
                    //     //     passed++;
                    //     
                    // }

                    foreach (var vertex in vertices)
                    {
                        if (triangleNode2.Triangle.Vertex1.Position == vertex)
                            passed++;
                        
                        if (triangleNode2.Triangle.Vertex2.Position == vertex)
                            passed++;
                        
                        if (triangleNode2.Triangle.Vertex3.Position == vertex)
                            passed++;
                    }
                    
                    if (passed == 2)
                        triangleNode1.Neighbours.Add(triangleNode2);
                        
                }
            }

            return nodes;
        }

        // public static List<TriangleNode> FindNeighbours(TriangleNode triangleNode)
        // {
        //     
        // }

        private void Update()
        {
            var origin = new Vector2(Start.position.x, Start.position.z);
            var target = new Vector2(End.position.x, End.position.z);
            
            Debug.DrawLine(Start.position, End.position, Color.blue, 0.01f);
            
            var blockingEdges = new List<HalfEdge>();
            
            foreach (var halfEdge in HalfEdges)
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
            
           // _path = AStar.FindPath(Start.position, End.position, _nodeGraph);
        }
    }
}