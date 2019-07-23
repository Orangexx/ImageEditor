#include "UnityCG.cginc"

struct v2a
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
};
struct v2f
{
	float2 uv : TEXCOORD0;
	float4 pos : SV_POSITION;
};
v2f vert (v2a v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.texcoord;
	return o;
}

// 用于计算的纹理 (UIImage 的 Sprite)
sampler2D _MainTex;


		
fixed4 ProcessColor(v2f IN);

// 片元着色器
fixed4 frag(v2f IN) : SV_Target
{				
	// 生成灰度颜色(不用考虑  lerp 只看 float4里的就好
	fixed4 color = ProcessColor(IN);
			
	return color;
}

// 各种自定义方法

	// 3x3 滤波
    float4 filter(float3x3 filter,float _BlurOffset, sampler2D tex, float2 coord)
    {
                float4 outCol = float4(0,0,0,0);
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        //计算采样点，得到当前像素附近的像素的坐标
                        float2 newUV= float2(coord.x + (i-1)*_BlurOffset, coord.y + (j-1)*_BlurOffset);
                        //采样并乘以滤波器权重，然后累加
                        outCol += tex2D(tex, newUV) * filter[i][j];
						//是否处理透明度
						//outCol.a = tex2D(tex, coord).a;
                    } 
                }
                return outCol;
    }
