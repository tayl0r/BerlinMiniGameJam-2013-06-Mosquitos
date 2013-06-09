/*
* FarseerUnity based on Farseer Physics Engine port:
* Copyright (c) 2012 Gabriel Ochsenhofer https://github.com/gabstv/Farseer-Unity3D
* 
* Original source Box2D:
* Copyright (c) 2011 Ian Qvist http://farseerphysics.codeplex.com/
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/
using UnityEngine;
using System.Collections;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

[AddComponentMenu("FarseerUnity/Dynamics/Body Component")]
public class FSBodyComponent : MonoBehaviour
{
	public FSBody body;
	
	public BodyType Type = BodyType.Dynamic;

	protected bool initialized = false;

	public enum UpdateType { Update, FixedUpdate, LateUpdate }
	public UpdateType UpdateMode;
	
	public virtual void Start ()
	{
		if(initialized)
			return;
		initialized = true;
		//Body = BodyFactory.CreateRectangle(FSWorldComponent.PhysicsWorld, 1f, 1f, Density);
		body = new FSBody(FSWorldComponent.PhysicsWorld);
		FSShapeComponent[] shapecs = GetComponentsInChildren<FSShapeComponent>();
		//print("shapes " + name + ": " + shapecs.Length);
		foreach(FSShapeComponent shp in shapecs) {
			FSFixture fixture = body.CreateFixture(shp.GetShape());
			fixture.Friction = shp.Friction;
			fixture.Restitution = shp.Restitution;
			if(shp.tag.Length > 0) fixture.UserTag = shp.tag;

			if(shp.CollisionFilter == CollisionGroupDef.Manually) {
				fixture.CollisionCategories = shp.BelongsTo;
				fixture.CollidesWith = shp.CollidesWith;
			}
			else if(shp.CollisionFilter == CollisionGroupDef.PresetFile) {
				if(shp.CollisionGroup != null) {
					fixture.CollisionCategories = shp.CollisionGroup.BelongsTo;
					fixture.CollidesWith = shp.CollisionGroup.CollidesWith;
				}
			}
		}
		// try to get a single shape at the same level
		// if theres no children
		if(shapecs.Length < 1) {
			var shape = GetComponent<FSShapeComponent>();
			if(shape != null) {
				var fixture = body.CreateFixture(shape.GetShape());
				fixture.Friction = shape.Friction;
				fixture.Restitution = shape.Restitution;
				if(shape.tag.Length > 0)
					fixture.UserTag = shape.tag;
				if (shape.CollisionFilter == CollisionGroupDef.Manually) {
					fixture.CollisionCategories = shape.BelongsTo;
					fixture.CollidesWith = shape.CollidesWith;
				}
				else if(shape.CollisionFilter == CollisionGroupDef.PresetFile) {
					if (shape.CollisionGroup != null) {
						fixture.CollisionCategories = shape.CollisionGroup.BelongsTo;
						fixture.CollidesWith = shape.CollisionGroup.CollidesWith;
					}
				}
			}
		}
		
		body.BodyType = Type;
		body.Position = new FVector2(transform.position.x, transform.position.y);
		body.Rotation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
		if(this.tag.Length > 0)
			body.UserTag = this.tag;
		body.UserFSBodyComponent = this;
	}
	
	// Update is called once per frame
	void Update () {
		if (UpdateMode == UpdateType.Update) CopyBodyTransformData();
	}

	void FixedUpdate() {
		if (UpdateMode == UpdateType.Update) CopyBodyTransformData();
	}

	void LateUpdate() {
		if (UpdateMode == UpdateType.LateUpdate) CopyBodyTransformData();
	}

	private void CopyBodyTransformData( ) {
		Vector3 pos = transform.position;
		pos.x = body.Position.X;
		pos.y = body.Position.Y;
		Vector3 rot = transform.rotation.eulerAngles;
		rot.z = body.Rotation * Mathf.Rad2Deg;
		transform.position = pos;
		transform.rotation = Quaternion.Euler (rot);
	}

	public void ApplyTransformToBody(Transform thisTransform) {
		body.Position = new FVector2(thisTransform.position.x, thisTransform.position.y);
		body.Rotation = thisTransform.rotation.z;
	}

	protected virtual void OnDestroy()
	{
		// destroy this body on Farseer Physics
		FSWorldComponent.PhysicsWorld.RemoveBody(PhysicsBody);
	}
	
	public FSBody PhysicsBody {
		get {
			if(!initialized)
				Start();
			return body;
		}
	}
}
