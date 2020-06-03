using System;
using System.Collections.Generic;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Components {

  public class InputComponent : GameObjectComponent {
    Dictionary<string, Tuple<ActionEvent, bool>> keyboardEventTriggers = new Dictionary<string, Tuple<ActionEvent, bool>>();
    Dictionary<string, Tuple<Action<GameObject>, bool>> keyboardFunctionTriggers = new Dictionary<string, Tuple<Action<GameObject>, bool>>();




    public InputComponent(GameObject context) : base(context) { 
      EventManager.AddEventListener<InputEvent>(this);
    }

    public override void Initialize() { }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      if (gameEvent is InputEvent) {




      }
      return gameEvent;
    }
  }
}
