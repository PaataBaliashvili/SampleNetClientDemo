﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tools
{
    public class MeshDrawer : MonoBehaviour
    {
        public Transform Start;
        public Transform End;
        public MeshFilter MeshFilter;
        private void Awake()
        {
            var path = Path.GetFullPath(@"C:\Users\patat\Documents\NavmeshExport\NavmeshExportNavMesh.obj");
            var geometry = GeometryLoader.LoadGeometry(path);
            
     
            GameObject meshObj = new GameObject("mesh");
            //meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            MeshFilter = meshObj.AddComponent<MeshFilter>();
           
            var mesh = new Mesh();
            meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
            mesh.vertices = geometry.Item1.ToArray();
            mesh.triangles = geometry.Item2.ToArray();

            MeshFilter.sharedMesh = mesh;
            mesh.RecalculateNormals();
            
            foreach (var triangle in geometry.Item3)
            {
                Debug.DrawLine(triangle.Vertex1.Position, triangle.Vertex2.Position, Color.red, 10000000);
                Debug.DrawLine(triangle.Vertex2.Position, triangle.Vertex3.Position, Color.red, 10000000);
                Debug.DrawLine(triangle.Vertex3.Position, triangle.Vertex1.Position, Color.red, 10000000);
            }
            
            Debug.DrawLine(Start.position, End.position, Color.blue, 10000000);
            var halfEdges = GeometryUtils.TransformFromTriangleToHalfEdge(geometry.Item3);

            
            var origin = new Vector2(Start.position.x, Start.position.z);
            var target = new Vector2(End.position.x, End.position.z);
            var blockingEdges = new List<HalfEdge>();
            
            foreach (var halfEdge in halfEdges)
            {
                var p1 = new Vector2(halfEdge.V.Position.x, halfEdge.V.Position.z);
                var p2 = new Vector2(halfEdge.nextEdge.V.Position.x, halfEdge.nextEdge.V.Position.z);
                
                Debug.DrawLine(new Vector3(p1.x, 0f, p1.y), new Vector3(p2.x, 0f, p2.y), Color.cyan, 10000000);
                
                if (GeometryUtils.AreLinesIntersecting(origin, target,
                        p2, p1, false))
                {
                    if (halfEdge.oppositeEdge == null)
                    {
                        Debug.Log("cant walk, Half edge: ");
                        
                        blockingEdges.Add(halfEdge);
                    }
         
                }
            }

            foreach (var blockingEdge in blockingEdges)
            {
                var p1 = new Vector3(blockingEdge.V.Position.x, 0f, blockingEdge.V.Position.z);
                var p2 = new Vector3(blockingEdge.nextEdge.V.Position.x, 0f, blockingEdge.nextEdge.V.Position.z);
                
                Debug.DrawLine(p1, p2, Color.yellow, 10000000);
            }
        }
    }
}