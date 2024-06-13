using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }

    public static Player LocalInstance { get; private set; }


    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositionList;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private LayerMask wallLayer;

    private CinemachineVirtualCamera virtualCamera;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private List<GameObject> hiddenWalls = new List<GameObject>(); // Danh sách các tường bị ẩn


    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            LocalInstance = this;
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>(); // Tìm đối tượng camera
            if (virtualCamera != null)
            {
                virtualCamera.Follow = transform; // Đặt camera theo dõi nhân vật
            }
        }

        transform.position = spawnPositionList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if(clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        HandleMovement();
        HandleInteractions();
        HandleCameraObstructions(); // Thêm phương thức xử lý vật cản camera
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    //Xu ly va cham
    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;

        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                //Has ClearCounter
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        //luu giu huong di chuyen trong khong gian 3d voi y = 0
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        //khoảng cách di chuyển dựa trên tốc độ di chuyển (moveSpeed) và thời gian giữa các khung hình
        float moveDistance = moveSpeed * Time.deltaTime;


        float playerRadius = .7f;
        //float playerHeight = 2f;
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionsLayerMask);
        if (!canMove)
        {
            //Khong the di chuyen theo moveDir

            //Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.BoxCast(transform.position,
                Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionsLayerMask);

            if (canMove)
            {
                //chi co the di chuyen theo truc X
                moveDir = moveDirX;
            }
            else
            {
                //khong the di chuyen theo truc X
                //Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;

                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.BoxCast(transform.position,
                    Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionsLayerMask);

                if (canMove)
                {
                    //chi co the di chuyen theo truc Z
                    moveDir = moveDirZ;
                }
                else
                {
                    //Khong the di chuyen theo bat ky huong nao


                }
            }
        }
        if (canMove)
        {
            //di chuyen nhan vat
            transform.position += moveDir * moveDistance;
        }

        //true neu moveDir khac 0 va nguoc lai
        isWalking = moveDir != Vector3.zero;

        //xoay nhan vat
        float rotateSpeed = 10f;
        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }
    }

    private void HandleCameraObstructions()
    {
        // Hiển thị lại các bức tường đã ẩn
        foreach (GameObject wall in hiddenWalls)
        {
            wall.SetActive(true);
        }
        hiddenWalls.Clear();

        Vector3 directionToPlayer = (transform.position - virtualCamera.transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, virtualCamera.transform.position);

        if (Physics.Raycast(virtualCamera.transform.position, directionToPlayer, out RaycastHit hitInfo, distanceToPlayer, wallLayer))
        {
            GameObject hitObject = hitInfo.transform.gameObject;
            if (hitObject.layer == LayerMask.NameToLayer("Walls"))
            {
                hitObject.SetActive(false); // Ẩn tường
                hiddenWalls.Add(hitObject); // Thêm vào danh sách các tường bị ẩn
            }
        }
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventEventArgs
        {
            selectedCounter = selectedCounter,
        });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
