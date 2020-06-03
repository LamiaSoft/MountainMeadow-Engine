using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Behavior;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Managers;

namespace MountainMeadowEngine.Components {

  public class GameObject : IEventListener, IDisposable {
    public enum ObjectStatuses { ACTIVE, INACTIVE, STANDBY, IDLE };
    int id;

    Dictionary<Type, List<GameObjectComponent>> components = new Dictionary<Type, List<GameObjectComponent>>();
    MovableEntity movableEntity;
    Vector3 prevPosition, position;
    Vector2 drawPosition = new Vector2();
    ObjectStatuses status = ObjectStatuses.STANDBY;
    Type sceneContextType;

    public GameObject groundedOn;
    public CollisionArea groundedOnArea;
    public float groundedOffset;

    public GameObject() {
      EventManager.AddEventListener<UpdateEvent>(this);
      EventManager.AddEventListener<CollisionEvent>(this);
    }

    public int GetId() {
      return id;
    }

    public void PushEventToComponents(GameEvent gameEvent) {
      List<GameObjectComponent> _gameComponents = GetComponents();
      for (int i = 0; i < _gameComponents.Count; i++) {
        _gameComponents[i].OnEvent(gameEvent);
      }
    }

    public void AddComponent(GameObjectComponent component) {
      if (component.GetType() == typeof(MovableEntity)) {
        if (movableEntity != null) {
          throw new Exception("Cannot add more than one MovableEntity component to a GameObject.");
        }
        movableEntity = (MovableEntity)component;
        return;
      }
      if (component.GetType() == typeof(DirectionalCollisionArea)) {
        if (components.ContainsKey(typeof(DirectionalCollisionArea))) {
          throw new Exception("Cannot add more than one DirectionalCollisionArea component to a GameObject.");
        }
      }

      if (!components.ContainsKey(component.GetType())) {
        components.Add(component.GetType(), new List<GameObjectComponent>());
      }
      components[component.GetType()].Add(component);
    }

    public static T Create<T>(int id, float x, float y, float z, Type sceneContextType, ObjectStatuses status = ObjectStatuses.IDLE) {
      Type t = typeof(T);

      if (t.IsSubclassOf(typeof(GameObject))) {


        ConstructorInfo ctor = t.GetConstructor(new Type[] { });
        Tools.Debug.Json(t.GetConstructors());
        T instance = (T)ctor.Invoke(new object[] { });

        
        ((GameObject)(object)instance).id = id;
        ((GameObject)(object)instance).sceneContextType = sceneContextType;
        ((GameObject)(object)instance).status = status;
        ((GameObject)(object)instance).prevPosition = new Vector3(x, y, z);
        ((GameObject)(object)instance).SetPosition(x, y, z);
        ((GameObject)(object)instance).drawPosition = new Vector2(x, y + z);
        ((GameObject)(object)instance).Initialize();
       
        return instance;
      } else {
        throw new Exception("Cannot create GameObject of Type \"" + t.Name +"\".");
      }
    }

    public virtual void Initialize() { }

    public void Reset(int id, float x, float y, float z, Type sceneContextType, ObjectStatuses status = ObjectStatuses.IDLE) {
      DisposeComponents();

      this.id = id;
      this.sceneContextType = sceneContextType;
      this.status = status;
      this.prevPosition = new Vector3(x, y, z);
      this.SetPosition(x, y, z);
      this.drawPosition = new Vector2(x, y + z);

      this.groundedOn = null;
      this.groundedOnArea = null;
      this.groundedOffset = 0;

      Initialize();
    }

    public virtual void Activate() {
      status = ObjectStatuses.ACTIVE;
    }

    public virtual void Deactivate() {
      status = ObjectStatuses.INACTIVE;
    }

    public virtual void Standby() {
      status = ObjectStatuses.STANDBY;  
    }

    public virtual void SetIdle() {
      status = ObjectStatuses.IDLE;
    }

    public Vector3 GetPrevPosition() {
      return prevPosition;
    }

    public Vector3 GetPosition() {
      return position;
    }

    public Vector2 GetDrawPosition() {
      return drawPosition;
    }

    public void SetPosition(float? x, float? y, float? z, bool setPrevPosition = false) {
      bool update = false;

      if (setPrevPosition) {
        prevPosition = position;  
      }

      if (x != null) {
        position.X = (float)x;
        update = true;
      }
      if (y != null) {
        position.Y = (float)y;
        update = true;
      }
      if (z != null) {
        position.Z = (float)z;
        update = true;
      }

      if (update) {
        PushEventToComponents(GameEvent.Create<PositionEvent>(PositionEvent.Values.UPDATE, this));
      }
    }

