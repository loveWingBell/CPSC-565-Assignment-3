# Antymology - CPSC 565 Assignment 3
## Elda Britu - 30158734 - February 13, 2026

---

## 📋 Table of Contents
- [Model Overview](#-model-overview)
- [How to Use the Model](#-how-to-use-the-model)
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

### What Makes Ants "Smart"?
Ants have **6 genes** (weights between 0.05-2.0) that influence decision-making:
- **Gene 0:** Seek nearby mulch (food-finding behavior)
- **Gene 1:** Move randomly (exploration)
- **Gene 2:** Special move (workers seek queen, queen seeks mulch)
- **Gene 3:** Eat mulch when standing on it
- **Gene 4:** Dig the current block
- **Gene 5:** Share health with nearby ants

Each tick, an ant chooses an action probabilistically based on gene weights. High-weight genes are more likely to be selected.


### Ant Types

| Ant | Appearance | Role | Max Health |
|-----|------------|------|------------|
| **Queen** | Gold sphere (larger) | Places nest blocks | 300 HP |
| **Worker** | Dark red cube (smaller) | Supports queen, explores terrain | 200 HP |

### Health Bars
- **Green Bar:** Health > 50%
- **Yellow Bar:** Health 25-50%
- **Red Bar:** Health < 25%

### Visual Indicators
- **Green Glowing Sphere:** Ants are sharing health (lasts 1 second)
- **Purple Blocks:** Nests placed by the queen (goal metric)

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
   - `SimulationManager` (with SimulationManager script)
   - `WorldManager` (with WorldManager script)
   - `ConfigurationManager` (with ConfigurationManager script)
   - `Canvas` (Screen Space - Overlay mode)
   - `HealthBarsCanvas` (empty GameObject with AntHealthBarsUI script)

5. Press **Play**.

### First Run
The simulation will:
1. Generate a 3D world with terrain
2. Spawn a queen ant (gold sphere) and worker ants (red cubes)
3. Begin the first generation
4. Display stats in the top-left and genes on the right panel


## References
Cooper, Davies. “Daviescooper/Antymology: The Underlying Framework for CPSC 565 Assignment 3.” GitHub. Accessed February 11, 2026. https://github.com/DaviesCooper/Antymology. 