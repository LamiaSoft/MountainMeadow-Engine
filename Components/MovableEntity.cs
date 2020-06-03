using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Components
{

    public class MovableEntity : GameObjectComponent {
    public enum Direction {
      NORTH, NORTH_EAST, NORTH_EAST1, NORTH_EAST2, EAST, SOUTH_EAST, SOUTH_EAST1, SOUTH_EAST2, SOUTH,
      SOUTH_WEST, SOUTH_WEST1, SOUTH_WEST2, WEST, NORTH_WEST, NORTH_WEST1, NORTH_WEST2, NONE, DEFAULT
    }

    // Current dynamics
    Vector3 velocity = new Vector3();
    Vector3 direction = new Vector3();
    Vector3 prevDirection = new Vector3();
    Vector3 currentSpeed = new Vector3();
    float currentGravity;
    Direction currentDirection = Direction.NONE;
    bool useIncreasedSpeed = false;
    bool impulsedX, impulsedY, impulsedZ;

    // Defines
    float jumpingSpeed = 0;
    float gravity = 0;
    Vector3 speed = new Vector3();
    float maxFallingSpeed = 0;
    Vector3 increasedSpeed = new Vector3();
    Vector3 accelerationFactor = new Vector3(0.45f, 0.45f, 1);
    Vector3 decelerationFactor = new Vector3(0.45f, 0.45f, 1);

    Vector3 currentFactor = new Vector3();
    Vector3 currentPosition = new Vector3();

    public MovableEntity(GameObject context) : base(context) {
      EventManager.AddEventListener<UpdateEvent>(this, UpdateEvent.Values.OBJECT_MOVEMENT);
    }

    public Vector3 GetVelocity() {
      return velocity;
    }

    public float GetJumpingSpeed() {
      return jumpingSpeed;
    }

    public void SetDirection(float? x, float? y, float? z) {
      this.prevDirection = this.direction;

      if (x != null) 
        this.direction.X = (float)x;
      if (y != null)
        this.direction.Y = (float)y;
      if (z != null)
        this.direction.Z = (float)z;

      this.currentDirection = GetDirectionEnum();
    }

    public Vector3 GetDirection() {
      return direction;
    }

    public Direction GetCurrentDirection() {
      if (currentDirection == Direction.NONE) {
        Vector3 oldDirection = direction;

        int absX = (int)Math.Abs(velocity.X), absY = (int)Math.Abs(velocity.X);
        if (absX + absY > 0) {
          if (absX >= absY) {
            direction.X = (velocity.X > 0) ? 1 : -1;
            direction.Y = (velocity.Y > 0) ? absY / absX : 0 - (absY / absX);
          } else {
            direction.Y = (velocity.Y > 0) ? 1 : -1;
            direction.X = (velocity.X > 0) ? absX / absY : 0 - (absX / absY);
          }

          Direction improvisedDirection = GetDirectionEnum();
          direction = oldDirection;
          return improvisedDirection;
        } 
      }
      return currentDirection;
    }

    private bool IsZero(float number) {
      return (number < 0.01 && number > -0.01); 
    }

    private Direction GetDirectionEnum() {
      if (IsZero(direction.X) && IsZero(direction.Y)) {
        return Direction.NONE;
      }
      if (direction.X > .1 && IsZero(direction.Y)) {
        return Direction.EAST;
      }
      if (direction.X < -.1 && IsZero(direction.Y)) {
        return Direction.WEST;
      }
      if (IsZero(direction.X) && direction.Y > .1) {
        return Direction.SOUTH;
      }
      if (IsZero(direction.X) && direction.Y < -.1) {
        return Direction.NORTH;
      }

      if (direction.X >= .6) {
        if (direction.Y >= .6) {
          return Direction.SOUTH_EAST;
        } else if (direction.Y > .1 && direction.Y < .6) {
          return Direction.SOUTH_EAST1;
        } else if (direction.Y < -.1 && direction.Y > -.6) {
          return Direction.NORTH_EAST2;
        } else {
          return Direction.NORTH_EAST;
        }
      }

      if (direction.X > .1 && direction.X < .6) {
        if (direction.Y >= .6) {
          return Direction.SOUTH_EAST2;
        } else if (direction.Y > .1 && direction.Y < .6) {
          return Direction.SOUTH_EAST;
        } else if (direction.Y < -.1 && direction.Y > -.6) {
          return Direction.NORTH_EAST;
        } else {
          return Direction.NORTH_EAST1;
        }
      }

      if (direction.X < -.1 && direction.X > -.6) {
        if (direction.Y >= .6) {
          return Direction.SOUTH_WEST2;
        } else if (direction.Y > .1 && direction.Y < .6) {
          return Direction.SOUTH_WEST;
        } else if (direction.Y < -.1 && direction.Y > -.6) {
          return Direction.NORTH_WEST;
        } else {
          return Direction.NORTH_WEST1;
        }
      }

      if (direction.X <= -.6) {
        if (direction.Y >= .6) {
          return Direction.SOUTH_WEST;
        } else if (direction.Y > .1 && direction.Y < .6) {
          return Direction.SOUTH_WEST1;
        } else if (direction.Y < -.1 && direction.Y > -.6) {
          return Direction.NORTH_WEST2;
        } else {
          return Direction.NORTH_WEST;
        }
      }

      return Direction.NONE;
    }

    public static Direction GetNearestDirection(Direction currentDirection, List<Direction> availableDirections) {
      if (availableDirections.Contains(currentDirection))
        return currentDirection;

      if (currentDirection == Direction.NORTH_EAST1 || currentDirection == Direction.NORTH_EAST2 && (availableDirections.Contains(Direction.NORTH_EAST))) {
        return Direction.NORTH_EAST;
      }
      if (currentDirection == Direction.SOUTH_EAST1 || currentDirection == Direction.SOUTH_EAST2 && (availableDirections.Contains(Direction.SOUTH_EAST))) {
        return Direction.SOUTH_EAST;
      }
      if (currentDirection == Direction.SOUTH_WEST1 || currentDirection == Direction.SOUTH_WEST2 && (availableDirections.Contains(Direction.SOUTH_WEST))) {
        return Direction.SOUTH_WEST;
      }
      if (currentDirection == Direction.NORTH_WEST1 || currentDirection == Direction.NORTH_WEST2 && (availableDirections.Contains(Direction.NORTH_WEST))) {
        return Direction.NORTH_WEST;
      }

      switch (currentDirection) {
        case Direction.NORTH_EAST:
          if (availableDirections.Contains(Direction.EAST))
            return Direction.EAST;
          return (availableDirections.Contains(Direction.NORTH)) ? Direction.NORTH : Direction.NONE;
        case Direction.NORTH_EAST1:
          if (availableDirections.Contains(Direction.NORTH))
            return Direction.NORTH;
          return (availableDirections.Contains(Direction.EAST)) ? Direction.EAST : Direction.NONE;
        case Direction.NORTH_EAST2:
          if (availableDirections.Contains(Direction.EAST))
            return Direction.EAST;
          return (availableDirections.Contains(Direction.NORTH)) ? Direction.NORTH : Direction.NONE;

        case Direction.SOUTH_EAST:
          if (availableDirections.Contains(Direction.EAST))
            return Direction.EAST;
          return (availableDirections.Contains(Direction.SOUTH)) ? Direction.SOUTH : Direction.NONE;
        case Direction.SOUTH_EAST1:
          if (availableDirections.Contains(Direction.EAST))
            return Direction.EAST;
          return (availableDirections.Contains(Direction.SOUTH)) ? Direction.SOUTH : Direction.NONE;
        case Direction.SOUTH_EAST2:
          if (availableDirections.Contains(Direction.SOUTH))
            return Direction.SOUTH;
          return (availableDirections.Contains(Direction.EAST)) ? Direction.EAST : Direction.NONE;

        case Direction.SOUTH_WEST:
          if (availableDirections.Contains(Direction.WEST))
            return Direction.WEST;
          return (availableDirections.Contains(Direction.SOUTH)) ? Direction.SOUTH : Direction.NONE;
        case Direction.SOUTH_WEST1:
          if (availableDirections.Contains(Direction.WEST))
            return Direction.WEST;
          return (availableDirections.Contains(Direction.SOUTH)) ? Direction.SOUTH : Direction.NONE;
        case Direction.SOUTH_WEST2:
          if (availableDirections.Contains(Direction.SOUTH))
            return Direction.SOUTH;
          return (availableDirections.Contains(Direction.WEST)) ? Direction.WEST : Direction.NONE;

        case Direction.NORTH_WEST:
          if (availableDirections.Contains(Direction.WEST))
            return Direction.WEST;
          return (availableDirections.Contains(Direction.NORTH)) ? Direction.NORTH : Direction.NONE;
        case Direction.NORTH_WEST1:
          if (availableDirections.Contains(Direction.NORTH))
            return Direction.NORTH;
          return (availableDirections.Contains(Direction.WEST)) ? Direction.WEST : Direction.NONE;
        case Direction.NORTH_WEST2:
          if (availableDirections.Contains(Direction.WEST))
            return Direction.WEST;
          return (availableDirections.Contains(Direction.NORTH)) ? Direction.NORTH : Direction.NONE;
      }

      return Direction.NONE;
    }

    public bool UseIncreasedSpeed() {
      return useIncreasedSpeed;
    }

    public MovableEntity SetSpeed(float xSpeed, float ySpeed, float zSpeed) {
      speed.X = xSpeed;
      speed.Y = ySpeed;
      speed.Z = zSpeed;
      currentSpeed = speed;
      return this;
    }

    public MovableEntity SetMaxFallingSpeed(float maxFallingSpeed) {
      this.maxFallingSpeed = maxFallingSpeed;
      return this;
    }

    public MovableEntity SetIncreasedSpeed(float xincreasedSpeed, float yincreasedSpeed, float zincreasedSpeed) {
      increasedSpeed.X = xincreasedSpeed;
      increasedSpeed.Y = yincreasedSpeed;
      increasedSpeed.Z = zincreasedSpeed;
      return this;
    }

    public MovableEntity SetJumpingSpeed(float jumpingSpeed) {
      this.jumpingSpeed = jumpingSpeed;
      // Speed += ((MoveDirection * MaximumSpeed) - Speed) * AccelerationFactor
      return this;
    }

    public void Jump() {
      if (HasGravity()) {
        this.currentSpeed.Z += (jumpingSpeed - speed.Z);
        this.direction.Z = -1;
      }
      // Speed += ((MoveDirection * MaximumSpeed) - Speed) * AccelerationFactor

    }

    public void Impuls(float? x, float? y, float? z) {
      if (x != null) {
        velocity.X += (float)x;
        impulsedX = true;
      }
      if (y != null) {
        velocity.Y += (float)y;
        impulsedY = true;
      }
      if (z != null) {
        if (z < 0) {
          this.currentSpeed.Z += (float)(Math.Abs((float)z) - speed.Z);
          this.direction.Z = -1;
        } else {
          this.currentSpeed.Z += (float)z;
        }
        impulsedZ = true;
      }
    }

    public MovableEntity SetGravity(float gravity) {
      this.gravity = gravity;
      if (IsZero(maxFallingSpeed))
        maxFallingSpeed = gravity * 11;
      Stop(false, false, true);
      // Speed += ((MoveDirection * MaximumSpeed) - Speed) * AccelerationFactor
      return this;
    }

    public float GetGravity() {
      return gravity;
    }

    public bool HasGravity() {
      return (gravity > 0.1);
    }

    public MovableEntity SetAccelerationFactor(float x, float y, float z) {
      accelerationFactor.X = x;
      accelerationFactor.Y = y;
      accelerationFactor.Z = z;
      return this;
    }

    public MovableEntity SetDecelerationFactor(float x, float y, float z) {
      decelerationFactor.X = x;
      decelerationFactor.Y = y;
      decelerationFactor.Z = z;
      return this;
    }

    public float GetJumpingHeight() {
      if (!HasGravity())
        return 0;
      
      float heighestZPos = 1, zPos = 0, zVel = 0, grav = 0, zSpeed = jumpingSpeed;

      while (zPos < heighestZPos) {
        heighestZPos = zPos;
        zVel += ((-1 * zSpeed) - zVel) * currentFactor.Z;
        grav += gravity;
        zVel += grav;
        zPos += zVel;
      }

      return Math.Abs(heighestZPos);
    }

    public void UseIncreasedSpeed(bool increasedSpeed) {
      useIncreasedSpeed = increasedSpeed;
      if (increasedSpeed) {
        currentSpeed = this.increasedSpeed;
      } else {
        currentSpeed = speed;
      }
    }

    public void Stop(bool x, bool y, bool z, bool setDirection = true) {
      if (x) {
        //SetDirection(0, null, null);
        velocity.X = 0;
      }
      if (y) {
        //SetDirection(null, 0, null);
        velocity.Y = 0;
      }
      if (z) {
        if (HasGravity())
          SetDirection(null, null, 0);
        velocity.Z = 0;
        currentGravity = 0;
        currentSpeed.Z = (!UseIncreasedSpeed()) ? speed.Z : increasedSpeed.Z;
      }
    }

    protected virtual void Update() {
      if (context.GetStatus() != GameObject.ObjectStatuses.ACTIVE)
        return;
      
      //currentFactor.X = (prevDirection.X == 0 || prevDirection.X == direction.X) ? accelerationFactor.X : decelerationFactor.X;
      //currentFactor.Y = (prevDirection.Y == 0 || prevDirection.Y == direction.Y) ? accelerationFactor.Y : decelerationFactor.Y;
      //currentFactor.Z = (prevDirection.Z == 0 || prevDirection.Z == direction.Z) ? accelerationFactor.Z : decelerationFactor.Z;

      currentFactor.X = (direction.X == 0)  ? decelerationFactor.X : accelerationFactor.X;
      currentFactor.Y = (direction.Y == 0) ? decelerationFactor.Y : accelerationFactor.Y;
      currentFactor.Z = (direction.Z == 0) ? decelerationFactor.Z : accelerationFactor.Z;

      //if (context is Pot) {
      //  Debug.Json("VEL: ", velocity);
      //  Debug.Json("DIR: ", direction);
      //  Debug.Json("SPEED: ", currentSpeed);
      //  Debug.Json("FACTOR: ", currentFactor);
      //  Debug.Json("IMPULSE: ", new List<bool>() { impulsedX, impulsedY, impulsedZ });
      //}

      if (context.groundedOn != null || (!impulsedX && !impulsedY && !impulsedZ)) {
        velocity += ((direction * currentSpeed) - velocity) * currentFactor;
      } else {
        if (!IsZero(direction.X)) {
          velocity.X += ((direction.X * currentSpeed.X) - velocity.X) * currentFactor.X;
        }
        if (!IsZero(direction.Y)) {
          velocity.Y += ((direction.Y * currentSpeed.Y) - velocity.Y) * currentFactor.Y;
        }
        velocity.Z += ((direction.Z * currentSpeed.Z) - velocity.Z) * currentFactor.Z;
      }


      if (velocity.X > -0.5 && velocity.X < 0.5 && direction.X == 0) {
        velocity.X = 0;
        impulsedX = false;
      }
      if (velocity.Y > -0.5 && velocity.Y < 0.5 && direction.Y == 0) {
        velocity.Y = 0;
        impulsedY = false;
      }
      if (velocity.Z > -0.5 && velocity.Z < 0.5 && direction.Z == 0) {
        velocity.Z = 0;
        impulsedZ = false;
      }

      currentGravity += gravity;
      velocity.Z += currentGravity;

      if (!IsZero(maxFallingSpeed) && velocity.Z > maxFallingSpeed) {
        if (direction.Z == -1)
          direction.Z = 0;
                 
        velocity.Z = maxFallingSpeed;
        currentGravity -= gravity;
      }

      currentPosition = context.GetPosition();
      currentPosition += velocity;

      context.SetPosition(currentPosition.X, currentPosition.Y, currentPosition.Z);
    }

    public override void Initialize() {
      
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      Update();
      return gameEvent;
    }
  }
}
