using System.Collections;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    [Header("Function Library")]
    [SerializeField] ComputeShader computeShader   = default;

    [Header("Draw Settings")]
    [SerializeField] Material material = default;
    [SerializeField] Mesh mesh = default;
    const int maxResolution = 1000;
    [SerializeField, Range(10, maxResolution)]
    int resolution = 300;

    [Header("Graph Control")]
    [SerializeField] FunctionName function;
    [SerializeField] TransitionMode transitionMode = TransitionMode.None;
    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;


    float duration;
    bool transitioning;
    bool transition => transitionMode != TransitionMode.None;
    FunctionName transitionFunction;

    ComputeBuffer positionsBuffer;
    static readonly int positionsId          = Shader.PropertyToID("_Positions"),
                        resolutionId         = Shader.PropertyToID("_Resolution"),
                        stepId               = Shader.PropertyToID("_Step"),
                        timeId               = Shader.PropertyToID("_Time"),
                        transitionProgressId = Shader.PropertyToID("_TransitionProgres");




    private void Awake() => StartCoroutine(UpdateDuration());
    private void OnEnable() => positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
    private void OnDisable() => ReleaseBuffer();

    private void Update() => UpdateFunctionOnGPU();

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        if (transitioning)
        {
            computeShader.SetFloat(transitionProgressId, Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        }

        var kernelIndex = (int)function + (int)(transitioning ? transitionFunction : function) * FLUtils.numberOdFunctions;
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    IEnumerator UpdateDuration()
    {
        while (true)
        {
            duration += Time.deltaTime;

            if (transitioning)
            {
                if (duration >= transitionDuration)
                {
                    duration -= transitionDuration;
                    transitioning = false;
                }
            }
            else if (duration >= functionDuration)
            {
                duration -= functionDuration;
                transitioning = true;
                transitionFunction = function;
                if (transition)
                    PickNextFunction();
            }

            yield return null;
        }
    }

    void PickNextFunction()
    {
        function = GetNextFunctionName(function);

    }

    public static FunctionName GetNextFunctionName(FunctionName name)
    {
        return (FunctionName)(((int)name + 1) % FLUtils.numberOdFunctions);
    }

    void ReleaseBuffer()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }
}
