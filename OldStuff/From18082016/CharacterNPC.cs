using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DialogInformation))]
[RequireComponent(typeof(VendorInformation))]
[RequireComponent(typeof(GiftingInformation))]

public class CharacterNPC : CharacterMaster
{
    [HideInInspector]
    public DialogInformation dialogInformation;
    [HideInInspector]
    public VendorInformation vendorInformation;
    [HideInInspector]
    public GiftingInformation giftingInformation;

    public enum State { InitStep, Execute, Pathfinding, Exception, Aggro };
    public State npcState;
    string exceptionMessage;

    public enum MovementPointType { Idle, MoveTo, LookAt, PlayerFollow };
    public enum MovementPointRandom { Constant, Random };

    public enum ParameterType { HasAggro, CanConversate, PassiveAggro, IsScared };
    public List<ParameterType> parameters = new List<ParameterType>();

    [System.Serializable]
    public class MovementPoint
    {
        public MovementPointType type;
        public MovementPointRandom randomizationTarget;
        public MovementPointRandom randomizationTime;

        public float min;
        public float max;

        public float speed;
        public float time;
        public Vector2 target;
        public Vector2 target2;
        public Vector2 realTarget;

        public MovementPoint()
        {

        }

        public MovementPoint(MovementPoint p)
        {
            type = p.type;
            randomizationTarget = p.randomizationTarget;
            randomizationTime = p.randomizationTime;

            min = p.min;
            max = p.max;

            speed = p.speed;
            time = p.time;
            target = p.target;
            target2 = p.target2;
            realTarget = p.realTarget;
        }
    }

    [System.Serializable]
    public class ActionContainer
    {
        public string tag = "";
        public List<MovementPoint> actions = new List<MovementPoint>();
        public int index = 0;
        public int previndex = -1;

        public ActionContainer()
        {
            index = 0;
            previndex = -1;
        }

        public ActionContainer(string tag)
        {
            this.tag = tag;
            index = 0;
            previndex = -1;
        }

        public ActionContainer(ActionContainer c)
        {
            tag = c.tag;
            index = c.index;
            previndex = c.previndex;
            actions.Clear();
            foreach(MovementPoint mp in c.actions)
            {
                actions.Add(new MovementPoint(mp));
            }
        }
    }

    public List<ActionContainer> backupActions = new List<ActionContainer>();
    public ActionContainer mainAction = new ActionContainer("Default");

    public enum ActionType { Check, Execute, ReduceUses };
    public enum ActionCheckType { WhenStatIs, WhenPlayerIs, IfTheresUses };
    public enum ActionStat { BiggerThan, SmallerThan, BiggerOrEqualThan, SmallerOrEqualThan, Equal};
    public enum ActionPlayer { InDistance, InClearSight };

    [System.Serializable]
    public class AttackAction
    {
        public ActionType type;
        public ActionCheckType checkType;
        public ActionStat stat;
        public CharacterStats.StatType statType;
        public float statValue;
        public ActionPlayer player;
        public float playerValue;
        public int usesAmount;

        public int activateThis;
        public float customCooldown;

        public AttackAction()
        {

        }

        public AttackAction(AttackAction a)
        {
            type = a.type;
            stat = a.stat;
            statType = a.statType;
            statValue = a.statValue;
            player = a.player;
            playerValue = a.playerValue;
            activateThis = a.activateThis;
        }
    }

    [System.Serializable]
    public class AttackContainer
    {
        public string eventName = "";
        public int usesAmount = 0;
        public List<AttackAction> actions = new List<AttackAction>();

        public AttackContainer()
        {

        }

        public AttackContainer(AttackContainer a)
        {
            eventName = a.eventName;
            usesAmount = a.usesAmount;
            actions.Clear();
            foreach(AttackAction attack in a.actions)
            {
                actions.Add(new AttackAction(attack));
            }
        }
    }

    public List<AttackContainer> attackActions = new List<AttackContainer>();

    int attackIndex = -1;
    float interval = 0;
    int state = 0;
    int moodState = 0;

