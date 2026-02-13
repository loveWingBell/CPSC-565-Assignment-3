# Evolutionary Ant Colony Simulation - CPSC 565 - Assignment 3
## Elda Britu - 30158734 - February 13, 2026

---

## 📋 Table of Contents
- [Model Overview](#-model-overview)
- [How to Use the Model](#-how-to-use-the-model)
- [Explaining the Algorithm](#-explaining-the-algorithm)
- [Observations](#-observations)
- [References](#-references)

## Model Overview

### Simulation Loop
Each **generation** consists of:

1. **Spawn Phase** (1 second)
   - Queen and workers spawn on the terrain
   - Genes are initialized (random for Gen 1, inherited thereafter)

2. **Evaluation Phase** (up to 500 ticks)
   - Ants move, eat mulch, share health, and dig
   - Queen places nest blocks (costs 1/3 max health each)
   - Ants lose health every tick (2 HP/tick, 4 HP/tick on acidic blocks)
   - Generation ends when all ants die OR tick limit reached

3. **Evolution Phase** (1 second)
   - Ants sorted by fitness score
   - Top 50% (survivors) reproduce
   - Offspring inherit parent genes via crossover + mutation
   - Weak performers are eliminated

4. **Next Generation**
   - Process repeats with evolved ants
   - Successful strategies accumulate over time

### Ant Types

| Ant | Appearance | Role | Max Health |
|-----|------------|------|------------|
| **Queen** | Gold sphere (larger) | Places nest blocks | 300 HP |
| **Worker** | Dark red cube (smaller) | Supports queen, explores terrain | 200 HP |

### Block Types

| Block | Color | Description |
|-------|-------|-------------|
| **Air** | Transparent | Empty space, invisible |
| **Grass** | Yellow | Base terrain layer |
| **Stone** | Gray | Underground bedrock |
| **Mulch** | Green | Food source - restores 50 HP when consumed |
| **Nest** | Red | Placed by queen|
| **Acidic** | Purple | Hazard - doubles health decay rate |
| **Container** | Black | Indestructible walls and obstacles |

### Top-Left Stats Panel
```
Generation: x          ← Current generation number
Tick: xx / xx          ← Current tick / Max ticks
Nest Blocks: xx        ← Total nests placed (all time)
Survivors (last gen): xx/xx  ← How many ants survived previous generation
Current Ants Alive: xx/xx    ← Living ants this generation
```

### Right Side Gene Panel
Shows **real-time gene values** for queen and average workers:

```
=== CURRENT GENES ===

QUEEN:
  Seek Mulch:
  Random Move:
  Special Move:
  Eat Mulch:
  Dig:
  Share Health:
  Health: xxx/xxx       ← Current health
  Nests: xx             ← Nests placed this generation

WORKERS (average):      ← Averaged across all living workers
  Seek Mulch:
  Random Move:
  ...
  Avg Health: xxx
  Count: xx             ← Number of workers alive
```

### Console Output
```
[Gen X] Nest blocks: xx | Total: xx | Survivors: xx/xx
```
Logged at the end of each generation.

### Health Bars
- **Green Bar:** Health > 50%
- **Yellow Bar:** Health 25-50%
- **Red Bar:** Health < 25%
- Additionally, the **green glowing sphere** indicates that ants are sharing health (lasts 1 second)

---

## How to Use the Model

### Prerequisites
- **Unity 6000.3.x** (any minor patch version)
- Git (for cloning the repository)

### Installation
1. Clone this repository:
   ```bash
   git clone https://github.com/loveWingBell/CPSC-565-Assignment-3.git
   ```

2. Open the project in Unity Hub

4. Ensure these GameObjects exist in your scene:
   - `MainCamera` (with FlyCamera script)
   - `SimulationManager` (with SimulationManager script)
   - `WorldManager` (with WorldManager script)
   - `ConfigurationManager` (with ConfigurationManager script)
   - `Genes` (Screen Space - Overlay mode, with Gene Text attached with GeneDisplayUI script)
   - `HealthBarsCanvas` (empty GameObject with Canvas attached with AntHealthBarsUI script)
   - `Nest Counter` (Canvas with Nest Text attached with NestCounterUI script)


5. Press **Play**.

### First Run
The simulation will:
1. Generate a 3D world with terrain
2. Spawn a queen ant (gold sphere) and worker ants (red cubes)
3. Begin the first generation
4. Display stats in the top-left and genes on the right panel


### Camera Controls
- **WASD** - Move forward/back/left/right
- **Q/E** - Move down/up
- **Arrow Keys** - Rotate camera
- **Right-Click + Mouse** - Look around (mouse look)
- **Scroll Wheel** - Adjust movement speed
- **Shift** - Move faster (3× speed multiplier)

### Simulation Controls
- **Space** - Pause/Resume (Unity Editor only)
- **Escape** - Exit play mode

---
## Explaining the Algorithm

The fitness function used consists of the following.
- Queen Fitness = (nestBlocksPlaced × 100) + ticksAlive + (mulchConsumed × 5)
- Worker Fitness = ticksAlive + (mulchConsumed × 10)

**Design rationale:**
- Nest blocks are heavily weighted (100×) because that's the primary objective
- Survival (ticksAlive) ensures ants don't evolve suicidal strategies
- Mulch consumption rewards sustainable food-finding behavior

From this, the top 50% of the population survive as parents. The bottom 50% would be eliminated. I believed this elitist strategy would work best because it.
- Preserves best solutions found so far
- Prevents "genetic drift" where good genes are lost
- Faster convergence than pure roulette selection

### How the Genes Mix

Taking two parent array of genes, this function creates a new array of genes (“ the child”) by combining the two using a single-point crossover, where values before a random split come from one parent and the rest from the other. Each value then has a small chance to be randomly mutated by a slight amount, introducing variation.
```c#
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
```

**Mutation parameters:**
- **Rate:** 10% per gene (balanced exploration/exploitation)
- **Magnitude:** ±0.2 (small tweaks, not random resets)
- **Bounds:** [0.05, 2.0] prevents degenerate values

In the case that all of the ants die before the end of a generation, a new set of ants is generated with random genes. 

# Observations

Admittedly, I was not able to explore the full capabilities of the simulation, lest my laptop overheats and damages itself. However, from what I did manage to test:

**Generation 1-3: The Chaos Phase:**

- Ants spawn and wander randomly in all directions
- Most ants ignore mulch blocks even when standing on them
- Workers don't cluster around the queen
- Queen moves erratically, places 2-5 nests before dying almost immediately.
- High mortality: 70-80% of ants die before generation ends, if not all of them.

**Generation 4-8: Emergence of Basic Strategies**
- Some ants begin seeking mulch
- Workers occasionally cluster near queen (within 10 blocks)
- Queen places 2-5 nests per generation
- Survival improves: 40-60% of ants survive

Beyond this, the computer began running too slow and the CPU was heating up. I assume more could be observed with a better graphics card.

## References
Christina Creates Games.“Unity UI Canvas Anchors Explained.” YouTube, April 2, 2024. Accessed February 12, 2026. https://www.youtube.com/watch?v=Lf8gbfEHgcI. 

Cooper, Davies. “Daviescooper/Antymology: The Underlying Framework for CPSC 565 Assignment 3.” GitHub. Accessed February 11, 2026. https://github.com/DaviesCooper/Antymology. 

DJ Oamen. “How to Fix Pink Textures in Unity 6.” YouTube, October 27, 2024. Accessed February 11, 2026. https://www.youtube.com/watch?v=8MplcYrkwqU. 

“Elitist Genetic Algorithm #.” Elitist Genetic Algorithm | Algorithm Afternoon, April 17, 2024. Accessed February 12, 2026. https://algorithmafternoon.com/genetic/elitist_genetic_algorithm/.

Mastering 2D cameras in unity: A tutorial for game developers | toptal®. Accessed February 13, 2026. https://www.toptal.com/developers/unity/2d-camera-in-unity. 

Response to “How do I show that the ants are actually sharing health?”. Claude AI Sonnet 4.5 Extended. Anthropic. Accessed February 13, 2026.