    public ObjectStatuses GetStatus() {
      return status;
    }

    public void SetStatus(ObjectStatuses status) {
      bool push = (status != this.status);
      this.status = status;
      if (push)
        EventManager.PushEvent(GameEvent.Create<ObjectEvent>(ObjectEvent.Values.STATUS_CHANGE, this, sceneContextType).SetGameObjectId(id).SetStatus(status));
    }

    public Type GetSceneContextType() {
      return sceneContextType;
    }
    
    public MovableEntity GetMovableEntity() {
      return movableEntity;
    }

    public List<GameObjectComponent> GetComponents() {
      List<GameObjectComponent> gameComponents = new List<GameObjectComponent>();
      foreach (List<GameObjectComponent> list in components.Values) {
        for (int i = 0; i < list.Count; i++) {
          gameComponents.Add(list[i]);
        }
      }
      return gameComponents;
    }

    public List<T> GetComponents<T>() {
      List<T> list = new List<T>();

      if (components.ContainsKey(typeof(T))) {
        for (int i = 0; i < components[typeof(T)].Count; i++) {
          list.Add((T)(object)components[typeof(T)][i]);
        }
      }
      return list;
    }

    protected virtual void DrawUpdate(GameTime gameTime, double timeSinceLastUpdate, ObjectManager objectManager) {
      drawPosition.X = (float)((prevPosition.X + (position.X - prevPosition.X) * (timeSinceLastUpdate / GameWorld.UPDATE_STEP)));
      drawPosition.Y = (float)((prevPosition.Y + (position.Y - prevPosition.Y) * (timeSinceLastUpdate / GameWorld.UPDATE_STEP)));
      drawPosition.Y += (float)((prevPosition.Z + (position.Z - prevPosition.Z) * (timeSinceLastUpdate / GameWorld.UPDATE_STEP)));
    }

    protected void HandleCollisionEvent(ref GameEvent gameEvent) {
      if (gameEvent is CollisionEvent) {
        if (!((CollisionEvent)gameEvent).IsSolved()) {

          if ((CollisionEvent.Values)gameEvent.GetValue() == CollisionEvent.Values.ENTER) {
            if (((CollisionEvent)gameEvent).GetObject1() == this) {
              Collision col = new Collision(((CollisionEvent)gameEvent).GetContactPoints1(), ((CollisionEvent)gameEvent).GetContactPoints2(),
                                            ((CollisionEvent)gameEvent).GetCollisionArea1(), ((CollisionEvent)gameEvent).GetCollisionArea2());
              ((CollisionEvent)gameEvent).SetImpact(col.GetImpact());

              bool solved = OnCollisionEnter(((CollisionEvent)gameEvent).GetObject2(), ((CollisionEvent)gameEvent).GetContactPoints1(), ((CollisionEvent)gameEvent).GetContactPoints2(),
                                             ((CollisionEvent)gameEvent).GetCollisionArea1(), ((CollisionEvent)gameEvent).GetCollisionArea2());
              ((CollisionEvent)gameEvent).SetCollisionSolved(solved);
            } else if (((CollisionEvent)gameEvent).GetObject2() == this) {
              Collision col = new Collision(((CollisionEvent)gameEvent).GetContactPoints2(), ((CollisionEvent)gameEvent).GetContactPoints1(),
                                            ((CollisionEvent)gameEvent).GetCollisionArea2(), ((CollisionEvent)gameEvent).GetCollisionArea1());
              ((CollisionEvent)gameEvent).SetImpact(col.GetImpact());

              bool solved = OnCollisionEnter(((CollisionEvent)gameEvent).GetObject1(), ((CollisionEvent)gameEvent).GetContactPoints2(), ((CollisionEvent)gameEvent).GetContactPoints1(),
                                             ((CollisionEvent)gameEvent).GetCollisionArea2(), ((CollisionEvent)gameEvent).GetCollisionArea1());
              ((CollisionEvent)gameEvent).SetCollisionSolved(solved);
            }

          } else {
            if (((CollisionEvent)gameEvent).GetObject1() == this) {
              OnCollisionExit(((CollisionEvent)gameEvent).GetObject2(), ((CollisionEvent)gameEvent).GetCollisionArea2());
            } else if (((CollisionEvent)gameEvent).GetObject2() == this) {
              OnCollisionExit(((CollisionEvent)gameEvent).GetObject1(), ((CollisionEvent)gameEvent).GetCollisionArea1());
            }
          }

         
        }
      }
    }

