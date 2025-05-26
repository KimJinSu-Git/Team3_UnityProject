Shader "Custom/VideoRemoveWhiteWithAlpha"
{
    Properties
    {
        _MainTex ("Video Texture", 2D) = "white" {}
        _Threshold ("Luminance Threshold", Range(0,1)) = 0.95
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Threshold;
            float _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float luminance = (col.r + col.g + col.b) / 3.0;

                // 밝은 영역(흰색)에 대해 알파 제거
                if (luminance > _Threshold)
                {
                    col.a = 0;
                }

                // 전체 알파 적용
                col.a *= _Alpha;

                return col;
            }
            ENDCG
        }
    }
}
