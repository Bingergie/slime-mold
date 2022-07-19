using UnityEngine;

[CreateAssetMenu(fileName = "New SlimeSettings", menuName = "Slime Settings", order = 0)]
public class SlimeSettings : ScriptableObject
{
    [Header("Simulation Settings")]
    [Min(1)] public int stepsPerFrame = 1;
    public int width = 1280;
    public int height = 720;
    public bool bounceOffEdge; 
    public bool allowRandom;
    public int numAgents = 100;
    public Simulation.SpawnMode spawnMode;

    public Texture2D attractantMask;
    public Color attractantColor;
    [Min(0)] public float attractantStrength;
    
    public Texture2D repellentMask;
    public Color repellentColor;
    [Min(0)] public float repellentStrength;
    
    public Texture2D obstacleMask;
    public Color obstacleColor;

    [Header("Agent Settings")]
    public float sensorAngleOffset = 22.5f;
    public float sensorOffsetDistance = 9;
    [Min(1)] public int sensorSize = 1;
    public float rotationAngle = 45f;
    public int stepSize = 9;
    public Color color = Color.white;

    [Header("Trail Settings")]
    public float trailWeight = 1;
    public float decayRate = 1;
    public float diffuseRate = 1;
    public float trailAttractiveStrength = 1;
}