using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ReadmePopup : EditorWindow
{
    private static readonly string SHOWN_KEY = "ReadmePopup_Shown";
    private Vector2 scrollPosition;
    private Texture2D levelScriptImage;

    private void OnEnable()
    {
        levelScriptImage = Resources.Load<Texture2D>("LevelScriptImage");
    }
    
    private static string readmeText = @"<color=#4EC9B0><b>───── Prerequisites ─────</b></color>

<color=#DCDCAA><b>System Requirements:</b></color>
All experiments were conducted on a system equipped with:
  • <color=#9CDCFE>RAM:</color> 32 GB DDR4
  • <color=#9CDCFE>GPU:</color> NVIDIA GTX 1050 Ti (4 GB)
  • <color=#9CDCFE>CPU:</color> Intel Core i5-10400

<color=#4EC9B0><b>───── Changes Made to BirdAgent Script ─────</b></color>

<color=#DCDCAA><b>1. Death Handling Changed</b></color>
- <color=#9CDCFE>Original:</color> Agent disabled, no episode end
- <color=#9CDCFE>New:</color> -2f penalty added + EndEpisode() called
- Agent properly resets instead of just disabling

<color=#DCDCAA><b>2. More Observations Added</b></color>
- <color=#9CDCFE>Original:</color> Only 3 observations (bird Y, pipe X, velocity Y)
- <color=#9CDCFE>New:</color> 5 observations (added ground distance + sky distance)
- Agent can now see boundaries better

<color=#DCDCAA><b>3. Complete Reward System Added</b></color>
- <color=#9CDCFE>Original:</color> Only +1 reward for passing pipes
- <color=#9CDCFE>New:</color> Multiple reward sources:
  • Survival reward: +0.008f every step (encourages staying alive)
  • Pipe reward: +1f per pipe (same as original)
  • Death penalty: -2f on collision
  • Boundary warnings: Progressive penalties when too close to edges
  • Velocity control: -0.005f penalty for excessive speed
  • Time milestones: +0.5f at 10 seconds, +1f at 60 seconds
  • Success bonus: +10f when completing imitation episode
  • Progressive goals: +1f bonus at milestones in reinforcement mode

<color=#DCDCAA><b>4. Training Mode Support</b></color>
- <color=#C586C0>Imitation Learning:</color> Episode ends after maxStepsPerEpisode with +10f reward
- <color=#C586C0>Reinforcement Learning:</color> Episode continues with progressive milestones - first milestone at 100 pipes, then 200, then 300, and so on. Each milestone gives +1f bonus and resets the episode.

<color=#DCDCAA><b>5. Configuration Files</b></color>
After keen observations, two separate config files have been created for different training modes and placed in the project's config file folder with appropriate naming conventions - one for Imitation Learning and one for Reinforcement Learning.

<color=#4EC9B0><b>───── Changes Made to Level Script ─────</b></color>

The Level script now includes three inspector checkboxes for controlling pipe generation:

<color=#DCDCAA><b>1. useInspectorValues</b></color>
- <color=#9CDCFE>When enabled:</color> Uses fixed values from inspectorGapSize and inspectorPipeHeight
- Gap size and pipe heights remain constant as specified in the inspector
- Provides consistent, predictable pipe patterns

<color=#DCDCAA><b>2. randomizePipeHeights</b></color>
- <color=#9CDCFE>When enabled:</color> Randomizes the vertical position (height) of pipes
- Gap size remains fixed from inspectorGapSize
- Each pipe spawns at a different height within valid bounds
- Useful for training agents to handle varying pipe positions while maintaining consistent gap difficulty

<color=#DCDCAA><b>3. randomizedEnvironment</b></color>
- <color=#9CDCFE>When enabled:</color> Randomizes both gap size (40-80 range) and pipe heights
- Creates maximum variability in the environment
- Overrides inspector values for gap size
- Provides the most challenging and diverse training scenarios

<color=#CE9178><i>Note: These checkboxes work independently and give you full control over environment difficulty and variation during training.</i></color>";
    static ReadmePopup()
    {
        EditorApplication.delayCall += ShowWindowOnce;
    }

    private static void ShowWindowOnce()
    {
        if (!SessionState.GetBool(SHOWN_KEY, false))
        {
            SessionState.SetBool(SHOWN_KEY, true);
            ShowWindow();
        }
    }

    [MenuItem("Help/Project Readme")]
    public static void ShowWindow()
    {
        ReadmePopup window = GetWindow<ReadmePopup>("Project Readme");
        window.minSize = new Vector2(650, 550);
        window.Show();
    }

    private void OnGUI()
    {
        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        headerStyle.normal.textColor = new Color(0.3f, 0.8f, 1f);
    
        GUILayout.Space(10);
        GUILayout.Label("🎮 Flappy Bird ML-Agents - Project Documentation", headerStyle);
        GUILayout.Space(15);

        // Scrollable content
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
    
        GUIStyle textStyle = new GUIStyle(EditorStyles.label);
        textStyle.wordWrap = true;
        textStyle.richText = true;
        textStyle.fontSize = 12;
    
        GUILayout.Label(readmeText, textStyle);
    
        // Add image section for Level Script
        GUILayout.Space(20);
        GUILayout.Label("<color=#4EC9B0><b>───── Level Script Location ─────</b></color>", textStyle);
        GUILayout.Space(10);
    
        if (levelScriptImage != null)
        {
            // Center the image
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(levelScriptImage, GUILayout.Width(250), GUILayout.Height(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("⚠️ Image not found in Resources folder. Please add 'LevelScriptImage.png' to Assets/Resources/", EditorStyles.helpBox);
        }
    
        GUILayout.Space(10);
        GUILayout.Label("<color=#9CDCFE>The <b>Level script</b> is attached to the <b>Background</b> GameObject in the scene hierarchy as shown above.</color>", textStyle);
        GUILayout.Space(20);
    
        GUILayout.EndScrollView();

        // Close button
        GUILayout.Space(10);
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.7f);
        if (GUILayout.Button("Close", GUILayout.Height(35)))
        {
            Close();
        }
        GUI.backgroundColor = Color.white;
    
        GUILayout.Space(5);
    }
}