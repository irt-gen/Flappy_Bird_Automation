#region Flappy Bird Original Code

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class OriginalBird : Agent
{
    [SerializeField] private Level level;
    private Bird bird;
    private bool isJumpInputDown;
    public int pipesCount;

    #region Added by Awais

    private float groundY;
    private float distanceToGround;
    private float distanceToSky;

    private float episodeTimer = 0f;
    private float maxEpisodeTime = 60f; // Reduced from 60 to prevent overly long episodes
    private int maxStepsPerEpisode = 9500; // Add step limit
    private int currentSteps = 0;
    private bool milestoneReached30 = false;
    private bool milestoneReached50 = false;

    private int previousMilStoneCount = 100; 
    #endregion

    private void Awake()
    {
        bird = GetComponent<Bird>();
    }

    private void Start()
    {
        bird.OnDied += Bird_OnDied;
        level.OnPipePassed += Level_OnPipePassed;

        #region Added by Awais

        groundY = -47.5f; // Same value as in Level.cs SpawnInitialGround()
        distanceToGround = bird.transform.position.y - groundY;
        distanceToSky = level.skyCollider.transform.position.y - bird.transform.position.y;

        #endregion
    }


    private void Level_OnPipePassed(object sender, System.EventArgs e)
    {
        pipesCount++;
        AddReward(1f);
    }

    private void Bird_OnDied(object sender, System.EventArgs e)
    {
        Debug.Log("<color=cyan> ---Dead--- </color>");
        AddReward(-2f);
        EndEpisode();
        // gameObject.SetActive(false);
        //Loader.Load(Loader.Scene.GameScene);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpInputDown = true;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R");
            StartCoroutine(ResumeDelay());
        }
    }

    IEnumerator ResumeDelay()
    {
        yield return new WaitForSeconds(3f);
        UnityEditor.EditorApplication.isPaused = false;

    }

    public override void OnEpisodeBegin()
    {
        bird.Reset();
        pipesCount = 0;
        level.Reset(); // Make sure level resets too
        episodeTimer = 0f;
        currentSteps = 0;
        milestoneReached30 = false;
        milestoneReached50 = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        #region Old Observations

        float worldHeight = 100f;
        float birdNormalizedY = (bird.transform.position.y + (worldHeight / 2f)) / worldHeight;
        sensor.AddObservation(birdNormalizedY);

        float pipeSpawnXPosition = 100f;
        Level.PipeComplete pipeComplete = level.GetNextPipeComplete();
        if (pipeComplete != null && pipeComplete.pipeBottom != null && pipeComplete.pipeBottom.pipeBodyTransform != null)
        {
            sensor.AddObservation(pipeComplete.pipeBottom.GetXPosition() / pipeSpawnXPosition);
        }
        else
        {
            sensor.AddObservation(1f);
        }

        sensor.AddObservation(bird.GetVelocityY() / 200f);

        #region Added by Awais

        // Ground distance observation
        float normalizedGroundDistance = distanceToGround / worldHeight;
        float normalizedSkyColliderDistance = distanceToSky / worldHeight;
        sensor.AddObservation(normalizedGroundDistance);
        sensor.AddObservation(normalizedSkyColliderDistance);

        #endregion

        #endregion

        #region New Observations

        // // Bird position
        // float birdNormalizedY = (bird.transform.position.y + 50f) / 100f;
        // sensor.AddObservation(birdNormalizedY);
        //
        // // Bird velocity
        // sensor.AddObservation(bird.GetVelocityY() / 200f);
        //
        // // Next pipe info
        // Level.PipeComplete pipeComplete = level.GetNextPipeComplete();
        // if (pipeComplete != null && pipeComplete.pipeBottom != null)
        // {
        //     float pipeX = pipeComplete.pipeBottom.GetXPosition() / 100f;
        //     sensor.AddObservation(pipeX);
        //
        //     // Gap center position
        //     float gapY = pipeComplete.gapY / 100f;
        //     sensor.AddObservation(gapY);
        //
        //     // Distance to gap center
        //     float distanceToGap = (pipeComplete.gapY - bird.transform.position.y) / 100f;
        //     sensor.AddObservation(distanceToGap);
        // }
        // else
        // {
        //     sensor.AddObservation(1f);
        //     sensor.AddObservation(0.5f);
        //     sensor.AddObservation(0f);
        // }

        #endregion
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        #region Previous attempts

        // currentSteps++;
        // episodeTimer += Time.deltaTime;
        //
        // if (actions.DiscreteActions[0] == 1)
        // {
        //     bird.Jump();
        // }
        //
        //
        // // Episode time limit
        // if (episodeTimer >= maxEpisodeTime || currentSteps >= maxStepsPerEpisode)
        // {
        //     // Small penalty for timing out to encourage faster completion
        //     AddReward(-0.5f);
        //     EndEpisode();
        //     return;
        // }
        // else
        // {
        //     // Survival Reward
        //     AddReward(0.01f);
        // }
        //
        // //AddReward(10f / MaxStep);
        //
        // //Debug.Log(GetCumulativeReward());
        //
        // // Check if bird is in reasonable bounds
        // // float birdNormalizedY = (bird.transform.position.y + (100 / 2f)) / 100;
        // // if (birdNormalizedY > 0.97f || birdNormalizedY < 0.2)
        // // {
        // //     Debug.Log($"Height :: {birdNormalizedY}");
        // //     AddReward(-2f); // Penalty for going too high or low
        // //     EndEpisode();
        // // }
        //
        //
        // float birdY = bird.transform.position.y;
        // // Distance thresholds (adjust as needed)
        // float dangerZoneDistance = 5f; // Distance considered "too close"
        //
        // // // Ground collision penalty
        // // float distanceToGround = birdY - groundY;
        // // if (distanceToGround < dangerZoneDistance)
        // // {
        // //     float penalty = (dangerZoneDistance - distanceToGround) / dangerZoneDistance;
        // //     AddReward(-0.01f); // Gradually increasing penalty as bird gets closer
        // // }
        // //
        // // // Sky collision penalty
        // // float distanceToSkyWithBird = distanceToSky - birdY;
        // // if (distanceToSky < dangerZoneDistance)
        // // {
        // //     float penalty = (dangerZoneDistance - distanceToSkyWithBird) / dangerZoneDistance;
        // //     AddReward(-0.01f); // Gradually increasing penalty as bird gets closer
        // // }
        //
        // // Immediate collision check (if bird actually hits boundaries)
        // if (birdY <= groundY + 1f || birdY >= distanceToSky - 1f)
        // {
        //     Debug.Log("~~ Bird is out of bounds");
        //     AddReward(-1f); // Big penalty for actual collision
        //     EndEpisode();
        // }
        //
        // if (pipesCount >= 350)
        // {
        //     AddReward(10f); // Penalty for going too high or low
        //     EndEpisode();
        // }

        #endregion

        #region New Attempt

        currentSteps++;
        episodeTimer += Time.deltaTime;

        if (actions.DiscreteActions[0] == 1)
        {
            bird.Jump();
        }

// Episode time limit
        // if (episodeTimer >= maxEpisodeTime || currentSteps >= maxStepsPerEpisode)
        // {
        //     AddReward(-0.5f); // Reduced timeout penalty
        //     if (!level.trunOffPipes)
        //     {
        //         EndEpisode();
        //         return;
        //     }
        // }

// MAIN REWARD: Survival only (simplified)
        AddReward(0.008f); // Slightly reduced for final stability

        float birdY = bird.transform.position.y;
        float skyY = level.skyCollider.transform.position.y;
        float groundY = this.groundY;

// BOUNDARY PENALTIES (simplified and balanced)
        float safeZoneHeight = (skyY - groundY) * 0.8f; // 80% of total height is safe
        float centerY = (groundY + skyY) / 2f;
        float maxSafeDistance = safeZoneHeight / 2f;
        float distanceFromCenter = Mathf.Abs(birdY - centerY);

// Progressive boundary penalty (only when getting dangerous)
        if (distanceFromCenter > maxSafeDistance)
        {
            float dangerRatio = (distanceFromCenter - maxSafeDistance) / (maxSafeDistance * 0.25f);
            dangerRatio = Mathf.Clamp01(dangerRatio);
            AddReward(-dangerRatio * 0.02f); // Small, progressive penalty
        }

// VELOCITY CONTROL (simplified)
        float velocityY = bird.GetVelocityY();
        if (Mathf.Abs(velocityY) > 150f) // Penalize excessive velocity
        {
            AddReward(-0.005f);
        }

// COLLISION CHECK (immediate termination)
        if (birdY <= groundY + 1f || birdY >= skyY - 1f)
        {
            Debug.Log("~~ Bird is out of bounds");
            AddReward(-2f); // Reduced death penalty (was -10)
            EndEpisode();
            return;
        }

// SUCCESS CONDITION
        if (pipesCount >= previousMilStoneCount)
        {
            previousMilStoneCount += 100;
            AddReward(1f); // Increased success reward
            Debug.Log("previousMilStoneCount " + previousMilStoneCount);
            // EndEpisode();
        }

// MILESTONE REWARDS (reduced for stability)
        if (episodeTimer >= 10f && !milestoneReached30) // 30 second milestone
        {
            AddReward(0.5f); // Reduced milestone reward
            milestoneReached30 = true;
            Debug.Log("30 second survival milestone!");
        }

        if (episodeTimer >= 60f && !milestoneReached50) // 60 second milestone
        {
            AddReward(1f); // Reduced milestone reward
            milestoneReached50 = true;
            Debug.Log("60 second survival milestone!");
        }

        #endregion
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = isJumpInputDown ? 1 : 0;

        isJumpInputDown = false;
    }
}

