using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Collision;
using MountainMeadowEngine.Tools;
using static MountainMeadowEngine.Collision.Rectangle3D;

namespace MountainMeadowEngine.Components {

  public class Collision {
    public enum ContactSides { TOP, BOTTOM, LEFT, RIGHT, BACK, FRONT, FULL_TOP, FULL_BOTTOM, FULL_LEFT, FULL_RIGHT, FULL_BACK, FULL_FRONT };

    List<Tuple<int, Vector3>> contactPoints1, contactPoints2;
    Vector3 impact = Vector3.Zero;
    Dictionary<ContactSides, Vector3> impactSides = new Dictionary<ContactSides, Vector3>();
    CollisionArea collisionArea1, collisionArea2;
    //List<ContactSides> contactSides1, contactSides2;

    public Dictionary<ContactSides, Vector3> GetImpactSides() {
      return impactSides;
    }

    public Collision(List<Tuple<int, Vector3>> contactPoints1, List<Tuple<int, Vector3>> contactPoints2, CollisionArea collisionArea1, CollisionArea collisionArea2) {
      this.contactPoints1 = contactPoints1;
      this.contactPoints2 = contactPoints2;
      this.collisionArea1 = collisionArea1;
      this.collisionArea2 = collisionArea2;

      List<ContactSides> contactSides1 = GetContactSides(contactPoints1);
      List<ContactSides> contactSides2 = GetContactSides(contactPoints2);

      if (collisionArea2.rectangle.HasZSlope()) {
        if (collisionArea2.rectangle.type == RectangleTypes.ZSLOPEX_DOWN || collisionArea2.rectangle.type == RectangleTypes.ZSLOPEX_UP) {
          float xPos = collisionArea1.rectangle.Left + (collisionArea1.rectangle.Length / 2);
          float absPos = xPos - collisionArea2.rectangle.Left;
          float ratio = (collisionArea2.rectangle.type == RectangleTypes.ZSLOPEX_DOWN) ? (absPos / collisionArea2.rectangle.Length) : 1 - (absPos / collisionArea2.rectangle.Length);

          if (collisionArea1.prevRectangle.Front <= collisionArea2.rectangle.Back || collisionArea1.prevRectangle.Back >= collisionArea2.rectangle.Front) {
            if (collisionArea1.rectangle.Z >= collisionArea2.rectangle.Top + (ratio * collisionArea2.rectangle.Height)) {
              GetImpact(contactSides1, collisionArea1, collisionArea2, false);
            }
          }
          if ((collisionArea2.rectangle.type == RectangleTypes.ZSLOPEX_DOWN && collisionArea1.prevRectangle.Right <= collisionArea2.rectangle.Left) ||
              (collisionArea2.rectangle.type == RectangleTypes.ZSLOPEX_UP && collisionArea1.prevRectangle.Left >= collisionArea2.rectangle.Right)) {
            GetImpact(contactSides1, collisionArea1, collisionArea2, false);
          }

          if (impactSides.Count == 0) {
            if (ratio <= 0) {
              Vector3 val = GetCalculatedValue(0, 0, (collisionArea2.rectangle.Top - collisionArea1.rectangle.Bottom - 1), false);
              impactSides.Add(ContactSides.BOTTOM, val);
            } else if (ratio >= 1) {
              if (collisionArea1.rectangle.Z >= collisionArea2.rectangle.Bottom - 1) {
                Vector3 val = GetCalculatedValue(0, 0, (collisionArea2.rectangle.Bottom - 1), false);
                impactSides.Add(ContactSides.BOTTOM, val);
              }
            } else if (collisionArea1.rectangle.Z >= collisionArea2.rectangle.Top + (ratio * collisionArea2.rectangle.Height)) {
              impactSides.Add(ContactSides.BOTTOM, GetCalculatedValue(0, 0, (collisionArea2.rectangle.Top + (ratio * collisionArea2.rectangle.Height) - collisionArea1.rectangle.Z - 1), false));
            }
          }
        } else if (collisionArea2.rectangle.type == RectangleTypes.ZSLOPEY_DOWN || collisionArea2.rectangle.type == RectangleTypes.ZSLOPEY_UP) {
          float yPos = collisionArea1.rectangle.Back + (collisionArea1.rectangle.Width / 2);
          float absPos = yPos - collisionArea2.rectangle.Back;
          float ratio = (collisionArea2.rectangle.type == RectangleTypes.ZSLOPEY_DOWN) ? (absPos / collisionArea2.rectangle.Width) : 1 - (absPos / collisionArea2.rectangle.Width);

          if (collisionArea1.prevRectangle.Right <= collisionArea2.rectangle.Left || collisionArea1.prevRectangle.Left >= collisionArea2.rectangle.Right) {
            if (collisionArea1.rectangle.Z >= collisionArea2.rectangle.Top + (ratio * collisionArea2.rectangle.Height)) {
              GetImpact(contactSides1, collisionArea1, collisionArea2, false);
            }
          }
          if ((collisionArea2.rectangle.type == RectangleTypes.ZSLOPEY_DOWN && collisionArea1.prevRectangle.Front <= collisionArea2.rectangle.Back) ||
              (collisionArea2.rectangle.type == RectangleTypes.ZSLOPEY_UP && collisionArea1.prevRectangle.Back >= collisionArea2.rectangle.Front)) {
            GetImpact(contactSides1, collisionArea1, collisionArea2, false);
          }

          if (impactSides.Count == 0) {
            if (collisionArea1.rectangle.Z >= collisionArea2.rectangle.Top + (ratio * collisionArea2.rectangle.Height)) {
              impactSides.Add(ContactSides.BOTTOM, GetCalculatedValue(0, 0, (collisionArea2.rectangle.Top + (ratio * collisionArea2.rectangle.Height) - collisionArea1.rectangle.Z - 1), false));
            }
          }
        } 
      } else if (collisionArea1.prevRectangle.Bottom <= collisionArea2.rectangle.Top) {
        impactSides.Add(ContactSides.BOTTOM, GetCalculatedValue(0, 0, collisionArea2.rectangle.Top - collisionArea1.rectangle.Bottom - 1, false));
      } else if (collisionArea1.prevRectangle.Top >= collisionArea2.rectangle.Bottom) {
        impactSides.Add(ContactSides.TOP, GetCalculatedValue(0, 0, collisionArea2.rectangle.Bottom - collisionArea1.rectangle.Top + 1, false));
      } else if (contactPoints1.Count > 0) {
        GetImpact(contactSides1, collisionArea1, collisionArea2, false);
      } else {
        GetImpact(contactSides2, collisionArea2, collisionArea1, true);
      }

      float x = 0, y = 0, z = 0;

      foreach (Vector3 impactSide in impactSides.Values) {
        if (!IsZero(impactSide.X)) {
          if (IsZero(x) || Math.Abs(impactSide.X) < Math.Abs(x))
            x = impactSide.X;
        }
        if (!IsZero(impactSide.Y)) {
          if (IsZero(y) || Math.Abs(impactSide.Y) < Math.Abs(y))
            y = impactSide.Y;
        }
        if (!IsZero(impactSide.Z)) {
          if (IsZero(z) || Math.Abs(impactSide.Z) < Math.Abs(z))
            z = impactSide.Z;
        }
      }


      if (!IsZero(x) && (IsZero(y) || Math.Abs(x) < Math.Abs(y))) {
        if (IsZero(z) || Math.Abs(x) < Math.Abs(z)) {
          this.impact.X = x;
          this.impact.Y = 0;
          this.impact.Z = 0;
        } else {
          this.impact.X = 0;
          this.impact.Y = 0;
          this.impact.Z = z;
        }
      } else {
        if (IsZero(z) || (!IsZero(y) && Math.Abs(y) < Math.Abs(z))) {
          this.impact.X = 0;
          this.impact.Y = y;
          this.impact.Z = 0;
        } else {
          this.impact.X = 0;
          this.impact.Y = 0;
          this.impact.Z = z;
        }
      }

    }

