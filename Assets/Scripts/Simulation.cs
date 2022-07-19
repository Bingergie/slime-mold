using UnityEngine;
using Random = UnityEngine.Random;

public class Simulation : MonoBehaviour
{
    public enum SpawnMode
    {
        Random,
        Point,
        InwardCircle,
        SmallCircle,
        TinyCircle,
        RandomCircle
    }

    private int _updateKernel;
    private int _diffuseKernel;
    private int _renderKernel;

    public ComputeShader compute;
    public SlimeSettings[] settings;
    [Min(0)] public int activeSettingIndex;
    public bool showAgentsOnly;
    public FilterMode filterMode;

    private ComputeBuffer _agentBuffer;
    private RenderTexture _displayTexture;
    private RenderTexture _diffusedTrailMap;
    private RenderTexture _trailMap;

    private Texture2D _attractantMask;
    private Texture2D _repellentMask;
    private Texture2D _obstacleMask;


    private void Start()
    {
        _updateKernel = compute.FindKernel("Update");
        _diffuseKernel = compute.FindKernel("Diffuse");
        _renderKernel = compute.FindKernel("Render");

        if (activeSettingIndex >= settings.Length) activeSettingIndex = settings.Length - 1;

        // transform.localScale =
        //     new Vector3(settings[activeSettingIndex].width / settings[activeSettingIndex].height * 9, 9, 1);
        Init();
        GetComponent<MeshRenderer>().material.mainTexture = _displayTexture;
    }

    private void FixedUpdate()
    {
        if (activeSettingIndex >= settings.Length) activeSettingIndex = settings.Length - 1;
        ComputeUtils.SetSettings(compute, settings[activeSettingIndex]);

        for (var i = 0; i < settings[activeSettingIndex].stepsPerFrame; i++) RunShader();
    }

    private void LateUpdate()
    {
        ComputeUtils.Dispatch(compute, settings[activeSettingIndex].width, settings[activeSettingIndex].height, 1,
            _renderKernel);
        // ComputeUtils.CopyRenderTexture(_trailMap, _displayTexture);
    }

    private void OnDestroy()
    {
        _agentBuffer.Release();
    }

    private void Init()
    {
        // Init agents
        var agents = new Agent[settings[activeSettingIndex].numAgents];
        for (var i = 0; i < agents.Length; i++)
        {
            var center = new Vector2(settings[activeSettingIndex].width / 2f, settings[activeSettingIndex].height / 2f);
            var startPos = Vector2.zero;
            var randomAngle = Random.value * Mathf.PI * 2;
            float rotation = 0;

            switch (settings[activeSettingIndex].spawnMode)
            {
                case SpawnMode.Point:
                    startPos = center;
                    rotation = randomAngle;
                    break;
                case SpawnMode.Random:
                    startPos = new Vector2(Random.Range(0, settings[activeSettingIndex].width),
                        Random.Range(0, settings[activeSettingIndex].height));
                    rotation = randomAngle;
                    break;
                case SpawnMode.InwardCircle:
                    startPos = center + Random.insideUnitCircle * settings[activeSettingIndex].height * 0.5f;
                    rotation = Mathf.Atan2((center - startPos).normalized.y, (center - startPos).normalized.x);
                    break;
                case SpawnMode.SmallCircle:
                    startPos = center + Random.insideUnitCircle * settings[activeSettingIndex].height * 0.1f;
                    rotation = Mathf.Atan2((center - startPos).normalized.y, (center - startPos).normalized.x);
                    break;
                case SpawnMode.TinyCircle:
                    startPos = center + Random.insideUnitCircle * settings[activeSettingIndex].height * 0.05f;
                    rotation = Mathf.Atan2((center - startPos).normalized.y, (center - startPos).normalized.x);
                    break;
                case SpawnMode.RandomCircle:
                    startPos = center + Random.insideUnitCircle * settings[activeSettingIndex].height * 0.49f;
                    rotation = randomAngle;
                    break;
            }

            agents[i] = new Agent {position = startPos, rotation = rotation};
        }

        // Init renderTextures
        _trailMap = new RenderTexture(settings[activeSettingIndex].width, settings[activeSettingIndex].height, 32)
            {enableRandomWrite = true, filterMode = filterMode};
        _diffusedTrailMap = new RenderTexture(_trailMap) {enableRandomWrite = true, filterMode = filterMode};
        _displayTexture = new RenderTexture(settings[activeSettingIndex].width, settings[activeSettingIndex].height, 32)
            {enableRandomWrite = true, filterMode = filterMode};

        // Init masks
        _attractantMask = settings[activeSettingIndex].attractantMask == null
            ? Texture2D.blackTexture
            : settings[activeSettingIndex].attractantMask;
        _repellentMask = settings[activeSettingIndex].repellentMask == null
            ? Texture2D.blackTexture
            : settings[activeSettingIndex].repellentMask;
        _obstacleMask = settings[activeSettingIndex].obstacleMask == null
            ? Texture2D.blackTexture
            : settings[activeSettingIndex].obstacleMask;

        // Init computeBuffers
        _agentBuffer = new ComputeBuffer(settings[activeSettingIndex].width * settings[activeSettingIndex].height,
            sizeof(float) * 3);

        // Init settings
        ComputeUtils.SetSettings(compute, settings[activeSettingIndex]);

        // Init updateKernel
        compute.SetTexture(_updateKernel, "trailMap", _trailMap);
        compute.SetTexture(_updateKernel, "attractantMask", _attractantMask);
        compute.SetTexture(_updateKernel, "repellentMask", _repellentMask);
        compute.SetTexture(_updateKernel, "obstacleMask", _obstacleMask);

        _agentBuffer.SetData(agents);
        compute.SetBuffer(_updateKernel, "agents", _agentBuffer);

        // Init diffuseKernel
        compute.SetTexture(_diffuseKernel, "trailMap", _trailMap);
        compute.SetTexture(_diffuseKernel, "diffusedTrailMap", _diffusedTrailMap);
        compute.SetTexture(_updateKernel, "attractantMask", _attractantMask);
        compute.SetTexture(_updateKernel, "repellentMask", _repellentMask);
        compute.SetTexture(_diffuseKernel, "obstacleMask", _obstacleMask);

        // Init renderKernel
        compute.SetTexture(_renderKernel, "trailMap", _trailMap);
        compute.SetTexture(_updateKernel, "attractantMask", _attractantMask);
        compute.SetTexture(_updateKernel, "repellentMask", _repellentMask);
        compute.SetTexture(_renderKernel, "obstacleMask", _obstacleMask);
        compute.SetTexture(_renderKernel, "displayTexture", _displayTexture);
    }

    private void RunShader()
    {
        compute.SetFloat("deltaTime", Time.fixedDeltaTime);
        compute.SetFloat("time", Time.fixedTime);

        ComputeUtils.Dispatch(compute, settings[activeSettingIndex].numAgents, kernelIndex: _updateKernel);
        ComputeUtils.Dispatch(compute, settings[activeSettingIndex].width, settings[activeSettingIndex].height, 1,
            _diffuseKernel);

        ComputeUtils.CopyRenderTexture(_diffusedTrailMap, _trailMap);
    }

    public struct Agent
    {
        public Vector2 position;
        public float rotation;
    }
}