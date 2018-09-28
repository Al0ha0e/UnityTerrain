// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/TerrainShader"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
		_Position("Position", Vector) = (0, 0, 0, 1.0)// w is the size of the chunk
	}
	SubShader 
	{
		Pass 
		{ 
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#include "Lighting.cginc"
			StructuredBuffer<float3> Vertices;
			StructuredBuffer<float3> Edges;		
			StructuredBuffer<float3> Normals;
			fixed4 _Diffuse;
			float4 _Position;
			uint GetEdge(uint3 pos);
			struct v2g
			{
				float3 vertex : TEXCOORD1;
			};
			struct g2f 
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
			};
			v2g vert(uint id : SV_VertexID)  
			{
				v2g o;
				o.vertex = Vertices[id];
				return o;
			}
			[maxvertexcount(3)]
			void geom(
						point v2g gin[1],
						inout TriangleStream<g2f> triStream
					 )
			{
				uint3 Table[12] =
				{
					{2,1,0},
					{2,2,1},
					{2,1,2},
					{2,0,1},
					{0,1,0},
					{0,2,1},
					{0,1,2},
					{0,0,1},
					{1,0,0},
					{1,2,0},
					{1,2,2},
					{1,0,2}
				};
				float3 pos = floor(gin[0].vertex/100.0f);
				float3 edge  = gin[0].vertex - pos * 100.0f;
				uint pos1;
				//float3 normal = mul(cross(pos2-pos1,pos3-pos2),(float3x3)unity_WorldToObject);
				//_Position.w /= 16.0f;
				g2f o1;
				pos1 = GetEdge(pos * 2 + Table[(int)edge[0]]);
				//o1.pos = UnityObjectToClipPos((Edges[pos1] + _Position.xyz - float3(1.0f, 1.0f, 1.0f)) * _Position.w);
				o1.pos = UnityObjectToClipPos(((Edges[pos1] - float3(1.0f, 1.0f, 1.0f))/16.0f + _Position.xyz ) * _Position.w);
				pos1 = pos1 << 2;
				o1.worldNormal = mul((Normals[pos1] + Normals[pos1 + 1] + Normals[pos1 + 2] + Normals[pos1 + 3]) , (float3x3)unity_WorldToObject);
				triStream.Append(o1);
				pos1 = GetEdge(pos * 2 + Table[(int)edge[1]]);
				//o1.pos = UnityObjectToClipPos((Edges[pos1] + _Position.xyz - float3(1.0f, 1.0f, 1.0f)) * _Position.w);
				o1.pos = UnityObjectToClipPos(((Edges[pos1] - float3(1.0f, 1.0f, 1.0f)) / 16.0f + _Position.xyz) * _Position.w);
				pos1 = pos1 << 2;
				o1.worldNormal = mul((Normals[pos1] + Normals[pos1 + 1] + Normals[pos1 + 2] + Normals[pos1 + 3]) , (float3x3)unity_WorldToObject);
				triStream.Append(o1);
				pos1 = GetEdge(pos * 2 + Table[(int)edge[2]]);
				//o1.pos = UnityObjectToClipPos((Edges[pos1] + _Position.xyz - float3(1.0f, 1.0f, 1.0f)) * _Position.w);
				o1.pos = UnityObjectToClipPos(((Edges[pos1] - float3(1.0f, 1.0f, 1.0f)) / 16.0f + _Position.xyz) * _Position.w);
				pos1 = pos1 << 2;
				o1.worldNormal = mul((Normals[pos1] + Normals[pos1 + 1] + Normals[pos1 + 2] + Normals[pos1 + 3]) , (float3x3)unity_WorldToObject);
				triStream.Append(o1);
				triStream.RestartStrip();
			}
			
			fixed4 frag(g2f i) : SV_Target 
			{
				// Get ambient term
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				// Get the normal in world space
				fixed3 worldNormal = normalize(i.worldNormal);
				// Get the light direction in world space
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
				// Compute diffuse term
				//fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * (dot(worldNormal, worldLightDir) * 0.5 + 0.5);
				fixed3 color = ambient + diffuse;
				return fixed4(color, 1.0);
			}
			uint GetEdge(uint3 pos)
			{
				uint dx = pos.x & 1;
				uint dz = pos.z & 1;
				uint ret = dz * 12996 + dx * 6498;
				dx = 19 - dx;
				dz = 19 - dz;
				return ret + (pos.y >> 1) * dz * dx + (pos.x >> 1) * dz + (pos.z >> 1);
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
