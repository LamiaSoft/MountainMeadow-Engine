using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {

  public class CollisionEvent : GameEvent {

    public enum Values {
      ENTER, EXIT 
    };

    GameObject object1, object2;
    List<Tuple<int, Vector3>> contactPoints1, contactPoints2;
    bool collisionSolved = false;
    CollisionArea collisionArea1, collisionArea2;
    Vector3 impact = new Vector3();

    public CollisionEvent SetObjects(GameObject object1, GameObject object2) { this.object1 = object1; this.object2 = object2; return this; }
    public CollisionEvent SetContactPoints(List<Tuple<int, Vector3>> contactPoints1, List<Tuple<int, Vector3>> contactPoints2) { 
      this.contactPoints1 = contactPoints1;
      this.contactPoints2 = contactPoints2;
      return this;
    }
    public CollisionEvent SetCollisionAreas(CollisionArea collisionArea1, CollisionArea collisionArea2) { this.collisionArea1 = collisionArea1; this.collisionArea2 = collisionArea2; return this; }
    public CollisionEvent SetCollisionSolved(bool solved) { this.collisionSolved = solved; return this; }
    public CollisionEvent SetImpact(Vector3 impact) { this.impact = impact; return this; }

    public GameObject GetObject1() { return object1; }
    public GameObject GetObject2() { return object2; }
    public List<Tuple<int, Vector3>> GetContactPoints1() { return contactPoints1; }
    public List<Tuple<int, Vector3>> GetContactPoints2() { return contactPoints2; }
    public bool IsSolved() { return collisionSolved; }
    public CollisionArea GetCollisionArea1() { return collisionArea1; }
    public CollisionArea GetCollisionArea2() { return collisionArea2; }
    public Vector3 GetImpact() { return impact; }
  }

}
