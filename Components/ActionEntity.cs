using System;
using System.Collections.Generic;
using System.Reflection;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Tools;

namespace MountainMeadowEngine.Components {
  
  public class ActionEntity : GameObjectComponent {
    Dictionary<ActionEvent.Values, ObjectAction> boundObjectActions = new Dictionary<ActionEvent.Values, ObjectAction>();

    ObjectAction currentAction;
    ActionEvent.Values currentActionName;
    ActionEvent.Values? previousActionName;

    public ActionEntity(GameObject context) : base(context) {
      EventManager.AddEventListener<ActionEvent>(this);
      EventManager.AddEventListener<AnimationEvent>(this);
    }

    public ActionEntity BindObjectAction(ActionEvent.Values actionName, ObjectAction objectAction) {
      boundObjectActions.Add(actionName, objectAction);
      return this;
    }

    public override void Initialize() { }

    protected void ProcessActionEvent(GameEvent gameEvent, bool nextAction = false) {
      if (gameEvent is ActionEvent || gameEvent.GetType().IsSubclassOf(typeof(ActionEvent))) {
        ActionEvent.Values value = (ActionEvent.Values)gameEvent.GetValue();

        if (boundObjectActions.ContainsKey(value)) {
          if (boundObjectActions[value].Run(currentActionName, previousActionName, nextAction, gameEvent)) {
            previousActionName = currentActionName;
            currentAction = boundObjectActions[value];
            currentActionName = value;
          }
        }
      }
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      if (this.GetContext().GetStatus() != GameObject.ObjectStatuses.IDLE && this.GetContext().GetStatus() != GameObject.ObjectStatuses.INACTIVE) {
        ProcessActionEvent(gameEvent);
      }

      if (gameEvent is AnimationEvent && currentAction != null) {
        if (((AnimationEvent)gameEvent).GetAnimationType() == currentAction.GetAnimationType()) {
          if ((AnimationEvent.Values)gameEvent.GetValue() == AnimationEvent.Values.STOPPED) {
            ActionEvent nextAction = currentAction.AnimationStopped(previousActionName);
            if (nextAction != null) {
              ProcessActionEvent(nextAction, true);
            }
          } 
        }  
      }

      return gameEvent;
    }

    public override void Dispose() {
      base.Dispose();
      foreach (ObjectAction action in boundObjectActions.Values) {
        action.Dispose();
      }
      boundObjectActions.Clear();
      currentAction = null;
    }
  }
}
