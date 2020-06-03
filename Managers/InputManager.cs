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

  public class InputManager : IDisposable {
    GameCamera HUDCam;
    KeyboardState keyboardState;
    List<Keys> keysPressed = new List<Keys>();
    TouchCollection touchCollection;
    MouseState mouseState;
    Vector2? mouseCoordinates = null;

    int scrollValue = 0;
    int horizontalScrollValue = 0;

    public void SetHUDCam(GameCamera camera) {
      HUDCam = camera;
    }







    public void Dispose() {

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
        switch (tl.State) {
          case TouchLocationState.Pressed:
            EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.TOUCH_DOWN, this).SetCoordinates(HUDCam.Unproject(tl.Position.X, tl.Position.Y)));
            break;
          case TouchLocationState.Moved:
            EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.TOUCH_MOVED, this).SetCoordinates(HUDCam.Unproject(tl.Position.X, tl.Position.Y)));
            break;
          default:
          case TouchLocationState.Released:
            EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.TOUCH_UP, this).SetCoordinates(HUDCam.Unproject(tl.Position.X, tl.Position.Y)));
            break;

        }
      }

      MouseState currentMouseState = Mouse.GetState();

      if (currentMouseState.LeftButton == ButtonState.Pressed) {
        Vector2 currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);

        if (mouseState.LeftButton == ButtonState.Released) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_LEFT_DOWN, this).SetCoordinates(currentCoordinates));
        } else if (currentCoordinates.X != ((Vector2)mouseCoordinates).X || currentCoordinates.Y != ((Vector2)mouseCoordinates).Y) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_DRAGGED, this).SetCoordinates(currentCoordinates));
        }
        mouseCoordinates = currentCoordinates;
      } else {
        if (mouseState.LeftButton == ButtonState.Pressed) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_LEFT_UP, this).SetCoordinates(HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y)));
        }
      }

      if (currentMouseState.MiddleButton == ButtonState.Pressed) {
        Vector2 currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);

        if (mouseState.MiddleButton == ButtonState.Released) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_MIDDLE_DOWN, this).SetCoordinates(currentCoordinates));
        } else if (currentCoordinates.X != ((Vector2)mouseCoordinates).X || currentCoordinates.Y != ((Vector2)mouseCoordinates).Y) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_DRAGGED, this).SetCoordinates(currentCoordinates));
        }
        mouseCoordinates = currentCoordinates;
      } else {
        if (mouseState.MiddleButton == ButtonState.Pressed) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_MIDDLE_UP, this).SetCoordinates(HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y)));
        }
      }

      if (currentMouseState.RightButton == ButtonState.Pressed) {
        Vector2 currentCoordinates = HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y);

        if (mouseState.RightButton == ButtonState.Released) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_RIGHT_DOWN, this).SetCoordinates(currentCoordinates));
        } else if (currentCoordinates.X != ((Vector2)mouseCoordinates).X || currentCoordinates.Y != ((Vector2)mouseCoordinates).Y) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_DRAGGED, this).SetCoordinates(currentCoordinates));
        }
        mouseCoordinates = currentCoordinates;
      } else {
        if (mouseState.RightButton == ButtonState.Pressed) {
          EventManager.PushEvent(GameEvent.Create<InputEvent>(InputEvent.Values.MOUSE_RIGHT_UP, this).SetCoordinates(HUDCam.Unproject(currentMouseState.Position.X, currentMouseState.Position.Y)));
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


  }
}
