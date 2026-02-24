using Microsoft.Xna.Framework;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.NPCs
{
    internal class ObjFishermanBoat : GameObject
    {
        private readonly ObjPhotoMouse _photoMouse;
        private bool _pullMouse;
        private bool _pulledMouse;
        private bool _falling;

        public BodyComponent _body;
        public readonly Animator _animator;
        private readonly CSprite _sprite;
        private readonly BodyDrawComponent _drawComponent;
        private readonly BodyDrawShadowComponent _shadowComponent;
        private readonly InteractComponent _interactionComponent;

        private readonly Vector2 _spawnPosition;

        private readonly string _dialogId;
        private string _currentAnimation;
        private string _spawnCondition;
        private bool _directionMode = true;

        public ObjFishermanBoat(Map.Map map, int posX, int posY, string spawnCondition, string animationId, string dialogId, Rectangle bodyRectangle) : base(map)
        {
            SprEditorImage = Resources.SprNpCs;
            EditorIconSource = new Rectangle(276, 2, 15, 16);

            if (string.IsNullOrEmpty(animationId))
            {
                IsDead = true;
                return;
            }
            EntityPosition = new CPosition(posX + 8, posY + 16, 0);
            EntitySize = new Rectangle(bodyRectangle.X - bodyRectangle.Width / 2, bodyRectangle.Y - bodyRectangle.Height, bodyRectangle.Width, bodyRectangle.Height);

            _spawnPosition = EntityPosition.Position;
            _spawnCondition = spawnCondition;
            _dialogId = dialogId;
            _animator = AnimatorSaveLoad.LoadAnimator("NPCs/" + animationId);

            if (_animator == null)
            {
                IsDead = true;
                return;
            }
            _sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(_animator, _sprite, Vector2.Zero);

            _body = new BodyComponent(EntityPosition, bodyRectangle.X - bodyRectangle.Width / 2, bodyRectangle.Y - bodyRectangle.Height, bodyRectangle.Width, bodyRectangle.Height, bodyRectangle.Height) { Gravity = -0.15f };

            AddComponent(KeyChangeListenerComponent.Index, new KeyChangeListenerComponent(OnKeyChange));
            AddComponent(BodyComponent.Index, _body);
            AddComponent(InteractComponent.Index, _interactionComponent = new InteractComponent(_body.BodyBox, Interact));
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(UpdateComponent.Index, new UpdateComponent(Update));
            AddComponent(DrawComponent.Index, _drawComponent = new BodyDrawComponent(_body, _sprite, Values.LayerBottom) { WaterOutline = false });
            AddComponent(DrawShadowComponent.Index, _shadowComponent = new BodyDrawShadowComponent(_body, _sprite));

            if (Game1.GameManager.SaveManager.GetString("photoMouseActive") == "1" &&
                Game1.GameManager.SaveManager.GetString("photo_sequence_bridge") == null)
            {
                _photoMouse = new ObjPhotoMouse(map, posX - 17, posY + 40, null, "mouseSeqBoat");
                map.Objects.SpawnObject(_photoMouse);
                _photoMouse.DisableInteractions();
            }
        }

        private void SetActive(bool isActive)
        {
            _interactionComponent.IsActive = isActive;
            _drawComponent.IsActive = isActive;
            _shadowComponent.IsActive = isActive;
        }

        private void Update()
        {
            if (_directionMode)
            {
                var playerDistance = new Vector2(
                    MapManager.ObjLink.EntityPosition.X - (EntityPosition.X),
                    MapManager.ObjLink.EntityPosition.Y - (EntityPosition.Y - 4));

                var dir = 3;

                // rotate in the direction of the player
                if (playerDistance.Length() < 32)
                    dir = AnimationHelper.GetDirection(playerDistance);

                // look at the player
                if (_currentAnimation == null)
                {
                    var animationIndex = _animator.GetAnimationIndex("stand_" + dir);
                    if (animationIndex >= 0)
                        _animator.Play(animationIndex);
                    else
                        _animator.Play("stand_" + (playerDistance.Y < 0 ? "1" : "3"));
                }
            }
            // finished playing
            if (_currentAnimation != null && !_animator.IsPlaying)
            {
                _currentAnimation = null;
                Game1.GameManager.SaveManager.SetString(_dialogId + "Finished", "1");
            }

            if (_pullMouse && _photoMouse != null && !_falling)
            {
                var targetPosition = new Vector2(EntityPosition.X - 25, EntityPosition.Y - 1);
                var pullDirection = targetPosition - _photoMouse.EntityPosition.Position;
                var pullSpeed = (_photoMouse.Body.CurrentFieldState & MapStates.FieldStates.DeepWater) != 0 ? 0.25f : 0.5f;

                if (pullDirection.Length() > pullSpeed * Game1.TimeMultiplier)
                {
                    pullDirection.Normalize();
                    _photoMouse.Body.VelocityTarget = pullDirection * pullSpeed;
                }
                else
                {
                    if (!_pulledMouse)
                    {
                        _pulledMouse = true;
                        Game1.GameManager.SaveManager.SetString("mousePulledUp", "1");
                    }
                    _photoMouse.Body.VelocityTarget = Vector2.Zero;
                    _photoMouse.EntityPosition.Set(targetPosition);
                }
            }
        }

        public void DisableRotating()
        {
            _directionMode = false;
        }

        private bool Interact()
        {
            Game1.GameManager.StartDialogPath(_dialogId);
            return true;
        }

        private void SetVisibility(bool visible)
        {
            _sprite.IsVisible = visible;
            _shadowComponent.IsActive = visible;
        }

        private void OnKeyChange()
        {
            if (_spawnCondition != null)
            {
                var spawnValue = Game1.GameManager.SaveManager.GetString(_spawnCondition);
                if (spawnValue == "1")
                    SetActive(true);
            }

            // start new animation?
            var animationString = _dialogId + "Animation";
            var animationValues = Game1.GameManager.SaveManager.GetString(animationString);
            if (animationValues != null)
            {
                if (animationValues == "-")
                {
                    _currentAnimation = null;
                }
                else if (animationValues != "")
                {
                    SetVisibility(true);
                    _currentAnimation = animationValues;
                    _animator.Play(_currentAnimation);
                }
                else
                {
                    SetVisibility(false);
                    _currentAnimation = null;
                }

                Game1.GameManager.SaveManager.RemoveString(animationString);
            }

            var pullMouseString = "mousePullUp";
            var pullMouseValue = Game1.GameManager.SaveManager.GetString(pullMouseString);
            if (!string.IsNullOrEmpty(pullMouseValue))
            {
                _pullMouse = true;
                Game1.GameManager.SaveManager.RemoveString(pullMouseString);
            }

            var fallString = "fisherman_fall";
            var fallValue = Game1.GameManager.SaveManager.GetString(fallString);
            if (!string.IsNullOrEmpty(fallValue))
            {
                _falling = true;
                _body.Velocity = new Vector3(-1.75f, -0.75f, 0);
                if (_photoMouse != null)
                {
                    _photoMouse.Body.IgnoresZ = false;
                    _photoMouse.Body.Velocity.X = -0.25f;
                }
                Game1.GameManager.SaveManager.RemoveString(fallString);
            }

            var resetString = "fisherman_reset";
            var resetValue = Game1.GameManager.SaveManager.GetString(resetString);
            if (!string.IsNullOrEmpty(resetValue))
            {
                _pullMouse = false;
                EntityPosition.Set(new Vector2(_spawnPosition.X, _spawnPosition.Y - 2));
                if (_photoMouse != null)
                    Map.Objects.DeleteObjects.Add(_photoMouse);
                Game1.GameManager.SaveManager.RemoveString(resetString);
            }

            var tradeName = "hook_trade";
            var tradeValue = Game1.GameManager.SaveManager.GetString(tradeName);
            if (!string.IsNullOrEmpty(tradeValue))
            {
                if (tradeValue == "0")
                    MapManager.ObjLink.DisableDirHack2D = true;
                if (tradeValue == "1")
                    MapManager.ObjLink.DisableDirHack2D = false;
                Game1.GameManager.SaveManager.RemoveString(tradeName);
            }

            var photoString = "fisherman_photo";
            var photoValue = Game1.GameManager.SaveManager.GetString(photoString);
            if (!string.IsNullOrEmpty(photoValue))
            {
                if (photoValue == "0")
                    MapManager.ObjLink.DisableDirHack2D = true;
                if (photoValue == "1")
                    MapManager.ObjLink.DisableDirHack2D = false;
                Game1.GameManager.SaveManager.RemoveString(photoString);
            }
        }
    }
}