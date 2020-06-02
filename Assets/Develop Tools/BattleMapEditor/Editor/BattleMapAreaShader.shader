﻿Shader "Hidden/EditorGUI/BattleMapEditorShader"
{
    Properties
    {
        _MainTex ("-", 2D) = "white" {}
		_Color ("-", Color) = (1,1,1,1)
		_SrcBlend ("-", Float) = 1.0
		_DstBlend ("-", Float) = 0.0
        _ColorIn ("-", Color) = (1,1,1,1)
        _ColorOut ("-", Color) = (1,0,0,0)
        _Bounds ("-", Vector) = (0,0,50,50)
    }
    SubShader
    {
        Tags {  }
		
		//0 : vertex color pass
        Pass
        {
			Blend [_SrcBlend] [_DstBlend]
			ZWrite off
			ZTest off
			Cull off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
				half4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
				half4 color : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.uv);
                return color * i.color;
            }
            ENDCG
        }

        //0 : area cull Pass
        Pass
        {
			Blend [_SrcBlend] [_DstBlend]
			ZWrite off
			ZTest off
			Cull off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _ColorIn;
            float4 _ColorOut;
            float4 _Bounds;

            struct appdata
            {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
				half4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
				half4 color : TEXCOORD1;
                float2 location : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.color = v.color;
                o.location = v.vertex.xy;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 colorIn = _ColorIn * i.color;
                half4 colorOut = _ColorOut * i.color;
                half4 inOrOut = half4(i.location - _Bounds.xy, _Bounds.zw - i.location);
                inOrOut = saturate(sign(inOrOut)); //1: in 0: out;
                half4 finalColor = lerp(colorOut, colorIn, inOrOut.x * inOrOut.y * inOrOut.z * inOrOut.w);
                return finalColor * tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
