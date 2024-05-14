struct i2v
{
	float2 Position : POSITION;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
}

struct v2f
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
}

v2f vertex(i2v input)
{
	v2f output;
	output.Position = float4(input.Position, 0.0, 1.0);
	output.Color = input.Color;
	output.UV = input.UV;

	return output;
}

sampler PrimarySampler : register(s0);
Texture2D PrimaryTexture : register(t0);

float4 pixel(v2f input) : SV_TARGET
{
	return input.Color * PrimaryTexture.sample(PrimarySampler, input.UV);
}