    protected void HandleUpdateEvent(ref GameEvent gameEvent) {
      if (gameEvent is UpdateEvent) {
        switch ((UpdateEvent.Values)gameEvent.GetValue()) {
          case UpdateEvent.Values.OBJECT_MOVEMENT:
            if (GetStatus() != ObjectStatuses.IDLE) {
              SetPosition(null, null, null, true);
            }
            break;
          case UpdateEvent.Values.PHYSICS:
            if (GetStatus() != ObjectStatuses.IDLE) {
              if (groundedOn != null)
                SetPosition(position.X + (groundedOn.GetPosition().X - groundedOn.GetPrevPosition().X),
                            position.Y + (groundedOn.GetPosition().Y - groundedOn.GetPrevPosition().Y), null);
            }
            break;
          case UpdateEvent.Values.POST_PHYSICS:
            if (GetStatus() != ObjectStatuses.IDLE) {
              ((UpdateEvent)gameEvent).GetObjectManager().UpdateObject(id);
            }
            break;
          case UpdateEvent.Values.OBJECT_RENDERING:
            DrawUpdate(((UpdateEvent)gameEvent).GetGameTime(), ((UpdateEvent)gameEvent).GetTimeSinceLastUpdate(), ((UpdateEvent)gameEvent).GetObjectManager());
            break;
          case UpdateEvent.Values.RENDERING:
            ((UpdateEvent)gameEvent).GetObjectManager().UpdateObjectRendering(id);
            break;
        }
      }
    }

    public virtual GameEvent OnEvent(GameEvent gameEvent) {
      if ((GetStatus() != ObjectStatuses.ACTIVE && GetStatus() != ObjectStatuses.IDLE) ||
          (SceneManager.GetSceneStatus(sceneContextType) == GameScene.SceneStatuses.INACTIVE || SceneManager.SceneIsPaused(sceneContextType))) {
        return gameEvent;
      }

      HandleUpdateEvent(ref gameEvent);
      HandleCollisionEvent(ref gameEvent);
      return gameEvent;
    }

    public virtual bool OnCollisionEnter(GameObject gameObject, List<Tuple<int, Vector3>> contactPoints1, List<Tuple<int, Vector3>> contactPoints2, CollisionArea collisionArea1, CollisionArea collisionArea2) {
      if (!collisionArea1.ShouldResolve())
        return false;

      Collision collision = new Collision(contactPoints1, contactPoints2, collisionArea1, collisionArea2);

      if (collision.GetImpact().Z < 0) {
        if (groundedOn == null || groundedOffset < collisionArea1.GetOffset().Z) {
          groundedOn = gameObject;
          groundedOnArea = collisionArea2;
          groundedOffset = collisionArea1.GetOffset().Z;
        }
      }

      if (groundedOn != gameObject || groundedOnArea != collisionArea2 || groundedOffset != collisionArea1.GetOffset().Z || collisionArea2.rectangle.HasZSlope()) {
        collision.Solve(this);
        return true;
      } else {
        SetPosition(null, null, collisionArea2.rectangle.Top - groundedOffset - 1);
        GetMovableEntity().Stop(false, false, true, false);
        return true;
      }
    }

    public virtual void OnCollisionExit(GameObject gameObject, CollisionArea collisionArea) {
      if (gameObject == groundedOn && collisionArea == groundedOnArea) {
        groundedOn = null;
        groundedOnArea = null;
      }
    }

    protected void RemoveComponent<T>(int index = 0) {
      if (components.ContainsKey(typeof(T))) {
        if (index < components[typeof(T)].Count) {
          components[typeof(T)].RemoveAt(index);
          if (components[typeof(T)].Count == 0) {
            components.Remove(typeof(T));
          }
        }
      }
    }

    protected void DisposeComponents() {
      if (movableEntity != null) {
        movableEntity.Dispose();
        movableEntity = null;
      }

      foreach (Type compKey in components.Keys) {
        for (int i = 0; i < components[compKey].Count; i++) {
          components[compKey][i].Dispose();
        }
      }
      components.Clear();
    }

    public void Dispose() {
      groundedOn = null;
      groundedOnArea = null;
      EventManager.RemoveEventListener(this);
      DisposeComponents();
    }
  }
}
