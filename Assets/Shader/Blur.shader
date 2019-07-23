Shader "PostEffect/Blur"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _BlurOffset("Blur Offset", Range(0, 0.5)) = 0.05
    }

    SubShader
    {
        Pass
        {
                ZTest Always Cull Off ZWrite Off
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "Assets/Shader/PostEffectDeafult.cginc"

            float _BlurOffset;
            fixed4 ProcessColor(v2f IN)
			{          
                float3x3 boxFilter = 
                {
                    1.0f/9, 1.0f/9, 1.0f/9, 
                    1.0f/9, 1.0f/9, 1.0f/9, 
                    1.0f/9, 1.0f/9, 1.0f/9, 
                };
                
                return filter(boxFilter,_BlurOffset, _MainTex,IN.uv);
			}
           
            ENDCG
        }
    }
    Fallback Off
}
