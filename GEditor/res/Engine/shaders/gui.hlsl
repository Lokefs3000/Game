struct i2v
{
	float2 Position : POSITION;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
	float Attribute : PSIZE0;
};

struct v2f
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
	float Attribute : FOG;
};

cbuffer ConstantBuffer : register(b0)
{
	matrix sProjection;
};

v2f vertex(i2v input)
{
	v2f output;
	output.Position = mul(float4(input.Position, 0.0, 1.0), sProjection);
	output.Color = input.Color;
	output.UV = input.UV;
	output.Attribute = input.Attribute;

	return output;
}

sampler PrimarySampler : register(s0);
Texture2D PrimaryTexture : register(t0);

float4 pixel(v2f input) : SV_TARGET
{
	[forcecase] switch(input.Attribute)
	{
	case 0.0:
		return input.Color;
	case 1.0:
		return input.Color * PrimaryTexture.Sample(PrimarySampler, input.UV);
	case 2.0:
		return input.Color * float4(1.0, 1.0, 1.0, PrimaryTexture.Sample(PrimarySampler, input.UV).r);
	}

	return float4(1.0, 0.0, 1.0, 1.0);
}

##float2 : POSITION
##float2 : TEXCOORD0
##byte4 : COLOR0
##float : PSIZE0