using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public sealed class BattleInterferenceRendererFeature : ScriptableRendererFeature
{
    public Shader shader;
    Material material;
    InterferencePass pass;
    public override void Create()
    {
        CoreUtils.Destroy(material);
        material = shader ? CoreUtils.CreateEngineMaterial(shader) : null;
        pass = new InterferencePass { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (!Application.isPlaying || !material || data.cameraData.cameraType != CameraType.Game ||
            !data.cameraData.camera.TryGetComponent<BattleScreenInterference>(out var effect) || !effect.IsActive) return;
        pass.Setup(material, effect);
        renderer.EnqueuePass(pass);
    }
    protected override void Dispose(bool disposing) => CoreUtils.Destroy(material);

    sealed class InterferencePass : ScriptableRenderPass
    {
        Material material;
        BattleScreenInterference effect;
        public void Setup(Material mat, BattleScreenInterference state)
        {
            material = mat; effect = state;
            requiresIntermediateTexture = true;
        }
        public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer) return;
            var source = resources.activeColorTexture;
            var desc = graph.GetTextureDesc(source);
            desc.name = "Battle screen interference";
            desc.clearBuffer = false;
            var destination = graph.CreateTexture(desc);
            var properties = new MaterialPropertyBlock();
            properties.SetFloat("_GlitchLines", effect.GlitchLines);
            properties.SetFloat("_CRTAmount", effect.CRTAmount);
            properties.SetFloat("_Clock", Time.unscaledTime);
            // Blit binds the sampled source explicitly as _BlitTexture; never sample the write target.
            var blit = new RenderGraphUtils.BlitMaterialParameters(source, destination, material, 0,
                properties, 0, 0, sourceTexturePropertyID: Shader.PropertyToID("_BlitTexture"));
            graph.AddBlitPass(blit, passName: "Battle full-screen CRT and scanline tearing");
            resources.cameraColor = destination;
        }
    }
}
