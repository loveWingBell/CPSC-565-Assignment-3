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
        public float tickInterval = 0.2f;

        [Tooltip("Maximum ticks before generation ends")]
        public int maxTicksPerGeneration = 300;

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
        public int SurvivorsLastGen  { get; private set; } = 0;
        public int CurrentTick       { get; private set; } = 0;

        private bool running = false;

        void Start()
        {
            // Called by WorldManager
        }

        /// <summary>
        /// Called by WorldManager.GenerateAnts() to kick off the simulation.
        /// </summary>
        public void StartSimulation()
        {
            CurrentGeneration = 0;
            TotalNestBlocks   = 0;
            SurvivorsLastGen  = 0;
            CurrentTick       = 0;
            workerGenePool.Clear();
            queenGenes = null;
            StartCoroutine(RunGeneration());
        }

        // Generation loop

        private IEnumerator RunGeneration()
        {
            CurrentGeneration++;
            CurrentTick = 0;
            running     = true;

            SpawnAnts();

            // Tick loop
            while (running)
            {
                yield return new WaitForSeconds(tickInterval);

                CurrentTick++;
                TickAllAnts();
                RemoveDeadAnts();

                bool allDead = allAnts.Count == 0;
                bool timeUp  = CurrentTick >= maxTicksPerGeneration;

                if (allDead || timeUp)
                    running = false;
            }

            // Count nests BEFORE despawning (queen might be dead)
            int nestsThisGen = (Queen != null) ? Queen.nestBlocksPlaced : 0;
            //TotalNestBlocks += nestsThisGen;
            SurvivorsLastGen = allAnts.Count;

            Debug.Log($"[Gen {CurrentGeneration}] Nest blocks: {nestsThisGen} | Total: {TotalNestBlocks} | Survivors: {SurvivorsLastGen}/{workerCount + 1}");

            EvolveGenes();
            DespawnAnts();

            // Start next generation
            yield return new WaitForSeconds(1f);
            StartCoroutine(RunGeneration());
        }

        // Tick ------------------------------

        private void TickAllAnts()
        {
            foreach (var ant in allAnts)
                if (ant != null && ant.health > 0)
                    ant.Tick();
        }

        private void RemoveDeadAnts()
        {
            List<AntBase> toRemove = new List<AntBase>();
            foreach (var ant in allAnts)
                if (ant == null || ant.health <= 0)
                    toRemove.Add(ant);

            foreach (var ant in toRemove)
            {
                allAnts.Remove(ant);
                if (ant is WorkerAnt w) workers.Remove(w);
                if (ant is QueenAnt  q && Queen == q) Queen = null;
                if (ant != null) Destroy(ant.gameObject);
            }
        }

        // Spawning ------------------------------
        private void SpawnAnts()
        {
            workers.Clear();
            allAnts.Clear();
            Queen = null;

            int worldW = ConfigurationManager.Instance.World_Diameter *
                         ConfigurationManager.Instance.Chunk_Diameter;

            // Spawn queen
            Queen = CreateAnt<QueenAnt>("Queen", worldW);
            if (queenGenes != null)
                System.Array.Copy(queenGenes, Queen.genes, queenGenes.Length);
            else
                Queen.RandomiseGenes();
            allAnts.Add(Queen);

            // Spawn workers
            for (int i = 0; i < workerCount; i++)
            {
                WorkerAnt w = CreateAnt<WorkerAnt>($"Worker_{i}", worldW);
                if (i < workerGenePool.Count)
                    System.Array.Copy(workerGenePool[i], w.genes, workerGenePool[i].Length);
                else
                    w.RandomiseGenes();
                workers.Add(w);
                allAnts.Add(w);
            }
        }

        private T CreateAnt<T>(string antName, int worldW) where T : AntBase
        {
            GameObject go = new GameObject(antName);
            T ant = go.AddComponent<T>();

            // Find a safe spawn location (not on container, preferably near mulch)
            int x = -1, z = -1, y = -1;
            int attempts = 0;
            
            while (attempts < 100)
            {
                x = Random.Range(5, worldW - 5);
                z = Random.Range(5, worldW - 5);
                y = GetSurfaceY(x, z);
                
                if (y >= 0)
                {
                    AbstractBlock surface = WorldManager.Instance.GetBlock(x, y, z);
                    // Skip if it's a container block
                    if (!(surface is ContainerBlock))
                    {
                        // Good spawn - either mulch or grass/stone
                        break;
                    }
                }
                attempts++;
            }
            
            // Fallback to center if no good spot found
            if (attempts >= 100)
            {
                x = worldW / 2;
                z = worldW / 2;
                y = Mathf.Max(5, GetSurfaceY(x, z));
            }

            ant.SetPosition(x, y, z);
            return ant;
        }

        private void DespawnAnts()
        {
            foreach (var ant in allAnts)
                if (ant != null) Destroy(ant.gameObject);
            allAnts.Clear();
            workers.Clear();
            Queen = null;
        }

        // Evolution ------------------------------

        private void EvolveGenes()
        {
            // Evolve workers, sort by fitness
            workers.Sort((a, b) => b.Fitness().CompareTo(a.Fitness()));

            int keepCount = Mathf.Max(1, Mathf.RoundToInt(workers.Count * survivalRate));
            List<float[]> survivors = new List<float[]>();
            for (int i = 0; i < Mathf.Min(keepCount, workers.Count); i++)
                survivors.Add((float[])workers[i].genes.Clone());

            // Safety: if all ants died, reinitialize with random genes
            if (survivors.Count == 0)
            {
                Debug.LogWarning("All ants died! Reinitializing with random genes.");
                workerGenePool.Clear();
                queenGenes = null;
                return;
            }

            workerGenePool.Clear();
            // Keep survivors unchanged
            foreach (var g in survivors) workerGenePool.Add(g);

            // Fill rest with crossover children
            while (workerGenePool.Count < workerCount)
            {
                float[] parentA = survivors[Random.Range(0, survivors.Count)];
                float[] parentB = survivors[Random.Range(0, survivors.Count)];
                workerGenePool.Add(AntBase.Crossover(parentA, parentB, mutationRate));
            }

            // Evolve queen
            if (Queen != null)
            {
                // Mutate queen genes slightly each generation
                if (queenGenes == null)
                    queenGenes = (float[])Queen.genes.Clone();
                else
                    queenGenes = AntBase.Crossover(queenGenes, Queen.genes, mutationRate);
            }
        }

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

        // Helper for UI to get all ants
        public List<AntBase> GetAllAnts()
        {
            return new List<AntBase>(allAnts);
        }

        // Get live nest count directly from the queen
        public int GetCurrentNestCount()
        {
            return TotalNestBlocks + (Queen != null ? Queen.nestBlocksPlaced : 0);
        }
    }
}