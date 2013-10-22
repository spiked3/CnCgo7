
cbuffer ConstantBuffer {
	matrix World;
	matrix View;
	matrix Projection;

	float4 LightDir[2] : POSITION;
	float4 LightColor[2] : COLOR;
	float4 OutputColor;
}

struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float3 norm : NORMAL;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR0;
	float3 norm: NORMAL;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN) 0;

	output.pos = mul(input.pos, World);
	output.pos = mul(output.pos, View);
	output.pos = mul(output.pos, Projection);
	output.col = input.col;
	output.norm = mul(float4(input.norm, 1), World).xyz;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return input.col;	// +++without lighting for now

	float4 finalColor = 0;

		//do NdotL lighting for 2 lights
	for (int i = 0; i < 2; i++)
		finalColor += saturate(dot((float3)LightDir[i], input.norm) * LightColor[i]);
	finalColor.a = 1;
	return finalColor * input.col;
}

technique10 Render
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PS()));
	}
}