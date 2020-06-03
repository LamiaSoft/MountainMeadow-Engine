using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Tools;

namespace MountainMeadowEngine.Components {

  public class FixedPositionObject : GameObjectComponent {
    bool currentlyPressed = false;

    public FixedPositionObject(GameObject context) : base(context) {
      EventManager.AddEventListener<InputEvent>(this);
    }

    public override void Initialize() {
      
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      Vector2 coordinates;
      List<CollisionArea> collisionAreas;

      switch ((InputEvent.Values)gameEvent.GetValue()) {
        case InputEvent.Values.MOUSE_LEFT_DOWN:
        case InputEvent.Values.MOUSE_RIGHT_DOWN:
        case InputEvent.Values.MOUSE_MIDDLE_DOWN:
          coordinates = ((InputEvent)gameEvent).GetCoordinates();
          collisionAreas = context.GetComponents<CollisionArea>();

          for (int i = 0; i < collisionAreas.Count; i++) {
            if (collisionAreas[i].rectangle.Contains(coordinates.X, coordinates.Y, false)) {
              context.OnEvent(GameEvent.Create<HUDEvent>(HUDEvent.Values.PRESSED, this).SetCoordinates(coordinates));
              currentlyPressed = true;
              return null;
            }
          }
          break;
        case InputEvent.Values.MOUSE_DRAGGED:
          if (currentlyPressed) {
            coordinates = ((InputEvent)gameEvent).GetCoordinates();
            context.OnEvent(GameEvent.Create<HUDEvent>(HUDEvent.Values.DRAGGED, this).SetCoordinates(coordinates));
            return null;
          }
          break;
        case InputEvent.Values.MOUSE_LEFT_UP:
        case InputEvent.Values.MOUSE_RIGHT_UP:
        case InputEvent.Values.MOUSE_MIDDLE_UP:
          if (currentlyPressed) {
            coordinates = ((InputEvent)gameEvent).GetCoordinates();
            context.OnEvent(GameEvent.Create<HUDEvent>(HUDEvent.Values.RELEASED, this).SetCoordinates(coordinates));
            currentlyPressed = false;
          }
          break;
      }

      return gameEvent;
    }


  }
}
