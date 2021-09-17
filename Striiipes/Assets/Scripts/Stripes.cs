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
    private Vector2 direction = Vector2.zero;
    private Vector2 rayDirection;
    private bool goingLeft = false;
    private bool goingRight = false;
    private bool goingUp = false;
    private bool goingDown = false;

    //Body Segments
    [Header("Body Segments")]    
    [SerializeField] private Transform segmentPrefab;    
    [SerializeField] private Sprite stripesButtSprite;
    [SerializeField] private Sprite stripesBodySprite;
    private List<Transform> segments = new List<Transform>();

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
        SpriteSwap();
        CollisionDetection();
        DeathMethod();
        InvincibilityState();
    }

    private void FixedUpdate()
    {
        Movement();               
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == treatCollider)
        {
            Grow();
        }
    }

    private void Controls()
    {
        if (!goingRight && (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)))
        {
            direction = Vector2.left;
            rayDirection = Vector2.left;
            goingLeft = true;
            goingUp = false;
            goingDown = false;
        }
        else if (!goingLeft && (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)))
        {
            direction = Vector2.right;
            rayDirection = Vector2.right;
            goingRight = true;
            goingUp = false;
            goingDown = false;
        }
        else if (!goingDown && (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)))
        {
            direction = Vector2.up;
            rayDirection = Vector2.up;
            goingUp = true;
            goingRight = false;
            goingLeft = false;
        }
        else if (!goingUp && (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)))
        {
            direction = Vector2.down;
            rayDirection = Vector2.down;
            goingDown = true;
            goingRight = false;
            goingLeft = false;
        }
        else if (Input.GetKey(KeyCode.F))
        {
            direction = Vector2.zero;
            goingDown = false;
            goingUp = false;
            goingLeft = false;
            goingRight = false;
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
        RaycastHit2D hit = Physics2D.Raycast(((Vector2)transform.position + rayDirection), rayDirection);
        Debug.DrawRay(transform.position, direction, Color.green);

        if (hit.collider != null)
        {
            if (isInvincible == true)
            {
                if (hit.distance < .5f && hit.collider.tag == "Wall")
                {
                    isColliding = true;
                }
                else isColliding = false;
            }
            else if (isInvincible == false)
            {
                if (hit.distance < .5f && (hit.collider.tag == "Wall" || hit.collider.tag == "Segments"))
                {
                    isColliding = true;
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
                Debug.Log("You are no longer Invincible. Timer: " + invincibilityTimer);
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
        //Get Last Segment and set the correct sprite
        Transform lastSegment = segments[segments.Count - 1];
        SpriteRenderer lastSegmentSprite = lastSegment.GetComponentInChildren<SpriteRenderer>();
        lastSegmentSprite.transform.localScale = new Vector3(0.15f, 0.15f, 0f);
        lastSegmentSprite.sprite = stripesButtSprite;
        lastSegmentSprite.sortingOrder = 2;

        if (segments.Count >= 3)
        {
            Transform bodySegment = segments[segments.Count - 2];
            SpriteRenderer bodySprite = bodySegment.GetComponentInChildren<SpriteRenderer>();
            bodySprite.transform.localScale = new Vector3(1f, 1f, 0f);
            bodySprite.sprite = stripesBodySprite;
            bodySprite.sortingOrder = 0;
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
            segments[i].position = (Vector2)segments[i - 1].position - new Vector2(0, 1);
        }

        treat.GetComponent<Treat>().RandomizePosition();

        //Reset Direciton variables;
        
        direction = Vector2.zero;
        rayDirection = Vector2.zero;
        goingUp = true;
        goingDown = false;
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