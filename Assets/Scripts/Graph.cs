using UnityEngine;

public class Graph : MonoBehaviour
{
	public enum TransitionMode { Cycle, Random }

	[SerializeField]
	Transform pointPrefab;

	[SerializeField, Range(10, 100)]
	int resolution = 10;

	[SerializeField]
	FunctionLibrary.FunctionName function;

	[SerializeField]
	TransitionMode transitionMode;

	[SerializeField, Min(0f)]
	float functionDuration = 1f, transitionDuration = 1f;


	Transform[] points;
	float duration;
	bool transitioning;
	FunctionLibrary.FunctionName transitionFunction;

	void Awake()
	{
		points = new Transform[resolution * resolution];

		float step = 2f / resolution;
		Vector3 Scale = Vector3.one * step;

		for (int i = 0; i < points.Length; i++)
		{
			Transform point = points[i] = Instantiate(pointPrefab);
			point.localScale = Scale;
			point.SetParent(transform, false);
		}
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

		if (transitioning)
		{
			UpdateFunctionTransition();
		}
		else
		{
			UpdateFunction();
		}
	}

	void PickNextFunction()
	{
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary.GetNextFunctionName(function) :
			FunctionLibrary.GetRandomFunctionNameOtherThan(function);
	}

	void UpdateFunction()
	{
		FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);  // Basically pointer function
		float step = 2f / resolution;


		float time = Time.time;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
		{
			if (x == resolution)
			{
				x = 0;
				z++;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			points[i].localPosition = f(u, v, time);
		}
	}

	void UpdateFunctionTransition()
	{
		FunctionLibrary.Function from = FunctionLibrary.GetFunction(transitionFunction);
		FunctionLibrary.Function to = FunctionLibrary.GetFunction(function);
		float progress = duration / transitionDuration;

		float step = 2f / resolution;
		float time = Time.time;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
		{
			if (x == resolution)
			{
				x = 0;
				z++;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
		}
	}

}
