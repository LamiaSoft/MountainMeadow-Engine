using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Cameras;
using MountainMeadowEngine.Collision;
using MountainMeadowEngine.Managers;
using MountainMeadowEngine.Tools;
using static MountainMeadowEngine.GameWorld;

namespace MountainMeadowEngine.Components {

  public class ParallaxBackground : GameCameraComponent {
    
    // Defines
    int layerDepth = 1;  // 0 = frontmost
    bool isForeground = false;
    int rows, columns;
    float scrollSpeedRatio = 0.5f;
    List<Background> textures = new List<Background>();


    // Current dynamics
    float _leftMost, _rightMost, _topMost, _bottomMost;
    Rectangle3D _bgLocation, _viewport;
    Vector2 _offset, _drawPosition;
    List<Background> _drawables = new List<Background>();
    List<Background> _backgroundsInRange = new List<Background>();
    List<Tuple<int, float>> _rowList = new List<Tuple<int, float>>();
    List<Tuple<int, float>> _colList = new List<Tuple<int, float>>();


    public ParallaxBackground(GameCamera context) : base(context) {
      _viewport = new Rectangle3D();
      _viewport.SetHeight(1);
    }

    public ParallaxBackground AddTexture(string name, Vector2 position) {
      if (textures.Count == 0) {
        textures.Add(new Background(name, position));
        rows = 1;
        columns = 1;
        return this;
      }

      int count = textures.Count;
      for (int i = 0; i < count; i++) {
        if ((position.X < textures[i].GetLocation().Left && position.Y <= textures[i].GetLocation().Back) || position.Y < textures[i].GetLocation().Back) { 
          textures.Insert(i, new Background(name, position));
          break;
        }
      }

      if (count == textures.Count)
        textures.Add(new Background(name, position));
      
      int? firstX = null, firstY = 0;
      rows = 0;
      columns = 0;

      for (int i = 0; i < textures.Count; i++) {
        if (i == 0) {
          _leftMost = textures[i].GetLocation().X;
          _topMost = textures[i].GetLocation().Y;

          firstX = (int)textures[i].GetLocation().X;
          firstY = (int)textures[i].GetLocation().Y;
        }

        if ((int)(textures[i].GetLocation().Y) == firstY) {
          columns++;
          _rightMost = textures[i].GetLocation().X + textures[i].GetLocation().Length;
        }
        if ((int)(textures[i].GetLocation().X) == firstX) {
          rows++;
          _bottomMost = textures[i].GetLocation().Y + textures[i].GetLocation().Width;
        }
      }
      return this;
    }

    public ParallaxBackground SetLayerDepth(int depth) {
      this.layerDepth = depth;
      return this;
    }

    public int GetLayerDepth() {
      return layerDepth;
    }

    public bool IsForeGround() {
      return isForeground;
    }

    public ParallaxBackground SetScrollSpeedRatio(float ratio) {
      this.scrollSpeedRatio = ratio;
      return this;
    }

    public ParallaxBackground SetForeground(bool foreground) {
      this.isForeground = foreground;
      return this;
    }

