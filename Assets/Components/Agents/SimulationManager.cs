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
        {
            CurrentGeneration++;
            currentTick = 0;
            running     = true;

            SpawnAnts();

            // Tick loop
            while (running)
            {
                yield return new WaitForSeconds(tickInterval);

                currentTick++;
                TickAllAnts();
                RemoveDeadAnts();

                bool allDead = allAnts.Count == 0;
                bool timeUp  = currentTick >= maxTicksPerGeneration;

                if (allDead || timeUp)
                    running = false;
            }

            // Record nest blocks placed this generation
            if (Queen != null)
                TotalNestBlocks += Queen.nestBlocksPlaced;

            Debug.Log($"[Gen {CurrentGeneration}] Nest blocks this gen: " +
                      (Queen != null ? Queen.nestBlocksPlaced : 0) +
                      $" | Total: {TotalNestBlocks}");

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

            // Place at a random surface location
            int x = Random.Range(1, worldW - 1);
            int z = Random.Range(1, worldW - 1);
            int y = GetSurfaceY(x, z);
            if (y < 0) y = 5;

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
    }
}