    public CollisionArea GetCollisionArea1() {
      return collisionArea1;  
    }

    public CollisionArea GetCollisionArea2() {
      return collisionArea2;
    }

    public void Solve(GameObject gameObject, GameObject contraObject = null) {
      if (impact.Z != 0) {
        if (contraObject == null || gameObject.GetPosition().Z + impact.Z != gameObject.GetPrevPosition().Z) {
          gameObject.SetPosition(null, null, gameObject.GetPosition().Z + impact.Z);
          if (gameObject.GetMovableEntity() != null)
            gameObject.GetMovableEntity().Stop(false, false, true, false);
        } else if (contraObject != null) {
          contraObject.SetPosition(null, null, contraObject.GetPosition().Z + (0 - impact.Z));
          if (contraObject.GetMovableEntity() != null)
            contraObject.GetMovableEntity().Stop(false, false, true, false);
        }
      }

      if (impact.X != 0 && (impact.Y == 0 || Math.Abs(impact.X) < Math.Abs(impact.Y))) {
        if (contraObject == null || gameObject.GetPosition().X + impact.X != gameObject.GetPrevPosition().X) {
          gameObject.SetPosition(gameObject.GetPosition().X + impact.X, null, null);
          if (gameObject.GetMovableEntity() != null)
            gameObject.GetMovableEntity().Stop(true, false, false, false);
        } else if (contraObject != null) {
          contraObject.SetPosition(contraObject.GetPosition().X + (0 - impact.X), null, null);
          if (contraObject.GetMovableEntity() != null)
            contraObject.GetMovableEntity().Stop(true, false, false, false);
        }
      } else if (impact.Y != 0 && (impact.X == 0 || Math.Abs(impact.Y) < Math.Abs(impact.X))) {
        if (contraObject == null || gameObject.GetPosition().Y + impact.Y != gameObject.GetPrevPosition().Y) {
          gameObject.SetPosition(null, gameObject.GetPosition().Y + impact.Y, null);
          if (gameObject.GetMovableEntity() != null)
            gameObject.GetMovableEntity().Stop(false, true, false, false);
        } else if (contraObject != null) {
          contraObject.SetPosition(null, contraObject.GetPosition().Y + (0 - impact.Y), null);
          if (contraObject.GetMovableEntity() != null)
            contraObject.GetMovableEntity().Stop(false, true, false, false);
        }
      }  
    }

