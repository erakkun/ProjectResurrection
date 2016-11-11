using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AnimationSystem))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(CharacterAttacks))]

public class CharacterMaster : MonoBehaviour
{
    public static RaycastHit2D DrawBox(Vector2 origin, Vector2 size, Vector2 direction, float distance, int layer_mask) {
        float angle = 0f;
        RaycastHit2D rch = Physics2D.BoxCast(origin, size, angle, direction, distance, layer_mask);
 
        float half_width = size.x / 2;
        float half_height = size.y / 2;
 
        // 4 points of the origin box
        Vector2 p1 = new Vector2(origin.x - half_width, origin.y + half_height);
        Vector2 p2 = new Vector2(origin.x + half_width, origin.y + half_height);
        Vector2 p3 = new Vector2(origin.x - half_width, origin.y - half_height);
        Vector2 p4 = new Vector2(origin.x + half_width, origin.y - half_height);
 
        // 4 points of the destination box
        Vector2 dest_origin = origin + (distance * direction);
        Vector2 t1 = new Vector2(dest_origin.x - half_width, dest_origin.y + half_height);
        Vector2 t2 = new Vector2(dest_origin.x + half_width, dest_origin.y + half_height);
        Vector2 t3 = new Vector2(dest_origin.x - half_width, dest_origin.y - half_height);
        Vector2 t4 = new Vector2(dest_origin.x + half_width, dest_origin.y - half_height);
 
        Color box_color = rch ? Color.green : Color.blue; // If it's a hit, turn GREEN, else color is BLUE
 
        Debug.DrawLine(p1, p2, box_color);
        Debug.DrawLine(p2, p4, box_color);
        Debug.DrawLine(p4, p3, box_color);
        Debug.DrawLine(p3, p1, box_color);
 
        Debug.DrawLine(t1, t2, box_color);
        Debug.DrawLine(t2, t4, box_color);
        Debug.DrawLine(t4, t3, box_color);
        Debug.DrawLine(t3, t1, box_color);
 
        Debug.DrawLine(p1, t1, box_color);
        Debug.DrawLine(p2, t2, box_color);
        Debug.DrawLine(p3, t3, box_color);
        Debug.DrawLine(p4, t4, box_color);
 
        return rch;
    }

    [SerializeField]
    protected float movementSpeed = 5f;
    [SerializeField]
    protected LayerMask cannotMoveAgainst;

    public bool canMove = true;
    public bool canRotate = true;
    public bool canAction = true;
    public bool canAttack = true;

    [SerializeField]
    protected GameObject hitArea;

    protected Vector2 destPos = Vector2.zero;
    protected Vector2 oldTrgt = Vector2.zero;
    protected Vector2 prevPos = Vector2.zero;
    protected bool isMoving = false;
    protected bool isMovingNPC = false;

    [SerializeField]
    protected bool forwardBlock = false;
    [SerializeField]
    protected bool forwardAboutBlock = false;
    [SerializeField]
    protected bool forwardMovingNPC = false;
    [SerializeField]
    protected bool forwardDashBlock = false;

    protected Vector2 gridPos;
    protected Vector2 gridPosPrev;
    protected Vector2 gridVector;

    public Vector2 GetGridPos()
    {
        return new Vector2((int)gridVector.x, (int)gridVector.y);
    }

    protected AnimationSystem animationSystem;
    protected string animationDirection = "Right";

    [HideInInspector]
    public CharacterStats characterStats;
    [HideInInspector]
    public CharacterAttacks characterAttacks;

    protected bool initialized = false;

    protected virtual bool DoDuringStart()
    {
        return true;
    }

    public bool IsMoving()
    {
        return isMoving || isMovingNPC;
    }

    void Start()
    {
        destPos = transform.position;
        if (!hitArea)
        {
            Debug.Log("Hit area is undefined.");
            return;
        }

        animationSystem = GetComponent<AnimationSystem>();
        if (!animationSystem)
        {
            Debug.Log("AnimationSystem not available.");
            return;
        }

        characterStats = GetComponent<CharacterStats>();
        if (!characterStats)
        {
            Debug.Log("CharacterStats not available.");
            return;
        }

        characterAttacks = GetComponent<CharacterAttacks>();
        if(!characterAttacks)
        {
            Debug.Log("CharacterAttacks not available.");
            return;
        }

        characterAttacks.Init();
        characterAttacks.ChangeAttack(0);

        animationSystem.Init();
        animationSystem.Play(animationDirection);

        if(!DoDuringStart())
        {
            Debug.Log("Child startup failed.");
            return;
        }

        StartCoroutine(GridPosInit());

        initialized = true;
    }

