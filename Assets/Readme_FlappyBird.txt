-----System Requirements-----
All experiments were conducted on a system equipped with:
  • RAM: 32 GB DDR4
  • GPU: NVIDIA GTX 1050 Ti (4 GB)
  • CPU: Intel Core i5-10400

------Changes Made to BirdAgent Script-----
1. Death Handling Changed

Original: Agent disabled, no episode end
New: -2f penalty added + EndEpisode() called
Agent properly resets instead of just disabling

2. More Observations Added

Original: Only 3 observations (bird Y, pipe X, velocity Y)
New: 5 observations (added ground distance + sky distance)
Agent can now "see" boundaries better

3. Complete Reward System Added

Original: Only +1 reward for passing pipes
New: Multiple reward sources:

Survival reward: +0.008f every step (encourages staying alive)
Pipe reward: +1f per pipe (same as original)
Death penalty: -2f on collision
Boundary warnings: Progressive penalties when too close to edges
Velocity control: -0.005f penalty for excessive speed
Time milestones: +0.5f at 10 seconds, +1f at 60 seconds
Success bonus: +10f when completing imitation episode
Progressive goals: +1f bonus at milestones in reinforcement mode



4. Training Mode Support

Imitation Learning: Episode ends after maxStepsPerEpisode with +10f reward
Reinforcement Learning: Episode continues with progressive milestones - first milestone at 100 pipes, then 200, then 300, and so on. Each milestone gives +1f bonus and resets the episode.

5. Configuration Files
After keen observations, two separate config files have been created for different training modes and placed in the project's config file folder with appropriate naming conventions - one for Imitation Learning and one for Reinforcement Learning.


-----Changes Made to Level Script-----
The Level script now includes three inspector checkboxes for controlling pipe generation:
1. useInspectorValues

When enabled: Uses fixed values from inspectorGapSize and inspectorPipeHeight
Gap size and pipe heights remain constant as specified in the inspector
Provides consistent, predictable pipe patterns

2. randomizePipeHeights

When enabled: Randomizes the vertical position (height) of pipes
Gap size remains fixed from inspectorGapSize
Each pipe spawns at a different height within valid bounds
Useful for training agents to handle varying pipe positions while maintaining consistent gap difficulty

3. randomizedEnvironment

When enabled: Randomizes both gap size (40-80 range) and pipe heights
Creates maximum variability in the environment
Overrides inspector values for gap size
Provides the most challenging and diverse training scenarios

Note: These checkboxes work independently and give you full control over environment difficulty and variation during training.