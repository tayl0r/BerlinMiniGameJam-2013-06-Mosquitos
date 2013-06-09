using UnityEngine;
using System.Collections.Generic;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics;
using FVector2 = Microsoft.Xna.Framework.FVector2;

public class FsDeflect : MonoBehaviour {
	public float Force = 55f;
	private FSBody _body;
	//private List<Contact> _lastContacts;

	void Start() {
		_body = GetComponent<FSBodyComponent>().PhysicsBody;
		//_lastContacts = new List<Contact>();
		_body.OnCollision += OnCollisionEvent;
	}
	
	private bool OnCollisionEvent(FSFixture fixtureA, FSFixture fixtureB, Contact contact) {
		FSBody bodyB = fixtureB.Body;
		//if (bodyB.UserTag == "Respawn") {
		FVector2 normal;
		FarseerPhysics.Common.FixedArray2<FVector2> contactPoints;
		contact.GetWorldManifold(out normal, out contactPoints);
		bodyB.ApplyLinearImpulse(normal * (-1 * Force));
		//}
		return true;
	}
}
