// CPSC 565 Assignment 3 - Elda Britu - 30158734 - February 13, 2025

using UnityEngine;

namespace Antymology.Agents
{
    /// <summary>
    /// A worker ant. Gene[2] makes it move toward the queen.
    /// Should probably be a dark red cube . (will change later if it doesn't look good)
    /// </summary>
    public class WorkerAnt : AntBase
    
        protected override void Awake()
        {
            base.Awake();

            // Visual: small dark-red cube
            var filter   = gameObject.AddComponent<MeshFilter>();
            var renderer = gameObject.AddComponent<MeshRenderer>();
            filter.mesh  = CreateCubeMesh(0.6f);
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.55f, 0.1f, 0.1f); // dark red
        }

        /// <summary>
        /// Gene[2]: move toward the queen so workers cluster around her.
        /// </summary>
        protected override void MoveSpecial()
        {
            QueenAnt queen = SimulationManager.Instance.Queen;
            if (queen == null) { base.MoveSpecial(); return; }

            // Pick the neighbouring direction that brings us closest to the queen
            int bestDir = -1;
            float bestDist = float.MaxValue;

            for (int d = 0; d < 4; d++)
            {
                int dx = 0, dz = 0;
                switch (d) { case 0: dz=1; break; case 1: dz=-1; break; case 2: dx=1; break; case 3: dx=-1; break; }

                int nx = GridX + dx, nz = GridZ + dz;
                int ny = GetSurfaceY(nx, nz);
                if (ny < 0 || System.Math.Abs(ny - GridY) > 2) continue;

                float dist = Vector2.Distance(new Vector2(nx, nz),
                                              new Vector2(queen.GridX, queen.GridZ));
                if (dist < bestDist) { bestDist = dist; bestDir = d; }
            }

            if (bestDir >= 0) TryMove(bestDir);
            else              TryMove(Random.Range(0, 4));
        }

        // Simple cube mesh
        private Mesh CreateCubeMesh(float size)
        {
            var mesh = new Mesh();
            float h = size / 2f;
            mesh.vertices = new Vector3[]
            {
                new Vector3(-h,-h,-h), new Vector3( h,-h,-h),
                new Vector3( h, h,-h), new Vector3(-h, h,-h),
                new Vector3(-h,-h, h), new Vector3( h,-h, h),
                new Vector3( h, h, h), new Vector3(-h, h, h),
            };
            mesh.triangles = new int[]
            {
                0,2,1, 0,3,2,  1,2,6, 1,6,5,  5,6,7, 5,7,4,
                4,7,3, 4,3,0,  3,7,6, 3,6,2,  0,1,5, 0,5,4
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}