    public float aggroLookDistance;
    public float aggroActDistance;
    public float aggroSpeedMultiplier;

    public bool staticPath = false;

    Vector2 stepsInto = Vector2.zero;
    Vector2 curPoint;
    Vector2 trgtPoint = Vector2.zero;
    Vector2 pathTarget;
    Vector2 startpoint;
    Vector2 origStartPoint;
    Vector2 staticOrigStartPoint;
    bool pathfinding = false;
    bool targetPlayer = false;
    bool aggro = false;
    bool aggroBounce = false;
    CharacterMaster player;

    [HideInInspector]
    public bool showParameters = false;
    [HideInInspector]
    public bool showActions = false;
    [HideInInspector]
    public bool showAttacks = false;

    List<PathFind.Point> path = new List<PathFind.Point>();

    void UpdateRealTargets()
    {
        Vector2 current = transform.position;
        if (initialized)
        {
            current = origStartPoint;
            if(staticPath && !pathfinding && !aggroBounce)
            {
                current = staticOrigStartPoint;
                //Debug.Log("Origin: static");
            }
            else
            {
                //Debug.Log("Origin: original " + staticPath + " " + pathfinding + " " + aggroBounce);
            }
        }
        else
        {
            //Debug.Log("Origin: current");
        }

        foreach (MovementPoint action in mainAction.actions)
        {
            current += action.target;
            action.realTarget = current;
        }
    }

    void SetState(State npcState, int state = 0)
    {
        this.state = state;
        this.npcState = npcState;
    }

    void BackupCurrentActions(string newName)
    {
        backupActions.Add(new ActionContainer(mainAction));
        mainAction.tag = newName;
    }

    ActionContainer GetAction(string tag)
    {
        foreach (ActionContainer container in backupActions)
        {
            if (container.tag == tag)
            {
                return container;
            }
        }

        return null;
    }

    void RestoreAction(string old)
    {
        foreach(ActionContainer container in backupActions)
        {
            if(container.tag == old)
            {
                mainAction = new ActionContainer(container);
                break;
            }
        }

        backupActions.Clear();
    }

    void ExecuteAttacks()
    {
        if(isMoving)
        {
            return;
        }

        foreach(AttackContainer a in attackActions)
        {
            foreach(AttackAction action in a.actions)
            {
                if (action.type == ActionType.Execute)
                {
                    LookAt(player.transform.position);
                    characterAttacks.ChangeAttack(action.activateThis);
                    characterAttacks.GetCurrent().animationCooldown = action.customCooldown;
                    characterAttacks.Attack(characterStats, this, animationSystem, animationDirection);
                    continue;
                }
                if(action.type == ActionType.ReduceUses)
                {
                    a.usesAmount -= action.usesAmount;
                    continue;
                }
                if(action.type == ActionType.Check)
                {
                    bool checkSuccess = false;
                    if(action.checkType == ActionCheckType.IfTheresUses)
                    {
                        if(a.usesAmount > 0)
                        {
                            checkSuccess = true;
                        }
                    }
                    if(action.checkType == ActionCheckType.WhenStatIs)
                    {
                        //stat checking here
                    }
                    if(action.checkType == ActionCheckType.WhenPlayerIs)
                    {
                        if(action.player == ActionPlayer.InDistance)
                        {
                            float distance = Vector2.Distance(transform.position, player.transform.position);
                            if (distance < action.playerValue)
                            {
                                checkSuccess = true;
                            }
                        }
                        if(action.player == ActionPlayer.InClearSight)
                        {
                            if(!Physics2D.Raycast(transform.position, player.transform.position - transform.position, action.playerValue, cannotMoveAgainst))
                            {
                                checkSuccess = true;
                            }
                        }
                    }

                    if(!checkSuccess)
                    {
                        break;
                    }
                }
            }
        }
    }

