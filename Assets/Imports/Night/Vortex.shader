Shader "Skybox/HyperspeedStarfield"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex ("Cubemap (HDR)", Cube) = "grey" {}

        // Propriétés pour les étoiles et l'effet hyperspeed
        _StarDensity ("Star Density", Range(1, 20)) = 10
        _StarSpeed ("Star Speed", Range(0, 10)) = 1
        _RotateSpeed ("Vortex Rotate Speed", Range(0, 10)) = 0.5
        _StarLayers ("Star Layers", Range(1, 5)) = 3
        _StarSize ("Star Size", Range(0.1, 1)) = 0.5
        _StarBrightness ("Star Brightness", Range(0, 5)) = 1
        _StarStretch ("Star Stretch Max", Range(1, 20)) = 5
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            float4 RotateAroundYInDegrees (float4 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
                return o;
            }

            samplerCUBE _Tex;
            half4 _Tint;
            half _Exposure;
            float _Rotation;

            // Propriétés pour les étoiles et l'effet hyperspeed
            float _StarLayers;
            float _StarDensity;
            float _StarSpeed;
            float _RotateSpeed;
            float _StarSize;
            float _StarBrightness;
            float _StarStretch;

            // Fonctions optimisées
            float2x2 Rot(float a) {
                float s = sin(a), c = cos(a);
                return float2x2(c, -s, s, c);
            }

            float Star(float2 uv, float size, float stretch) {
                float rad = length(uv);
                if (rad == 0.0) return 0.0;
                float2 norm_rad = uv / rad;
                float2 norm_tan = float2(-norm_rad.y, norm_rad.x);
                float proj_rad = dot(uv, norm_rad);
                float proj_tan = dot(uv, norm_tan);
                float d = sqrt(proj_rad * proj_rad + (proj_tan * stretch) * (proj_tan * stretch)); // Étirement radial (allongé dans la direction radiale)
                float m = (0.02 / d) * size;
                m *= smoothstep(0.1, 0.01, d); // Point arrondi avec cutoff
                return m;
            }

            float Hash21(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float3 StarLayer(float2 uv, float t, float density, float size, float stretch) {
                float3 col = 0;
                float2 gv = frac(uv * density) - 0.5;
                float2 id = floor(uv * density);
                int neighborRange = 1; // Gardé à 1 pour 9 cellules, mais si trop lourd, réduire à 0 (seulement centre, mais tiling visible)
                for (int y = -neighborRange; y <= neighborRange; y++) {
                    for (int x = -neighborRange; x <= neighborRange; x++) {
                        float2 offs = float2(x, y);
                        float r = Hash21(id + offs);
                        float starSize = (1.0 - 0.5 * frac(r * 345.32)) * size;
                        float2 pos = gv - offs - float2(r, frac(r * 34.)) + 0.5;
                        float star = Star(pos, starSize, stretch);
                        // Enlevé le twinkle pour optimisation
                        float3 color = sin(float3(0.2, 0.3, 0.9) * frac(r * 34.)) * 0.5 + 0.5;
                        color = lerp(float3(1,1,1), color, 0.2); // Moins de couleur pour plus blanc
                        col += star * starSize * color;
                    }
                }
                return col;
            }

            float3 StarField(float2 uv, float t, int layers, float density, float speed, float rotateSpeed, float size, float maxStretch) {
                float3 col = 0;
                uv = mul(Rot(t * rotateSpeed), uv); // Rotation pour vortex
                for (int i = 0; i < layers; i++) {
                    float depth = frac(float(i) / float(layers) + t * speed);
                    float scale = lerp(20., 0.5, depth); // Zoom de loin à près
                    float fade = depth * smoothstep(1., 0.9, depth); // Fade in/out
                    float stretch = lerp(1.0, maxStretch, depth); // Plus d'étirement pour les étoiles "proches" (plus rapides)
                    col += StarLayer(uv * scale + float(i) * 453.2, t, density, size, stretch) * fade;
                }
                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.viewDir);
                dir = RotateAroundYInDegrees(float4(dir, 1), _Rotation).xyz;

                // Sample base cubemap
                half4 tex = texCUBE(_Tex, dir);
                half4 c = tex * _Tint;
                c.rgb *= _Exposure;

                // Calcul UV projeté aligné avec camera forward
                float3 camForward = -UNITY_MATRIX_V[2].xyz;
                float3 camRight = UNITY_MATRIX_V[0].xyz;
                float3 camUp = UNITY_MATRIX_V[1].xyz;
                float dotF = dot(dir, camForward);
                if (dotF < -0.9) return c; // Évite artifacts derrière
                float2 uv = float2(dot(dir, camRight), dot(dir, camUp)) / (dotF + 1.1); // Projection stéréographique approx

                // Ajoute les étoiles avec effet hyperspeed
                float3 stars = StarField(uv, _Time.y * 0.1, _StarLayers, _StarDensity, _StarSpeed * 10, _RotateSpeed * 0.1, _StarSize, _StarStretch);
                stars *= _StarBrightness;

                c.rgb += stars;

                return c;
            }
            ENDCG
        }
    }
}
