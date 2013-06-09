using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteMask")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Sprite implementation which maintains its own Unity Mesh. Leverages dynamic batching.
/// </summary>
public class tk2dSpriteMask : tk2dBaseSprite
{
	public Mesh mesh;
	Vector3[] meshVertices;
	Vector3[] meshNormals = null;
	Vector4[] meshTangents = null;
	Color[] meshColors;
	public Texture _maskTexture;
	
	new void Awake()
	{
		base.Awake();

		// Create mesh, independently to everything else
		mesh = new Mesh();
#if UNITY_EDITOR
		mesh.name = "tk2d mesh";
#endif
		mesh.hideFlags = HideFlags.DontSave;
		GetComponent<MeshFilter>().mesh = mesh;
		
		// This will not be set when instantiating in code
		// In that case, Build will need to be called
		if (Collection)
		{
			// reset spriteId if outside bounds
			// this is when the sprite collection data is corrupt
			if (_spriteId < 0 || _spriteId >= Collection.Count)
				_spriteId = 0;
			
			Build();
		}
	}
	
	protected void OnDestroy()
	{
		if (mesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(mesh);
#else
			Destroy(mesh);
#endif
		}
		
		if (meshColliderMesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(meshColliderMesh);
#else
			Destroy(meshColliderMesh);
#endif
		}
	}
	
	void OnEnable() {
		Build();
	}
	
	public override void Build()
	{
		var sprite = collectionInst.spriteDefinitions[spriteId];

		meshVertices = new Vector3[sprite.positions.Length];
        meshColors = new Color[sprite.positions.Length];
		
		meshNormals = new Vector3[0];
		meshTangents = new Vector4[0];
		
		if (sprite.normals != null && sprite.normals.Length > 0)
		{
			meshNormals = new Vector3[sprite.normals.Length];
		}
		if (sprite.tangents != null && sprite.tangents.Length > 0)
		{
			meshTangents = new Vector4[sprite.tangents.Length];
		}
		
		SetPositions(meshVertices, meshNormals, meshTangents);
		SetColors(meshColors);
		
		if (mesh == null)
		{
			mesh = new Mesh();
#if UNITY_EDITOR
			mesh.name = "tk2d mesh";
#endif
			mesh.hideFlags = HideFlags.DontSave;
			GetComponent<MeshFilter>().mesh = mesh;
		}
		
		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.colors = meshColors;
		mesh.uv = sprite.uvs;
		mesh.triangles = sprite.indices;
		mesh.uv2 = new Vector2[] {
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		UpdateMaterial();
		CreateCollider();
	}
	
	/// <summary>
	/// Adds a tk2dSprite as a component to the gameObject passed in, setting up necessary parameters and building geometry.
	/// Convenience alias of tk2dBaseSprite.AddComponent<tk2dSprite>(...).
	/// </summary>
	public static tk2dSprite AddComponent(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		return tk2dBaseSprite.AddComponent<tk2dSprite>(go, spriteCollection, spriteId);
	}
	
	/// <summary>
	/// Create a sprite (and gameObject) displaying the region of the texture specified.
	/// Use <see cref="tk2dSpriteCollectionData.CreateFromTexture"/> if you need to create a sprite collection
	/// with multiple sprites.
	/// Convenience alias of tk2dBaseSprite.CreateFromTexture<tk2dSprite>(...)
	/// </summary>
	public static GameObject CreateFromTexture(Texture2D texture, tk2dRuntime.SpriteCollectionSize size, Rect region, Vector2 anchor)
	{
		return tk2dBaseSprite.CreateFromTexture<tk2dSprite>(texture, size, region, anchor);
	}

	protected override void UpdateGeometry() { UpdateGeometryImpl(); }
	protected override void UpdateColors() { UpdateColorsImpl(); }
	protected override void UpdateVertices() { UpdateVerticesImpl(); }
	
	
	protected void UpdateColorsImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (mesh == null || meshColors == null || meshColors.Length == 0)
			return;
#endif
		
		SetColors(meshColors);
		mesh.colors = meshColors;
	}
	