    /*void ExecuteActions()
    {
        if (mainAction.actions.Count == 0 || mainAction.index >= mainAction.actions.Count || mainAction.index < 0)
        {
            SetState(State.Exception);
            exceptionMessage = "ActionIndexOutOfRange";
            return;
        }

        MovementPoint action = mainAction.actions[mainAction.index];
        if (state == 0)
        {
            if (action.type == MovementPointType.Idle)
            {
                if (action.randomizationTime == MovementPointRandom.Constant)
                {
                    interval = action.time;
                }
                if (action.randomizationTime == MovementPointRandom.Random)
                {
                    interval = (int)Random.Range(action.min, action.max);
                }
                isMovingNPC = false;
                state = 1;
            }
            if (action.type == MovementPointType.LookAt)
            {
                curPoint = transform.position;
                if (action.randomizationTarget == MovementPointRandom.Constant)
                {
                    trgtPoint = action.realTarget;
                }
                if (action.randomizationTarget == MovementPointRandom.Random)
                {
                    Vector2 rand = new Vector2((int)Random.Range(action.target.x, action.target.y), (int)Random.Range(action.target2.x, action.target2.y));
                    trgtPoint = curPoint + rand;
                }

                if (action.randomizationTime == MovementPointRandom.Constant)
                {
                    interval = action.time;
                }
                if (action.randomizationTime == MovementPointRandom.Random)
                {
                    interval = (int)Random.Range(action.min, action.max);
                }
                isMovingNPC = false;
                LookAt(trgtPoint);
                state = 1;
            }
            if (action.type == MovementPointType.PlayerFollow)
            {
                curPoint = transform.position;
                trgtPoint = player.GetGridPos();
                LookAt(trgtPoint);
                isMovingNPC = true;
                targetPlayer = true;

                SetState(State.Pathfinding);
                pathTarget = trgtPoint;
            }
            if (action.type == MovementPointType.MoveTo)
            {
                curPoint = transform.position;
                if (action.randomizationTarget == MovementPointRandom.Constant)
                {
                    trgtPoint = action.realTarget;
                }
                if (action.randomizationTarget == MovementPointRandom.Random)
                {
                    trgtPoint = curPoint + new Vector2((int)Random.Range(action.target.x, action.target.y), (int)Random.Range(action.target2.x, action.target2.y));
                }

                LookAt(trgtPoint);
                isMovingNPC = true;
                state = 1;
            }
        }
        else if (state == 1)
        {
            if (!canMove || !canRotate)
            {
                return;
            }

            if (action.type == MovementPointType.Idle)
            {
                if (interval > 0)
                {
                    interval -= Time.deltaTime;
                }
                else
                {
                    state = 2;
                }
            }
            if (action.type == MovementPointType.LookAt)
            {
                if (interval > 0)
                {
                    interval -= Time.deltaTime;
                }
                else
                {
                    state = 2;
                }
            }
            if (action.type == MovementPointType.MoveTo || action.type == MovementPointType.PlayerFollow)
            {
                LookAt(trgtPoint);
                if (!isMoving)
                {
                    UpdateBlocks(Vector2.zero);

                    if (forwardBlock || targetPlayer && pathTarget != player.GetGridPos())
                    {
                        SetState(State.Pathfinding);
                        pathTarget = player.GetGridPos();
                        //Debug.Log("Seek path! " + pathTarget);
                        state = 0;
                    }
                    else if (!forwardMovingNPC && !forwardBlock)
                    {
                        destPos = transform.position + hitArea.transform.localPosition;
                        
                        isMoving = true;
                        stepsInto += destPos;
                    }
                }
                else
                {
                    float distanceTarget = Vector2.Distance(transform.position, trgtPoint);
                    if (distanceTarget < 0.05f)
                    {
                        state = 2;
                        transform.position = trgtPoint;
                        isMoving = false;
                        stepsInto = Vector2.zero;
                    }
                    else
                    {
                        if (Vector2.Distance(transform.position, destPos) < 0.05f)
                        {
                            transform.position = destPos;
                            isMoving = false;
                        }
                        else
                        {
                            transform.position = Vector2.MoveTowards(transform.position, destPos, Time.deltaTime * action.speed);
                            //Vector2 trgt = destPos - (Vector2)transform.position;
                            //transform.Translate(trgt.normalized * action.speed * Time.deltaTime);
                        }
                    }
                }
            }
        }
        else if (state == 2)
        {
            state = 0;
            if (mainAction.index < mainAction.actions.Count - 1)
            {
                mainAction.index += 1;
            }
            else
            {
                mainAction.index = 0;

                if(pathfinding)
                {
                    //Debug.Log("Path over: " + mainAction.index + " " + mainAction.previndex);
                    pathfinding = false;
                    targetPlayer = false;
                    int prev = mainAction.previndex;
                    RestoreAction("Default");
                    mainAction.index = prev;
                    //Debug.Log("Starting at index: " + mainAction.index + " " + mainAction.actions.Count);
                    SetState(State.InitStep);
                }
                //if (pathFindActions.Count > 0)
                //{
                //    pathFindActions.Clear();
                //    index = previndex;
                //    state = 2;
                //    pathfinding = false;
                //}
}
        }
    }

    void ExecutePathFinding()
    {
        if (state == 0 && TileKeeper.levelSet)
        {
            UpdateRealTargets();
            if (!pathfinding)
            {
                BackupCurrentActions("Pathfinding");
            }
            ActionContainer originalAction = GetAction("Default");

            startpoint = transform.position;
            Vector2 origPathTarget = pathTarget;

            Vector2 current = transform.position;

            if (!TileKeeper.OutOfBounds(pathTarget) && !TileKeeper.OutOfBounds(transform.position))
            {
                //Debug.Log("Path from " + current + " to " + pathTarget + " seems valid.");
                path.Clear();
                Vector2 fixedPosition = pathTarget;

                int usedIndex = originalAction.index;
                int checkPrev = -1;
                while (path.Count == 0)
                {
                    Vector2 pointCurrent = TileKeeper.TranslateToPoint(current);
                    Vector2 pointFixedPosition = TileKeeper.TranslateToPoint(fixedPosition);
                    //Debug.Log("Searching for position: " + fixedPosition + " " + current + " -> " + pointFixedPosition + " " + pointCurrent);

                    if (TileKeeper.PointIsBlock(pointFixedPosition, true) && !targetPlayer)
                    {
                        if (checkPrev == originalAction.previndex)
                        {
                            //Debug.Log("Seems like we might be a little stuck...");
                            if (originalAction.previndex < originalAction.actions.Count - 1)
                            {
                                originalAction.previndex += 1;
                            }
                            else
                            {
                                originalAction.previndex = 0;
                            }
                        }

                        usedIndex = originalAction.previndex;
                        checkPrev = originalAction.previndex;
                        //Debug.Log("No way to reach target. Jumping to next one. Current index: " + usedIndex);
                        if (usedIndex < originalAction.actions.Count - 1)
                        {
                            usedIndex += 1;
                        }
                        else
                        {
                            usedIndex = 0;
                        }

                        pathTarget = originalAction.actions[usedIndex].realTarget;

                        fixedPosition = pathTarget;
                        //Debug.Log("Trying this next time: " + TileKeeper.TranslateToPoint(fixedPosition) + " " + usedIndex);
                        originalAction.index = usedIndex;
                    }
                    else
                    {
                        if(targetPlayer)
                        {
                            path = PathFind.Pathfinding.FindPath(TileKeeper.gridNoPlayer, new PathFind.Point((int)pointCurrent.x, (int)pointCurrent.y), new PathFind.Point((int)pointFixedPosition.x, (int)pointFixedPosition.y));
                        }
                        else
                        {
                            path = PathFind.Pathfinding.FindPath(TileKeeper.grid, new PathFind.Point((int)pointCurrent.x, (int)pointCurrent.y), new PathFind.Point((int)pointFixedPosition.x, (int)pointFixedPosition.y));
                        }

                        if (path.Count > 0)
                        {
                            mainAction.index = 0;
                            mainAction.previndex = usedIndex;
                            //Debug.Log("Success, new target: " + fixedPosition);
                        }
                        else
                        {
                            //Debug.Log("Failed, fuck.");
                            //Debug.Log("No way to reach target. Jumping to next one. Current index: " + usedIndex);
                            if (usedIndex < originalAction.actions.Count - 1)
                            {
                                usedIndex += 1;
                            }
                            else
                            {
                                usedIndex = 0;
                            }

                            pathTarget = originalAction.actions[usedIndex].realTarget;

                            fixedPosition = pathTarget;
                            //Debug.Log("Trying this next time: " + TileKeeper.TranslateToPoint(fixedPosition) + " " + usedIndex);
                            originalAction.index = usedIndex;
                        }
                    }
                }

                mainAction.actions.Clear();
                Vector2 thisPosition = current;

                if (path.Count > 0)
                {
                    //Debug.Log(gameObject.name + " searches for path from " + current + " to " + fixedPosition + "!");

                    if (Random.Range(0, 2) == 0 && !targetPlayer)
                    {
                        MovementPoint mp = new MovementPoint();
                        mp.time = Random.Range(1, 3);
                        mp.type = MovementPointType.Idle;

                        mainAction.actions.Add(mp);
                    }

                    foreach (PathFind.Point point in path)
                    {
                        Vector2 pointVector = new Vector2(point.x, point.y);

                        Vector2 relativeVector = TileKeeper.TranslateToRelative(pointVector);
                        Vector2 thisPoint = TileKeeper.TranslateToPoint(thisPosition);

                        Vector2 difPoint = pointVector - thisPoint;
                        difPoint.y = -difPoint.y;
                        relativeVector.y = -relativeVector.y;

                        MovementPoint mp = new MovementPoint();
                        mp.target = difPoint;
                        mp.speed = 1;
                        if (aggro)
                        {
                            mp.speed *= aggroSpeedMultiplier;
                        }
                        mp.randomizationTarget = MovementPointRandom.Constant;
                        mp.type = MovementPointType.MoveTo;

                        //Debug.Log(relativeVector + " -> " + pointVector + ", " + thisPosition + " -> " + thisPoint + ": " + difPoint);

                        thisPosition += difPoint;
                        mp.realTarget = relativeVector;

                        mainAction.actions.Add(mp);
                    }

                    Debug.Log(gameObject.name + " reached the end!");
                    SetState(State.InitStep);

                    pathfinding = true;
                    if(aggroBounce)
                    {
                        aggroBounce = false;
                    }
                }
            }
            else
            {
                //Debug.Log(gameObject.name + " target out of reach: " + pathTarget);

                if (mainAction.index > mainAction.actions.Count - 1)
                {
                    mainAction.index += 1;
                }
                else
                {
                    mainAction.index = 0;
                }
                mainAction.previndex = mainAction.index;
                pathTarget = mainAction.actions[mainAction.index].realTarget;
            }
        }
        else
        {

        }
    }

    void ExecuteAggro()
    {
        if (parameters.Contains(ParameterType.HasAggro))
        {
            float distance = Vector2.Distance(player.gameObject.transform.position, transform.position);
            if (!aggro)
            {
                if (distance < aggroActDistance)
                {
                    BackupCurrentActions("Aggro");

                    mainAction.actions.Clear();

                    MovementPoint mp = new MovementPoint();
                    mp.type = MovementPointType.PlayerFollow;
                    mainAction.actions.Add(mp);

                    mainAction.index = 0;

                    aggro = true;

                    SetState(State.InitStep);
                }
            }
            else
            {
                if(distance > aggroLookDistance)
                {
                    transform.position = new Vector2((int)transform.position.x, (int)transform.position.y);

                    RestoreAction("Default");
                    aggro = false;
                    pathfinding = false;
                    targetPlayer = false;

                    UpdateRealTargets();
                    aggroBounce = true;

                    if(staticPath)
                    {
                        int smallestIndex = -1;
                        float smallestDistance = 10000;
                        int ind = 0;
                        bool alreadyOnPath = false;

                        foreach (MovementPoint mp in mainAction.actions)
                        {
                            float dist = Vector2.Distance(mp.realTarget, transform.position);
                            if (dist < smallestDistance)
                            {
                                smallestDistance = dist;
                                smallestIndex = ind;

                                if(mp.realTarget == (Vector2)transform.position)
                                {
                                    alreadyOnPath = true;
                                    break;
                                }
                            }

                            ind += 1;
                        }

                        mainAction.index = smallestIndex;
                        mainAction.previndex = smallestIndex;

                        if(!alreadyOnPath)
                        {
                            SetState(State.Pathfinding);
                            pathTarget = mainAction.actions[mainAction.index].realTarget;
                            Debug.Log("Pathfinding back to " + smallestIndex + " " + pathTarget);
                        }
                        else
                        {
                            aggroBounce = false;
                            SetState(State.InitStep);
                            Debug.Log("Going back to " + smallestIndex + " " + mainAction.actions[mainAction.index].realTarget);
                        }
                    }
                    else
                    {
                        SetState(State.InitStep);
                    }
                }
            }
        }
    }
    */

