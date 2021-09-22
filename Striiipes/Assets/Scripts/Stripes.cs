using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stripes : MonoBehaviour
{
    #region Variables
    //Accessibility
    [Header("Accessibility Stuff")]
    [SerializeField][Range(2, 6)] private int initialSize = 3;
    [SerializeField][Range(0.02f, 0.5f)] private float baseGameSpeed;

    //Movement Variables
    private Vector2 direction;
    private List<Vector2> inputBuffer = new List<Vector2>();
    private Vector2 rayDirection;
    private bool goingLeft = false;
    private bool goingRight = false;
    private bool goingUp = false;
    private bool goingDown = false;

    //Body Segments
    [Header("Body Segments")]
    [SerializeField] private SpriteRenderer stripesHeadRenderer;
    [SerializeField] private Sprite stripesHeadUp;
    [SerializeField] private Sprite stripesHeadDown;
    [SerializeField] private Sprite stripesHeadHorizontal;

    [SerializeField] private Transform segmentPrefab;
    [SerializeField] private Sprite[] stripesBodySprites;

    [SerializeField] private Sprite stripesButtUp;
    [SerializeField] private Sprite stripesButtDown;
    [SerializeField] private Sprite stripesButtHorizontal;
    private Transform lastSegment;
    private SpriteRenderer lastSegmentSprite;
    private float lastSegmentX;
    private float lastSegmentY;

    private List<Transform> segments = new List<Transform>();
    private bool bodySwapNeeded = false;

    //Collision
    [Header("Collision Detectors")]
    [SerializeField] private GameObject treat;
    [SerializeField] private Collider2D treatCollider;
    private bool isColliding = false;

    //Death Variables
    [Header("Death Variables")]
    [SerializeField] [Range(.5f, 5)] private float baseDeathTimer = 5;
    private float deathTimer;
    [SerializeField] [Range(0, 4)] private int baseLivesCount = 1;
    private int livesCount;
    [SerializeField] [Range(.5f, 5)] private float baseInvincibilityTimer = 5;
    private float invincibilityTimer;
    private bool isInvincible = false;
    #endregion

    private void Start()
    {
        ResetState();        
    }

    private void Update()
    {
        Controls();
        CollisionDetection();
        DeathMethod();
        InvincibilityState();        
    }

    private void FixedUpdate()
    {
        BufferedActions();
        Movement();
    }

    private void LateUpdate()
    {
        SpriteSwap();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == treatCollider)
        {
            Grow();
            bodySwapNeeded = true;
        }
    }

    private void Controls()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (goingRight == true || direction == Vector2.left)
            {
                return;
            }
            else
            {
                inputBuffer.Add(new Vector2(-1, 0));
                goingLeft = true;
                goingRight = false;
                goingUp = false;
                goingDown = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (goingLeft == true || direction == Vector2.right)
            {
                return;
            }
            else
            {
                inputBuffer.Add(new Vector2(1, 0));
                goingRight = true;
                goingLeft = false;
                goingUp = false;
                goingDown = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (goingDown == true || direction == Vector2.up)
            {
                return;
            }
            else
            {
                inputBuffer.Add(new Vector2(0, 1));
                goingUp = true;
                goingDown = false;
                goingRight = false;
                goingLeft = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (goingUp == true || direction == Vector2.down)
            {
                return;
            }
            else
            {
                inputBuffer.Add(new Vector2(0, -1));
                goingDown = true;
                goingUp = false;
                goingRight = false;
                goingLeft = false;
            }
        }          
    }

    private void BufferedActions()
    {
        if (inputBuffer.Count > 0)
        {
            foreach (Vector2 bufferDirection in inputBuffer.ToArray())
            {
                rayDirection = bufferDirection;
                CollisionDetection();
                if (isColliding == false)
                {
                    direction = bufferDirection;
                }
                inputBuffer.Remove(bufferDirection);

                break;
            }
        }
    }

    private void Movement()
    {        
        if (direction != Vector2.zero)
        {
            for (int i = segments.Count - 1; i > 0; i--)
            {
                segments[i].position = segments[i - 1].position;
            }
        }

        // Move According to Player Input
        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x) + direction.x,
            Mathf.Round(this.transform.position.y) + direction.y,
            0.0f);
    }

    private void CollisionDetection()
    {
        RaycastHit2D hit = Physics2D.Raycast(((Vector2)transform.position), rayDirection);
        Debug.DrawRay(((Vector2)transform.position), rayDirection, Color.green);

        if (hit.collider != null)
        {
            if (isInvincible == true)
            {
                if (hit.distance < 1.5f && hit.collider.tag == "Wall")
                {
                    isColliding = true;
                }
                else isColliding = false;
            }
            else if (isInvincible == false)
            {
                if (hit.distance < 1.5f && (hit.collider.tag == "Wall" || hit.collider.tag == "Segments"))
                {
                    isColliding = true;
                    Debug.DrawRay(((Vector2)transform.position), rayDirection, Color.red);
                }
                else isColliding = false;
            }            
        }
        else isColliding = false;

        if (isColliding == true)
        {
            direction = Vector2.zero;
        }
    }

    private void DeathMethod()
    {
        if (isColliding == true && isInvincible == false)
        {
            deathTimer -= Time.deltaTime;

            if (deathTimer <= 0)
            {
                if (livesCount != 0)
                {
                    livesCount -= 1;
                    Debug.Log("You Lost A Life! Lives Left: " + livesCount);
                    isInvincible = true;
                    direction = rayDirection;
                }
                else if (livesCount == 0)
                {
                    ResetState();
                }
            }            
        }
        else if (isColliding == false)
        {
            deathTimer = baseDeathTimer;
        }
    }

    private void InvincibilityState()
    {
        if (isInvincible == true)
        {
            invincibilityTimer -= Time.deltaTime;

            if (invincibilityTimer <= 0)
            {                
                invincibilityTimer = baseInvincibilityTimer;
                Debug.Log("You are no longer Invincible.");
                isInvincible = false;
            }            
        }
    }

    private void Grow()
    {
        Transform segment = Instantiate(this.segmentPrefab);
        segment.position = segments[segments.Count - 1].position;
        segments.Add(segment);        
    }

    private void SpriteSwap()
    {
        //Set the Head Sprite
        if (rayDirection == Vector2.left)
        {
            stripesHeadRenderer.sprite = stripesHeadHorizontal;
            stripesHeadRenderer.flipX = true;
        }
        else if (rayDirection == Vector2.right)
        {
            stripesHeadRenderer.sprite = stripesHeadHorizontal;
            stripesHeadRenderer.flipX = false;
        }
        else if (rayDirection == Vector2.up)
        {
            stripesHeadRenderer.sprite = stripesHeadUp;
        }
        else if (rayDirection == Vector2.down)
        {
            stripesHeadRenderer.sprite = stripesHeadDown;
        }

        //Get Last Segment
        lastSegment = segments[segments.Count - 1];            
        lastSegmentSprite = lastSegment.GetComponentInChildren<SpriteRenderer>();
        lastSegmentSprite.transform.localScale = new Vector3(0.55f, 0.55f, 0f);
        //Set Last Segment

        if (lastSegmentX > lastSegment.position.x)
        {
            lastSegmentSprite.sprite = stripesButtHorizontal;
            lastSegmentSprite.flipX = true;
        }
        else if (lastSegmentX < lastSegment.position.x)
        {
            lastSegmentSprite.sprite = stripesButtHorizontal;
            lastSegmentSprite.flipX = false;
        }
        else if (lastSegmentY < lastSegment.position.y)
        {
            lastSegmentSprite.sprite = stripesButtUp;
        }
        else if (lastSegmentY > lastSegment.position.y)
        {
            lastSegmentSprite.sprite = stripesButtDown;
        }

        lastSegmentX = lastSegment.position.x;
        lastSegmentY = lastSegment.position.y;


        //Set the sprite for the body segment
        if (segments.Count >= 3 && bodySwapNeeded == true)
        {
            Transform bodySegment = segments[segments.Count - 2];
            SpriteRenderer bodySprite = bodySegment.GetComponentInChildren<SpriteRenderer>();
            bodySprite.transform.localScale = new Vector3(.55f, .55f, 0f);
            int randomIndex = Random.Range(0, stripesBodySprites.Length);
            bodySprite.sprite = stripesBodySprites[randomIndex];
            bodySwapNeeded = false;
        }
    }

    private void ResetState()
    {
        //Reset Position & Segments List
        this.transform.position = Vector3.zero;

        for (int i = 1; i < segments.Count; i++)
        {
            Destroy(segments[i].gameObject);
        }
        segments.Clear();
        segments.Add(this.transform);

        for (int i = 1; i < this.initialSize; i++)
        {
            segments.Add(Instantiate(this.segmentPrefab));
        }

        for (int i = 1; i < initialSize; i++)
        {
            segments[i].position = (Vector2)segments[i - 1].position - new Vector2(0, -1);
        }

        SpriteSwap();
        lastSegmentSprite.sprite = stripesButtDown;

        treat.GetComponent<Treat>().RandomizePosition();

        //Reset Direciton variables;
        
        direction = Vector2.zero;
        rayDirection = Vector2.down;
        goingUp = false;
        goingDown = true;
        goingLeft = false;
        goingRight = false;
        

        //Reset Death Variables
        deathTimer = baseDeathTimer;
        livesCount = baseLivesCount;
        invincibilityTimer = baseInvincibilityTimer;
        isInvincible = false;

        //Reset Game Speed
        Time.fixedDeltaTime = baseGameSpeed;
    }
}