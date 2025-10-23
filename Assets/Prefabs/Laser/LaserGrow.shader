Shader "Custom/LaserSimpleGlow"
{
    Properties
    {
        _Color("Laser Color", Color) = (1, 0, 0, 1)
        _EmissionStrength("Emission Strength", Range(0, 50)) = 10
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One One  // Additive blending for glow
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

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Centrer le rayon : plus lumineux au milieu, atténué sur les bords
                float distFromCenter = abs(IN.uv.x - 0.5) * 2.0; // 0 au centre, 1 sur les bords
                float intensity = saturate(1.0 - pow(distFromCenter, 3.0)); // adoucissement
                float glow = intensity * _EmissionStrength;

                return half4(_Color.rgb * glow, _Color.a);
            }
            ENDHLSL
        }
    }
}
