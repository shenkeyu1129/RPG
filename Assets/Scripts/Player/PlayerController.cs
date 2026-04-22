using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动与旋转设置")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _mouseSensitivity;

    [Header("相机设置")]
    [SerializeField] private Transform _cameraRoot; // 请在Inspector拖入你的相机根物体（如Player下的CameraHolder）

    private float _xRotation = 0f;
    private CharacterController _controller;
    private Vector3 _velocity;
    [SerializeField] private float _gravity;
    private GameObject _currentCanGetObject = null;

    [SerializeField] private Transform _equipMentRoot;
    public static Transform EquipMentRoot;
    public static FarmLand CurrentFarmLand;

    public static GameObject CurrentEquipMentObject;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        CurrentFarmLand = null;
        CurrentEquipMentObject = null;
        EquipMentRoot = _equipMentRoot;
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
        // 角色自身旋转（左右看）
        LookRotation();

        // 角色移动
        Move();

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
            if (!_currentCanGetObject) return;
            GetObject(_currentCanGetObject);
        }
    }

    void LateUpdate()
    {
        //相机旋转（上下看）
        LookCamera();

    }

    #region 核心逻辑拆分
    // 人物左右旋转
    void LookRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    // 相机上下旋转
    void LookCamera()
    {
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -70f, 30f);

        // 直接设置相机根的局部旋转，不受角色旋转影响
        _cameraRoot.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    //人物移动
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = transform.right * x + transform.forward * z;
        _controller.Move(_moveSpeed * Time.deltaTime * dir);

        // 重力
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
        PlayerEvents.Center.Trigger<ItemData, int>(PlayerEvent.GetItem, obj.GetComponent<ItemModel>().itemData, 1);
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

        // 这里可以实现装备物品的逻辑，如实例化装备模型并挂载到EquipMentRoot
        Debug.Log($"装备了{itemData.itemName}");
    }
    #endregion

    #region 触发器逻辑
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CanGetObj"))
        {
            _currentCanGetObject = other.gameObject;
        }
        if (other.CompareTag("FarmLand"))
        {
            CurrentFarmLand = other.GetComponent<FarmLand>();
            Debug.Log(CurrentFarmLand.FarmCurrentStatue);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CanGetObj") && other.gameObject == _currentCanGetObject)
        {
            _currentCanGetObject = null;
        }
        if (other.CompareTag("FarmLand"))
        {
            CurrentFarmLand = null;
            Debug.Log(222222);
        }
    }
    #endregion
}