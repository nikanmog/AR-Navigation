﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Wire Transparent" {
    Properties{
        _Color("Main Color (RGBA)", Color) = (1, 1, 1, 0.5)
    }
        SubShader{
            Tags { "Queue" = "Transparent" }
            Pass {
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                half4 _Color;

                struct v2f {
                    float4 pos : SV_POSITION;
                };

                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    return o;
                }

                half4 frag(v2f i) : COLOR
                {
                    return _Color;
                }

                ENDCG
            }
    }
        FallBack Off
}