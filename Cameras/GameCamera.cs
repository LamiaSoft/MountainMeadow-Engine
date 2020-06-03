using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Tools;
//using MountainMeadowEngine.Objects;
using MountainMeadowEngine.ViewportAdapters;

namespace MountainMeadowEngine.Cameras {

  public class GameCamera {
    Dictionary<Type, List<GameCameraComponent>> components = new Dictionary<Type, List<GameCameraComponent>>();

		protected Camera2D camera;
		protected Matrix viewMatrix;
		protected bool matrixLoaded = false;
    protected Vector2 position = new Vector2(0, 0), prevPosition = new Vector2(0, 0);
    protected GameObject followedObject;

    protected List<Tuple<Vector2, float, bool>> scenario = new List<Tuple<Vector2, float, bool>>();
    protected int scenarioIndex = -1;
    protected bool scenarioRemoveAfterFinish = true;

    protected Vector2? tweenDistance, startPos;
    protected float tweenDuration;
    protected double accumulator = 0;

		public GameCamera(GraphicsDevice graphicsDevice, int viewPortWidth, int viewPortHeight) {
			ScalingViewportAdapter adapter = new ScalingViewportAdapter(graphicsDevice, viewPortWidth, viewPortHeight);
			camera = new Camera2D(adapter);
    }
    		
    public void AddComponent(GameCameraComponent component) {
      if (!components.ContainsKey(component.GetType())) {
        components.Add(component.GetType(), new List<GameCameraComponent>());
      }
      components[component.GetType()].Add(component);
    }

    public List<GameCameraComponent> GetComponents() {
      List<GameCameraComponent> gameComponents = new List<GameCameraComponent>();
      foreach (List<GameCameraComponent> list in components.Values) {
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

    public void FollowObject(GameObject gameObject) {
      followedObject = gameObject;
    }

    public void Update(GameTime gameTime) {
      
    }

    public void LaunchScenario(bool removeAfterFinish = true) {
      if (scenario.Count > 0) {
        scenarioIndex = 0;
        this.scenarioRemoveAfterFinish = removeAfterFinish;
      }
    }

    public void AddScenario(Vector2 destination, float duration, bool relativeDestination = false) {
      if (scenarioIndex == -1)
        scenario.Add(Tuple.Create(destination, duration, relativeDestination));
    }

    public void Tween(Vector3 destination, float duration, bool relativeDestination = false) {
      Tween(new Vector2(destination.X, destination.Y), duration, relativeDestination);
    }

    public void Tween(Vector2 destination, float duration, bool relativeDestination = false) {
      startPos = position;
      if (!relativeDestination)
        tweenDistance = destination - position;
      else 
        tweenDistance = (position + destination) - position;

      tweenDuration = duration;
      accumulator = 0;
    }

    public virtual void DrawUpdate(GameTime gameTime, double timeSinceLastUpdate) {
      
      if (followedObject != null) {
        position.X = followedObject.GetDrawPosition().X - (ViewportManager.GAME_VIEWPORT.X / 2);
        position.Y = followedObject.GetDrawPosition().Y - (ViewportManager.GAME_VIEWPORT.Y / 2);
       } else if (scenarioIndex == 0 && tweenDistance == null) {
        Tween(scenario[0].Item1, scenario[0].Item2, scenario[0].Item3);
      }

      if (tweenDistance != null) {
        accumulator += gameTime.ElapsedGameTime.TotalSeconds;
        if (accumulator < tweenDuration) {
          double ratio = accumulator / tweenDuration;
          position = (Vector2)startPos + (((Vector2)tweenDistance) * (float)ratio);
        } else {
          position = (Vector2)startPos + (Vector2)tweenDistance;

          if (scenarioIndex > -1) {
            if (scenarioIndex + 1 < scenario.Count) {
              scenarioIndex++;
              Tween(scenario[scenarioIndex].Item1, scenario[scenarioIndex].Item2, scenario[scenarioIndex].Item3);
            } else {
              if (scenarioRemoveAfterFinish) {
                scenario.Clear();
              }
              scenarioIndex = -1;
              accumulator = 0;
              tweenDuration = 0;
              tweenDistance = null;
            } 
          } else {
            accumulator = 0;
            tweenDuration = 0;
            tweenDistance = null;
          }

        }
      }

      prevPosition = camera.Position;

      if (position.X != camera.Position.X || position.Y != camera.Position.Y) {
        camera.Position = new Vector2(position.X, position.Y);
        matrixLoaded = false;

      } 

      foreach (GameCameraComponent comp in GetComponents()) {
        comp.DrawUpdate(gameTime);
      }
    }

		public Matrix GetViewMatrix() {
      if (matrixLoaded == false) {
				viewMatrix = camera.GetViewMatrix();
				matrixLoaded = true;
			}
			return viewMatrix;
		}

		public Vector2 Unproject(float x, float y) {
			return camera.ScreenToWorld(x, y);
		}

    public Vector2 GetPrevPosition() {
      return prevPosition;
    }

    public Vector2 GetPosition() {
      return position;
    }

		public void SetPositionX(float x) {
      position.X = x;
    }

		public void SetPositionY(float y) {
      position.Y = y;
    }

		public void MoveX(float x) {
      position.X += x;
		}

		public void MoveY(float y) {
      position.Y += y;
  	}

		public Camera2D GetCamera() {
			return camera;
		}

		
  }

}
