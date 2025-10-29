# Flappy Bird ML-Agents Project

This file details the system specifications used for training and the specific modifications made to the project scripts to support reinforcement and imitation learning.

---

## 💻 System Requirements

All experiments were conducted on a system equipped with:

* **RAM:** 32 GB DDR4
* **GPU:** NVIDIA GTX 1050 Ti (4 GB)
* **CPU:** Intel Core i5-10400

---

## 🛠️ Script Modifications

### `BirdAgent.cs`

Significant changes were made to the agent's logic, observations, and reward system.

**1. Death Handling Changed**
* **Original:** Agent was simply disabled upon collision, and the episode did not formally end.
* **New:** A `-2f` penalty is applied, and `EndEpisode()` is called. This ensures the agent and environment properly reset for the next training episode.

**2. More Observations Added**
* **Original:** 3 observations (Bird's Y position, next pipe's X position, bird's Y velocity).
* **New:** 5 observations. Added **ground distance** and **sky distance**. This allows the agent to "see" the boundaries and learn to avoid them.

**3. Complete Reward System Added**
* **Original:** Only `+1` reward for successfully passing a pipe.
* **New:** A more complex reward structure was implemented to encourage specific behaviors:
    * **Survival reward:** `+0.008f` added every step to encourage staying alive.
    * **Pipe reward:** `+1f` for each pipe passed (retained from original).
    * **Death penalty:** `-2f` on collision with a pipe or boundary.
    * **Boundary warnings:** Progressive penalties are applied when the agent gets too close to the top or bottom edges.
    * **Velocity control:** `-0.005f` penalty for excessive vertical speed to encourage smoother flight.
    * **Time milestones:** `+0.5f` at 10 seconds and `+1f` at 60 seconds of survival.
    * **Success bonus:** `+10f` when an imitation learning episode is completed successfully.
    * **Progressive goals:** In reinforcement mode, `+1f` bonuses are given at milestones (100 pipes, 200, 300, etc.) to encourage long-term play.

**4. Training Mode Support**
* **Imitation Learning:** The episode ends after `maxStepsPerEpisode` is reached, granting a `+10f` success reward.
* **Reinforcement Learning:** The episode continues indefinitely, using the progressive milestone system to reward long-term survival and high scores. Each milestone resets the episode.

**5. Configuration Files**
* Two separate `.yaml` configuration files have been created and placed in the `config` folder: one optimized for Imitation Learning and one for Reinforcement Learning.

---

### `Level.cs`

The `Level` script was updated to allow for dynamic control over the environment's difficulty and variability directly from the Unity Inspector.

This script now includes three public checkboxes:

**1. `useInspectorValues`**
* **When enabled:** Uses fixed values from `inspectorGapSize` and `inspectorPipeHeight`.
* **Effect:** Gap size and pipe heights remain constant as specified in the inspector.
* **Use Case:** Provides a consistent, predictable environment for debugging or initial training.

**2. `randomizePipeHeights`**
* **When enabled:** Randomizes the vertical position (height) of the pipes within valid bounds.
* **Effect:** The gap size remains fixed (using `inspectorGapSize`), but each pipe pair spawns at a different height.
* **Use Case:** Useful for training the agent to handle varying pipe positions while maintaining a consistent gap difficulty.

**3. `randomizedEnvironment`**
* **When enabled:** Randomizes *both* the gap size (within a 40-80 range) and the pipe heights.
* **Effect:** This setting overrides all inspector values for gap size and height, creating maximum variability.
* **Use Case:** Provides the most challenging and diverse training scenarios for a robust agent.

> **Note:** These checkboxes work independently, giving you full control over the environment's difficulty and variation during training.