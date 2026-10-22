Shader "Hidden/Battle/ScreenInterference"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            float _GlitchLines, _CRTAmount, _Clock;
            float Noise(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float tick = floor(_Clock * 35.0);
                float jitter = (Noise(float2(tick,1))-.5)*.003;
                float bandMask = 0;
                if (_GlitchLines > .5)
                {
                    float d = abs(uv.y - .5 - jitter);
                    if (_GlitchLines > 1.5) d = min(abs(uv.y-.475-jitter),abs(uv.y-.525+jitter));
                    bandMask = 1-smoothstep(.0015,.005,d);
                }
                float amount = saturate(_CRTAmount);
                float grain = Noise(floor(uv * _ScreenParams.xy) + tick);
                float tear = (Noise(float2(floor(uv.y*380),tick))-.5);
                float2 sampleUV = uv;
                sampleUV.x += bandMask * tear * .10 + amount * amount * tear * .012;
                half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, saturate(sampleUV)).rgb;
                color = lerp(color, color*.45 + grain*.65, bandMask*.8);
                float luminance = dot(color,float3(.2126,.7152,.0722));
                color = lerp(color,luminance.xxx,amount*.75);
                float scan = .82 + .18*sin(uv.y*_ScreenParams.y*3.14159);
                color *= lerp(1,scan,amount);
                color += (grain-.5)*amount*.45;
                color *= 1-amount*.6*saturate(length((uv-.5)*1.5));
                // During the final part, collapse the picture to a horizontal TV bandMask, then black.
                float closing = smoothstep(.65,1,amount);
                float halfHeight = lerp(.51,0,closing);
                float visible = 1-smoothstep(halfHeight,halfHeight+.006,abs(uv.y-.5));
                color *= lerp(1,visible,step(.65,amount));
                color *= 1-smoothstep(.94,1,amount);
                return half4(max(color,0),1);
            }
            ENDHLSL
        }
    }
}

