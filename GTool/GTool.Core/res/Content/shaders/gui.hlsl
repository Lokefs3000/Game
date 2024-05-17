struct i2v
{
	float2 Position : POSITION;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
};

struct v2f
{
	float4 Position : SV_POSITION;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
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

	return output;
}

sampler PrimarySampler : register(s0);
Texture2D PrimaryTexture : register(t0);

float4 pixel(v2f input) : SV_TARGET
{
	return input.Color;
	//return input.Color * PrimaryTexture.Sample(PrimarySampler, input.UV);
}

##float2 : POSITION
##float2 : TEXCOORD0
##byte4 : COLOR