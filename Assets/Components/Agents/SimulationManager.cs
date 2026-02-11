// CPSC 565 Assignment 3 - Elda Britu - 30158734 - February 13, 2025

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Antymology.Helpers;
using Antymology.Terrain;

namespace Antymology.Agents
{
    /// <summary>
    /// Manages the entire evolutionary simulation:
    ///   1. Spawns ants at the start of each generation
    ///   2. Runs a tick loop
    ///   3. After each generation: scores fitness, selects survivors, breeds/mutates, repeats
    /// </summary>
    public class SimulationManager : Singleton<SimulationManager>
    {
        // Configuration 
        [Header("Ant counts")]
        public int workerCount = 10;

        [Header("Timing")]
        [Tooltip("Seconds between simulation ticks")]
        public float tickInterval = 0.3f;

        [Tooltip("Maximum ticks before generation ends")]
        public int maxTicksPerGeneration = 200;

        [Header("Evolution")]
        [Range(0f, 1f)] public float mutationRate = 0.1f;
        [Tooltip("Fraction of population kept as parents each generation")]
        [Range(0.1f, 0.9f)] public float survivalRate = 0.5f;

        // Runtime state
        public QueenAnt Queen { get; private set; }
        private List<WorkerAnt> workers = new List<WorkerAnt>();
        private List<AntBase> allAnts  = new List<AntBase>();

        // Saved gene pools between generations
        private List<float[]> workerGenePool = new List<float[]>();
        private float[]       queenGenes     = null;

        public int CurrentGeneration { get; private set; } = 0;
        public int TotalNestBlocks   { get; private set; } = 0;

        private int  currentTick  = 0;
        private bool running      = false;

        void Start()
        {
            // WorldManager calls StartSimulation via GenerateAnts().
            // If you prefer, you can call it from here after a short delay:
            // StartCoroutine(DelayedStart());
        }

        /// <summary>
        /// Called by WorldManager.GenerateAnts() to kick off the simulation.
        /// </summary>
        public void StartSimulation()
        {
            CurrentGeneration = 0;
            TotalNestBlocks   = 0;
            workerGenePool.Clear();
            queenGenes = null;
            StartCoroutine(RunGeneration());
        }

        // Generation loop

        private IEnumerator RunGeneration()
        {}

        // Tick ------------------------------

        private void TickAllAnts()
        {}

        private void RemoveDeadAnts()
        {}

        // Spawning ------------------------------
        private void SpawnAnts()
        {}

        private T CreateAnt<T>(string antName, int worldW) where T : AntBase
        {}

        private void DespawnAnts()
        {}

        // Evolution ------------------------------

        private void EvolveGenes()
        { }
        
        // Helper queries for AntBase ------------------------------

        /// <summary>Returns true if any ant OTHER than 'self' is at (x,y,z).</summary>
        public bool AnotherAntAt(int x, int y, int z, AntBase self)
        {
            foreach (var ant in allAnts)
                if (ant != self && ant.GridX == x && ant.GridY == y && ant.GridZ == z)
                    return true;
            return false;
        }

        /// <summary>Returns a random ant OTHER than 'self' that shares (x,y,z), or null.</summary>
        public AntBase RandomAntAt(int x, int y, int z, AntBase self)
        {
            List<AntBase> matches = new List<AntBase>();
            foreach (var ant in allAnts)
                if (ant != self && ant.GridX == x && ant.GridY == y && ant.GridZ == z)
                    matches.Add(ant);
            if (matches.Count == 0) return null;
            return matches[Random.Range(0, matches.Count)];
        }

        // Utility ------------------------------

        private int GetSurfaceY(int x, int z)
        {
            int maxY = ConfigurationManager.Instance.World_Height *
                       ConfigurationManager.Instance.Chunk_Diameter - 1;
            for (int y = maxY; y >= 0; y--)
                if (!(WorldManager.Instance.GetBlock(x, y, z) is AirBlock)) return y;
            return -1;
        }
    }
}