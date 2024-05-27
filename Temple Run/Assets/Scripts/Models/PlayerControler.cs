using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using UnityEditor;
using static UnityEditor.Timeline.TimelinePlaybackControls;


namespace TempleRun.Player
{

    [RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Animator))]
    public class PlayerControler : MonoBehaviour
    {
        [SerializeField] private float initialPlayerSpeed = 8f;
        [SerializeField] private float maxPlayerSpeed = 60f;
        [SerializeField] private float playerSpeedIncrease = 0.1f;
        [SerializeField] private float playerJumpHeight = 2f;
        [SerializeField] private float initialGravity = -9.81f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask turnLayer;
        [SerializeField] private AnimationClip slideAnimation;
        [SerializeField] private AnimationClip jumpAnimation;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip fallingClip;
        [SerializeField] private AudioClip dieClip;
        [SerializeField] private AudioClip tropezarClip;
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip slideClip;
        [SerializeField] private AudioClip zombieClip;
        [SerializeField] private GameObject godModeTxt;

        private float playerSpeed;
        private float playerGravity;
        private Vector3 playerDirection = Vector3.forward;
        private Vector3 playerVelocity;
        private PlayerInput PlayerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;
        private InputAction godAction;
        private CharacterController characterController;
        private bool sliding = false;
        private int slideAnimationHash;
        private int jumpAnimationHash;
        private int dieAnimationHash;
        private int fallAnimationHash;
        private int tropezarAnimationHash;
        private int zombieDieAnimationHash;
        private Animator animator;
        bool transToSlide = false;
        private float score = 0;
        private bool isDead = false;
        private bool zombiesNear = false;
        private bool godMode = false;

        [SerializeField] private UnityEvent<Vector3> turnEvent;
        [SerializeField] private UnityEvent<int> gameOverEvent;
        [SerializeField] private UnityEvent<int> scoreUpdateEvent;

        private Vector3 originalControllerCenter;
        private float originalControllerHeight;

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            characterController = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            slideAnimationHash = Animator.StringToHash("Slide");
            jumpAnimationHash = Animator.StringToHash("Jump");
            dieAnimationHash = Animator.StringToHash("Die");
            fallAnimationHash = Animator.StringToHash("Falling");
            tropezarAnimationHash = Animator.StringToHash("Tropiezo");
            zombieDieAnimationHash = Animator.StringToHash("zombieDie");
            turnAction = PlayerInput.actions["Turn"];
            jumpAction = PlayerInput.actions["Jump"];
            slideAction = PlayerInput.actions["Slide"];
            godAction = PlayerInput.actions["God"];

            originalControllerCenter = characterController.center;
            originalControllerHeight = characterController.height;
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

        private bool canTurn = true;

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            Vector3? turnPos = checkTurn(context.ReadValue<float>());
            if (turnPos == null || !canTurn) return;

            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * playerDirection;
            turnEvent.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPos.Value);

            StartCoroutine(ResetTurnCooldown());
        }

        private IEnumerator ResetTurnCooldown()
        {
            canTurn = false;
            yield return new WaitForSeconds(1f);
            canTurn = true;
        }

        private void Turn(float turnValue, Vector3 turnPos)
        {
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
            if ((isGrounded() || sliding) && !godMode)
            {
                audioSource.clip = jumpClip;
                audioSource.pitch = 1.0f;
                audioSource.Play();
                animator.Play(jumpAnimationHash);
                playerVelocity.y = Mathf.Sqrt(playerJumpHeight * -3f * playerGravity);
                characterController.Move(playerVelocity * Time.deltaTime);

                if (sliding) {
                    characterController.height = originalControllerHeight;
                    characterController.center = originalControllerCenter;
                    sliding = false;
                } 
            }
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {
            if (!sliding && isGrounded() && !godMode)
            {
                StartCoroutine(Slide());
            }
            else if(!sliding && !godMode)
            {
                playerGravity = initialGravity * 10f;
                transToSlide = true;
            }
        }

        private IEnumerator Slide()
        {
            audioSource.clip = slideClip;
            audioSource.pitch = 1.3f;
            audioSource.Play();
            sliding = true;

            // Collider mas pequeño
            Vector3 originalControllerCenter = characterController.center;
            Vector3 newControllerCenter = originalControllerCenter;
            characterController.height /= 2;
            newControllerCenter.y -= characterController.height / 2;
            characterController.center = newControllerCenter;

            animator.Play(slideAnimationHash);
            // restart animation if already playing
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Slide")) animator.Play(slideAnimationHash, 0, 0f);
            yield return new WaitForSeconds(slideAnimation.length - 0.6f);

            // Collider normal
            characterController.height = originalControllerHeight;
            characterController.center = originalControllerCenter;
            sliding = false;
        }

        private void Update()
        {
            // si se pulsa la tecla godmode
            if (godAction.triggered)
            {
                godMode = !godMode;
                if (godMode)
                {
                    playerSpeed = 16f;
                    godModeTxt.SetActive(true);
                }
                else
                {
                    godModeTxt.SetActive(false);
                }
            }   

            if (isGrounded() && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
                if (transToSlide)
                {
                    StartCoroutine(Slide());
                    transToSlide = false;
                    playerGravity = initialGravity;
                }
            }

            playerVelocity.y += playerGravity * Time.deltaTime;
            // only if controller active
            if (characterController.enabled && transform.position.y > -10.3f) characterController.Move(playerVelocity * Time.deltaTime);

            if (isDead) return;

            // si el jugador esta a menos de 0 de altura game over
            if (transform.position.y < 0)
            {
                if (transform.position.y > -0.1f) {
                    animator.Play(fallAnimationHash);
                    audioSource.pitch = 1.75f;
                    audioSource.clip = fallingClip;
                    audioSource.Play();
                } 
                if (transform.position.y < -10.3f) {
                    _ = GameOver();
                    return;
                }
            }

            // Score Update
            score += 10 * Time.deltaTime;
            scoreUpdateEvent.Invoke((int)score);


            characterController.Move(transform.forward * playerSpeed * Time.deltaTime);

            

            float horizontalInput = PlayerInput.actions["Turn"].ReadValue<float>();
            if (horizontalInput < 0 && canTurn && !godMode) // mover izquierda
            {
                Vector3 leftMovement = transform.right * -0.5f * playerSpeed * Time.deltaTime; // Move half as fast to the left
                characterController.Move(leftMovement);

            }
            else if (horizontalInput > 0 && canTurn && !godMode) // mover derecha
            {
                Vector3 rightMovement = transform.right * 0.5f * playerSpeed * Time.deltaTime; // Move half as fast to the right
                characterController.Move(rightMovement);
            }

            if (playerSpeed < maxPlayerSpeed && !godMode)
            {
                playerSpeed += playerSpeedIncrease * Time.deltaTime;
            }
        }

        private bool isGrounded(float length = .2f)
        {
            Vector3 raycastOriginFirst = transform.position;
            raycastOriginFirst.y -= characterController.height - 3.2f;
            raycastOriginFirst.y += 0.1f;

            Vector3 raycastOriginSecond = raycastOriginFirst;
            raycastOriginFirst -= transform.forward * .2f;
            raycastOriginSecond -= transform.forward * -.2f;

            Debug.DrawRay(raycastOriginFirst, Vector3.down * length, Color.red);
            Debug.DrawRay(raycastOriginSecond, Vector3.down * length, Color.red);

            if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) || Physics.Raycast(raycastOriginSecond, Vector3.down, out hit, length, groundLayer))
            {
                return true;
            }
            return false;
        }

        private async Task GameOver(bool byZombies = false)
        {
            if (byZombies) animator.Play(zombieDieAnimationHash);
            else animator.Play(dieAnimationHash);

            if(zombiesNear && !byZombies)
            {
                GameObject zombies = GameObject.Find("Zombie");
                zombies.GetComponent<EnemyFollow>().AtacarAnimacion();
            }

            if (!isDead) {
                if(byZombies) {
                    
                }
                else
                {
                    audioSource.pitch = 1.0f;
                    audioSource.clip = dieClip;
                    audioSource.Play();
                }
            } 

            // stop updating score and speed = 0 and stop getting input
            scoreUpdateEvent.RemoveAllListeners();
            playerSpeed = 0f;
            PlayerInput.enabled = false;
            isDead = true;

            await Task.Delay(1000); // Delay for 1 second
            gameOverEvent.Invoke((int)score);
        }

        private async void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                _ = GameOver();
            }
            await Task.Delay(0); // Delay for 1 second
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Raices"))
            {
                animator.Play(tropezarAnimationHash);
                audioSource.pitch = 1.5f;
                audioSource.clip = tropezarClip;
                audioSource.Play();

                if (!zombiesNear)
                {
                    GameObject zombies = GameObject.Find("Zombie");
                    zombies.GetComponent<EnemyFollow>().Acercar();

                    audioSource.pitch = 1.0f;
                    audioSource.clip = zombieClip;
                    audioSource.Play();

                    // reducir un 10% la velocidad
                    playerSpeed -= playerSpeed * 0.1f;

                    zombiesNear = true;
                }
                else
                {
                    GameObject zombies = GameObject.Find("Zombie");
                    zombies.GetComponent<EnemyFollow>().Comer();

                    _ = GameOver(true);
                    return;
                }
            }
            else if (other.gameObject.CompareTag("restoreZombies"))
            {
                GameObject zombies = GameObject.Find("Zombie");
                zombies.GetComponent<EnemyFollow>().Alejar();
                zombiesNear = false;
            }
            else if (godMode)
            {
                if (other.gameObject.CompareTag("jump"))
                {
                    audioSource.clip = jumpClip;
                    audioSource.pitch = 1.0f;
                    audioSource.Play();
                    animator.Play(jumpAnimationHash);
                    playerVelocity.y = Mathf.Sqrt(playerJumpHeight * -3f * playerGravity);
                    characterController.Move(playerVelocity * Time.deltaTime);

                    if (sliding)
                    {
                        characterController.height = originalControllerHeight;
                        characterController.center = originalControllerCenter;
                        sliding = false;
                    }
                }
                else if (other.gameObject.CompareTag("slide"))
                {
                    if (!sliding && isGrounded())
                    {
                        StartCoroutine(Slide());
                    }
                    else if (!sliding)
                    {
                        playerGravity = initialGravity * 10f;
                        transToSlide = true;
                    }
                }
                else if (other.gameObject.CompareTag("right"))
                {
                    StartCoroutine(moverDerecha(0.2f));
                }
                else if(other.gameObject.CompareTag("left"))
                {
                    StartCoroutine(moverIzquierda(0.2f));
                }
                else if(other.gameObject.CompareTag("strLeft"))
                {
                    StartCoroutine(moverIzquierda(1.8f));
                }
                else if(other.gameObject.CompareTag("strRight"))
                {
                    StartCoroutine(moverDerecha(1.8f));
                }
                else if(other.gameObject.CompareTag("turnLeft"))
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, turnLayer);
                    Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                    Vector3 turnPos = tile.pivot.position;

                    Vector3 targetDirection = Quaternion.AngleAxis(90 * -1, Vector3.up) * playerDirection;
                    turnEvent.Invoke(targetDirection);
                    Turn(-1, turnPos);

                    StartCoroutine(ResetTurnCooldown());
                }
                else if(other.gameObject.CompareTag("turnRight"))
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, turnLayer);
                    Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                    Vector3 turnPos = tile.pivot.position;

                    Vector3 targetDirection = Quaternion.AngleAxis(90 * 1, Vector3.up) * playerDirection;
                    turnEvent.Invoke(targetDirection);
                    Turn(1, turnPos);
                }
                else if(other.gameObject.CompareTag("turn"))
                {
                    //random left or right random
                    int random = Random.Range(0, 2);
                    if(random == 0)
                    {
                        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, turnLayer);
                        Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                        Vector3 turnPos = tile.pivot.position;

                        Vector3 targetDirection = Quaternion.AngleAxis(90 * -1, Vector3.up) * playerDirection;
                        turnEvent.Invoke(targetDirection);
                        Turn(-1, turnPos);
                    }
                    else
                    {
                        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, turnLayer);
                        Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                        Vector3 turnPos = tile.pivot.position;

                        Vector3 targetDirection = Quaternion.AngleAxis(90 * 1, Vector3.up) * playerDirection;
                        turnEvent.Invoke(targetDirection);
                        Turn(1, turnPos);
                    }
                }
            }
        }
        IEnumerator moverDerecha(float wait)
        {
            for(int i = 0; i < 120; i++)
            {
                Vector3 rightMovement = transform.right * 0.5f * playerSpeed * Time.deltaTime; // Move half as fast to the right
                characterController.Move(rightMovement);
                yield return new WaitForSeconds(0.005f);
            }
            /*yield return new WaitForSeconds(wait);
            for(int i = 0; i < 55; i++)
            {
                Vector3 leftMovement = transform.right * -0.5f * playerSpeed * Time.deltaTime; // Move half as fast to the left
                characterController.Move(leftMovement);
                yield return new WaitForSeconds(0.005f);
            }*/
        }

        IEnumerator moverIzquierda(float wait)
        {
            for(int i = 0; i < 120; i++)
            {
                Vector3 leftMovement = transform.right * -0.5f * playerSpeed * Time.deltaTime; // Move half as fast to the left
                characterController.Move(leftMovement);
                yield return new WaitForSeconds(0.005f);
            }
            /*yield return new WaitForSeconds(wait);
            for(int i = 0; i < 55; i++)
            {
                Vector3 rightMovement = transform.right * 0.5f * playerSpeed * Time.deltaTime; // Move half as fast to the right
                characterController.Move(rightMovement);
                yield return new WaitForSeconds(0.005f);
            }*/
        }

    }

}