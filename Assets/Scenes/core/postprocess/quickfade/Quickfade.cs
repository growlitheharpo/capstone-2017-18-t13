using System;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(QuickfadeRenderer), PostProcessEvent.AfterStack, "Custom/Quickfade")]
public sealed class Quickfade : PostProcessEffectSettings
{
	public FloatParameter time = new FloatParameter { value = 0.1f };
	public ColorParameter color = new ColorParameter { value = Color.black };

	public FloatParameter blend = new FloatParameter { value = 0.0f };

	public void Activate(MonoBehaviour runner, bool autoReverse = true)
	{
		runner.StartCoroutine(Coroutines.InvokeEveryTick((currentTime) =>
		{
			if (currentTime < time.value)
			{
				blend.Override(currentTime / time.value);
				return true;
			}

			if (autoReverse)
				Deactivate(runner);

			return false;
		}));
	}

	public void Deactivate(MonoBehaviour runner)
	{
		runner.StartCoroutine(Coroutines.InvokeEveryTick((currentTime) =>
		{
			if (currentTime < time.value)
			{
				blend.Override(1.0f - (currentTime / time));
				return true;
			}

			return false;
		}));
	}
}

public sealed class QuickfadeRenderer : PostProcessEffectRenderer<Quickfade>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Quickfade"));
		sheet.properties.SetFloat("_Blend", settings.blend);
		context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
	}
}
