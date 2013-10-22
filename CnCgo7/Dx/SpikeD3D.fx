matrix World;
matrix View;
matrix Projection;

struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR0;
};

PS_IN VS( VS_IN input )
{	
    PS_IN output;
 
    input.pos = mul( input.pos, View ); 
    input.pos = mul( input.pos, World );
    output.pos = mul( input.pos, Projection );
    
    output.col = input.col;
 
    return output;
}

float4 PS( PS_IN input ) : SV_Target
{
    return input.col;
}

technique10 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}