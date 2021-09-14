using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stripes : MonoBehaviour
{
    //Movement Variables
    private Vector2 direction = Vector2.zero;
    private bool goingLeft = false;
    private bool goingRight = false;
    private bool goingUp = false;
    private bool goingDown = false;

    //Body Segments
    private List<Transform> segments = new List<Transform>();
    [SerializeField] private Transform segmentPrefab;
    [SerializeField] private int initialSize = 3;
    [SerializeField] private Sprite stripesButtSprite;
    [SerializeField] private Sprite stripesBodySprite;

    //Collisions
    [SerializeField] private GameObject treat;
    [SerializeField] private Collider2D treatCollider;
    //[SerializeField] private float deathTimer = 2;

    private void Start()
    {
        ResetState();
        for (int i = 1; i < initialSize; i++)
        {
            segments[i].position = (Vector2)segments[i - 1].position - new Vector2(0, 1);
        }
    }

    private void Update()
    {
        Controls();
        SpriteSwap();
    }

    private void FixedUpdate()
    {
        if (direction != Vector2.zero)
        {
            for (int i = segments.Count - 1; i > 0; i--)
            {
                segments[i].position = segments[i - 1].position;
            }
        }
        
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
            goingLeft = true;
            goingUp = false;
            goingDown = false;
        }
        if (!goingLeft && (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)))
        {
            direction = Vector2.right;
            goingRight = true;
            goingUp = false;
            goingDown = false;
        }
        if (!goingDown && (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)))
        {
            direction = Vector2.up;
            goingUp = true;
            goingRight = false;
            goingLeft = false;
        }
        if (!goingUp && (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)))
        {
            direction = Vector2.down;
            goingDown = true;
            goingRight = false;
            goingLeft = false;
        }
    }

    private void Movement()
    {
        // Move According to Player Input
        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x) + direction.x,
            Mathf.Round(this.transform.position.y) + direction.y,
            0.0f);
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
        lastSegmentSprite.transform.localScale = new Vector3(0.145f, 0.145f, 0f);
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

        this.transform.position = Vector3.zero;
        direction = Vector2.zero;
        goingLeft = false;
        goingRight = false;
        goingUp = false;
        goingDown = false;
    }

    private void RandomizeTreat()
    {
        treat.GetComponent<Treat>().RandomizePosition();
    }
}