	protected void UpdateVerticesImpl()
	{
		var sprite = collectionInst.spriteDefinitions[spriteId];
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (mesh == null || meshVertices == null || meshVertices.Length == 0)
			return;
#endif
		
		// Clear out normals and tangents when switching from a sprite with them to one without
		if (sprite.normals.Length != meshNormals.Length)
		{
			meshNormals = (sprite.normals != null && sprite.normals.Length > 0)?(new Vector3[sprite.normals.Length]):(new Vector3[0]);
		}
		if (sprite.tangents.Length != meshTangents.Length)
		{
			meshTangents = (sprite.tangents != null && sprite.tangents.Length > 0)?(new Vector4[sprite.tangents.Length]):(new Vector4[0]);
		}
		
		SetPositions(meshVertices, meshNormals, meshTangents);
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.uv = sprite.uvs;
		mesh.uv2 = new Vector2[] {
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		mesh.bounds = GetBounds();
	}

	protected void UpdateGeometryImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (mesh == null)
			return;
#else
		if (mesh == null)
			Build();
#endif
		
		var sprite = collectionInst.spriteDefinitions[spriteId];
		if (meshVertices == null || meshVertices.Length != sprite.positions.Length)
		{
			meshVertices = new Vector3[sprite.positions.Length];
			meshNormals = (sprite.normals != null && sprite.normals.Length > 0)?(new Vector3[sprite.normals.Length]):(new Vector3[0]);
			meshTangents = (sprite.tangents != null && sprite.tangents.Length > 0)?(new Vector4[sprite.tangents.Length]):(new Vector4[0]);
			meshColors = new Color[sprite.positions.Length];
		}
		SetPositions(meshVertices, meshNormals, meshTangents);
		SetColors(meshColors);

		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.colors = meshColors;
		mesh.uv = sprite.uvs;
		mesh.uv2 = new Vector2[] {
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		mesh.bounds = GetBounds();
        mesh.triangles = sprite.indices;
	}
	
	Material _normalMat;
	
	public void SetMaterial(Material mat) {
		renderer.sharedMaterial = mat;
	}
	
	protected override void UpdateMaterial()
	{
	}
		//Debug.Log("update material 0");
//		Debug.Log(GetCurrentSpriteDef().name);
//		Debug.Log((mesh.uv[0] * 1000f) + " x " + (mesh.uv[1] * 1000f));
//		Debug.Log((mesh.uv[2] * 1000f) + " x " + (mesh.uv[3] * 1000f));
//		Debug.Log((GetCurrentSpriteDef().uvs[0] * 1000f) + " x " + (GetCurrentSpriteDef().uvs[1] * 1000f));
//		Debug.Log((GetCurrentSpriteDef().uvs[2] * 1000f) + " x " + (GetCurrentSpriteDef().uvs[3] * 1000f));
//		if (_normalMat != collectionInst.spriteDefinitions[spriteId].materialInst) {
//			//Debug.Log("update material 1");
//			_normalMat = collectionInst.spriteDefinitions[spriteId].materialInst;
//			if (collectionInst.maskMaterial != null) {
//				renderer.sharedMaterial = collectionInst.maskMaterial;
//			} else {
//				if (_normalMat != null) {
//					SpriteMaskHelper helper = GetComponent<SpriteMaskHelper>();
//					Material newMat = new Material(helper._shader);
//					//newMat.shader = helper._shader;
//					//newMat.mainTexture = _normalMat.mainTexture;
//					newMat.SetTexture("_MainTex", _normalMat.mainTexture);
//					newMat.SetTexture("_MaskTex", helper._mask);
//					renderer.sharedMaterial = newMat;
//					collectionInst.maskMaterial = newMat;
//					helper._mat = newMat;
//					//Debug.Log(_savedMat.mainTexture.width + "x" + _savedMat.mainTexture.height);
//				}
//			}
//		}
////		if (renderer.sharedMaterial == null && _savedMat != null) {
////			//Debug.Log("update material 4");
////			renderer.sharedMaterial = _savedMat;
////		}
//	}
	
	protected override int GetCurrentVertexCount()
	{
#if UNITY_EDITOR
		if (meshVertices == null)
			return 0;
#else
		if (meshVertices == null)
			Build();
#endif
		// Really nasty bug here found by Andrew Welch.
		return meshVertices.Length;
	}
	
	public override void ForceBuild()
	{
		base.ForceBuild();
		GetComponent<MeshFilter>().mesh = mesh;
	}
}
