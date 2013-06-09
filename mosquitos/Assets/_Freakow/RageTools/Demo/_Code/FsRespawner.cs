using UnityEngine;
using System.Collections.Generic;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics;
using FVector2 = Microsoft.Xna.Framework.FVector2;

public class FsRespawner : MonoBehaviour {
	public Transform RespawnPosition;
	private FSBody _body;
	//private List<Contact> _lastContacts;

	void Start() {
		_body = GetComponent<FSBodyComponent>().PhysicsBody;
		//_lastContacts = new List<Contact>();
		_body.OnCollision += OnCollisionEvent;
		//var shape = GetComponent<FSShapeComponent>();
	}
	
	private bool OnCollisionEvent(FSFixture fixtureA, FSFixture fixtureB, Contact contact) {
		FSBody bodyB = fixtureB.Body;
		//if (bodyB.UserTag == "Respawn") {
		FVector2 normal;
		FarseerPhysics.Common.FixedArray2<FVector2> contactPoints;
		contact.GetWorldManifold(out normal, out contactPoints);
		bodyB.SetTransform(FSHelper.Vector3ToFVector2(RespawnPosition.position), 0f);
		bodyB.ResetDynamics();
		//}
		return true;
	}
}
