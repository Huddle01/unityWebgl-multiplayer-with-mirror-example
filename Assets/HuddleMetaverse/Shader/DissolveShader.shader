Shader "Custom/RandomDissolveShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _DissolveColor("Dissolve Color", Color) = (1,1,1,1)
        _DissolveThreshold("Dissolve Threshold", Range(0,1)) = 0.5
        _DissolveEdgeWidth("Dissolve Edge Width", Range(0,1)) = 0.1
        _DissolveSpeed("Dissolve Speed", Range(0.1, 10.0)) = 1.0
        _NumDissolveShapes("Number of Dissolve Shapes", Range(1,10)) = 3
        _WholeMeshDissolveAmount("Whole Mesh Dissolve Amount", Range(0,1)) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _NoiseTex;
                float4 _MainTex_ST;
                float4 _DissolveColor;
                float _DissolveThreshold;
                float _DissolveEdgeWidth;
                float _DissolveSpeed;
                int _NumDissolveShapes;
                float _WholeMeshDissolveAmount;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Sample the main texture
                    fixed4 col = tex2D(_MainTex, i.uv);

                // Sample the noise texture and offset UVs
                float2 noiseUV = i.uv * _DissolveSpeed;
                float noiseValue = tex2D(_NoiseTex, noiseUV).r;

                // Calculate whole mesh dissolve factor based on _WholeMeshDissolveAmount
                float wholeMeshDissolveFactor = smoothstep(0.0, 1.0, _WholeMeshDissolveAmount);

                // Apply whole mesh dissolve
                col.rgb = lerp(_DissolveColor.rgb, col.rgb, wholeMeshDissolveFactor);
                col.a = lerp(1.0, 0.0, wholeMeshDissolveFactor);

                // Calculate dissolve factors for each shape based on noise texture
                float dissolveShapes = 0.0;
                float shapeStep = 1.0 / float(_NumDissolveShapes);
                float shapeThreshold = shapeStep;
                for (int j = 0; j < _NumDissolveShapes; ++j)
                {
                    if (noiseValue < shapeThreshold)
                    {
                        dissolveShapes = 1.0;
                        break;
                    }
                    shapeThreshold += shapeStep;
                }

                // Calculate dissolve threshold with time-based oscillation
                float dissolveThreshold = _DissolveThreshold + sin(_Time.y * _DissolveSpeed) * 0.5;

                // Calculate the final dissolve factor
                float dissolveFactor = smoothstep(dissolveThreshold - _DissolveEdgeWidth, dissolveThreshold + _DissolveEdgeWidth, noiseValue) * dissolveShapes;

                // Blend between dissolve color and the original texture color
                col.rgb = lerp(_DissolveColor.rgb, col.rgb, dissolveFactor);

                // Apply alpha based on dissolve factor
                col.a = lerp(1.0, 0.0, dissolveFactor);

                return col;
            }
            ENDCG
        }
        }
            FallBack "Diffuse"
}
