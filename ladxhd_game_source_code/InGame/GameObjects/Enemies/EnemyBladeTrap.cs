using Microsoft.Xna.Framework;
using ProjectZ.Base;
using ProjectZ.InGame.GameObjects.Base;
using ProjectZ.InGame.GameObjects.Base.CObjects;
using ProjectZ.InGame.GameObjects.Base.Components;
using ProjectZ.InGame.GameObjects.Base.Components.AI;
using ProjectZ.InGame.Map;
using ProjectZ.InGame.SaveLoad;
using ProjectZ.InGame.Things;

namespace ProjectZ.InGame.GameObjects.Enemies
{
    internal class EnemyBladeTrap : GameObject
    {
        private readonly AiComponent _aiComponent;

        private readonly RectangleF[] _collisionRectangles = new RectangleF[4];
        private readonly Vector2[] _directions = { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) };
        private readonly int[] _maxPosition = new int[4];

        private const int MovementStep = 2;

        private Vector2 _startPosition;
        private float _movePosition;
        private int _moveDir;

        public EnemyBladeTrap() : base("bladeTrap") { }

        public EnemyBladeTrap(Map.Map map, int posX, int posY, int left, int right, int top, int bottom) : base(map)
        {
            Tags = Values.GameObjectTag.Damage;

            EntityPosition = new CPosition(posX, posY, 0);
            ResetPosition  = new CPosition(posX, posY, 0);
            _startPosition = new Vector2(posX, posY);
            CanReset = true;
            OnReset += Reset;

            var animator = AnimatorSaveLoad.LoadAnimator("Enemies/bladetrap");
            animator.Play("idle");

            var sprite = new CSprite(EntityPosition);
            var animationComponent = new AnimationComponent(animator, sprite, new Vector2(0, 0));

            var padding = 2;
            var width = 16 + 2 * padding;
            var height = 16 + 2 * padding;

            _maxPosition[0] = left * 16;
            _maxPosition[1] = right * 16;
            _maxPosition[2] = top * 16;
            _maxPosition[3] = bottom * 16;

            _collisionRectangles[0] = new RectangleF(posX - left * 16 - 16, posY - padding, left * 16 + 16, height);
            _collisionRectangles[1] = new RectangleF(posX + 16, posY - padding, right * 16 + 16, height);
            _collisionRectangles[2] = new RectangleF(posX - padding, posY - top * 16 - 16, width, top * 16 + 16);
            _collisionRectangles[3] = new RectangleF(posX - padding, posY + 16, width, bottom * 16 + 16);

            var stateWait = new AiState();
            stateWait.Trigger.Add(new AiTriggerCountdown(350, null, () => _aiComponent.ChangeState("back")));
            var stateCooldown = new AiState();
            stateCooldown.Trigger.Add(new AiTriggerCountdown(500, null, () => _aiComponent.ChangeState("idle")));

            _aiComponent = new AiComponent();
            _aiComponent.States.Add("idle", new AiState(UpdateIdle));
            _aiComponent.States.Add("snap", new AiState(UpdateSnap));
            _aiComponent.States.Add("wait", stateWait);
            _aiComponent.States.Add("back", new AiState(UpdateMoveBack));
            _aiComponent.States.Add("cooldown", stateCooldown);
            _aiComponent.ChangeState("idle");

            var bodyBox = new CBox(EntityPosition, 0, 0, 0, 16, 16, 4);
            var damageBox =  new CBox(EntityPosition, 6, 6, 0, 4, 4, 4);

            AddComponent(PushableComponent.Index, new PushableComponent(bodyBox, OnPush));
            AddComponent(DamageFieldComponent.Index, new DamageFieldComponent(damageBox, HitType.Enemy, 4));
            AddComponent(BaseAnimationComponent.Index, animationComponent);
            AddComponent(AiComponent.Index, _aiComponent);
            AddComponent(HittableComponent.Index, new HittableComponent(bodyBox, OnHit));
            AddComponent(DrawComponent.Index, new DrawCSpriteComponent(sprite, Values.LayerPlayer));
        }

        private void Reset()
        {
            _movePosition = 0;
            UpdatePosition();
            _aiComponent.ChangeState("idle");
        }

        private bool OnPush(Vector2 direction, PushableComponent.PushType type)
        {
            return true;
        }

        private Values.HitCollision OnHit(GameObject originObject, Vector2 direction, HitType hitType, int damage, bool pieceOfPower)
        {
            // Because of the way the hit system works, this needs to be in any hit that doesn't default to "None" hit collision.
            if ((hitType & HitType.CrystalSmash) != 0 || (hitType & HitType.ClassicSword) != 0)
                return Values.HitCollision.None;

            return Values.HitCollision.RepellingParticle;
        }

        private int PredictMaxMove(int dirIndex)
        {
            int step = 1;
            int max = _maxPosition[dirIndex];
            Vector2 direction = _directions[dirIndex];

            // Sweep forward until max distance
            for (int dist = 0; dist <= max; dist += step)
            {
                float x = _startPosition.X + direction.X * dist;
                float y = _startPosition.Y + direction.Y * dist;

                // Check collision at this position
                Box block = Box.Empty;
                Box box = new Box(x, y, 0, 16, 16, 4);
                if (Map.Objects.Collision(box, Box.Empty, Values.CollisionTypes.Normal, 0, 0, ref block))
                    return dist;
            }
            return max;
        }

        private void UpdateIdle()
        {
            // Loop through the directions the trap can move.
            for (int i = 0; i < _directions.Length; i++)
            {
                // Check if a direction intercepts Link.
                if (_collisionRectangles[i].Intersects(MapManager.ObjLink.BodyRectangle))
                {
                    // Predict the amount of pixels it can move.
                    int predicted = PredictMaxMove(i);

                    // The predicted value must exceed it's movement step.
                    if (predicted < MovementStep)
                        return;

                    // Set the maximum position it can move.
                    _maxPosition[i] = predicted;

                    // Play the movement sound effect and start moving.
                    Game1.GameManager.PlaySoundEffect("D378-10-0A");
                    _aiComponent.ChangeState("snap");
                    _moveDir = i;
                }
            }
        }

        private void UpdateSnap()
        {
            // Move the trap towards Link's position.
            _movePosition += MovementStep * Game1.TimeMultiplier;

            // Reached the end of its destination.
            if (_movePosition >= _maxPosition[_moveDir])
            {
                // Play the collision sound effect and snap the trap into place.
                _movePosition = _maxPosition[_moveDir];
                _aiComponent.ChangeState("wait");
                Game1.GameManager.PlaySoundEffect("D360-07-07");
            }
            // Update the trap's position until it reaches it's max movement value.
            UpdatePosition();
        }

        private void UpdateMoveBack()
        {
            // Move the trap back into it's original position.
            if (_movePosition > 0)
                _movePosition -= 0.5f * Game1.TimeMultiplier;

            // When it reaches it's destination, snap it into place.
            else
            {
                _movePosition = 0;
                _aiComponent.ChangeState("cooldown");
            
            }
            // Update the trap's position until it reaches it's original position.
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            // Update the position of the trap.
            EntityPosition.Set(_startPosition + _directions[_moveDir] * _movePosition);
        }
    }
}