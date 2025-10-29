using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BirdAgent : Agent
{
    [SerializeField] private Level level;
    private Bird bird;
    private bool isJumpInputDown;
    public int pipesCount;

    #region Added by GenITeam

    private float groundY;
    private float distanceToGround;
    private float distanceToSky;

    private float episodeTimer = 0f;
    private float maxEpisodeTime = 60f; // Reduced from 60 to prevent overly long episodes
    [SerializeField]private int maxStepsPerEpisode = 9500; // Add step limit
    private bool milestoneReached30 = false;
    private bool milestoneReached60 = false;

    private int previousMilStoneCount = 100;

    public bool isTrainingWithImitation;
    public bool isTrainingWithReinforcement;
    #endregion

    private void Awake()
    {
        bird = GetComponent<Bird>();
    }

    private void Start()
    {
        bird.OnDied += Bird_OnDied;
        level.OnPipePassed += Level_OnPipePassed;

        #region Added by GenITeam

        groundY = -47.5f;
        distanceToGround = bird.transform.position.y - groundY;
        distanceToSky = level.skyCollider.transform.position.y - bird.transform.position.y;

        #endregion
    }

    private void OnDestroy()
    {
        bird.OnDied -= Bird_OnDied;
        level.OnPipePassed -= Level_OnPipePassed;
    }


    private void Level_OnPipePassed(object sender, System.EventArgs e)
    {
        pipesCount++;
        AddReward(1f);
    }

    private void Bird_OnDied(object sender, System.EventArgs e)
    {
        AddReward(-2f);
        EndEpisode();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpInputDown = true;
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
        milestoneReached30 = false;
        milestoneReached60 = false;
        
        if (isTrainingWithImitation)
            MaxStep = maxStepsPerEpisode;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
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

        #region Added by GenITeam

        // Ground distance observation
        float normalizedGroundDistance = distanceToGround / worldHeight;
        float normalizedSkyColliderDistance = distanceToSky / worldHeight;
        sensor.AddObservation(normalizedGroundDistance);
        sensor.AddObservation(normalizedSkyColliderDistance);

        #endregion
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        episodeTimer += Time.deltaTime;

        if (actions.DiscreteActions[0] == 1)
        {
            bird.Jump();
        }

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
            AddReward(-2f); // Reduced death penalty (was -10)
            EndEpisode();
            return;
        }

        // SUCCESS CONDITION
        if (isTrainingWithReinforcement && pipesCount >= previousMilStoneCount)
        {
            previousMilStoneCount += 100;
            AddReward(10f);
            EndEpisode();
        }

        if (isTrainingWithImitation && StepCount - 2 == MaxStep)
        {
            AddReward(10);
            EndEpisode();
        }

        // MILESTONE REWARDS (reduced for stability)
        if (episodeTimer >= 10f && !milestoneReached30) // 30 second milestone
        {
            AddReward(0.5f); // Reduced milestone reward
            milestoneReached30 = true;
        }

        if (episodeTimer >= 60f && !milestoneReached60) // 60 second milestone
        {
            AddReward(1f); // Reduced milestone reward
            milestoneReached60 = true;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = isJumpInputDown ? 1 : 0;

        isJumpInputDown = false;
    }
}