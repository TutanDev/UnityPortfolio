using System.Collections;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [Header("Draw Settings")]
    [SerializeField]
    Transform pointPrefab = default;
    [SerializeField, Range(10,100)]
    int resolution = 60;

    [Header("Graph Control")]
    [SerializeField]
    FunctionName function;
    [SerializeField]
    TransitionMode transitionMode = TransitionMode.None;
    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;


    Transform[] points;
    Vector3[] initialPos;
    float duration;
    bool transition => transitionMode != TransitionMode.None;
    bool transitioning;
    FunctionName transitionFunction;

    private void Awake()
    {
        CreatePointGrid();
        StartCoroutine(UpdatekDuration());
    }

    private void Update()
    {
        if (!transition)
        {
            UpdateFunction();
            return;
        }

        if (transitioning)
        {
            Morph();
        }
        else
        {
            UpdateFunction();
        }
    }

    IEnumerator UpdatekDuration()
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
                if(transition)
                    PickNextFunction();
            }

            yield return null;
        }
    }

    void CreatePointGrid()
    {
        float step = 2f / resolution; // pinto de -1 a 1, por eso el 2
        var scale = Vector3.one * step;

        points = new Transform[resolution * resolution];
        initialPos = new Vector3[resolution * resolution];
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
            }
            Transform point = Instantiate(pointPrefab);
            //i + 0.5 para que en el 1º punto el grosor del cubo no salga por la izquierda, -1 porque si no iria de 0 a 2
            initialPos[i].x = (x + 0.5f) * step - 1f;
            initialPos[i].y = 0;
            initialPos[i].z = (z + 0.5f) * step - 1f;
            point.localPosition = initialPos[i];
            point.localScale = scale;
            point.SetParent(transform, false);
            points[i] = point;
        }
    }

    void UpdateFunction()
    {
        FunctionLibrary.Function func = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f / resolution;

        for (int i = 0; i < points.Length; i++)
        {
            points[i].localPosition = func(initialPos[i].x, initialPos[i].z, time);
        }

        //Calcular las posiciones niciales cada frame en vez de cachearlas --> cambia donde coloca los puntos
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            v = (z + 0.5f) * step - 1f;
            points[i].localPosition = func(u, v, time);
        }
    }

    void Morph()
    {
        FunctionLibrary.Function from = FunctionLibrary.GetFunction(transitionFunction),
                                   to = FunctionLibrary.GetFunction(function);

        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2f / resolution;

        for (int i = 0; i < points.Length; i++)
        {
            points[i].localPosition = FunctionLibrary.Morph(initialPos[i].x, initialPos[i].z, time, from, to, progress);
        }
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
}