    public Vector3 GetImpact() {
      return impact;
    }

    private Vector3 GetCalculatedValue(float x, float y, float z, bool reversed) {
      return (!reversed) ? new Vector3(x, y, z) : new Vector3(0 - x, 0 - y, 0 - z);
    }

    private Vector3 GetCalculatedValue(float x, float y, bool reversed) {
      return GetCalculatedValue(x, y, 0, reversed);
    }

    private void GetImpact(List<ContactSides> contactSides, CollisionArea collisionArea1, CollisionArea collisionArea2, bool reversed = false) {
      float xCollision, yCollision;
      float slopePointX, slopePointY, subtraction;

      foreach (ContactSides side in contactSides) {
        switch (side) {
          case ContactSides.FULL_RIGHT:
            impactSides.Add(side, GetCalculatedValue(collisionArea2.rectangle.Left - collisionArea1.rectangle.Right - 1, 0, reversed));
            return;
            break;
          case ContactSides.FULL_LEFT:
            impactSides.Add(side, GetCalculatedValue(collisionArea2.rectangle.Right - collisionArea1.rectangle.Left + 1, 0, reversed));
            return;
            break;
          case ContactSides.FULL_BACK:
            impactSides.Add(side, GetCalculatedValue(0, collisionArea2.rectangle.Front - collisionArea1.rectangle.Back + 1, reversed));
            return;
            break;
          case ContactSides.FULL_FRONT:
            impactSides.Add(side, GetCalculatedValue(0, collisionArea2.rectangle.Back - collisionArea1.rectangle.Front - 1, reversed));
            return;
            break;
          case ContactSides.RIGHT:
            xCollision = collisionArea2.rectangle.Left - collisionArea1.rectangle.Right;
            yCollision = (contactSides.Contains(ContactSides.BACK)) ? collisionArea2.rectangle.Front - collisionArea1.rectangle.Back : collisionArea2.rectangle.Back - collisionArea1.rectangle.Front;

            if (collisionArea1.rectangle.type != RectangleTypes.XYSLOPE_UP_LEFT && collisionArea1.rectangle.type != RectangleTypes.XYSLOPE_DOWN_LEFT) {
              if (contactSides.Contains(ContactSides.BACK)) {
                if (collisionArea2.rectangle.type == RectangleTypes.XYSLOPE_DOWN_RIGHT) 
                  yCollision = GetImpactFromSlope(collisionArea1.rectangle.Right, collisionArea1.rectangle.Back, collisionArea2.rectangle);
                impactSides.Add(side, GetCalculatedValue(xCollision - 1, yCollision + 1, reversed));

              } else {
                if (collisionArea2.rectangle.type == RectangleTypes.XYSLOPE_UP_RIGHT) {
                  yCollision = GetImpactFromSlope(collisionArea1.rectangle.Right, collisionArea1.rectangle.Front, collisionArea2.rectangle);
                }
                impactSides.Add(side, GetCalculatedValue(xCollision - 1, yCollision - 1, reversed));
              }
              break;
            }

            slopePointX = collisionArea2.rectangle.Left;
            slopePointY = (contactSides.Contains(ContactSides.BACK)) ? collisionArea1.rectangle.Back : collisionArea1.rectangle.Front;
            subtraction = (contactSides.Contains(ContactSides.BACK)) ? collisionArea1.rectangle.Back - collisionArea2.rectangle.Back : collisionArea1.rectangle.Front - collisionArea2.rectangle.Front;

            yCollision = (contactSides.Contains(ContactSides.BACK)) ? 0 - GetImpactFromSlope(slopePointX, slopePointY, collisionArea1.rectangle) : GetImpactFromSlope(slopePointX, slopePointY, collisionArea1.rectangle);
            yCollision -= subtraction;

            if (contactSides.Contains(ContactSides.BACK))
              impactSides.Add(side, GetCalculatedValue(xCollision - 1, yCollision - 1, reversed));
            else
              impactSides.Add(side, GetCalculatedValue(xCollision - 1, yCollision + 1, reversed));
            break;
          case ContactSides.LEFT:
            xCollision = collisionArea2.rectangle.Right - collisionArea1.rectangle.Left;
            yCollision = (contactSides.Contains(ContactSides.BACK)) ? collisionArea2.rectangle.Front - collisionArea1.rectangle.Back : collisionArea2.rectangle.Back - collisionArea1.rectangle.Front;

            if (collisionArea1.rectangle.type != RectangleTypes.XYSLOPE_UP_RIGHT && collisionArea1.rectangle.type != RectangleTypes.XYSLOPE_DOWN_RIGHT) {
              if (contactSides.Contains(ContactSides.BACK)) {
                if (collisionArea2.rectangle.type == RectangleTypes.XYSLOPE_UP_LEFT)
                  yCollision = GetImpactFromSlope(collisionArea1.rectangle.Left, collisionArea1.rectangle.Back, collisionArea2.rectangle);
                impactSides.Add(side, GetCalculatedValue(xCollision + 1, yCollision + 1, reversed));
              } else {
                if (collisionArea2.rectangle.type == RectangleTypes.XYSLOPE_DOWN_LEFT)
                  yCollision = GetImpactFromSlope(collisionArea1.rectangle.Left, collisionArea1.rectangle.Front, collisionArea2.rectangle);
                impactSides.Add(side, GetCalculatedValue(xCollision + 1, yCollision - 1, reversed));
              }
              break;
            }

            slopePointX = collisionArea2.rectangle.Right;
            slopePointY = (contactSides.Contains(ContactSides.BACK)) ? collisionArea1.rectangle.Back : collisionArea1.rectangle.Front;
            subtraction = (contactSides.Contains(ContactSides.BACK)) ? collisionArea1.rectangle.Front - collisionArea2.rectangle.Front : collisionArea1.rectangle.Back - collisionArea2.rectangle.Back;

            yCollision = (contactSides.Contains(ContactSides.BACK)) ? 0 - GetImpactFromSlope(slopePointX, slopePointY, collisionArea1.rectangle) : GetImpactFromSlope(slopePointX, slopePointY, collisionArea1.rectangle);
            yCollision -= subtraction;

            if (contactSides.Contains(ContactSides.BACK)) {
              impactSides.Add(side, GetCalculatedValue(xCollision + 1, yCollision + 1, reversed));
            } else { 
              impactSides.Add(side, GetCalculatedValue(xCollision + 1, yCollision - 1, reversed));
            }
            break;
          case ContactSides.BACK:
            impactSides.Add(side, GetCalculatedValue(0, collisionArea2.rectangle.Front - collisionArea1.rectangle.Back + 1, reversed));
            break;
          case ContactSides.FRONT:
            impactSides.Add(side, GetCalculatedValue(0, collisionArea2.rectangle.Back - collisionArea1.rectangle.Front - 1, reversed));
            break;
        }
      }
    }