    void HandleMovement()
    {
        UpdateRealTargets();

        if(canAttack && canAction)
        {
            ExecuteAttacks();
        }

        if(canMove && canAction)
        {
            //ExecuteAggro();

            if (npcState == State.InitStep)
            {
                //origStartPoint = transform.position;
                //mainAction.index = 0;
                SetState(State.Execute);
            }
            else if (npcState == State.Pathfinding)
            {
                //ExecutePathFinding();
            }
            else if (npcState == State.Execute)
            {
                //ExecuteActions();
            }
            else if (npcState == State.Exception)
            {
                //Debug.Log(exceptionMessage);
            }
        }
        
    }

    void OnDrawGizmosSelected()
    {
        Vector2 current = transform.position;
        if(initialized)
        {
            current = origStartPoint;
            if(staticPath && !pathfinding && !aggro && !aggroBounce)
            {
                current = staticOrigStartPoint;
            }
        }

        for (int i = 0; i < mainAction.actions.Count; i++)
        {
            MovementPoint action = mainAction.actions[i];

            if (action.randomizationTarget == MovementPointRandom.Random)
            {
                continue;
            }

            if(action.type == MovementPointType.MoveTo)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(current, current + action.target);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(current + action.target, new Vector3(0.5f, 0.5f, 0.5f));

                current += action.target;
            }
            if(action.type == MovementPointType.LookAt)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, current + action.target);
            }
            if(action.type == MovementPointType.Idle)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(current, new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
    }

    void OnGUI()
    {
        Vector2 pos = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        pos.y = Screen.height - pos.y;

        Rect hpRect = new Rect(pos.x-50, pos.y-50, 100, 15);
        GUI.Box(hpRect, "");

        GUI.color = Color.red;
        float prcnt = characterStats.health.currentValue / characterStats.health.maxValue;
        hpRect = new Rect(hpRect.x + 2, hpRect.y + 2, hpRect.width * prcnt - 4, hpRect.height - 4);
        //Debug.Log(hpRect);
        GUI.Box(hpRect, "");
    }

    protected override bool DoDuringStart()
    {
        origStartPoint = transform.position;
        startpoint = transform.position;
        staticOrigStartPoint = transform.position;

        dialogInformation = GetComponent<DialogInformation>();
        if(!dialogInformation)
        {
            Debug.Log("DialogInformation is missing.");
            return false;
        }

        vendorInformation = GetComponent<VendorInformation>();
        if (!vendorInformation)
        {
            Debug.Log("VendorInformation is missing.");
            return false;
        }

        giftingInformation = GetComponent<GiftingInformation>();
        if (!giftingInformation)
        {
            Debug.Log("GiftingInformation is missing.");
            return false;
        }

        UpdateRealTargets();

        GameObject obj = GameObject.Find("Player");
        if(obj)
        {
            player = obj.GetComponent<CharacterMaster>();
            if(!player)
            {
                Debug.Log("Couldn't find player.");
                return false;
            }
        }
        else
        {
            Debug.Log("Couldn't find player.");
            return false;
        }

        return true;
    }

    protected override void DoOnTrigger()
    {
        Debug.Log("Activate dialog");
    }

    protected override void DoDuringUpdate()
    {
        HandleMovement();
    }
}