#endregion


#region Reinforcement Learning

//
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
//
// public class BirdAgent : Agent
// {
//     [SerializeField] private Level level;
//     [SerializeField] private float jumpReward = 0.1f;
//     [SerializeField] private float passReward = 5f;
//     [SerializeField] private float deathPenalty = -10f;
//     [SerializeField] private float timeStepPenalty = -0.01f;
//     [SerializeField] private float survivalReward = 0.02f;
//
//     private Bird bird;
//     private bool isJumpInputDown;
//     private float lastDistance;
//     private int pipesPassed;
//
//     private void Awake()
//     {
//         bird = GetComponent<Bird>();
//     }
//
//     private void Start()
//     {
//         bird.OnDied += Bird_OnDied;
//         level.OnPipePassed += Level_OnPipePassed;
//     }
//
//     private void Level_OnPipePassed(object sender, System.EventArgs e)
//     {
//         pipesPassed++;
//         AddReward(passReward);
//         Debug.Log($"Pipe passed! Total pipes: {pipesPassed}, Reward: {passReward}");
//     }
//
//     private void Bird_OnDied(object sender, System.EventArgs e)
//     {
//         // Death penalty
//         AddReward(deathPenalty);
//         Debug.Log($"Bird died! Final score: {pipesPassed}, Total reward: {GetCumulativeReward()}");
//
//         // End episode for RL training
//         EndEpisode();
//     }
//
//     private void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             isJumpInputDown = true;
//         }
//     }
//
//     public override void OnEpisodeBegin()
//     {
//         bird.Reset();
//         level.Reset(); // Make sure level resets too
//         pipesPassed = 0;
//         lastDistance = 0f;
//
//         Debug.Log("New episode started!");
//     }
//
//     public override void CollectObservations(VectorSensor sensor)
//     {
//         #region Previous Approach
//
//         // float worldHeight = 100f;
//         // float birdNormalizedY = (bird.transform.position.y + (worldHeight / 2f)) / worldHeight;
//         // sensor.AddObservation(birdNormalizedY);
//         //
//         // // Bird velocity
//         // sensor.AddObservation(bird.GetVelocityY() / 200f);
//         //
//         // // Next pipe information
//         // float pipeSpawnXPosition = 100f;
//         // Level.PipeComplete pipeComplete = level.GetNextPipeComplete();
//         //
//         // if (pipeComplete != null && pipeComplete.pipeBottom != null && pipeComplete.pipeBottom.pipeBodyTransform != null)
//         // {
//         //     // Horizontal distance to next pipe
//         //     float pipeX = pipeComplete.pipeBottom.GetXPosition();
//         //     sensor.AddObservation(pipeX / pipeSpawnXPosition);
//         //
//         //     // Vertical distance to pipe gap center
//         //     float pipeGapY = pipeComplete.gapY;
//         //     float birdY = bird.transform.position.y;
//         //     float normalizedGapDistance = (pipeGapY - birdY) / worldHeight;
//         //     sensor.AddObservation(normalizedGapDistance);
//         //
//         //     // Calculate reward based on getting closer to pipe
//         //     float currentDistance = Vector2.Distance(bird.transform.position, new Vector2(pipeX, pipeGapY));
//         //     if (lastDistance > 0 && currentDistance < lastDistance)
//         //     {
//         //         AddReward(0.001f); // Small reward for getting closer
//         //     }
//         //
//         //     lastDistance = currentDistance;
//         // }
//         // else
//         // {
//         //     sensor.AddObservation(1f);
//         //     sensor.AddObservation(0f);
//         // }
//         //
//         // // Add survival reward each step
//         // AddReward(survivalReward);
//
//         #endregion
//
//         #region Original Approach
//
//         float worldHeight = 100f;
//         float birdNormalizedY = (bird.transform.position.y + (worldHeight / 2f)) / worldHeight;
//         sensor.AddObservation(birdNormalizedY);
//
//         float pipeSpawnXPosition = 100f;
//         Level.PipeComplete pipeComplete = level.GetNextPipeComplete();
//         if (pipeComplete != null && pipeComplete.pipeBottom != null && pipeComplete.pipeBottom.pipeBodyTransform != null)
//         {
//             sensor.AddObservation(pipeComplete.pipeBottom.GetXPosition() / pipeSpawnXPosition);
//         }
//         else
//         {
//             sensor.AddObservation(1f);
//         }
//
//         sensor.AddObservation(bird.GetVelocityY() / 200f);
//
//         #endregion
//     }
//
//     public override void OnActionReceived(ActionBuffers actions)
//     {
//         if (actions.DiscreteActions[0] == 1)
//         {
//             bird.Jump();
//             // Small penalty for jumping to encourage efficient movement
//             AddReward(jumpReward);
//         }
//
//         // Time step penalty to encourage faster completion
//         AddReward(timeStepPenalty);
//
//         // Check if bird is in reasonable bounds
//         float birdNormalizedY = (bird.transform.position.y + (100 / 2f)) / 100;
//         if (birdNormalizedY > 0.97f || birdNormalizedY < 0.2)
//         {
//             Debug.Log($"Height :: {birdNormalizedY}");
//             AddReward(-1f); // Penalty for going too high or low
//         }
//     }
//
//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
//         discreteActions[0] = isJumpInputDown ? 1 : 0;
//         isJumpInputDown = false;
//     }
//
//     // Helper method to get current performance stats
//     public float GetPerformanceScore()
//     {
//         return pipesPassed + (GetCumulativeReward() / 100f);
//     }
// }