    IEnumerator GridPosInit()
    {
        while(!TileKeeper.levelSet)
        {
            yield return null;
        }

        gridPos = TileKeeper.TranslateToPoint(transform.position);
        gridPosPrev = gridPos;
        gridVector = transform.position;
        TileKeeper.SetAvailability(gridPos, false);
    }

    protected void TurnUp()
    {
        if (animationDirection != "Up")
        {
            animationSystem.StopAll(true);
        }
        animationDirection = "Up";
        hitArea.gameObject.transform.localPosition = new Vector2(0, 1);
        characterAttacks.Rotate(Quaternion.Euler(0, 0, 90));
    }

    protected void TurnDown()
    {
        if (animationDirection != "Down")
        {
            animationSystem.StopAll(true);
        }
        animationDirection = "Down";
        hitArea.gameObject.transform.localPosition = new Vector2(0, -1);
        characterAttacks.Rotate(Quaternion.Euler(0, 0, 270));
    }

    protected void TurnRight()
    {
        if (animationDirection != "Right")
        {
            animationSystem.StopAll(true);
        }
        animationDirection = "Right";
        hitArea.gameObject.transform.localPosition = new Vector2(1, 0);
        characterAttacks.Rotate(Quaternion.Euler(0, 0, 0));
    }

    protected void TurnLeft()
    {
        if (animationDirection != "Left")
        {
            animationSystem.StopAll(true);
        }
        animationDirection = "Left";
        hitArea.gameObject.transform.localPosition = new Vector2(-1, 0);
        characterAttacks.Rotate(Quaternion.Euler(0, 0, 180));
    }

    public string GetDirection()
    {
        return animationDirection;
    }

    public void LookAt(Vector2 destination)
    {
        Vector2 current = transform.position;
        Vector2 offPos = destination - current;
        float angle = Mathf.Atan2(offPos.y, offPos.x);
        angle = angle * Mathf.Rad2Deg;

        //up
        if (angle > 45 && angle < 135)
        {
            TurnUp();
        }
        //down
        else if (angle < -45 && angle > -135)
        {
            TurnDown();
        }
        //right
        else if (angle < 45 && angle >= 0 || angle > -45 && angle <= 0)
        {
            TurnRight();
        }
        //left
        else if (angle > 135 && angle <= 180 || angle < -135 && angle >= -180)
        {
            TurnLeft();
        }

        AnimateDirection();
    }

    protected void AnimateDirection()
    {
        if (!animationSystem.IsPlaying(animationDirection) && !characterAttacks.IsAttacking())
        {
            animationSystem.Stop(animationDirection, true, false);
            animationSystem.Play(animationDirection);
        }
    }

    public Vector2 GetCurrentDestination()
    {
        return destPos;
    }

    protected void UpdateBlocks(Vector2 direction)
    {
        if(direction == Vector2.zero)
        {
            direction = hitArea.transform.localPosition;
        }

        forwardDashBlock = false;
        if (Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, direction, 2f, cannotMoveAgainst))
        {
            forwardDashBlock = true;
        }

        forwardAboutBlock = false;
        if (Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, direction, 0.2f, cannotMoveAgainst))
        {
            forwardAboutBlock = true;
        }

        forwardBlock = false;
        if (Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, direction, 0.2f, cannotMoveAgainst))
        {
            forwardBlock = true;
        }

        forwardMovingNPC = false;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.5f), 0, direction, 0.2f, cannotMoveAgainst);
        if (hit.transform != null)
        {
            CharacterMaster m = hit.transform.gameObject.GetComponent<CharacterMaster>();
            if (m)
            {
                if (m.IsMoving())
                {
                    forwardMovingNPC = true;
                }
            }
        }
    }

    protected void UpdateGridPosition()
    {
        if(!TileKeeper.levelSet)
        {
            return;
        }

        gridPos = TileKeeper.TranslateToPoint(transform.position);
        //Debug.Log(gameObject.name + " " + gridPos + " " + gridPosPrev);

        bool player = false;
        if(gameObject.name == "Player")
        {
            player = true;
        }

        if (gridPos != gridPosPrev)
        {
            TileKeeper.SetAvailability(gridPosPrev, true, player);
            gridPosPrev = gridPos;
            gridVector = transform.position;
        }
        TileKeeper.SetAvailability(gridPos, false, player);
    }

    protected virtual void DoOnTrigger()
    {

    }

    protected virtual void DoDuringUpdate()
    {

    }

    void Update()
    {
        if(initialized)
        {
            UpdateGridPosition();
            UpdateBlocks(Vector2.zero);
            DoDuringUpdate();
            animationSystem.Handle();
        }
    }
}
