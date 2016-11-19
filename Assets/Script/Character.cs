using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    [HideInInspector]
    public CharacterValues stats;
    [HideInInspector]
    public AnimationSystem characterAnimation;

    public float movementSpeed = 2;
    public float maxMomentum = 2;
    public float staminaReduction = 2;

	protected string direction = "Right";
	private string animationTag = "";
    protected float momentum = 1;
    protected float oldMomentum = 0;
	protected float attackCooldown = 0;

	protected bool initialized = false;

    void Start()
    {
		beforeStart();
		initialize();
		afterStart();
    }

	void Update()
	{
		if(!initialized)
		{
			return;
		}

		beforeUpdate();

		defineAnimationTag();
		characterAnimation.run(animationTag);
        characterAnimation.speed(momentum);

		afterUpdate();
	}

	virtual protected void defineAnimationTag()
	{
		animationTag = direction;
	}

	virtual protected void beforeUpdate()
	{

	}

	virtual protected void afterUpdate()
	{

	}

	virtual protected void beforeStart()
	{

	}

	virtual protected void afterStart()
	{

	}

	virtual protected void initialize()
	{
        stats = GetComponent<CharacterValues>();
        characterAnimation = GetComponent<AnimationSystem>();

		initialized = checkInitialization();
	}

	virtual protected bool checkInitialization()
	{
		if(stats && characterAnimation)
		{
			return true;
		}

		return false;
	}

	Vector2 directionToVector()
	{
        Vector2 v = Vector2.zero;
        switch(direction)
        {
            case "Up":
                v = Vector2.up;
                break;
            case "Down":
                v = Vector2.down;
                break;
            case "Right":
                v = Vector2.right;
                break;
            case "Left":
                v = Vector2.left;
                break;
        }

		return v;
	}
}
