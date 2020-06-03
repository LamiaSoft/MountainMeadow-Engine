using Microsoft.Xna.Framework;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Tools;

namespace MountainMeadowEngine.Components {

  public class LiftingEntity : GameObjectComponent {
    
    GameObject touchedObject, liftedObject;
    int liftableWeight;

    public LiftingEntity(GameObject context) : base(context) {
      EventManager.AddEventListener<UpdateEvent>(this, UpdateEvent.Values.POST_COLLISION);
      EventManager.AddEventListener<CollisionEvent>(this, CollisionEvent.Values.ENTER);
    }

    public void SetTouchedObject(GameObject gameObject) {
      Debug.Output(gameObject.GetType().Name);
      if (gameObject.GetComponents<Liftable>().Count > 0) {
        this.touchedObject = gameObject;
      }
    }

    public void ReleaseTouchedObject() {
      touchedObject = null;  
    }

    public void ReleasedCarriedObject() {
      liftedObject = null;
    }

    public GameObject GetTouchedObject() {
      return touchedObject;
    }

    public void LiftObject() {
      if (this.touchedObject != null) {
        this.liftedObject = this.touchedObject;
        this.touchedObject = null;
        this.liftedObject.GetComponents<Liftable>()[0].SetLiftedBy(context);
      }
    }

    public GameObject GetLiftedObject() {
      return liftedObject;
    }

    public override void Initialize() { }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      if (gameEvent is UpdateEvent) {
        if (context.GetPrevPosition() != context.GetPosition()) {
          ReleaseTouchedObject();
        }
        if (GetLiftedObject() != null) {
          GetLiftedObject().SetPosition(GetLiftedObject().GetPosition().X + (context.GetPosition().X - context.GetPrevPosition().X),
                                        GetLiftedObject().GetPosition().Y + (context.GetPosition().Y - context.GetPrevPosition().Y), null);
        }
      }

      if (gameEvent is CollisionEvent) {
        if (((CollisionEvent)gameEvent).GetObject1() == context) {
          if (GetLiftedObject() == null) {
            if (((CollisionEvent)gameEvent).GetObject2().GetComponents<Liftable>().Count > 0) {
              SetTouchedObject(((CollisionEvent)gameEvent).GetObject2());
            }
          }
        } else if (((CollisionEvent)gameEvent).GetObject2() == context) {
          if (GetLiftedObject() == null) {
            if (((CollisionEvent)gameEvent).GetObject1().GetComponents<Liftable>().Count > 0) {
              SetTouchedObject(((CollisionEvent)gameEvent).GetObject1());
            }
          }
        }
      }

      return gameEvent;
    }
  }
}
