using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace TempleRun.Player
{

    [RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Animator))]
    public class PlayerControler : MonoBehaviour
    {
        [SerializeField] private float initialPlayerSpeed = 8f;
        [SerializeField] private float maxPlayerSpeed = 60f;
        [SerializeField] private float playerSpeedIncrease = 0.1f;
        [SerializeField] private float playerJumpHeight = 1f;
        [SerializeField] private float initialGravity = -9.81f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask turnLayer;

        private float playerSpeed;
        private float playerGravity;
        private Vector3 playerDirection = Vector3.forward;
        private Vector3 playerVelocity;
        private PlayerInput PlayerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;
        private CharacterController characterController;
        private bool sliding = false;
        private int slideAnimationHash;
        private int jumpAnimationHash;
        private Animator animator;

        [SerializeField] private UnityEvent<Vector3> turnEvent;

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            slideAnimationHash = Animator.StringToHash("Slide");
            jumpAnimationHash = Animator.StringToHash("Jump");
            turnAction = PlayerInput.actions["Turn"];
            jumpAction = PlayerInput.actions["Jump"];
            slideAction = PlayerInput.actions["Slide"];
        }

        private void OnEnable()
        {
            turnAction.performed += PlayerTurn;
            jumpAction.performed += PlayerJump;
            slideAction.performed += PlayerSlide;
        }

        private void OnDisable()
        {
            turnAction.performed -= PlayerTurn;
            jumpAction.performed -= PlayerJump;
            slideAction.performed -= PlayerSlide;
        }

        private void Start()
        {
            playerGravity = initialGravity;
            playerSpeed = initialPlayerSpeed;
        }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            Vector3? turnPos = checkTurn(context.ReadValue<float>());
            if (turnPos == null) return;
            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * playerDirection;
            turnEvent.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPos.Value);
        }

        private void Turn(float turnValue, Vector3 turnPos)
        {
            Vector3 tmpPlayerPos = new Vector3(turnPos.x, transform.position.y, turnPos.z);
            characterController.enabled = false;
            transform.position = tmpPlayerPos;
            characterController.enabled = true;

            transform.rotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            playerDirection = transform.forward.normalized;
        }

        private Vector3? checkTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, turnLayer);
            if (hitColliders.Length > 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                if ((tile.type == TileType.LEFT && turnValue == -1) ||
                    (tile.type == TileType.RIGHT && turnValue == 1) ||
                    (tile.type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }
            return null;
        }

        private void PlayerJump(InputAction.CallbackContext context)
        {
            if (isGrounded())
            {
                animator.Play(jumpAnimationHash);
                playerVelocity.y = Mathf.Sqrt(playerJumpHeight * -3f * playerGravity);
                characterController.Move(playerVelocity * Time.deltaTime);
            }
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {
            if(!sliding && isGrounded())
            {
                //StartCoroutine(Slide());
            }
        }

        private void Update()
        {
            characterController.Move(transform.forward * playerSpeed * Time.deltaTime);

            if (isGrounded() && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }

            playerVelocity.y += playerGravity * Time.deltaTime;
            characterController.Move(playerVelocity * Time.deltaTime);
        }

        private bool isGrounded(float length = 0.2f)
        {
            Vector3 raycastOriginFirst = transform.position;
            raycastOriginFirst.y -= characterController.height / 2;
            raycastOriginFirst.y += 0.1f;

            Vector3 raycastOriginSecond = raycastOriginFirst;
            raycastOriginFirst -= transform.forward * 2f;
            raycastOriginSecond -= transform.forward * -2f;

            if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) || Physics.Raycast(raycastOriginSecond, Vector3.down, out hit, length, groundLayer))
            {
                return true;
            }
            return false;
        }
    }

}