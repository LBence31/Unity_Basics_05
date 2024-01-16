using UnityEngine;

public class GPUGraph : MonoBehaviour
{
	public enum TransitionMode { Cycle, Random }

	[SerializeField]
	ComputeShader computeShader;

	[SerializeField, Range(10, 200)]
	int resolution = 10;

	[SerializeField]
	FunctionLibrary.FunctionName function;

	[SerializeField]
	TransitionMode transitionMode;

	[SerializeField, Min(0f)]
	float functionDuration = 1f, transitionDuration = 1f;

	ComputeBuffer positionsBuffer;
	float duration;
	bool transitioning;
	FunctionLibrary.FunctionName transitionFunction;

	static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
		resolutionId = Shader.PropertyToID("_Resolution"),
		stepId = Shader.PropertyToID("_Step"),
		timeId = Shader.PropertyToID("_Time");

	void OnEnable()
	{
		positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4); // count = amount of elements (cubes), stride = size of an element (cubes is float3, so 3*4 bytes)
	}

	void OnDisable()
	{
		positionsBuffer.Release();
		positionsBuffer = null;
	}

	void Update()
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
			PickNextFunction();
		}

		UpdateFunctionOnGPU();
	}

	void PickNextFunction()
	{
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary.GetNextFunctionName(function) :
			FunctionLibrary.GetRandomFunctionNameOtherThan(function);
	}

	void UpdateFunctionOnGPU()
	{
		float step = 2f / resolution;
		computeShader.SetInt(resolutionId, resolution);
		computeShader.SetFloat(stepId, step);
		computeShader.SetFloat(timeId, Time.time);

		computeShader.SetBuffer(0, positionsId, positionsBuffer);

		// Here if res=200, sending 25*25*1 = 625 groups. And group thread size is defined by numthreads (which is (8, 8, 1) = 64). So number of all threads is 64 * 625 = 40000, meaning 40000 cubes/points
		// Also means that there will be 625 groups that will run after each other, running 64 threads in pararell. So 64 points will be calcualted pararell 625 times.
		int groups = Mathf.CeilToInt(resolution / 8f);
		computeShader.Dispatch(0, groups, groups, 1);
	}

}
