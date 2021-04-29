using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace BNEGame
{
    using Network;

    [System.Serializable]
    public class QuiverNode
    {
        [SerializeField] private ArrowTypes _type;
        [SerializeField] private int _leftover;

        public ArrowTypes Type
        {
            set => _type = value;
            get => _type;
        }

        public int LeftoverAmount
        {
            set => _leftover = value;
            get => _leftover;
        }
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerEntity : LivingEntity
    {
        [Header("Player Informations")]
        [SerializeField] private float _friction = 1f;
        [SerializeField] private Transform _aimPivot = null;
        [SerializeField] private Transform _bowNeedle = null;
        [SerializeField] private Animator _animator = null;
        [SerializeField] private SpriteRenderer _anticipatorRenderer = null;
        [SerializeField] private LayerMask _floatingItemMask = ~0;

        private int currentUse = -1;
        private float gravityScaleHolder;
        private BoxCollider2D playerCollider;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isGrounded = false;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isHitLeftWall = false;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isHitRightWall = false;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isHitCeiling = false;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private int enemyKilled = 0;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 aimDir = Vector2.zero;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<QuiverNode> quiver = new List<QuiverNode>();
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool ableToCollectItem = false;

        #region Properties
        public bool IsGrounded => isGrounded;
        public bool IsHitLeftWall => isHitLeftWall;
        public bool IsHitRightWall => isHitRightWall;
        public bool AbleToCollectItem
        {
            set => ableToCollectItem = value;
            get => ableToCollectItem;
        }
        public Animator PlayerAnim => _animator;
        public List<QuiverNode> ArrowCollection => quiver;
        public Vector2 AimDirection
        {
            set
            {
                aimDir = value;

                float degreeAngle = Mathf.Atan(aimDir.y / aimDir.x) * Mathf.Rad2Deg;

                if (aimDir.x < 0)
                    degreeAngle += 180f;

                _aimPivot.eulerAngles = new Vector3(0, 0, degreeAngle);
            }
            get => aimDir;
        }
        public ArrowTypes CurrentArrowUse
        {
            set
            {
                if (value == ArrowTypes.None && quiver.Count == 0)
                {
                    currentUse = -1;
                    return;
                }

                for (int i = 0; i < quiver.Count; i++)
                {
                    QuiverNode node = quiver[i];
                    if (node.Type == value)
                    {
                        currentUse = i;
                        break;
                    }
                }
            }
            get
            {
                if (currentUse < 0)
                    return ArrowTypes.None;

                return quiver[currentUse].Type;
            }
        }
        public int EnemyKillCount
        {
            set => enemyKilled = value;
            get => enemyKilled;
        }
        public Vector2 BowShootOrigin => _bowNeedle == null ? transform.position : _bowNeedle.position;
        #endregion

        #region Unity BuiltIn Methods
        private void OnEnable()
        {
            // Subscribe event
            EventHandler.OnPlayerChangeArrowEvent += PlayerChangeArrow;

            ResetEntityValues();
            InformationUI.ShowUIFollower(true);
        }

        // Start is called before the first frame update
        private void Start()
        {
            playerCollider = GetComponent<BoxCollider2D>();
            gravityScaleHolder = EntityR2D.gravityScale;
            EntityR2D.gravityScale = 0;
            InformationUI.MaxHealthValue = MaxHealth;
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            CheckBeforeHitWall();

            if (ableToCollectItem)
                HandleCollectedItem();
        }

        private void OnDisable()
        {
            // Unsubscribe event
            EventHandler.OnPlayerChangeArrowEvent -= PlayerChangeArrow;

            CameraAutoController autoCam = FindObjectOfType<CameraAutoController>();
            if (tag == "MainPlayer" && autoCam != null)
                autoCam.CenterHook = null;

            InformationUI.ShowUIFollower(false);
        }

        private void OnDrawGizmos()
        {
            Vector2 vel = EntityR2D.velocity;
            Gizmos.DrawLine(transform.position, transform.position + (new Vector3(vel.x, vel.x, 0)).normalized);
        }
        #endregion

        #region Event Methods
        private void PlayerChangeArrow(PlayerChangeArrowEventArgs args)
        {
            if (args.Player.gameObject.Equals(gameObject))
            {
                ArrowQuiverElement e = ObjectManager._instance.GetArrowElement(args.ChangeTo);
                _anticipatorRenderer.sprite = e == null ? null : e.ItemSprite;
                _anticipatorRenderer.transform.localEulerAngles = e == null ? Vector3.zero : new Vector3(0, 0, e.OffsetDegreeAnticipation);
            }
        }
        #endregion

        #region Overriden Methods
        public override void AddHealth(int h)
        {
            CurrentHealth += h;
            CurrentHealth = MaxHealth < CurrentHealth ? MaxHealth : (CurrentHealth < 0) ? 0 : CurrentHealth;
            InformationUI.HealthValue = CurrentHealth;
        }

        public override void AddEffects(EntityEffects effect, float value, bool temporary = true)
        {
            if (temporary)
                StartCoroutine(TemporaryEffectRoutine(effect, -value, 3f));
        }

        public override void ResetEntityValues()
        {
            // Return back to max health
            CurrentHealth = MaxHealth;

            // Local GUI Sync
            InformationUI.HealthValue = CurrentHealth;
            InformationUI.DrawingTimeValue = 0;

            // Clear inventory when died
            quiver.Clear();
        }

        protected override IEnumerator TemporaryEffectRoutine(EntityEffects effect, float negativeValue, float seconds)
        {
            float tempTime = seconds;
            while (tempTime > 0)
            {
                tempTime -= Time.deltaTime;
                yield return null;
            }

            AddEffects(effect, negativeValue);
        }
        #endregion

        private void CheckGrounded()
        {
            // Get collider information
            Vector3 centerCol = playerCollider.bounds.center, extentCol = playerCollider.size / 2;

            // Check casting the ground sensitivity
            Vector2 boxSize = new Vector2(playerCollider.size.x, DETECT_RANGE);
            Vector3 origin = centerCol - new Vector3(0, extentCol.y + boxSize.y, 0);
            RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, boxSize.y, _groundMask);

            // Check grounded
            if (hit.collider != null)
            {
                isGrounded = true;
                if (hit.collider.tag == "Platform")
                {
                    if (EntityR2D.velocity.y > 0)
                        isGrounded = false;
                }
            }
            else
            {
                isGrounded = false;

                // Making sure not ignoring falling to Ground or Platform
                if (EntityR2D.velocity.y < 0)
                {
                    // Check casting before hit the ground by velocity
                    Vector2 boxSizeByVelY = new Vector2(playerCollider.size.x, Mathf.Abs(EntityR2D.velocity.y) * Time.deltaTime);
                    RaycastHit2D beforeHit = Physics2D.BoxCast(origin, boxSizeByVelY, 0, Vector2.zero, 0, _groundMask);

                    if (beforeHit.collider != null)
                    {
                        transform.Translate(EntityR2D.velocity.normalized * beforeHit.distance);
                        isGrounded = true;
                    }
                }
            }

            // Calculate gravity & mass
            Vector2 vel = EntityR2D.velocity;
            if (!isGrounded)
            {
                vel.y += Physics.gravity.y * gravityScaleHolder * Time.deltaTime;
            }
            else
            {
                float normalWeight = EntityR2D.mass * (Physics.gravity.y * gravityScaleHolder);
                float slowdownByTime = Mathf.Abs((normalWeight * _friction) / EntityR2D.mass) * Time.deltaTime;

                float xHolder = vel.x;
                vel.x = vel.x < 0 ? vel.x + slowdownByTime : (vel.x > 0) ? vel.x - slowdownByTime : 0;
                vel.x = (vel.x > 0 && xHolder < 0) || (vel.x < 0 && xHolder > 0) ? 0 : vel.x;

                if (vel.y <= 0)
                    vel.y = 0;
            }
            EntityR2D.velocity = vel;

#if UNITY_EDITOR
            // Debugger
            Vector2 leftMostBound = new Vector2(centerCol.x - extentCol.x, centerCol.y - extentCol.y);
            Vector2 rightMostBound = new Vector2(centerCol.x + extentCol.x, centerCol.y - extentCol.y);
            float distance = Vector2.Distance(leftMostBound, rightMostBound);

            Color debugColor = isGrounded ? Color.green : Color.red;
            Debug.DrawRay(leftMostBound, -new Vector2(0, DETECT_RANGE), debugColor);
            Debug.DrawRay(rightMostBound, -new Vector2(0, DETECT_RANGE), debugColor);
            Debug.DrawRay(leftMostBound, new Vector2(distance, 0), debugColor);
            Debug.DrawRay(leftMostBound - new Vector2(0, DETECT_RANGE), new Vector2(distance, 0), debugColor);
#endif
        }

        private void CheckBeforeHitCeiling()
        {
            // Get collider information
            Vector3 centerCol = playerCollider.bounds.center, extentCol = playerCollider.size / 2;

            // Condition
            Vector2 boxSize = new Vector2(playerCollider.size.x, Mathf.Abs(EntityR2D.velocity.y));
            Vector3 origin = centerCol + new Vector3(0, extentCol.y + boxSize.y, 0);
        }

        private void CheckBeforeHitWall()
        {
            // Get collider information
            Vector3 centerCol = playerCollider.bounds.center, extentCol = playerCollider.size / 2;

            // Condition
            Vector2 boxSize = new Vector2(DETECT_RANGE, playerCollider.size.y - (playerCollider.size.y / 4f));
            Vector3 originLeft = centerCol - new Vector3(extentCol.x + boxSize.x, 0);
            Vector3 originRight = centerCol + new Vector3(extentCol.x + boxSize.x, 0);

            // Check hit
            RaycastHit2D leftHit = Physics2D.BoxCast(originLeft, boxSize, 0, Vector2.left, -DETECT_RANGE, ~_nonWallMask);
            RaycastHit2D rightHit = Physics2D.BoxCast(originRight, boxSize, 0, Vector2.right, DETECT_RANGE, ~_nonWallMask);

            isHitLeftWall = leftHit.collider == null ? false : true;
            isHitRightWall = rightHit.collider == null ? false : true;

#if UNITY_EDITOR
            // Debugger
            Vector2 upperLeftMostBound = new Vector2(originLeft.x, originLeft.y) + new Vector2(-boxSize.x / 2, boxSize.y / 2);
            Vector2 upperRightMostBound = new Vector2(originRight.x, originRight.y) + new Vector2(boxSize.x / 2, boxSize.y / 2);

            // Draw ray left
            Color debugColor = isHitLeftWall ? Color.green : Color.red;
            Debug.DrawRay(upperLeftMostBound, new Vector2(boxSize.x, 0), debugColor);
            Debug.DrawRay(upperLeftMostBound, new Vector2(0, -boxSize.y), debugColor);
            Debug.DrawRay(upperLeftMostBound + new Vector2(0, -boxSize.y), new Vector2(boxSize.x, 0), debugColor);
            Debug.DrawRay(upperLeftMostBound + new Vector2(boxSize.x, 0), new Vector2(0, -boxSize.y), debugColor);

            // Draw ray right
            debugColor = isHitRightWall ? Color.green : Color.red;
            Debug.DrawRay(upperRightMostBound, new Vector2(-boxSize.x, 0), debugColor);
            Debug.DrawRay(upperRightMostBound, new Vector2(0, -boxSize.y), debugColor);
            Debug.DrawRay(upperRightMostBound + new Vector2(0, -boxSize.y), new Vector2(-boxSize.x, 0), debugColor);
            Debug.DrawRay(upperRightMostBound + new Vector2(-boxSize.x, 0), new Vector2(0, -boxSize.y), debugColor);
#endif
        }

        private void HandleCollectedItem()
        {
            // Get collision information
            RaycastHit2D hit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.size, 0, Vector2.zero, 0, _floatingItemMask);
            if (hit.collider != null)
            {
                // Check player collected an item
                FloatingItemBehaviour fItem = hit.collider.gameObject.GetComponent<FloatingItemBehaviour>();
                if (fItem != null)
                {
                    PlayerCollectItemEventArgs arg = new PlayerCollectItemEventArgs(fItem.OnCollectionIndex, this, fItem);
                    EventHandler.CallEvent(arg);

                    if (!arg.IsCancelled)
                    {
                        // Check player collected the item
                        if (!arg.Player.gameObject.Equals(gameObject))
                            return;

                        // Check arrow collected
                        if (arg.InfoElement is ArrowQuiverElement)
                        {
                            ArrowQuiverElement arrowElement = (ArrowQuiverElement)arg.InfoElement;
                            CollectArrow(arrowElement.Type, arrowElement.CollectAmount);
                        }

                        // Disable floating item, means that the item has been collected by player
                        arg.CollectedItem.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void BowShoot(Vector2 shootDir, Vector2 originPos, bool fromServer = false)
        {
            if (networkedPlayer != null && !fromServer)
            {
                networkedPlayer.SyncShootArrow(shootDir);
                return;
            }
            
            switch (CurrentArrowUse)
            {
                case ArrowTypes.ChopStick:
                    float offsetDegree = 15f;
                    float degreeChopstick = Mathf.Atan(shootDir.y / shootDir.x) * Mathf.Rad2Deg;
                    Shoot(CurrentArrowUse, RadianToVector2((degreeChopstick + offsetDegree + (shootDir.x < 0 ? 180 : 0)) * Mathf.Deg2Rad), originPos);
                    Shoot(CurrentArrowUse, RadianToVector2((degreeChopstick - offsetDegree + (shootDir.x < 0 ? 180 : 0)) * Mathf.Deg2Rad), originPos);
                    break;

                default:
                    Shoot(ArrowTypes.Normal, shootDir, originPos);
                    break;
            }
        }

        private bool Shoot(ArrowTypes type, Vector2 shootDir, Vector2 originPos)
        {
            ArrowBehaviour arrow = ObjectManager._instance.ArrowMaker.GetObjectRequired(CurrentArrowUse);
            if (!arrow.gameObject.activeSelf)
                arrow.gameObject.SetActive(true);

            // Call event first then shoot
            PlayerShootEventArgs arg = new PlayerShootEventArgs(this, shootDir, arrow);
            EventHandler.CallEvent(arg);

            if (!arg.IsCancelled)
            {
                arrow.Shoot(originPos, arg.ShootDirection);

                // Check inventory usage
                if (quiver.Find(q => q.Type == type) != null)
                    CollectArrow(type, -1);
            }

            return !arg.IsCancelled;
        }

        /// <summary>
        /// Arrow roller to choose which arrow will be use.
        /// </summary>
        public void NextArrowUse()
        {
            if (currentUse < 0)
                return;

            currentUse = currentUse + 1 >= quiver.Count ? 0 : currentUse + 1;
            EventHandler.CallEvent(new PlayerChangeArrowEventArgs(this, CurrentArrowUse));
        }

        public void CollectArrow(ArrowTypes type, int amount)
        {
            ArrowTypes temp = CurrentArrowUse;

            // Check if already have arrow
            QuiverNode node = quiver.Find(q => q.Type == type);
            int onCollectionIndex;
            if (node != null)
            {
                onCollectionIndex = quiver.IndexOf(node);
                node.LeftoverAmount += amount;

                // Check out of arrow
                if (node.LeftoverAmount <= 0)
                    quiver.Remove(node);
            }
            else
            {
                // Check whether the 'get' amount is invalid
                if (amount <= 0)
                    return;

                // Add new arrow to collection
                node = new QuiverNode() { Type = type, LeftoverAmount = amount };
                onCollectionIndex = quiver.Count;
                quiver.Add(node);
            }

            // Check if index exceeded the length of list
            if (currentUse >= quiver.Count)
            {
                // Check quiver empty
                if (quiver.Count == 0)
                    currentUse = -1;
            }

            // Check if new arrow was collected when the quiver was empty
            if (temp == ArrowTypes.None && quiver.Count > 0)
                currentUse = currentUse + 1 >= quiver.Count ? 0 : currentUse + 1;

            // Check if current use arrow has to change
            if (CurrentArrowUse != temp)
                EventHandler.CallEvent(new PlayerChangeArrowEventArgs(this, CurrentArrowUse));

            // Sync player collected arrow
            networkedPlayer?.SyncCollectArrowInventory(onCollectionIndex, type, amount);
        }

        #region Static Utility Methods
        private static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }
        #endregion
    }

}