    private bool IsZero(float number) {
      return (number < 0.01 && number > -0.01);
    }



    private float GetImpactFromSlope(float x, float y, Rectangle3D rectangle) {
      float absPos, ratio;

      //if (rectangle.type == RectangleTypes.ZSLOPEX_DOWN || rectangle.type == RectangleTypes.ZSLOPEX_UP) {
      //  absPos = x - rectangle.Left;
      //  ratio = (rectangle.type == RectangleTypes.ZSLOPEX_DOWN) ? (absPos / rectangle.Length) : 1 - (absPos / rectangle.Length);
      //  return z - ((rectangle.Top + (ratio * rectangle.Height))); //(z >= rectangle.Top + (ratio * rectangle.Height));
      //}
      //if (rectangle.type == RectangleTypes.ZSLOPEY_DOWN || rectangle.type == RectangleTypes.ZSLOPEY_UP) {
      //  absPos = y - rectangle.Back;
      //  ratio = (rectangle.type == RectangleTypes.ZSLOPEY_DOWN) ? (absPos / rectangle.Width) : 1 - (absPos / rectangle.Width);
      //  return z - ((rectangle.Top + (ratio * rectangle.Height))); //(z >= rectangle.Top + (ratio * rectangle.Height));
      //}
      if (rectangle.type == RectangleTypes.XYSLOPE_DOWN_LEFT || rectangle.type == RectangleTypes.XYSLOPE_UP_RIGHT) {
        absPos = x - rectangle.Left;
        ratio = (rectangle.type == RectangleTypes.XYSLOPE_DOWN_LEFT) ? (absPos / rectangle.Length) : 1 - (absPos / rectangle.Length);
        return (rectangle.Back + (ratio * rectangle.Width)) - y; //(y >= rectangle.Back + (ratio * rectangle.Width));
      }
      if (rectangle.type == RectangleTypes.XYSLOPE_UP_LEFT || rectangle.type == RectangleTypes.XYSLOPE_DOWN_RIGHT) {
        absPos = x - rectangle.Left;
        ratio = (rectangle.type == RectangleTypes.XYSLOPE_DOWN_RIGHT) ? (absPos / rectangle.Length) : 1 - (absPos / rectangle.Length);
        return (rectangle.Back + (ratio * rectangle.Width)) - y; //(y <= rectangle.Back + (ratio * rectangle.Width));
      }
      return 0;
    }



