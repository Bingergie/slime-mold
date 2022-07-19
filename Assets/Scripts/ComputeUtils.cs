using UnityEngine;

public static class ComputeUtils
{
    public static void Dispatch(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1,
        int kernelIndex = 0)
    {
        var threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
        var numGroupsX = Mathf.CeilToInt(numIterationsX / (float) threadGroupSizes.x);
        var numGroupsY = Mathf.CeilToInt(numIterationsY / (float) threadGroupSizes.y);
        var numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float) threadGroupSizes.y);
        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }

    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
    {
        compute.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);
        return new Vector3Int((int) x, (int) y, (int) z);
    }

    public static void CopyRenderTexture(Texture source, RenderTexture target)
    {
        Graphics.Blit(source, target);
    }

    public static void SetSettings(ComputeShader compute, SlimeSettings settings)
    {
        compute.SetInt("width", settings.width);
        compute.SetInt("height", settings.height);
        compute.SetBool("bounceOffEdge", settings.bounceOffEdge);
        compute.SetBool("allowRandom", settings.allowRandom);
        compute.SetInt("numAgents", settings.numAgents);
        
        compute.SetFloats("attractantColor", settings.attractantColor.r, settings.attractantColor.g,
            settings.attractantColor.b, settings.attractantColor.a);
        compute.SetFloat("attractantStrength", settings.attractantStrength);
        compute.SetFloats("repellentColor", settings.repellentColor.r, settings.repellentColor.g,
            settings.repellentColor.b, settings.repellentColor.a);
        compute.SetFloats("repellentStrength", settings.repellentStrength);
        compute.SetFloats("obstacleColor", settings.obstacleColor.r, settings.obstacleColor.g, settings.obstacleColor.b, settings.obstacleColor.a);
        
        compute.SetFloat("sensorAngleOffset", settings.sensorAngleOffset);
        compute.SetFloat("sensorOffsetDistance", settings.sensorOffsetDistance);
        compute.SetInt("sensorSize", settings.sensorSize);
        compute.SetFloat("rotationAngle", settings.rotationAngle);
        compute.SetInt("stepSize", settings.stepSize);
        compute.SetFloats("color", settings.color.r, settings.color.g, settings.color.b, settings.color.a);
        
        compute.SetFloat("trailWeight", settings.trailWeight);
        compute.SetFloat("decayRate", settings.decayRate);
        compute.SetFloat("diffuseRate", settings.diffuseRate);
        compute.SetFloat("trailAttractiveStrength", settings.trailAttractiveStrength);
    }
}