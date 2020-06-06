using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Objects;

namespace MountainMeadowEngine.Components {


  public class ClickableEntity : GameObjectComponent {
    Vector2 coordinates;
    bool currentlyPressed = false;

    public ClickableEntity(GameObject context) : base(context) {
      EventManager.AddEventListener<InputEvent>(this);
    }

    public override void Initialize() { }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      

      switch ((InputEvent.Values)gameEvent.GetValue()) {
        case InputEvent.Values.MOUSE_LEFT_DOWN:
        case InputEvent.Values.MOUSE_RIGHT_DOWN:
        case InputEvent.Values.MOUSE_MIDDLE_DOWN:
	case InputEvent.Values.TOUCH_DOWN:
          coordinates = ((InputEvent)gameEvent).GetCamCoordinates();
          Tuple<Vector2, Vector2> renderOffsets = ObjectManager.GetRenderOffset(context);


          if (coordinates.X >= renderOffsets.Item1.X + context.GetDrawPosition().X && coordinates.X <= renderOffsets.Item2.X + context.GetDrawPosition().X &&
            coordinates.Y >= renderOffsets.Item1.Y + context.GetDrawPosition().Y && coordinates.Y <= renderOffsets.Item2.Y + context.GetDrawPosition().Y) {
	 Console.WriteLine("YEP!!!");
            EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.OBJECT_TOUCHED, this).SetTouchedObject(context));
            currentlyPressed = true;
            return null;
          } else {
	 Console.WriteLine("NOPE!!!");	
Console.WriteLine(coordinates);
Console.WriteLine(renderOffsets);
}
          break;
        //case InputEvent.Values.TOUCH_MOVED:
        //  if (currentlyPressed) {
        //    coordinates = ((InputEvent)gameEvent).GetCoordinates();
        //    context.OnEvent(GameEvent.Create<HUDEvent>(HUDEvent.Values.DRAGGED, this).SetCoordinates(coordinates));
        //    return null;
        //  }
        //  break;
        //case InputEvent.Values.MOUSE_LEFT_UP:
        //case InputEvent.Values.MOUSE_RIGHT_UP:
        //case InputEvent.Values.MOUSE_MIDDLE_UP:
        //  if (currentlyPressed) {
        //    coordinates = ((InputEvent)gameEvent).GetCoordinates();
        //    context.OnEvent(GameEvent.Create<HUDEvent>(HUDEvent.Values.RELEASED, this).SetCoordinates(coordinates));
        //    currentlyPressed = false;
        //  }
        //  break;
      }

      return gameEvent;
    }


    
  }
}