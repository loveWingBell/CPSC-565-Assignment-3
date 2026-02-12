// CPSC 565 Assignment 3 - Elda Britu - 30158734 - February 13, 2025

using UnityEngine;
using Antymology.Terrain;

namespace Antymology.Agents
{
    /// <summary>
    /// The one queen ant per generation.
    /// Should probably be a larger gold/yellow sphere. (will change later if it doesn't look good)
    /// Gene[2]: moves toward high-mulch areas.
    /// Places a nest block every N ticks if she has enough health.
    /// </summary>
    public class QueenAnt : AntBase
    {
        // Nest placement: queen loses 1/3 of maxHealth per nest block
        public int nestBlocksPlaced = 0;
        private int ticksBetweenNestAttempts = 5;
        private int tickCounter = 0;

        protected override void Awake()
        {
            base.Awake();

            maxHealth = 300f; // queen is heartier :)
            health    = maxHealth;

            // Visual: larger gold/yellow sphere
            var filter   = gameObject.AddComponent<MeshFilter>();
            var renderer = gameObject.AddComponent<MeshRenderer>();
            filter.mesh  = CreateSphereMesh();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0.85f, 0f);  // gold
        }

        public override void Tick()
        {
            base.Tick();
            if (health <= 0) return;

            tickCounter++;
            if (tickCounter >= ticksBetweenNestAttempts)
            {
                tickCounter = 0;
                TryPlaceNestBlock();
            }
        }

        /// <summary>
        /// Places a nest block at the queen's current position.
        /// Costs 1/3 of maxHealth; queen must have enough health remaining.
        /// </summary>
        private void TryPlaceNestBlock()
        {
            float cost = maxHealth / 3f;
            if (health <= cost + 10f) return; // need to keep at least 10 hp

            AbstractBlock current = WorldManager.Instance.GetBlock(GridX, GridY, GridZ);
            // Only place on non-container, non-nest blocks
            if (current is ContainerBlock || current is NestBlock || current is AirBlock) 
                return;

            WorldManager.Instance.SetBlock(GridX, GridY, GridZ, new NestBlock());
            health -= cost;
            nestBlocksPlaced++;

            Debug.Log($"Queen placed nest block #{nestBlocksPlaced} at ({GridX}, {GridY}, {GridZ})");

            // Move up since the block we're on was just replaced
            int newY = GetSurfaceY(GridX, GridZ);
            if (newY >= 0) SetPosition(GridX, newY, GridZ);
        }

        /// <summary>
        /// Gene[2]: queen seeks out mulch-rich areas to stay alive.
        /// </summary>
        protected override void MoveSpecial()
        {
            MoveTowardMulch();
        }

        public override float Fitness()
        {
            return nestBlocksPlaced * 100f + ticksAlive + mulchConsumed * 5f;
        }

        // Simple sphere-ish mesh
        private Mesh CreateSphereMesh()
        {
            var go  = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            Destroy(go);
            return mesh;
        }
    }
}