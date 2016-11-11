using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPlayer : CharacterMaster
{
    ActionRecognition actionRecognition;

    [HideInInspector]
    public PlayerEvents playerEvents;
    [HideInInspector]
    public PlayerInventory playerInventory;
    [HideInInspector]
    public PlayerKeyword playerKeyword;

    float speedBonus = 1;
    public float dashBonus = 5;
    public float dashCooldown = 1;
    float dashTimer = 0;

    public void Dash()
    {
        speedBonus = dashBonus;
        dashTimer = dashCooldown;
    }

    protected override bool DoDuringStart()
    {
        actionRecognition = hitArea.GetComponent<ActionRecognition>();
        if (!actionRecognition)
        {
            Debug.Log("Action recognition is missing from hit area.");
            return false;
        }

        GameObject playerInfoContainer = GameObject.Find("PlayerInformation");
        if(!playerInfoContainer)
        {
            Debug.Log("PlayerInformation object not found.");
            return false;
        }

        playerInventory = playerInfoContainer.GetComponent<PlayerInventory>();
        if(!playerInventory)
        {
            Debug.Log("PlayerInventory missing.");
            return false;
        }

        playerEvents = playerInfoContainer.GetComponent<PlayerEvents>();
        if (!playerEvents)
        {
            Debug.Log("PlayerEvents missing.");
            return false;
        }

        playerKeyword = playerInfoContainer.GetComponent<PlayerKeyword>();
        if (!playerKeyword)
        {
            Debug.Log("PlayerKeyword missing.");
            return false;
        }

        return true;
    }

    /*public void Dash(Vector2 dest, float speedbonus)
    {
        destPos = dest;
        isMoving = true;
        prevPos = transform.position;
        speedBonus = speedbonus;
    }*/

    void Move(Vector2 dir, float speed = 1)
    {
        UpdateBlocks(dir);
        if (forwardBlock || forwardMovingNPC || forwardAboutBlock)
        {
            return;
        }

        transform.Translate(dir * (movementSpeed * speed) * speedBonus * Time.deltaTime);
    }

    void HandleMovement()
    {
        if(!canMove)
        {
            return;
        }

        float left_x = Input.GetAxisRaw("Horizontal");
        float left_y = Input.GetAxisRaw("Vertical");

        Vector2 curPos = transform.position;
        Vector2 aimPos = new Vector2(left_x, left_y);
        Vector2 tarPos = curPos + aimPos;
        Vector2 offPos = tarPos - curPos;

        Vector2 moveAt = Vector2.zero;

        //dash
        if (characterStats.HasStamina() && characterStats.CanDash() && dashTimer <= 0)
        {
            if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                Debug.Log("Dash attempt");
                if (!forwardDashBlock)
                {
                    Debug.Log("Success");
                    Dash();
                    characterStats.DashStamina();
                }
                else if (!forwardAboutBlock)
                {
                    Debug.Log("Success");
                    Dash();
                    characterStats.DashStamina();
                }
            }
        }

        if(dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }
        else
        {
            dashTimer = 0;
            speedBonus = 1;
        }

        //up
        if (left_y > 0)
        {
            if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
            {
                TurnUp();
            }

            AnimateDirection();
            moveAt = Vector2.up;
            Move(moveAt);
        }
        //down
        else if (left_y < 0)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
            {
                TurnDown();
            }

            AnimateDirection();
            moveAt = Vector2.down;
            Move(moveAt);
        }
        //right
        if (left_x > 0)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
            {
                TurnRight();
            }

            AnimateDirection();
            moveAt = Vector2.right;
            float speedRed = 1;
            if(left_y != 0)
            {
                speedRed = 0.5f;
            }
            Move(moveAt, speedRed);
        }
        //left
        else if (left_x < 0)
        {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
            {
                TurnLeft();
            }

            AnimateDirection();
            moveAt = Vector2.left;
            float speedRed = 1;
            if (left_y != 0)
            {
                speedRed = 0.5f;
            }
            Move(moveAt, speedRed);
        }

        if(moveAt == Vector2.zero && !animationSystem.IsPlaying(animationDirection) && !characterAttacks.IsAttacking())
        {
            animationSystem.Stop(animationDirection, true, false);
            animationSystem.Play(animationDirection);
        }

                /*if(offPos.x != 0 && offPos.y != 0)
                {
                    offPos = oldTrgt;
                }

                if (!isMoving && aimPos == Vector2.zero && !animationSystem.IsPlaying(animationDirection) && !characterAttacks.IsAttacking())
                {
                    animationSystem.Stop(animationDirection, true, false);
                    animationSystem.Play(animationDirection);
                }
                else if (!isMoving && aimPos != Vector2.zero)
                {
                    float angle = Mathf.Atan2(offPos.y, offPos.x);
                    angle = angle * Mathf.Rad2Deg;

                    Vector2 moveAt = Vector2.zero;

                    string prevDir = animationDirection;
                    bool isValidAngle = true;
                    //up
                    if (angle > 45 && angle < 135)
                    {
                        if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
                        {
                            TurnUp();
                        }

                        AnimateDirection();
                        moveAt = Vector2.up;
                    }
                    //down
                    else if (angle < -45 && angle > -135)
                    {
                        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
                        {
                            TurnDown();
                        }

                        AnimateDirection();
                        moveAt = Vector2.down;
                    }
                    //right
                    else if (angle < 45 && angle >= 0 || angle > -45 && angle <= 0)
                    {
                        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
                        {
                            TurnRight();
                        }

                        AnimateDirection();
                        moveAt = Vector2.right;
                    }
                    //left
                    else if (angle > 135 && angle <= 180 || angle < -135 && angle >= -180)
                    {
                        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.JoystickButton4) && canRotate)
                        {
                            TurnLeft();
                        }

                        AnimateDirection();
                        moveAt = Vector2.left;
                    }
                    else
                    {
                        isValidAngle = false;
                        destPos += oldTrgt;
                        LookAt(destPos);
                    }

                    if(isValidAngle)
                    {
                        UpdateBlocks();
                        if (!forwardBlock && !forwardMovingNPC && !forwardAboutBlock)
                        {
                            destPos += moveAt;
                            oldTrgt = moveAt;
                        }
                    }

                    prevPos = transform.position;
                    speedBonus = 1;
                    isMoving = true;
                }
                else
                {
                    if(characterStats.HasStamina() && characterStats.CanDash())
                    {
                        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.JoystickButton2))
                        {
                            Debug.Log("Dash attempt");
                            if (!forwardDashBlock)
                            {
                                Debug.Log("Success");
                                characterStats.DashStamina();
                                destPos += oldTrgt;
                                transform.position = destPos;
                                isMoving = false;
                            }
                            else if (!forwardAboutBlock)
                            {
                                Debug.Log("Success");
                                characterStats.DashStamina();
                                transform.position = destPos;
                                isMoving = false;
                            }
                        }
                    }

                    if (Vector2.Distance(transform.position, destPos) < 0.1f)
                    {
                        transform.position = destPos;
                        isMoving = false;
                    }
                    else
                    {
                        transform.position = Vector2.MoveTowards(transform.position, destPos, movementSpeed * speedBonus * Time.deltaTime);
                    }
                }*/
    }

    void HandleAction()
    {
        if (!canAction)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if(!actionRecognition)
            {
                Debug.Log("Can't recognize actions.");
                return;
            }

            actionRecognition.TriggerAction();
        }
    }

    void HandleAttack()
    {
        if (!canAttack)
        {
            return;
        }

        bool attack = false;
        if(Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            if(Input.GetKey(KeyCode.JoystickButton5))
            {
                characterAttacks.ChangeAttack(2);
            }
            else
            {
                characterAttacks.ChangeAttack(0);
            }
            attack = true;
        }
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            if (Input.GetKey(KeyCode.JoystickButton5))
            {
                characterAttacks.ChangeAttack(3);
            }
            else
            {
                characterAttacks.ChangeAttack(1);
            }
            attack = true;
        }

        if (attack)
        {
            if (characterAttacks.Attack(characterStats, this, animationSystem, animationDirection))
            {
                Debug.Log("Attacking!");
            }
            else
            {
                Debug.Log("Cannot attack when attacking is going on");
            }
        }
    }

    protected override void DoDuringUpdate()
    { 
        HandleMovement();
        HandleAction();
        HandleAttack();
    }

    void OnGUI()
    {

    }
}
