using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController)), RequireComponent(typeof(Player)), RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    private Action OnRecycle;

    private CharacterController _controller;
    private Player _player;
    private Tower _tower = null;
    private Camera _mainCamera;
    private GameObject _lockedOnEnemyGO = null;
    private GameObject _Barricade = null;
    private GameObject _RepairBarricade = null;
    //private GameObject _Resource = null;
    private UIManager uiManager;
    [HideInInspector]
    public NavMeshAgent agent;
    public GameObject enemyClickParticlePrefab;

    [Header("Unit Status")]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float minMoveSpeed = 10.0f;
    [SerializeField] private float maxMoveSpeed = 50.0f;
    [SerializeField] private float turnSpeed = 5.0f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deacceleration = 5f;
    [SerializeField] private float gravity = 1.0f;
    [SerializeField] private float burstSpeed = 40.0f;
    [SerializeField] private float attackCoolDown = 0.4f;
    [SerializeField] private float poisonAttackCoolDown = 3.0f;
    [SerializeField] private float intimidateAttackCoolDown = 5.0f;


    [Header("Trash Cans")]
    [SerializeField] [Tooltip("Timer for digging a trash cans")] private float diggingTime = 2.0f;
    private bool _isDigging = false;
    public int currentTrashes = 0;

    [SerializeField] private KeyCode _AttackButton = KeyCode.Space;
    [SerializeField] private KeyCode _PoisonAttackButton = KeyCode.E;
    [SerializeField] private KeyCode _LockTargetButton = KeyCode.Mouse0;
    [SerializeField] private KeyCode _UltimateButton = KeyCode.Q;
    [SerializeField] private KeyCode _PickUpButton = KeyCode.F;
    [SerializeField] private KeyCode _RepairButton = KeyCode.R;
    [SerializeField] private KeyCode _ReleaseLockButton = KeyCode.LeftShift;
    [SerializeField] private KeyCode _Intimidate = KeyCode.LeftControl;
    [SerializeField] private KeyCode _ClickMovementButton = KeyCode.Mouse1;
    [SerializeField] private KeyCode _ClickRestoreButton = KeyCode.H;
    [SerializeField] private KeyCode _ClickTowerButton = KeyCode.Mouse0;

    private bool _isTargetLockedOn = false;
    public bool IsLockedOn { get { return _isTargetLockedOn; } set { } }
    private bool _isHoldingItem = false;
    private bool _isRepairing = false;
    private bool _CanMove = true;

    private float currentAttackCoolDown = 0.0f;
    private float currentPoisonAttackCoolDown = 0.0f;
    private float currentIntimidateAttackCoolDown = 0.0f;
    private float holdClickTime = 0.0f;

    public float allowedRangeofResource = 3.5f;
    public float holdClickTimeMax = 2.0f;
    public bool isUsingMouseMovement = true;
    public bool attackEnabled = true;
    public bool poisonAttackEnabled = true;
    public bool intimidateAttackEnabled = true;
    public bool ultimateAttackEnabled = true;
    public bool autoAttack = true;
    public bool isUsingAbility = false;
    public bool isUsingUltimate = false;

    // For tutorial
    public bool usedFleasAbility = false;
    public bool usedStunAbility = false;
    public bool enableControls = true;

    public float stunTime = 0.0f;
    public bool isStunned = false;
    private float _timer = 0.0f;

    private UIbutton attackUIbutton;
    private UIbutton poisonUIbutton;
    private UIbutton intimidateUIbutton;
    private UIbutton ultUIbutton;

    private HapticFeedback hapticPoisonAbility;
    private HapticFeedback hapticIntimidateAbility;
    private HapticFeedback hapticUltAbility;

    //public Action onUsePoisonAbility;
    //public Action onUseIntimidateAbility;
    //public Action onUseUltAbility;

    private Animator animator;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        _controller = gameObject.GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        uiManager = ServiceLocator.Get<UIManager>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        poisonUIbutton = ServiceLocator.Get<UIManager>()?.poisonImg.GetComponent<UIbutton>();
        intimidateUIbutton = ServiceLocator.Get<UIManager>()?.intimidateImg.GetComponent<UIbutton>();
        ultUIbutton = ServiceLocator.Get<UIManager>()?.ultImg.GetComponent<UIbutton>();

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        VariableLoader variableLoader = ServiceLocator.Get<VariableLoader>();
        _player = ServiceLocator.Get<LevelManager>().playerInstance.GetComponent<Player>();
        _tower = ServiceLocator.Get<LevelManager>().towerInstance.GetComponent<Tower>();
        if (variableLoader.useGoogleSheets)
        {
            moveSpeed = variableLoader.PlayerStats["Speed"];
            agent.speed = moveSpeed;

            attackCoolDown = variableLoader.PlayerAbilties["Attack"]["Cooldown"];
            poisonAttackCoolDown = variableLoader.PlayerAbilties["Poison"]["Cooldown"];
            intimidateAttackCoolDown = variableLoader.PlayerAbilties["Stun"]["Cooldown"];
        }
        hapticPoisonAbility = poisonUIbutton.gameObject.GetComponent<HapticFeedback>();
        hapticIntimidateAbility = intimidateUIbutton.gameObject.GetComponent<HapticFeedback>();
        hapticUltAbility = ultUIbutton.gameObject.GetComponent<HapticFeedback>();
    }

    private void Update()
    {

        //if (stunTime < Time.time)
        //{
        //    agent.isStopped = false;
        //}
        //else
        //{
        //    agent.isStopped = true;
        //    return;
        //}

        if (isStunned)
        {
            agent.isStopped = true;
            if (stunTime < Time.time)
                isStunned = false;
            else
                return;
        }

        if (!_player.isAlive || !enableControls)
            return;
        if (_lockedOnEnemyGO)
        {
            if(_lockedOnEnemyGO.CompareTag("Enemy"))
            {
                if (_lockedOnEnemyGO.GetComponent<Enemy>().IsDead)
                {
                    _lockedOnEnemyGO = null;
                    _isTargetLockedOn = false;
                }
            }
            else if(_lockedOnEnemyGO.CompareTag("Boss"))
            {
                if (_lockedOnEnemyGO.GetComponent<Boss>().IsDead)
                {
                    _lockedOnEnemyGO = null;
                    _isTargetLockedOn = false;
                }
            }

        }

        ActivateTargetLockedOn();

        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            CalculateMovement();

        if (_isRepairing)
        {
            _isRepairing = _RepairBarricade.GetComponent<Barricade>().CheckRepairValid(transform);
        }
        UpdateUI();

        if (isUsingAbility)
            agent.isStopped = true;

        CheckMoveIndicatorActive();

        if (Input.GetMouseButtonDown(0))
        {
            CheckSpawnBarricade();
            CheckSpawnResource();
        }
        if (Input.GetMouseButton(0))
        {
            CheckRestoring();
        }

        //if (Input.GetKeyDown(_PickUpButton) || CheckHoldDownClick("BarricadeSpawner"))
        //{
        //    if (!_isHoldingItem)
        //    {
        //        _Barricade = _player.DetectBarricadeSpawner();
        //        if (_Barricade == null)
        //            _isHoldingItem = false;
        //        else if (_Barricade.GetComponent<Barricade>().CanBePickedUp())
        //        {
        //            _Barricade.GetComponent<Barricade>().PickUp(gameObject);
        //            _isHoldingItem = true;
        //        }
        //    }
        //    else if (!CheckBarricadePickUp())
        //    {
        //        StartCoroutine(PlaceBarricade());
        //    }

        //}

        //if (Input.GetKeyDown(_PickUpButton) || CheckHoldDownClick("ResourceSpawner"))
        //{
        //    if(!_isDigging)
        //    {
        //        _isDigging = true;
        //        StartCoroutine(DiggingTrash());
        //    }

        //}

        //if (placeUIbutton.isButtonPressed || CheckHoldDownClick("Ground"))
        //{
        //    if (_isHoldingItem && _Barricade != null)
        //    {
        //        _isHoldingItem = false;
        //        StartCoroutine(PlaceBarricade());
        //    }

        //}
        if (_isHoldingItem)
            return;

        //if (Input.GetKeyDown(_RepairButton) || CheckHoldDownClick("Barricade") || repairUIbutton.isButtonPressed)
        //{
        //    _RepairBarricade = _player.DetectBarricade();
        //    if (_RepairBarricade == null)
        //        _isHoldingItem = false;
        //    else if (!_isRepairing)
        //    {
        //        _isRepairing = true;
        //        _RepairBarricade.GetComponent<Barricade>().inRangeRepair = true;
        //        StartCoroutine(_RepairBarricade.GetComponent<Barricade>().Repair());
        //    }
        //}

        if (/*(Input.GetKeyDown(_AttackButton) && attackEnabled) ||*/ (autoAttack /*&& CheckCoolDownTimes()*/ && _isTargetLockedOn))
        {
            if (currentAttackCoolDown < Time.time)
            {
                StartCoroutine(_player.Attack());
                currentAttackCoolDown = Time.time + attackCoolDown;
            }
        }

        if ((Input.GetKeyDown(_PoisonAttackButton) || poisonUIbutton.isButtonPressed) && poisonAttackEnabled)
        {
            if (currentPoisonAttackCoolDown < Time.time)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Fleas") && !isUsingUltimate)
                {
                    usedFleasAbility = true;
                    agent.isStopped = true;
                    isUsingAbility = true;
                    animator.SetTrigger("Fleas");
                    StartCoroutine(_player.PoisonAttack());
                    hapticPoisonAbility?.Activate();
                    currentPoisonAttackCoolDown = Time.time + poisonAttackCoolDown;
                }

            }
        }

        if (Input.GetKeyDown(_LockTargetButton))
        {
            if (_isTargetLockedOn && !_lockedOnEnemyGO)
            {
                _isTargetLockedOn = false;
            }

            if (_isTargetLockedOn)
            {
                GameObject prevTarget = _lockedOnEnemyGO;
                CheckTargetLockedOn();

                if (_lockedOnEnemyGO != null && prevTarget != _lockedOnEnemyGO)
                {
                    //Deselect
                    if(prevTarget.CompareTag("Boss") && _lockedOnEnemyGO.CompareTag("Enemy"))
                    {
                        prevTarget?.GetComponent<Boss>().SwitchOnTargetIndicator(false);
                        _lockedOnEnemyGO?.GetComponent<Enemy>().SwitchOnTargetIndicator(true);
                    }
                    else if(prevTarget.CompareTag("Enemy") && _lockedOnEnemyGO.CompareTag("Enemy"))
                    {
                        prevTarget?.GetComponent<Enemy>().SwitchOnTargetIndicator(false);
                        _lockedOnEnemyGO?.GetComponent<Enemy>().SwitchOnTargetIndicator(true);
                    }
                    else if (prevTarget.CompareTag("Enemy") && _lockedOnEnemyGO.CompareTag("Boss"))
                    {
                        prevTarget?.GetComponent<Enemy>().SwitchOnTargetIndicator(false);
                        _lockedOnEnemyGO?.GetComponent<Boss>().SwitchOnTargetIndicator(true);
                    }
                }
                else
                {
                    _lockedOnEnemyGO = prevTarget;
                }
            }
            else
            {
                CheckTargetLockedOn();
                if (_lockedOnEnemyGO)
                {
                    //Select
                    _isTargetLockedOn = true;
                    if(_lockedOnEnemyGO.CompareTag("Enemy"))
                    {
                        _lockedOnEnemyGO.GetComponent<Enemy>()?.SwitchOnTargetIndicator(true);
                    }
                    else if(_lockedOnEnemyGO.CompareTag("Boss"))
                    {
                        _lockedOnEnemyGO.GetComponent<Boss>()?.SwitchOnTargetIndicator(true);
                    }

                }
                else
                    _isTargetLockedOn = false;
            }

        }

        if ((Input.GetKeyDown(_UltimateButton) || ultUIbutton.isButtonPressed) && ultimateAttackEnabled)
        {
            //_player.UltimateAttack();
            //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Ultimate"))
            //    animator.SetTrigger("Ultimate");
            ultUIbutton.isButtonPressed = false;
            if (_player._ultimateCharge >= 100.0f)
                hapticUltAbility?.Activate();
            StartCoroutine(_player.UltimateAttack());
        }

        if (Input.GetKeyDown(_ReleaseLockButton))
        {
            if (_isTargetLockedOn)
            {
                //Deselect
                _isTargetLockedOn = false;
                _lockedOnEnemyGO.GetComponent<Enemy>().SwitchOnTargetIndicator(false);
                _lockedOnEnemyGO = null;
            }
        }

        if ((Input.GetKeyDown(_Intimidate) || intimidateUIbutton.isButtonPressed) && intimidateAttackEnabled)
        {
            if (currentIntimidateAttackCoolDown < Time.time)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Intimidate") && !isUsingUltimate)
                {
                    usedStunAbility = true;
                    isUsingAbility = true;
                    agent.isStopped = true;
                    animator.SetTrigger("Intimidate");
                    hapticIntimidateAbility?.Activate();
                    StartCoroutine(_player.IntimidateAttack(/*_lockedOnEnemyGO*/));
                }
                currentIntimidateAttackCoolDown = Time.time + intimidateAttackCoolDown;
            }
        }

        //ActivateTargetLockedOn();

    }

    #endregion

    #region Ultility

    public void CheckMoveIndicatorActive()
    {
        List<GameObject> particles = ServiceLocator.Get<ObjectPoolManager>().GetActiveObjects("MoveIndicator");
        if (particles.Count > 0)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (Vector3.Distance(particles[i].transform.position, transform.position) < 1.0f)
                    ServiceLocator.Get<ObjectPoolManager>().RecycleObject(particles[i]);
                //else if (_isTargetLockedOn)
                //    Destroy(particles[i]);
            }
        }
    }

    public bool CheckHoldDownClick(string tagName)
    {
        if (Input.GetKey(_ClickMovementButton))
        {
            holdClickTime += Time.deltaTime;
            if (holdClickTime > holdClickTimeMax)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject.CompareTag(tagName))
                    {
                        holdClickTime = 0.0f;
                        return true;
                    }
                }
            }
        }
        else
        {
            holdClickTime = 0.0f;
        }
        return false;
    }

    public bool CheckBarricadePickUp()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("BarricadeSpawner"))
            {
                return true;
            }
        }
        return false;
    }

    public void CheckSpawnBarricade()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            GameObject go = hit.transform.gameObject;
            if (go.CompareTag("BarricadeSpawner"))
            {
                go.GetComponent<BarricadeSpawner>().SpawnBarricade();
            }
        }
    }

    public void CheckSpawnResource()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            GameObject go = hit.transform.gameObject;
            if (go.CompareTag("ResourceSpawner"))
            {
                go.GetComponent<ResourceSpawner>().GettingResource();
            }
        }
    }

    public void CheckRestoring()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            GameObject go = hit.transform.gameObject;
            if (go.CompareTag("Restoring"))
            {
                GameObject tower = ServiceLocator.Get<LevelManager>().towerInstance;
                tower.GetComponent<Tower>().restoring();
            }
        }
    }

    public bool CheckCoolDownTimes()
    {
        if (currentPoisonAttackCoolDown < Time.time && currentIntimidateAttackCoolDown < Time.time)
        {
            return true;
        }
        return false;
    }

    public bool CheckUIbuttonPressed()
    {
        if (/*!attackUIbutton.isButtonPressed &&*/ !ultUIbutton.isButtonPressed
            && !poisonUIbutton.isButtonPressed && !intimidateUIbutton.isButtonPressed
            /*&& !placeUIbutton.isButtonPressed && !repairUIbutton.isButtonPressed*/)
            return true;
        return false;
    }

    public void CalculateMovement()
    {
        if (!_CanMove || _isDigging)
            return;

        if (isUsingMouseMovement)
        {
            //Movement
            agent.speed = moveSpeed;
            //Rotation
            Vector3 direction = agent.destination - transform.position;
            if (direction.magnitude > 0.0f)
            {
                Quaternion newDirection = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, newDirection, Time.deltaTime * turnSpeed);
            }

            if (isUsingAbility || isUsingUltimate)
            {
                agent.isStopped = true;
                return;
            }

            if (Input.GetKeyDown(_ClickMovementButton))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {
                    if (/*(hit.transform.gameObject.CompareTag("Ground") || hit.transform.gameObject.CompareTag("PickUp")) &&*/
                        CheckUIbuttonPressed() && (hit.transform.gameObject.CompareTag("Ground")))
                    {
                        agent.isStopped = true;
                        agent.SetDestination(hit.point);
                        agent.isStopped = false;

                        //Deselect
                        _isTargetLockedOn = false;
                        _lockedOnEnemyGO?.GetComponent<Enemy>()?.SwitchOnTargetIndicator(false);
                        _lockedOnEnemyGO = null;

                        //Show MoveIndicator
                        ServiceLocator.Get<ObjectPoolManager>().RecycleAllObjects("MoveIndicator");
                        GameObject moveIndicator = ServiceLocator.Get<ObjectPoolManager>().GetObjectFromPool("MoveIndicator");
                        moveIndicator.transform.position = agent.destination;
                        moveIndicator.SetActive(true);

                        return;
                    }
                }
            }

            //Attack target
            if (_isTargetLockedOn)
            {
                if (_lockedOnEnemyGO)
                    agent.SetDestination(_lockedOnEnemyGO.transform.position);
                agent.isStopped = false;
                Vector3 look = agent.destination;
                look.y = transform.position.y;
                transform.LookAt(look);
                if (Vector3.Distance(transform.position, agent.destination) < _player.attackRange && !isUsingUltimate)
                {
                    agent.isStopped = true;
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Scratch") && !isUsingAbility)
                        animator.SetTrigger("Scratch");
                    return;
                }
            }
            if (agent.velocity.magnitude > 0.1f)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("RunCycle") && !isUsingAbility)
                    animator.SetTrigger("Run");
            }
            if (agent.velocity.magnitude <= 0.1f)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("IdleCycle") && !isUsingAbility)
                    animator.SetTrigger("Idle");
            }
            return;
        }

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        Vector3 forward = _mainCamera.transform.forward;
        Vector3 right = _mainCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * vertical + right * horizontal;

        if (move.magnitude == 0.0f)
        {
            moveSpeed = minMoveSpeed + burstSpeed;
        }

        if (move.magnitude > 0 && moveSpeed < maxMoveSpeed)
        {
            moveSpeed += (acceleration * Time.deltaTime);
        }
        else if (moveSpeed > minMoveSpeed)
        {
            moveSpeed -= (deacceleration * Time.deltaTime);
        }

        if (move.magnitude > 0)
        {
            Quaternion newDirection = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, newDirection, Time.deltaTime * turnSpeed);
        }

        move.y -= gravity;

        _controller.Move(move * Time.deltaTime * moveSpeed);

    }

    public IEnumerator PlaceBarricade()
    {
        _CanMove = false;
        yield return new WaitForSeconds(_Barricade.GetComponent<Barricade>()._barricadeBuildTime);
        _Barricade?.GetComponent<Barricade>().PlaceBarricade();
        _Barricade = null;
        _isHoldingItem = false;
        _CanMove = true;
    }

    #endregion

    #region Target Lock on

    public void ActivateTargetLockedOn()
    {
        if (_isTargetLockedOn && _lockedOnEnemyGO)
        {
            if (_lockedOnEnemyGO.activeInHierarchy)
            {
                Vector3 targetDirection = _lockedOnEnemyGO.transform.position;
                targetDirection.y = transform.position.y;
                transform.LookAt(targetDirection);
            }
            else
            {
                //Deselect
                if(_lockedOnEnemyGO.CompareTag("Enemy"))
                {
                    _lockedOnEnemyGO.GetComponent<Enemy>()?.SwitchOnTargetIndicator(false);
                }
                else if(_lockedOnEnemyGO.CompareTag("Boss"))
                {
                    _lockedOnEnemyGO.GetComponent<Boss>()?.SwitchOnTargetIndicator(false);
                }
                _isTargetLockedOn = false;
                _lockedOnEnemyGO = null;
            }
        }
    }

    public void CheckTargetLockedOn()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Enemy"))
            {
                if (!hit.transform.gameObject.GetComponent<Enemy>().IsDead)
                {
                    _lockedOnEnemyGO = hit.transform.gameObject;
                    GameObject go = Instantiate(enemyClickParticlePrefab, _lockedOnEnemyGO.transform);
                }
                else
                {
                    _lockedOnEnemyGO = null;
                }
            }
            else if (hit.transform.gameObject.CompareTag("Boss"))
            {
                if (!hit.transform.gameObject.GetComponent<Boss>().IsDead)
                {
                    _lockedOnEnemyGO = hit.transform.gameObject;
                    GameObject go = Instantiate(enemyClickParticlePrefab, _lockedOnEnemyGO.transform);
                }
                else
                {
                    _lockedOnEnemyGO = null;
                }
            }
            else
                _lockedOnEnemyGO = null;
        }
    }

    public void SwitchAutoLock(GameObject enemy)
    {
        if(enemy.CompareTag("Enemy"))
        {
            _lockedOnEnemyGO?.GetComponent<Enemy>()?.SwitchOnTargetIndicator(false);
            _lockedOnEnemyGO = enemy;
            _isTargetLockedOn = true;
            _lockedOnEnemyGO?.GetComponent<Enemy>()?.SwitchOnTargetIndicator(true);
        }
        else if (enemy.CompareTag("Boss"))
        {
            _lockedOnEnemyGO?.GetComponent<Boss>()?.SwitchOnTargetIndicator(false);
            _lockedOnEnemyGO = enemy;
            _isTargetLockedOn = true;
            _lockedOnEnemyGO?.GetComponent<Boss>()?.SwitchOnTargetIndicator(true);
        }
    }

    #endregion

    #region Getters

    public GameObject GetLockedOnTarget()
    {
        return _lockedOnEnemyGO;
    }

    public CharacterController GetController()
    {
        return _controller;
    }

    #endregion

    #region UI
    public void UpdateUI()
    {
        float fill;
        //    (currentAttackCoolDown - Time.time) / attackCoolDown;
        //fill = Mathf.Clamp(fill, 0.0f, 1.0f);
        //uiManager.UpdateImage(DamageType.Normal, fill);

        fill = (currentPoisonAttackCoolDown - Time.time) / poisonAttackCoolDown;
        fill = Mathf.Clamp(fill, 0.0f, 1.0f);
        uiManager.UpdateImage(DamageType.Poison, fill);

        fill = (currentIntimidateAttackCoolDown - Time.time) / intimidateAttackCoolDown;
        fill = Mathf.Clamp(fill, 0.0f, 1.0f);
        uiManager.UpdateImage(DamageType.Intimidate, fill);

        fill = ((_player._ultimateCharge) / 100.0f) - 1.0f;
        fill = Mathf.Clamp(-fill, 0.0f, 1.0f);
        uiManager.UpdateImage(DamageType.Ultimate, fill);

        fill = (holdClickTime / holdClickTimeMax);
        fill = Mathf.Clamp(fill, 0.0f, 1.0f);
        if (fill < 0.5f)
            fill = 0.0f;
        uiManager.UpdateImage(DamageType.Loading, fill);

        uiManager.repairIcon.enabled = _isRepairing;
    }

    #endregion

    public void EnablePoisonAttack(bool enable = true)
    {
        UIManager uiManager = ServiceLocator.Get<UIManager>();
        uiManager.poisonImg.SetActive(enable);
        poisonAttackEnabled = enable;
    }
    public void EnableIntimidateAttack(bool enable = true)
    {
        UIManager uiManager = ServiceLocator.Get<UIManager>();
        uiManager.intimidateImg.SetActive(enable);
        intimidateAttackEnabled = enable;
    }
    public void EnableUltAttack(bool enable = true)
    {
        UIManager uiManager = ServiceLocator.Get<UIManager>();
        uiManager.ultImg.SetActive(enable);
        ultimateAttackEnabled = enable;
    }
}