Shader "2D/Light/Map"
{  
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Off
            ColorMask RGBA
            Blend DstColor Zero, Zero One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
  
 
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
            uniform sampler2D _LightMap;
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv.y = 1 - o.uv.y;
                return o;
            }
                                                    
            float4 frag(v2f IN) : COLOR
            {
                return tex2D(_LightMap, IN.uv);
            }
 
            ENDCG
        }
    }
}
