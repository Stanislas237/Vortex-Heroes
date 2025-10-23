Shader "Custom/LaserImpactMetal"
{
    Properties
    {
        _Color("Color", Color) = (1, 0.7, 0.2, 1)
        _EmissionStrength("Emission", Range(0, 20)) = 10
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _EmissionStrength;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // dégradé circulaire : étincelle plus brillante au centre
                float2 uv = IN.uv - 0.5;
                float dist = length(uv) * 2.0;
                float intensity = saturate(1.0 - dist * dist);
                float glow = intensity * _EmissionStrength;

                return half4(_Color.rgb * glow, intensity);
            }
            ENDHLSL
        }
    }
}
