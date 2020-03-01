Shader "Hidden/EditorGUI/BattleMapEditorShader"
{
    Properties
    {
        _MainTex ("-", 2D) = "white" {}
		_Color ("-", Color) = (1,1,1,1)
		_SrcBlend ("-", Float) = 1.0
		_DstBlend ("-", Float) = 0.0
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

            struct appdata
            {
                float4 vertex : POSITION;
				half4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				half4 color : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
