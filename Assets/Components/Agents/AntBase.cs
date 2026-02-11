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
        public float maxHealth = 100f;
        public float health;

        // Base health lost per tick (doubled on acidic blocks)
        public float healthDecayPerTick = 5f;

        // Genes: 6 weights
        // [0] move toward nearest mulch
        // [1] move randomly
        // [2] move toward queen (workers) / move to high ground (queen)
        // [3] try eat mulch if standing on it
        // [4] try dig current block
        // [5] try share health with a nearby ant
      
    }
}
