using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Cameras;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Objects;

namespace MountainMeadowEngine.Managers {

    public class RenderManager {
        SpriteBatch spriteBatch;
        GraphicsDevice graphicsDevice;
        GameCamera HUDCam;

        GameObject _currentObject, _currentObject2;
        GameObjectComponent _currentComponent, _currentComponent2;
        Sprite _currentSprite;
        AnimatedSprite _currentAnimatedSprite;
        Vector3 _maxSides1, _maxSides2, _minSides1, _minSides2;
        Tuple<int, int, Type> _objectTuple;
        Type _componentType;
        bool _renderSlopeFront1, _renderSlopeFront2;
        int _result, _latest, _ZAdvantageIndex, _YAdvantageIndex, _blockingObjectIndex;
        List<int> _fixedPositionObjects = new List<int>();

        List<int> drawnItems = new List<int>();
        List<Tuple<int, int, Type>> renderOrder = new List<Tuple<int, int, Type>>();
        Tuple<int, int, Type>[] renderArray;

        List<FixedBackground> _fixedBackgrounds;
        List<RegularBackground> _regularBackgrounds;
        List<ParallaxBackground> _parallaxBackgrounds;
        List<int> _layers = new List<int>();
        List<GameScene> gameScenes = new List<GameScene>();

        int checks;
        Stopwatch stopwatch = new Stopwatch();

        public RenderManager(GraphicsDevice graphicsDevice, GameCamera HUDCam) {
            this.spriteBatch = new SpriteBatch(graphicsDevice);
            this.graphicsDevice = graphicsDevice;
            this.HUDCam = HUDCam;
        }

        public void Update(GameTime gameTime, double timeSinceLastUpdate, List<GameScene> gameScenes) {
            graphicsDevice.Clear(Microsoft.Xna.Framework.Color.DarkRed);

            for (int i = 0; i < gameScenes.Count; i++) {
                if (gameScenes[i].GetStatus() == GameScene.SceneStatuses.INACTIVE) {
                    continue;
                }

                if (!gameScenes[i].IsPaused()) {
                    EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.PRE_RENDERING, this, gameScenes[i].GetType())
                                             .SetGameTime(gameTime)
                                             .SetTimeSinceLastUpdate(timeSinceLastUpdate));
                }

                for (int camNum = 0; camNum < gameScenes[i].GetCameras().Count; camNum++) {

                    if (!gameScenes[i].IsPaused()) {
                        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.OBJECT_RENDERING, this, gameScenes[i].GetType())
                                               .SetGameTime(gameTime)
                                               .SetTimeSinceLastUpdate(timeSinceLastUpdate)
                                               .SetObjectManager(gameScenes[i].GetObjectManager()));
                    }

                    graphicsDevice.Viewport = gameScenes[i].GetViewportById(camNum);