#endregion


#region MyRegion

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
//
// public class BirdAgent : Agent
// {
//     [SerializeField] private Level level;
//     [SerializeField] private float maxEpisodeTime = 120f; // 2 minutes max
//
//     private Bird bird;
//     private bool isJumpInputDown;
//     public int pipesCount;
//     private float episodeStartTime;
//     private float lastPipeTime;
//     private int stepsWithoutProgress;
//
//     private void Awake()
//     {
//         bird = GetComponent<Bird>();
//     }
//
//     private void Start()
//     {
//         bird.OnDied += Bird_OnDied;
//         level.OnPipePassed += Level_OnPipePassed;
//     }
//
//     private void Level_OnPipePassed(object sender, System.EventArgs e)
//     {
//         pipesCount++;
//
//         // Progressive reward system
//         float baseReward = 10f;
//         float progressBonus = Mathf.Min(pipesCount * 0.1f, 5f); // Max 5f bonus
//         AddReward(baseReward + progressBonus);
//
//         lastPipeTime = Time.time;
//         stepsWithoutProgress = 0;
//
//         Debug.Log($"Pipe passed! Count: {pipesCount}, Reward: {baseReward + progressBonus}");
//     }
//
//     private void Bird_OnDied(object sender, System.EventArgs e)
//     {
//         // Death penalty based on progress
//         float deathPenalty = -5f;
//         if (pipesCount == 0) deathPenalty = -10f; // Harsh penalty if no progress
//
//         AddReward(deathPenalty);
//         Debug.Log($"Bird died! Pipes: {pipesCount}, Death penalty: {deathPenalty}");
//         EndEpisode();
//     }
//
//     private void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             isJumpInputDown = true;
//         }
//     }
//
//     public override void OnEpisodeBegin()
//     {
//         bird.Reset();
//         pipesCount = 0;
//         episodeStartTime = Time.time;
//         lastPipeTime = Time.time;
//         stepsWithoutProgress = 0;
//
//         // Add some randomization to prevent overfitting
//         float randomY = Random.Range(-5f, 5f);
//         Vector3 startPos = bird.transform.position;
//         startPos.y += randomY;
//         bird.transform.position = startPos;
//
//         level.Reset();
//
//         Debug.Log("New episode started!");
//     }
//
//     public override void CollectObservations(VectorSensor sensor)
//     {
//         // Consistent normalization
//         float worldHeight = 100f;
//         float worldCenter = 0f; // Assuming world center is at 0
//
//         // Bird position (normalized to -1 to 1)
//         float birdNormalizedY = bird.transform.position.y / (worldHeight / 2f);
//         sensor.AddObservation(Mathf.Clamp(birdNormalizedY, -1f, 1f));
//
//         // Bird velocity (normalized)
//         float normalizedVelocity = Mathf.Clamp(bird.GetVelocityY() / 20f, -1f, 1f);
//         sensor.AddObservation(normalizedVelocity);
//
//         // Next pipe info
//         Level.PipeComplete pipeComplete = level.GetNextPipeComplete();
//         if (pipeComplete != null && pipeComplete.pipeBottom != null)
//         {
//             // Horizontal distance to pipe (0 to 1)
//             float pipeX = pipeComplete.pipeBottom.GetXPosition();
//             float normalizedPipeX = Mathf.Clamp01(pipeX / 100f);
//             sensor.AddObservation(normalizedPipeX);
//
//             // Gap center position (normalized)
//             float normalizedGapY = pipeComplete.gapY / (worldHeight / 2f);
//             sensor.AddObservation(Mathf.Clamp(normalizedGapY, -1f, 1f));
//
//             // Vertical distance to gap center
//             float distanceToGap = (pipeComplete.gapY - bird.transform.position.y) / (worldHeight / 2f);
//             sensor.AddObservation(Mathf.Clamp(distanceToGap, -1f, 1f));
//
//             // Is pipe close? (binary feature)
//             sensor.AddObservation(pipeX < 20f ? 1f : 0f);
//         }
//         else
//         {
//             sensor.AddObservation(1f); // No pipe - far distance
//             sensor.AddObservation(0f); // No gap
//             sensor.AddObservation(0f); // No distance
//             sensor.AddObservation(0f); // Not close
//         }
//
//         // Additional context
//         sensor.AddObservation(pipesCount / 100f); // Progress indicator
//     }
//
//     public override void OnActionReceived(ActionBuffers actions)
//     {
//         // Action handling
//         if (actions.DiscreteActions[0] == 1)
//         {
//             bird.Jump();
//             // Very small reward for jumping to prevent spam
//             AddReward(0.005f);
//         }
//
//         // Small time penalty to encourage efficiency
//         AddReward(-0.001f);
//
//         // Track progress
//         stepsWithoutProgress++;
//
//         // Check boundaries with consistent normalization
//         float birdY = bird.transform.position.y;
//         float worldHeight = 100f;
//
//         if (birdY > (worldHeight / 2f) * 0.95f || birdY < -(worldHeight / 2f) * 0.95f)
//         {
//             Debug.Log($"Bird out of bounds: {birdY}");
//             AddReward(-3f);
//             EndEpisode();
//             return;
//         }
//
//         // Episode termination conditions
//         CheckEpisodeEnd();
//     }
//
//     private void CheckEpisodeEnd()
//     {
//         // Time-based termination
//         if (Time.time - episodeStartTime > maxEpisodeTime)
//         {
//             Debug.Log("Episode ended: Time limit reached");
//             AddReward(-1f); // Small penalty for timeout
//             EndEpisode();
//             return;
//         }
//
//         // Success termination
//         if (pipesCount >= 100) // Reasonable success threshold
//         {
//             Debug.Log("Episode ended: Success!");
//             AddReward(20f); // Big success reward
//             EndEpisode();
//             return;
//         }
//
//         // Stuck detection
//         if (stepsWithoutProgress > 2000) // About 10 seconds without progress
//         {
//             Debug.Log("Episode ended: No progress");
//             AddReward(-2f);
//             EndEpisode();
//             return;
//         }
//     }
//
//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
//         discreteActions[0] = isJumpInputDown ? 1 : 0;
//         isJumpInputDown = false;
//     }
// }

#endregion