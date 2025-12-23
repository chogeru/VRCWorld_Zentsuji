Shader "Custom/nikawashop_lensflare"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "gray" {}
        _Color("Color Tint", Color) = (1,1,1,1)
        _ColorRatio("Color Ratio", Range(0,3)) = 0.5
        _AlphaFlicker("Alpha Flicker Min", Range(0.5,1.0)) = 0.8
        _ScaleJitter("Scale Jitter Min", Range(0.5,1.0)) = 0.9
        _NoiseScrollSpeed("Noise Scroll Speed", Range(0.0,5.0)) = 0.5

        _OverlapNear("Overlap Show Threshold", Range(0.0,5)) = 0.02
        _OverlapFar("Overlap Fade End", Range(0.01,8)) = 0.10

        _EdgeFadeWidth("Edge Fade Width", Range(0.0,0.5)) = 0.10
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+10"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        float4 _MainTex_ST;

        float4 _Color;
        float _ColorRatio;
        float _AlphaFlicker;
        float _ScaleJitter;
        float _NoiseScrollSpeed;

        float _OverlapNear;
        float _OverlapFar;
        float _EdgeFadeWidth;

        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv  : TEXCOORD0;
            float4 screenPos : TEXCOORD1;
            half flicker : TEXCOORD2;
            half invScale : TEXCOORD3;
        };

        v2f vertCommon(appdata v)
        {
            v2f o;

            float3 worldCenter = unity_ObjectToWorld._m03_m13_m23;

            float3 camRight = unity_CameraToWorld._m00_m10_m20;
            float3 camUp    = unity_CameraToWorld._m01_m11_m21;

            float scaleX = length(unity_ObjectToWorld._m00_m10_m20);
            float scaleY = length(unity_ObjectToWorld._m01_m11_m21);

            float3 worldPos = worldCenter + (camRight * v.vertex.x * scaleX) + (camUp * v.vertex.y * scaleY);

            o.pos = UnityWorldToClipPos(worldPos);
            o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
            o.screenPos = ComputeScreenPos(o.pos);

            float2 noiseUVBase = float2(_Time.y * _NoiseScrollSpeed, _Time.y * _NoiseScrollSpeed * 0.2);
            half flickerNoise = tex2Dlod(_NoiseTex, float4(noiseUVBase,0,0)).r;

            half flicker = lerp(_AlphaFlicker, 1.0h, flickerNoise);
            half scale   = lerp(_ScaleJitter, 1.0h, flickerNoise);
            o.flicker = flicker;
            o.invScale = 1.0h / scale;

            return o;
        }

        void sampleFlare(v2f i, out half3 col, out half alpha)
        {
            half2 centered = i.uv - 0.5h;
            half2 scaledUV = centered * i.invScale + 0.5h;

            half4 tex = tex2D(_MainTex, scaledUV);

            half w = _EdgeFadeWidth;
            half fadeX = smoothstep(0.0h, w, scaledUV.x) * smoothstep(0.0h, w, 1.0h - scaledUV.x);
            half fadeY = smoothstep(0.0h, w, scaledUV.y) * smoothstep(0.0h, w, 1.0h - scaledUV.y);
            half edgeFade = fadeX * fadeY;

            half t = 1.0h - _ColorRatio * (1.0h - tex.a); 
            col = lerp(_Color.rgb, tex.rgb, saturate(t));

            alpha = tex.a * i.flicker * _Color.a * edgeFade;
        }
        ENDCG

        Pass
        {
            ZTest LEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v) { return vertCommon(v); }

            half4 frag(v2f i) : SV_Target
            {
                half3 c; half a;
                sampleFlare(i, c, a);
                if (a <= 0.001h) discard;
                return half4(c, a);
            }
            ENDCG
        }

        Pass
        {
            ZTest Greater
            CGPROGRAM
            #pragma vertex vert2
            #pragma fragment frag2

            v2f vert2(appdata v) { return vertCommon(v); }

            half4 frag2(v2f i) : SV_Target
            {
                float scene01  = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
                float sceneEye = LinearEyeDepth(scene01);
                float flareEye = LinearEyeDepth(i.screenPos.z / i.screenPos.w);

                float diff = flareEye - sceneEye;
                float overlapFactor = 1.0 - smoothstep(_OverlapNear, _OverlapFar, diff);
                if (overlapFactor <= 0.001) discard;

                half3 c; half a;
                sampleFlare(i, c, a);
                a *= (half)overlapFactor;
                if (a <= 0.001h) discard;

                return half4(c, a);
            }
            ENDCG
        }
    }
}