    public override void DrawUpdate(GameTime gameTime) {
      _drawables.Clear();

      _offset = context.GetPosition() - context.GetPrevPosition();

      for (int i = 0; i < textures.Count; i++) {
        textures[i].ApplyOffsetToLocation(_offset * scrollSpeedRatio);
      }

      _leftMost += (_offset.X * scrollSpeedRatio);
      _rightMost += (_offset.X * scrollSpeedRatio);
      _topMost += (_offset.Y * scrollSpeedRatio);
      _bottomMost += (_offset.Y * scrollSpeedRatio);

      _viewport.SetLength(ViewportManager.GAME_VIEWPORT.X);
      _viewport.SetWidth(ViewportManager.GAME_VIEWPORT.Y);
      _viewport.SetPosition(context.GetPosition().X, context.GetPosition().Y, 0);


      _rowList = DetermineRows();
      _colList = DetermineColumns();

      _backgroundsInRange.Clear();

      if (_rowList.Count >= _colList.Count) {
        for (int r = 0; r < _rowList.Count; r++) {
          for (int c = 0; c < _colList.Count; c++) {
            int index = (_rowList[r].Item1 * columns) + _colList[c].Item1;
            Background bg = new Background(textures[index].GetName(), new Vector2(_colList[c].Item2, _rowList[r].Item2));

            int count = _backgroundsInRange.Count;
            for (int i = 0; i < count; i++) {
              if ((bg.GetLocation().Left < _backgroundsInRange[i].GetLocation().Left && 
                   bg.GetLocation().Back <= _backgroundsInRange[i].GetLocation().Back) || bg.GetLocation().Back < _backgroundsInRange[i].GetLocation().Back) {
                _backgroundsInRange.Insert(i, bg);
                break;
              }
            }
            if (count == _backgroundsInRange.Count)
              _backgroundsInRange.Add(bg);
          } 
        }
      } else {
        for (int c = 0; c < _colList.Count; c++) {
          for (int r = 0; r < _rowList.Count; r++) {
            int index = (_rowList[r].Item1 * columns) + _colList[c].Item1;
            Background bg = new Background(textures[index].GetName(), new Vector2(_colList[c].Item2, _rowList[r].Item2));

            int count = _backgroundsInRange.Count;
            for (int i = 0; i < count; i++) {
              if ((bg.GetLocation().Left < _backgroundsInRange[i].GetLocation().Left &&
                   bg.GetLocation().Back <= _backgroundsInRange[i].GetLocation().Back) || bg.GetLocation().Back < _backgroundsInRange[i].GetLocation().Back) {
                _backgroundsInRange.Insert(i, bg);
                break;
              }
            }
            if (count == _backgroundsInRange.Count)
              _backgroundsInRange.Add(bg);
          }
        }
      }


      for (int i = 0; i < _backgroundsInRange.Count; i++) {
        _bgLocation = _backgroundsInRange[i].GetLocation();
               
        Rectangle sourceRectangle = new Rectangle(0, 0, (int)_bgLocation.Length, (int)_bgLocation.Width);
        _drawPosition.X = _viewport.X;
        _drawPosition.Y = _viewport.Y;
       
        if (_viewport.Left >= _bgLocation.Left) {
          sourceRectangle.X = (int)Math.Floor(_viewport.Left - _bgLocation.Left);
        } else {
          _drawPosition.X = _bgLocation.X;
        }
        if (_viewport.Right <= _bgLocation.Right) {
          sourceRectangle.Width = (int)Math.Ceiling(_viewport.Right - _drawPosition.X);
        } else {
          sourceRectangle.Width = (int)Math.Ceiling(_viewport.Right - _drawPosition.X - (_viewport.Right - _bgLocation.Right));
        }

        if (_viewport.Back >= _bgLocation.Back) {
          sourceRectangle.Y = (int)Math.Floor(_viewport.Back - _bgLocation.Back);
        } else {
          _drawPosition.Y = _bgLocation.Y;
        }
        if (_viewport.Front <= _bgLocation.Front) {
          sourceRectangle.Height = (int)Math.Ceiling(_viewport.Front - _drawPosition.Y);
        } else {
          sourceRectangle.Height = (int)Math.Ceiling(_viewport.Front - _drawPosition.Y - (_viewport.Front - _bgLocation.Front));
        }

        if (sourceRectangle.Width > 0 && sourceRectangle.Height > 0) {
          _backgroundsInRange[i].SetSourceRectangle(sourceRectangle);
          _backgroundsInRange[i].SetCurrentDrawPosition(_drawPosition);
          _drawables.Add(_backgroundsInRange[i]);
        }
      }


      for (int c = 0; c < _drawables.Count; c++) {
        for (int c2 = c + 1; c2 < _drawables.Count; c2++) {
          if (_drawables[c].GetCurrentDrawPosition().X < _drawables[c2].GetCurrentDrawPosition().X && (int)(_drawables[c2].GetCurrentDrawPosition().Y) == (int)(_drawables[c].GetCurrentDrawPosition().Y) && 
              _drawables[c].GetCurrentDrawPosition().X + _drawables[c].GetSourceRectangle().Width > _drawables[c2].GetCurrentDrawPosition().X) {
            
            _drawables[c2].SetCurrentDrawPosition(new Vector2(_drawables[c2].GetCurrentDrawPosition().X + ((_drawables[c].GetCurrentDrawPosition().X + _drawables[c].GetSourceRectangle().Width) - _drawables[c2].GetCurrentDrawPosition().X),
                                                             _drawables[c2].GetCurrentDrawPosition().Y));
          }
        }
      }

      //Debug.Output(ToString(), "NUM: " + _drawables.Count.ToString());
    }

    public List<Background> GetCurrentTextures() {
      return _drawables;
    }

    List<Tuple<int, float>> DetermineRows() {
      List<Tuple<int, float>> rowList = new List<Tuple<int, float>>();
      int index;
      float startYPos, drawYPosStart, drawYPosEnd;

      bool bottomToTop = (_viewport.Front <= _bottomMost);
      index = (bottomToTop) ? columns * (rows - 1) : 0;
      startYPos = (bottomToTop) ? textures[index].GetLocation().Front : textures[index].GetLocation().Back;

      for (float currentY = startYPos; ((bottomToTop) ? currentY >= _viewport.Back : currentY <= _viewport.Front);) {
        for (int i = index; ((bottomToTop) ? i >= 0 : i <= (columns * (rows - 1)));) {
          drawYPosStart = (bottomToTop) ? currentY - textures[i].GetLocation().Width : currentY;
          drawYPosEnd = (bottomToTop) ? currentY : currentY + textures[i].GetLocation().Width;

          if (drawYPosStart <= _viewport.Front && drawYPosEnd >= _viewport.Back) {
            rowList.Add(Tuple.Create(i / columns, drawYPosStart));
          }
          i = (bottomToTop) ? i - columns : i + columns;
          currentY = (bottomToTop) ? drawYPosStart : drawYPosEnd;
        }
      }
      return rowList;
    }

    List<Tuple<int, float>> DetermineColumns() {
      List<Tuple<int, float>> columnList = new List<Tuple<int, float>>();
      int index;
      float startXPos, drawXPosStart, drawXPosEnd;

      bool RightToLeft = (_viewport.Right <= _rightMost);
      index = (RightToLeft) ? columns - 1 : 0;
      startXPos = (RightToLeft) ? textures[index].GetLocation().Right : textures[index].GetLocation().Left;

      for (float currentX = startXPos; ((RightToLeft) ? currentX >= _viewport.Left : currentX <= _viewport.Right);) {
        for (int i = index; ((RightToLeft) ? i >= 0 : i < columns);) {
          drawXPosStart = (RightToLeft) ? currentX - textures[i].GetLocation().Length : currentX;
          drawXPosEnd = (RightToLeft) ? currentX : currentX + textures[i].GetLocation().Length;

          if (drawXPosStart <= _viewport.Right && drawXPosEnd >= _viewport.Left) {
            columnList.Add(Tuple.Create(i, drawXPosStart));
          }
          i = (RightToLeft) ? i - 1 : i + 1;
          currentX = (RightToLeft) ? drawXPosStart : drawXPosEnd;
        }
      }
      return columnList;
    }

    public override void Initialize() {

    }
  }
}