                    if (!gameScenes[i].IsPaused()) {
                        EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.RENDERING, this, gameScenes[i].GetType())
                                               .SetGameTime(gameTime)
                                               .SetTimeSinceLastUpdate(timeSinceLastUpdate)
                                               .SetObjectManager(gameScenes[i].GetObjectManager()));
                    }

                    gameScenes[i].GetCameraById(camNum).DrawUpdate(gameTime, timeSinceLastUpdate);

                    gameScenes[i].GetObjectManager().DrawUpdate(gameTime, gameScenes[i].GetCameraById(camNum).GetPosition().X, gameScenes[i].GetCameraById(camNum).GetPosition().Y,
                                                                gameScenes[i].GetViewportById(camNum).Width, gameScenes[i].GetViewportById(camNum).Height);

                    drawnItems.Clear();
                    renderOrder.Clear();


                    // START OF NEW PROCEDURE
                    //checks = 0;
                    //stopwatch.Reset();
                    //stopwatch.Start();

                    Dictionary<int, Dictionary<int, bool>> renderBuckets = gameScenes[i].GetObjectManager().spatialRenderBucketList;
                    renderOrder.Clear();
                    //Tools.Debug.Json(" render buckets ", renderBuckets);

                    foreach (int o in renderBuckets[0].Keys) {
                        int componentCount = gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetComponents().Count;

                        for (int oc = 0; oc < componentCount; oc++) {
                            _componentType = gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(o).GetComponents()[oc].GetType();

                            if (_componentType == typeof(Sprite) || _componentType == typeof(AnimatedSprite)) {
                                renderOrder.Add(Tuple.Create(o, oc, _componentType));
                            }
                        }
                    }

                    renderArray = renderOrder.ToArray();
                    //Array.Sort(renderArray, gameScenes[i].GetObjectManager().SortBySlope);
                    Array.Sort(renderArray, gameScenes[i].GetObjectManager());

                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, gameScenes[i].GetCameraById(camNum).GetViewMatrix());


                    // Load backgrounds
                    _fixedBackgrounds = gameScenes[i].GetCameraById(camNum).GetComponents<FixedBackground>();
                    _regularBackgrounds = gameScenes[i].GetCameraById(camNum).GetComponents<RegularBackground>();
                    _parallaxBackgrounds = gameScenes[i].GetCameraById(camNum).GetComponents<ParallaxBackground>();

                    // Start fixed background
                    foreach (FixedBackground background in _fixedBackgrounds) {
                        if (!background.IsForeGround())
                            spriteBatch.Draw(GameContentManager.LoadTexture(background.GetName()), background.GetCurrentDrawPosition(), null, Color.White);
                    }
                    // End fixed background

                    // Start regular backgrounds
                    _layers.Clear();
                    foreach (RegularBackground background in _regularBackgrounds) {
                        if (!background.IsForeGround() && !_layers.Contains(background.GetLayerDepth()))
                            _layers.Add(background.GetLayerDepth());
                    }
                    _layers.Sort();

                    for (int layer = _layers.Count - 1; layer >= 0; layer--) {
                        foreach (RegularBackground background in _regularBackgrounds) {
                            if (!background.IsForeGround() && background.GetLayerDepth() == _layers[layer]) {
                                spriteBatch.Draw(GameContentManager.LoadTexture(background.GetName()), background.GetCurrentDrawPosition(), null, Color.White);
                            }
                        }
                    }
                    // End regular backgrounds

                    // Start parallax backgrounds
                    _layers.Clear();
                    foreach (ParallaxBackground background in _parallaxBackgrounds) {
                        if (!background.IsForeGround() && !_layers.Contains(background.GetLayerDepth()))
                            _layers.Add(background.GetLayerDepth());
                    }
                    _layers.Sort();

                    for (int layer = _layers.Count - 1; layer >= 0; layer--) {
                        foreach (ParallaxBackground background in _parallaxBackgrounds) {
                            if (!background.IsForeGround() && background.GetLayerDepth() == _layers[layer]) {
                                foreach (Background texture in background.GetCurrentTextures()) {
                                    spriteBatch.Draw(GameContentManager.LoadTexture(texture.GetName()), texture.GetCurrentDrawPosition(), texture.GetSourceRectangle(), Color.White);
                                }
                            }
                        }
                    }
                    // End parallax backgrounds


                    for (int o = 0; o < renderArray.Length; o++) {
                        _currentObject = gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(renderArray[o].Item1);
                        _currentComponent = _currentObject.GetComponents()[renderArray[o].Item2];
                        if (renderArray[o].Item3 == typeof(Sprite)) {
                            _currentSprite = (Sprite)_currentComponent;
                            spriteBatch.Draw(_currentSprite.GetTexture(), ((_currentSprite.IsSpriteSheet()) ? _currentObject.GetDrawPosition() : _currentObject.GetDrawPosition() + _currentSprite.GetOffset()), _currentSprite.GetSourceRectangle(), _currentSprite.GetColor(), _currentSprite.GetRotation(), _currentSprite.GetOrigin(), _currentSprite.GetScale(), _currentSprite.GetSpriteEffects(), 0f);
                        } else {
                            _currentAnimatedSprite = (AnimatedSprite)_currentComponent;
                            List<ObjectSprite> _objectSprites = _currentAnimatedSprite.GetImageFrames();
                            for (int os = 0; os < _objectSprites.Count; os++) {
                                if (_objectSprites[os].texture != null)
                                    spriteBatch.Draw(_objectSprites[os].texture, _currentObject.GetDrawPosition(), _objectSprites[os].sourceRectangle, _objectSprites[os].color, _objectSprites[os].rotation, _objectSprites[os].origin, _objectSprites[os].scale, _objectSprites[os].spriteEffects, 0f);
                            }
                        }
                    }


                    // Start fixed foreground
                    foreach (FixedBackground background in _fixedBackgrounds) {
                        if (background.IsForeGround())
                            spriteBatch.Draw(GameContentManager.LoadTexture(background.GetName()), background.GetCurrentDrawPosition(), null, Color.White);
                    }
                    // End fixed foreground

                    // Start regular foregrounds
                    _layers.Clear();
                    foreach (RegularBackground background in _regularBackgrounds) {
                        if (background.IsForeGround() && !_layers.Contains(background.GetLayerDepth()))
                            _layers.Add(background.GetLayerDepth());
                    }
                    _layers.Sort();

                    for (int layer = _layers.Count - 1; layer >= 0; layer--) {
                        foreach (RegularBackground background in _regularBackgrounds) {
                            if (background.IsForeGround() && background.GetLayerDepth() == _layers[layer]) {
                                spriteBatch.Draw(GameContentManager.LoadTexture(background.GetName()), background.GetCurrentDrawPosition(), null, Color.White);
                            }
                        }
                    }
                    // End regular foregrounds

                    // Start parallax foregrounds
                    _layers.Clear();
                    foreach (ParallaxBackground background in _parallaxBackgrounds) {
                        if (background.IsForeGround() && !_layers.Contains(background.GetLayerDepth()))
                            _layers.Add(background.GetLayerDepth());
                    }
                    _layers.Sort();

                    for (int layer = _layers.Count - 1; layer >= 0; layer--) {
                        foreach (ParallaxBackground background in _parallaxBackgrounds) {
                            if (background.IsForeGround() && background.GetLayerDepth() == _layers[layer]) {
                                foreach (Background texture in background.GetCurrentTextures()) {
                                    spriteBatch.Draw(GameContentManager.LoadTexture(texture.GetName()), texture.GetCurrentDrawPosition(), texture.GetSourceRectangle(), Color.White);
                                }
                            }
                        }
                    }
                    // End parallax foregrounds


                    if (GameWorld.DEBUG_MODE != GameWorld.DebugModes.NONE) {
                        for (int o = 0; o < renderArray.Length; o++) {
                            if (!drawnItems.Contains(renderArray[o].Item1)) {
                                _currentObject = gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(renderArray[o].Item1);

                                foreach (CollisionArea col in _currentObject.GetComponents<CollisionArea>()) {
                                    //spriteBatch.Draw(col.GetDebugTextureFull(graphicsDevice), new Vector2(_currentObject.GetDrawPosition().X + col.GetOffset().X, _currentObject.GetDrawPosition().Y - col.GetDebugTextureFull(graphicsDevice).Height));
                                    spriteBatch.Draw(col.GetDebugTextureBottom(graphicsDevice), new Vector2(col.rectangle.X, col.rectangle.Y), Color.White);
                                }

                                if (_currentObject.GetComponents<DirectionalCollisionArea>().Count > 0) {
                                    foreach (CollisionArea col in _currentObject.GetComponents<DirectionalCollisionArea>()[0].GetCollisionAreas()) {
                                        //spriteBatch.Draw(col.GetDebugTextureFull(graphicsDevice), new Vector2(_currentObject.GetDrawPosition().X + col.GetOffset().X, _currentObject.GetDrawPosition().Y - col.GetDebugTextureFull(graphicsDevice).Height));
                                        spriteBatch.Draw(col.GetDebugTextureBottom(graphicsDevice), new Vector2(col.rectangle.X, col.rectangle.Y), Color.White);
                                    }
                                }

                                if (_currentObject.GetComponents<AnimatedSprite>().Count > 0) {
                                    foreach (CollisionArea col in _currentObject.GetComponents<AnimatedSprite>()[0].GetFrameCollisionAreas()) {
                                        //spriteBatch.Draw(col.GetDebugTextureFull(graphicsDevice), new Vector2(_currentObject.GetDrawPosition().X + col.GetOffset().X, _currentObject.GetDrawPosition().Y - col.GetDebugTextureFull(graphicsDevice).Height));
                                        spriteBatch.Draw(col.GetDebugTextureBottom(graphicsDevice), new Vector2(col.rectangle.X, col.rectangle.Y), Color.White);
                                    }
                                }

                                drawnItems.Add(renderArray[o].Item1);
                            }
                        }
                    }

                    spriteBatch.End();
                }

                _fixedPositionObjects = gameScenes[i].GetObjectManager().GetFixedPositionObjects();

                if (_fixedPositionObjects.Count > 0) {
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, HUDCam.GetViewMatrix());
                    for (int fo = 0; fo < _fixedPositionObjects.Count; fo++) {
                        _currentObject = gameScenes[i].GetObjectManager().GetGameObjectById<GameObject>(_fixedPositionObjects[fo]);

                        for (int cmp = 0; cmp < _currentObject.GetComponents().Count; cmp++) {
                            _currentComponent = _currentObject.GetComponents()[cmp];
                            if (_currentComponent.GetType() == typeof(Sprite)) {
                                _currentSprite = (Sprite)_currentComponent;
                                spriteBatch.Draw(_currentSprite.GetTexture(), ((_currentSprite.IsSpriteSheet()) ? _currentObject.GetDrawPosition() : _currentObject.GetDrawPosition() + _currentSprite.GetOffset()), _currentSprite.GetSourceRectangle(), _currentSprite.GetColor(), _currentSprite.GetRotation(), _currentSprite.GetOrigin(), _currentSprite.GetScale(), _currentSprite.GetSpriteEffects(), 0f);
                            } else if (_currentComponent.GetType() == typeof(AnimatedSprite)) {
                                _currentAnimatedSprite = (AnimatedSprite)_currentComponent;
                                List<ObjectSprite> _objectSprites = _currentAnimatedSprite.GetImageFrames();
                                for (int os = 0; os < _objectSprites.Count; os++) {
                                    if (_objectSprites[os].texture != null)
                                        spriteBatch.Draw(_objectSprites[os].texture, _currentObject.GetDrawPosition(), _objectSprites[os].sourceRectangle, _objectSprites[os].color, _objectSprites[os].rotation, _objectSprites[os].origin, _objectSprites[os].scale, _objectSprites[os].spriteEffects, 0f);
                                }
                            }
                        }

                        if (GameWorld.DEBUG_MODE != GameWorld.DebugModes.NONE) {
                            if (!drawnItems.Contains(_fixedPositionObjects[fo])) {

                                foreach (CollisionArea col in _currentObject.GetComponents<CollisionArea>()) {
                                    //spriteBatch.Draw(col.GetDebugTextureFull(graphicsDevice), new Vector2(_currentObject.GetDrawPosition().X + col.GetOffset().X, _currentObject.GetDrawPosition().Y - col.GetDebugTextureFull(graphicsDevice).Height));
                                    spriteBatch.Draw(col.GetDebugTextureBottom(graphicsDevice), new Vector2(_currentObject.GetPosition().X + col.GetOffset().X, _currentObject.GetPosition().Y + col.GetOffset().Y), Color.White);
                                }

                                if (_currentObject.GetComponents<DirectionalCollisionArea>().Count > 0) {
                                    foreach (CollisionArea col in _currentObject.GetComponents<DirectionalCollisionArea>()[0].GetCollisionAreas()) {
                                        //spriteBatch.Draw(col.GetDebugTextureFull(graphicsDevice), new Vector2(_currentObject.GetDrawPosition().X + col.GetOffset().X, _currentObject.GetDrawPosition().Y - col.GetDebugTextureFull(graphicsDevice).Height));
                                        spriteBatch.Draw(col.GetDebugTextureBottom(graphicsDevice), new Vector2(_currentObject.GetPosition().X + col.GetOffset().X, _currentObject.GetPosition().Y + col.GetOffset().Y), Color.White);
                                    }
                                }
                                drawnItems.Add(_fixedPositionObjects[fo]);
                            }
                        }
                    }
                    spriteBatch.End();
                }

                if (!gameScenes[i].IsPaused()) {
                    EventManager.PushEvent(GameEvent.Create<UpdateEvent>(UpdateEvent.Values.POST_RENDERING, this, gameScenes[i].GetType())
                                             .SetGameTime(gameTime)
                                             .SetTimeSinceLastUpdate(timeSinceLastUpdate));
                }
            }

            //Tools.StopWatch.Stop("RENDERING");
            //Tools.StopWatch.DisplayMilliseconds("RENDERING");
        }





    }
}
