// CPSC 565 Assignment 3 - Elda Britu - 30158734 - February 13, 2025

using System.Collections.Generic;
using UnityEngine;
using Antymology.Terrain;

namespace Antymology.Agents
{
    /// <summary>
    /// Base class shared by WorkerAnt and QueenAnt.
    /// Handles movement, health decay, eating mulch, digging, and sharing health.
    /// </summary>
    public class AntBase : MonoBehaviour
    {
        // World position (integer block coordinates)
        public int GridX { get; protected set; }
        public int GridY { get; protected set; }
        public int GridZ { get; protected set; }

        // Health
        public float maxHealth = 200f;
        public float health;

        // Base health lost per tick (doubled on acidic blocks)
        public float healthDecayPerTick = 2f;

        // Genes: 6 weights
        // [0] move toward nearest mulch
        // [1] move randomly
        // [2] move toward queen (workers) / move to high ground (queen)
        // [3] try eat mulch if standing on it
        // [4] try dig current block
        // [5] try share health with a nearby ant
      
        public float[] genes = new float[6];

        //Stats (used for fitness scoring)
        public int ticksAlive = 0;
        public int mulchConsumed = 0;

        protected virtual void Awake()
        {
            health = maxHealth;
        }

        /// <summary>
        /// Called every simulation tick by SimulationManager.
        /// </summary>
        public virtual void Tick()
        {
            if (health <= 0) return;

            ticksAlive++;

            // Decay health (double on acidic block)
            AbstractBlock standingOn = WorldManager.Instance.GetBlock(GridX, GridY, GridZ);
            float decay = (standingOn is AcidicBlock) ? healthDecayPerTick * 2f : healthDecayPerTick;
            health -= decay;

            if (health <= 0)
            {
                health = 0;
                return; // SimulationManager will remove us next frame
            }

            DecideAction();
        }

        /// <summary>
        /// Decide what action to take based on the ant's genes.
        /// </summary>
        protected virtual void DecideAction()
        {
            // Build a weighted list of candidate actions
            float totalWeight = 0f;
            for (int i = 0; i < genes.Length; i++) totalWeight += Mathf.Max(0, genes[i]);

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < genes.Length; i++)
            {
                cumulative += Mathf.Max(0, genes[i]);
                if (roll <= cumulative)
                {
                    ExecuteAction(i);
                    return;
                }
            }

            // Fallback
            TryMove(Random.Range(0, 4));
        }

        private void ExecuteAction(int geneIndex)
        {
            switch (geneIndex)
            {
                case 0: MoveTowardMulch(); break;
                case 1: TryMove(Random.Range(0, 4)); break;
                case 2: MoveSpecial(); break;
                case 3: TryEatMulch(); break;
                case 4: TryDig(); break;
                case 5: TryShareHealth(); break;
            }
        }

        // Movement ------------------------------

        /// <summary>
        /// Try to move in one of 4 directions (0=N,1=S,2=E,3=W).
        /// Respects the ≤2 height difference rule.
        /// </summary>
        protected void TryMove(int direction)
        {
            int dx = 0, dz = 0;
            switch (direction)
            {
                case 0: dz =  1; break;
                case 1: dz = -1; break;
                case 2: dx =  1; break;
                case 3: dx = -1; break;
            }

            int newX = GridX + dx;
            int newZ = GridZ + dz;

            // Find the surface Y of the target column
            int newY = GetSurfaceY(newX, newZ);
            if (newY < 0) return; // out of bounds

            // Height difference check
            if (Mathf.Abs(newY - GridY) > 2) return;

            // Don't walk into a ContainerBlock
            AbstractBlock target = WorldManager.Instance.GetBlock(newX, newY, newZ);
            if (target is ContainerBlock) return;

            SetPosition(newX, newY, newZ);
        }

        /// <summary>
        /// Scans adjacent cells and moves toward the closest mulch block.
        /// Falls back to random movement.
        /// </summary>
        protected void MoveTowardMulch()
        {
            int bestDir = -1;
            float bestDist = float.MaxValue;

            for (int d = 0; d < 4; d++)
            {
                int dx = 0, dz = 0;
                switch (d) { case 0: dz=1; break; case 1: dz=-1; break; case 2: dx=1; break; case 3: dx=-1; break; }
                int nx = GridX + dx, nz = GridZ + dz;
                int ny = GetSurfaceY(nx, nz);
                if (ny < 0 || Mathf.Abs(ny - GridY) > 2) continue;

                // Simple distance heuristic: look for mulch nearby
                float dist = FindNearestMulchDist(nx, nz);
                if (dist < bestDist) { bestDist = dist; bestDir = d; }
            }

            if (bestDir >= 0) TryMove(bestDir);
            else              TryMove(Random.Range(0, 4));
        }

