using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MountainMeadowEngine.Cameras;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Managers {

  public class InputManager : IDisposable, IEventListener {
    GameCamera HUDCam;
    KeyboardState keyboardState;
    List<Keys> keysPressed = new List<Keys>();
    TouchCollection touchCollection;
    MouseState mouseState;
    List<Vector2?> mouseCoordinates = new List<Vector2?>();

    Vector2 mainCamPosition = new Vector2(0, 0);
    Vector2 currentCoordinates = new Vector2(0, 0);

    int scrollValue = 0;
    int horizontalScrollValue = 0;

    public InputManager() {
      mouseCoordinates.Add(null);
      mouseCoordinates.Add(null);
      mouseCoordinates.Add(null);
      EventManager.AddEventListener<ViewportEvent>(this, ViewportEvent.Values.CAMERA_MOVED);
    }

    public void SetHUDCam(GameCamera camera) {
      HUDCam = camera;
    }

    public void Dispose() {
      EventManager.RemoveEventListener(this);
    }


    public void Update(GameTime gameTime) {
      keyboardState = Keyboard.GetState();
      List<Keys> currentKeys = keyboardState.GetPressedKeys().ToList();

      foreach (Keys key in currentKeys) {
        if (!keysPressed.Contains(key)) {
          keysPressed.Add(key);
        }
        EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.KEY_DOWN, this).SetKey(key));
      }

      foreach (Keys key in keysPressed) {
        if (!currentKeys.Contains(key)) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.KEY_UP, this).SetKey(key));
        }
      }

      keysPressed = currentKeys;

      touchCollection = TouchPanel.GetState();
      foreach (TouchLocation tl in touchCollection) {
        currentCoordinates = HUDCam.Unproject(tl.Position.X, tl.Position.Y);

        switch (tl.State) {
          case TouchLocationState.Pressed:
            EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.TOUCH_DOWN, this)
              .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
            break;
          case TouchLocationState.Moved:
            EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.TOUCH_MOVED, this)
              .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
            break;
          default:
          case TouchLocationState.Released:
            EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.TOUCH_UP, this)
              .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
            break;

        }
      }

      MouseState currentMouseState = Mouse.GetState();

      if (currentMouseState.LeftButton == ButtonState.Pressed) {
        currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);
        
        if (mouseState.LeftButton == ButtonState.Released) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_LEFT_DOWN, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        } else if (currentCoordinates.X != ((Vector2)mouseCoordinates[0]).X || currentCoordinates.Y != ((Vector2)mouseCoordinates[0]).Y) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_DRAGGED, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        }
        mouseCoordinates[0] = currentCoordinates;
      } else {
        if (mouseState.LeftButton == ButtonState.Pressed) {
          currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_LEFT_UP, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        }
      }

      if (currentMouseState.MiddleButton == ButtonState.Pressed) {
        currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);

        if (mouseState.MiddleButton == ButtonState.Released) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_MIDDLE_DOWN, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        } else if (currentCoordinates.X != ((Vector2)mouseCoordinates[1]).X || currentCoordinates.Y != ((Vector2)mouseCoordinates[1]).Y) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_DRAGGED, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        }
        mouseCoordinates[1] = currentCoordinates;
      } else {
        if (mouseState.MiddleButton == ButtonState.Pressed) {
          currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_MIDDLE_UP, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        }
      }

      if (currentMouseState.RightButton == ButtonState.Pressed) {
        currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);

        if (mouseState.RightButton == ButtonState.Released) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_RIGHT_DOWN, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        } else if (currentCoordinates.X != ((Vector2)mouseCoordinates[2]).X || currentCoordinates.Y != ((Vector2)mouseCoordinates[2]).Y) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_DRAGGED, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        }
        mouseCoordinates[2] = currentCoordinates;
      } else {
        if (mouseState.RightButton == ButtonState.Pressed) {
          currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_RIGHT_UP, this)
            .SetCoordinates(currentCoordinates).SetCamCoordinates(currentCoordinates += mainCamPosition));
        }
      }

      if (mouseState.ScrollWheelValue != scrollValue) {
        EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_SCROLL, this).SetScrolledValue(mouseState.ScrollWheelValue - scrollValue));
        scrollValue = mouseState.ScrollWheelValue;
      }

      if (mouseState.HorizontalScrollWheelValue != horizontalScrollValue) {
        EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_HORIZONTAL_SCROLL, this).SetScrolledValue(mouseState.HorizontalScrollWheelValue - horizontalScrollValue));
        horizontalScrollValue = mouseState.HorizontalScrollWheelValue;
      }

      mouseState = currentMouseState;
    }

    public GameEvent OnEvent(GameEvent gameEvent) {
      Console.WriteLine("ON EVENT IN INPUTMANAGER!");
      Console.WriteLine(gameEvent.GetValue());

      if (gameEvent is ViewportEvent && (ViewportEvent.Values)gameEvent.GetValue() == ViewportEvent.Values.CAMERA_MOVED) {
        if (((ViewportEvent)gameEvent).GetCameraIndex() == 0) {
          Console.WriteLine(((ViewportEvent)gameEvent).GetCameraPosition());
          mainCamPosition = ((ViewportEvent)gameEvent).GetCameraPosition();
        }
      }

      return gameEvent;
    }
  }
}