    private List<ContactSides> GetContactSides(List<Tuple<int, Vector3>> contactPoints) {
      List<ContactSides> contactSides = new List<ContactSides>();
      List<int> contact = new List<int>();

      if (contactPoints.Count == 0) {
        return contactSides;
      }

      foreach (Tuple<int, Vector3> c in contactPoints) {
        contact.Add(c.Item1);
      }

      bool gotFull = false;

      if (contact.Contains(0) && contact.Contains(1) && contact.Contains(2) && contact.Contains(3)) {
        contactSides.Add(ContactSides.FULL_TOP);
        return contactSides;
      } else if (contact.Contains(4) && contact.Contains(5) && contact.Contains(6) && contact.Contains(7)) {
        contactSides.Add(ContactSides.FULL_BOTTOM);
        return contactSides;
      } else {
        if ((contact.Contains(0) && contact.Contains(1)) || (contact.Contains(4) && contact.Contains(5))) {
          contactSides.Add(ContactSides.FULL_BACK);
          gotFull = true;
        }
        if ((contact.Contains(2) && contact.Contains(3)) || (contact.Contains(6) && contact.Contains(7))) {
          contactSides.Add(ContactSides.FULL_FRONT);
          gotFull = true;
        }
        if ((contact.Contains(0) && contact.Contains(3)) || (contact.Contains(4) && contact.Contains(7))) {
          contactSides.Add(ContactSides.FULL_LEFT);
          gotFull = true;
        }
        if ((contact.Contains(1) && contact.Contains(2)) || (contact.Contains(5) && contact.Contains(6))) {
          contactSides.Add(ContactSides.FULL_RIGHT);
          gotFull = true;
        }
      }

      if (contact.Contains(0) || contact.Contains(1) || contact.Contains(2) || contact.Contains(3)) {
        contactSides.Add(ContactSides.TOP);
      }
      if (contact.Contains(4) || contact.Contains(5) || contact.Contains(6) || contact.Contains(7)) {
        contactSides.Add(ContactSides.BOTTOM);
      }

      if (!gotFull) {
        for (int c = 0; c < contact.Count; c++) {
          switch (contact[c]) {
            case 0:
            case 4:
              if (!contactSides.Contains(ContactSides.BACK))
                contactSides.Add(ContactSides.BACK);
              if (!contactSides.Contains(ContactSides.LEFT))
                contactSides.Add(ContactSides.LEFT);
              break;
            case 1:
            case 5:
              if (!contactSides.Contains(ContactSides.FULL_BACK) && !contactSides.Contains(ContactSides.FULL_RIGHT)) {
                if (!contactSides.Contains(ContactSides.BACK))
                  contactSides.Add(ContactSides.BACK);
                if (!contactSides.Contains(ContactSides.RIGHT))
                  contactSides.Add(ContactSides.RIGHT);
              }
              break;
            case 2:
            case 6:
              if (!contactSides.Contains(ContactSides.FULL_FRONT) && !contactSides.Contains(ContactSides.FULL_RIGHT)) {
                if (!contactSides.Contains(ContactSides.FRONT))
                  contactSides.Add(ContactSides.FRONT);
                if (!contactSides.Contains(ContactSides.RIGHT))
                  contactSides.Add(ContactSides.RIGHT);
              }
              break;
            case 3:
            case 7:
              if (!contactSides.Contains(ContactSides.FULL_FRONT) && !contactSides.Contains(ContactSides.FULL_LEFT)) {
                if (!contactSides.Contains(ContactSides.FRONT))
                  contactSides.Add(ContactSides.FRONT);
                if (!contactSides.Contains(ContactSides.LEFT))
                  contactSides.Add(ContactSides.LEFT);
              }
              break;
          }
        }
      }

      return contactSides;
    }

  }
}