        private float FindNearestMulchDist(int cx, int cz)
        {
            // Quick scan in a small radius for mulch on the surface
            float best = float.MaxValue;
            int scan = 5;
            for (int sx = cx - scan; sx <= cx + scan; sx++)
                for (int sz = cz - scan; sz <= cz + scan; sz++)
                {
                    int sy = GetSurfaceY(sx, sz);
                    if (sy >= 0 && WorldManager.Instance.GetBlock(sx, sy, sz) is MulchBlock)
                    {
                        float d = Mathf.Sqrt((sx - cx) * (sx - cx) + (sz - cz) * (sz - cz));
                        if (d < best) best = d;
                    }
                }
            return best;
        }

        /// <summary>
        /// Should be overridden by QueenAnt and WorkerAnt once that get implemented
        /// </summary>
        protected virtual void MoveSpecial()
        {
            TryMove(Random.Range(0, 4));
        }

        // Eating ------------------------------

        /// <summary>
        /// Eat the mulch block the ant is standing on. Restores health fully.
        /// Fails if another ant is also standing here.
        /// </summary>
        public void TryEatMulch()
        {
            AbstractBlock block = WorldManager.Instance.GetBlock(GridX, GridY, GridZ);
            if (!(block is MulchBlock)) return;

            // Check no other ant is here
            if (SimulationManager.Instance.AnotherAntAt(GridX, GridY, GridZ, this)) return;

            // Consume
            WorldManager.Instance.SetBlock(GridX, GridY, GridZ, new AirBlock());
            health = Mathf.Min(health + 50f, maxHealth);
            mulchConsumed++;

            // Drop down one block
            int newY = GetSurfaceY(GridX, GridZ);
            if (newY >= 0) SetPosition(GridX, newY, GridZ);
        }

        // Digging ------------------------------

        /// <summary>
        /// Dig the block the ant is standing on (except ContainerBlock).
        /// </summary>
        public void TryDig()
        {
            AbstractBlock block = WorldManager.Instance.GetBlock(GridX, GridY, GridZ);
            if (block is ContainerBlock) return;
            if (block is AirBlock)       return;

            WorldManager.Instance.SetBlock(GridX, GridY, GridZ, new AirBlock());

            // Fall to the new surface
            int newY = GetSurfaceY(GridX, GridZ);
            if (newY >= 0) SetPosition(GridX, newY, GridZ);
        }

        // Health sharing ------------------------------

        /// <summary>
        /// Give 10 health to a random ant sharing this tile. Zero-sum.
        /// </summary>
        public void TryShareHealth()
        {
            AntBase partner = SimulationManager.Instance.RandomAntAt(GridX, GridY, GridZ, this);
            if (partner == null) return;

            float amount = Mathf.Min(10f, health - 1f); // keep at least 1 hp
            if (amount <= 0) return;
            health -= amount;
            partner.health = Mathf.Min(partner.health + amount, partner.maxHealth);
        }

        // Utility ------------------------------

        /// <summary>
        /// Returns the Y of the highest non-air block at (x,z), or -1 if out of bounds.
        /// </summary>
        protected int GetSurfaceY(int x, int z)
        {
            int maxY = ConfigurationManager.Instance.World_Height * ConfigurationManager.Instance.Chunk_Diameter - 1;
            int minY = 0;

            if (x < 0 || z < 0 ||
                x >= ConfigurationManager.Instance.World_Diameter * ConfigurationManager.Instance.Chunk_Diameter ||
                z >= ConfigurationManager.Instance.World_Diameter * ConfigurationManager.Instance.Chunk_Diameter)
                return -1;

            for (int y = maxY; y >= minY; y--)
            {
                AbstractBlock b = WorldManager.Instance.GetBlock(x, y, z);
                if (!(b is AirBlock)) return y;
            }
            return -1;
        }

        /// <summary>
        /// Move the ant to a new grid position and update its visual transform.
        /// </summary>
        public void SetPosition(int x, int y, int z)
        {
            GridX = x; GridY = y; GridZ = z;
            transform.position = new Vector3(x, y + 1f, z); // sit on top of block
        }

        // Genetics ------------------------------

        /// <summary>
        /// Randomise all genes.
        /// </summary>
        public void RandomiseGenes()
        {
            for (int i = 0; i < genes.Length; i++)
                genes[i] = Random.Range(0.1f, 1f);
        }

        /// <summary>
        /// Breed two parents: take a random split of each parent's genes then mutate.
        /// </summary>
        public static float[] Crossover(float[] parentA, float[] parentB, float mutationRate = 0.1f)
        {
            float[] child = new float[parentA.Length];
            int split = Random.Range(1, parentA.Length);
            for (int i = 0; i < parentA.Length; i++)
            {
                child[i] = (i < split) ? parentA[i] : parentB[i];
                if (Random.value < mutationRate)
                    child[i] += Random.Range(-0.2f, 0.2f);
                child[i] = Mathf.Clamp(child[i], 0.05f, 2f);
            }
            return child;
        }

        /// <summary>
        /// Fitness score used for selection (override in subclasses if desired).
        /// </summary>
        public virtual float Fitness()
        {
            return ticksAlive + mulchConsumed * 10f;
        }
    }
}