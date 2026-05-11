using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动与旋转设置")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private float mouseSensitivity = 100f;
    private float xRotation = 0f;

    [Header("相机设置")]
    [SerializeField] private Transform _cameraRoot; // 请在Inspector拖入你的相机根物体（如Player下的CameraHolder）
    [SerializeField] private Transform _cameraRootFirstPerson; // 第一人称相机根物体  


    private CharacterController _controller;
    private Vector3 _velocity;
    [SerializeField] private float _gravity;
    private GameObject _currentCanGetObject = null;

    [SerializeField] private Transform _equipMentRoot;
    public static Transform EquipMentRoot;
    public static FarmLand CurrentFarmLand;

    public static GameObject CurrentEquipMentObject;

    private bool _isInteracting = false;
    private bool _canMove = true;

    private string _interactText;
    private string _interactButtonText;

    private Animator _animator;

    private bool isFirstPerson;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        CurrentFarmLand = null;
        CurrentEquipMentObject = null;
        EquipMentRoot = _equipMentRoot;
        _animator = GetComponent<Animator>();
        isFirstPerson = CameraSwitch.isFirstPerson;
    }

    void OnEnable()
    {
        CameraEvents.Center.AddListener(CameraEvent.SwitchCamera, SwitchCamera);
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始装备第一个物品
        EquipObject(ToolBarManager.Instance.currentItemSlot.currentItemData);
    }

    void Update()
    {
        // 角色移动
        if (_canMove)
        {
            PlayerControl();
        }

        //工具栏切换(1 - 7)
        ChangeObject();

        // 物品使用
        if (Input.GetMouseButtonDown(0))
        {
            if (!CurrentEquipMentObject) return;
            UseObject(CurrentEquipMentObject);
        }


        // 采集检测(没有可获取的物体时什么也不做)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"当前可采集物体: {_currentCanGetObject?.name ?? "无"}");
            if (!_currentCanGetObject) return;
            Debug.Log($"当前可采集物体: {_currentCanGetObject?.name ?? "无"}");
            GetObject(_currentCanGetObject);
        }

        if (_isInteracting)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                _canMove = false;
                PlayerEvents.Center.Trigger(PlayerEvent.EnterShop);
                PlayerEvents.Center.Trigger(PlayerEvent.ExitInteractPanel);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _canMove = true;
                PlayerEvents.Center.Trigger(PlayerEvent.ExitShop);
                PlayerEvents.Center.Trigger(PlayerEvent.EnterInteractPanel);
            }
        }
    }
    #region 核心逻辑拆分



    //人物控制
    void PlayerControl()
    {
        ReadInput();
        HandleMove();
        HandleRotation();
        HandleAnimation();
        HandleGravity();
    }

    float x;
    float z;
    Vector3 moveDir;
    bool isMoving;

    // 输入处理
    void ReadInput()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        Vector3 camForward = _cameraRoot.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = _cameraRoot.right;
        camRight.y = 0;
        camRight.Normalize();

        moveDir = camRight * x + camForward * z;
        isMoving = moveDir.magnitude > 0.1f;
    }

    // 移动处理
    void HandleMove()
    {
        if (!isMoving) return;
        if (!isFirstPerson)
        {
            _controller.Move(_moveSpeed * Time.deltaTime * moveDir);
        }
        else
        {
            Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 moveDirection = forward * z + right * x;

        _controller.Move(moveDirection.normalized * _moveSpeed * Time.deltaTime);
        }
        
    }

    // 旋转处理
    void HandleRotation()
    {
         if (isFirstPerson)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 上下（相机）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -40f, 80f);
        _cameraRootFirstPerson.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 左右（角色）
         transform.Rotate(Vector3.up * mouseX);
        }
        if (!isMoving) return;

       
        if (!isFirstPerson)
        {
            //  第三人称

            Vector3 forward = moveDir.normalized;
            forward.y = 0;

            RotateTo(forward);
        }
    }

    //旋转函数
    void RotateTo(Vector3 dir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            _rotateSpeed * Time.deltaTime
        );
    }


    //动画控制
    void HandleAnimation()
    {
        _animator.SetBool("IsMove", isMoving);
    }

    // 重力处理
    void HandleGravity()
    {
        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
    #endregion

    #region 物品与工具栏逻辑

    //使用物体
    void UseObject(GameObject obj)
    {
        //PlayerEvents.Center.Trigger<GameObject>(PlayerEvent.UseItem,obj);
        if (ToolBarManager.Instance.currentItemSlot != null)
        {
            ToolBarManager.Instance.currentItemSlot.OnSlotClick(obj);
        }
    }

    //获取物体
    void GetObject(GameObject obj)
    {
        Crop crop = obj.TryGetComponent(out Crop c) ? c : null;

        if (crop && CurrentFarmLand)
        {
            if (!crop.IsMature || !(crop.IsMature && CurrentFarmLand.FarmCurrentStatue == FarmCurrentStatus.Planted))
            {
                return;
            }
            else
            {
                CurrentFarmLand.FarmCurrentStatue = FarmCurrentStatus.Tilled;
            }
        }
        PlayerEvents.Center.Trigger(PlayerEvent.ExitInteractPanel);

        PlayerEvents.Center.Trigger<ItemData, int>(PlayerEvent.GetItem, obj.GetComponent<ItemModel>().itemData, 1);
        if (crop)
        {
            PlayerEvents.Center.Trigger<Crop>(PlayerEvent.HideCropProgress, crop);
        }
        EquipObject(ToolBarManager.Instance.currentItemSlot.currentItemData);
        Debug.Log($"采集了{obj.name}");
        Destroy(obj);
        _currentCanGetObject = null;
    }

    //切换物体
    void ChangeObject()
    {
        for (int i = 0; i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                PlayerEvents.Center.Trigger<int>(PlayerEvent.ChangeSlot, i);
                EquipObject(ToolBarManager.Instance.currentItemSlot.currentItemData); // 装备新物品

                // 触发工具名称弹窗
                ItemData currentItem = ToolBarManager.Instance.currentItemSlot?.currentItemData;
                if (currentItem != null)
                {
                    PlayerEvents.Center.Trigger<string>(PlayerEvent.ShowToolName, currentItem.itemName);
                }
                else
                {
                    PlayerEvents.Center.Trigger(PlayerEvent.HideToolName);
                }
            }
        }
    }

    //装备物体
    void EquipObject(ItemData itemData)
    {
        if (!itemData) return;
        if (EquipMentRoot.childCount != 0)
        {
            Destroy(EquipMentRoot.GetChild(0).gameObject); // 卸下当前装备的物品
        }

        CurrentEquipMentObject = Instantiate(itemData.itemPrefab, EquipMentRoot);
        CurrentEquipMentObject.GetComponent<Collider>().isTrigger = false; // 设置为触发器，避免物理碰撞

        // 这里可以实现装备物品的逻辑，如实例化装备模型并挂载到EquipMentRoot
        Debug.Log($"装备了{itemData.itemName}");
    }

    //切换相机视角 
    void SwitchCamera()
    {
        isFirstPerson = !isFirstPerson;
    }
    #endregion

    #region 触发器逻辑
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crop"))
        {
            _interactText = $"拾取";
            _interactButtonText = $"F";
            Crop crop = other.gameObject.GetComponent<Crop>();
            PlayerEvents.Center.Trigger<Crop>(PlayerEvent.ShowCropProgress, crop);
            if (crop.IsMature)
            {
                PlayerEvents.Center.Trigger<String, String>(PlayerEvent.EnterInteractPanel, _interactText, _interactButtonText);
                _currentCanGetObject = other.gameObject;
            }
        }
        if (other.CompareTag("Equipment"))
        {
            _interactText = $"拾取";
            _interactButtonText = $"F";
            PlayerEvents.Center.Trigger<String, String>(PlayerEvent.EnterInteractPanel, _interactText, _interactButtonText);
            _currentCanGetObject = other.gameObject;
        }
        if (other.CompareTag("FarmLand"))
        {
            CurrentFarmLand = other.GetComponent<FarmLand>();
            CurrentFarmLand.GetBorder().SetActive(true);
            Debug.Log("进入田地范围");
            Debug.Log(CurrentFarmLand.FarmCurrentStatue);
        }
        if (other.CompareTag("Shopkeeper"))
        {
            _isInteracting = true;
            _interactText = $"打开商店";
            _interactButtonText = $"X";
            PlayerEvents.Center.Trigger<String, String>(PlayerEvent.EnterInteractPanel, _interactText, _interactButtonText);
            Debug.Log("进入商店范围");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Crop") || other.CompareTag("Equipment"))
        {
            if (other.gameObject == _currentCanGetObject)
            {
                PlayerEvents.Center.Trigger(PlayerEvent.ExitInteractPanel);
                _currentCanGetObject = null;
            }
            if (other.CompareTag("Crop"))
            {
                Crop crop = other.gameObject.GetComponent<Crop>();
                PlayerEvents.Center.Trigger<Crop>(PlayerEvent.HideCropProgress, crop);
            }
        }
        if (other.CompareTag("FarmLand"))
        {
            CurrentFarmLand.GetBorder().SetActive(false);
            CurrentFarmLand = null;
             Debug.Log("离开田地范围");
        }
        if (other.CompareTag("Shopkeeper"))
        {
            _isInteracting = false;
            PlayerEvents.Center.Trigger(PlayerEvent.ExitInteractPanel);
            Debug.Log("离开商店范围");
        }
    }
    #endregion



    void OnDisable()
    {
        CameraEvents.Center.RemoveListener(CameraEvent.SwitchCamera, SwitchCamera);
    }
}