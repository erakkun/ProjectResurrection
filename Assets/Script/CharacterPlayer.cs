﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPlayer : Character
{
    public Weapon currentWeapon;

	bool attackMode = false;
	bool plasmaMode = false;

    bool plasmaRecovery = false;
    bool staminaRecovery = false;

	bool attacking = false;
	bool sprinting = false;
	bool sprintAttacking = false;

	float modeCooldown = 0;
    float plasmaCount = 0;

    public GUIStyle style;

	override protected void initialize()
	{
		base.initialize();
	}

	override protected bool checkInitialization()
	{
		if(currentWeapon == null)
        {
			return false;
        }

		return base.checkInitialization();
	}

	override protected void defineAnimationTag()
	{
		base.defineAnimationTag();

        if(attackMode)
        {
            animationTag += currentWeapon.tag;
            if(currentWeapon.currentMode > 0)
            {
                animationTag += "Alt" + currentWeapon.currentMode.ToString();
            }
        }
	}

	override protected void beforeUpdate()
	{
		
	}

	override protected void afterUpdate()
	{
        handlePlasma();
        handleMomentum();
		handleControls();
		handleStamina();
		handleAttack();
	}

	void handleAttack()
	{
        if(attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        else
        {
			attacking = false;
			sprintAttacking = false;
        }

        if(modeCooldown > 0)
        {
            modeCooldown -= Time.deltaTime;
        }

		if(stats.values.stamina.currentValue <= 0 || staminaRecovery)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Space) && attackCooldown <= 0 && attackMode)
        {
			attacking = true;
            if(momentum >= maxMomentum)
            {
                sprintAttacking = true;
                characterAnimation.play("AttackRun");
            }
            else
            {
                characterAnimation.play("Attack");
            }

            oldMomentum = momentum;
            momentum = 1;
            attackCooldown = currentWeapon.GetCurrentMode().GetRealRecoveryTime();
            if (sprintAttacking)
            {
                attackCooldown *= 1.25f;
            }

            float power = currentWeapon.GetCurrentMode().GetRealPower(plasmaMode) * stats.points.strength.GetRealValue();
            float endurancePercent = ((float)currentWeapon.endurance / (float)currentWeapon.maxEndurance);
            if(endurancePercent * 100 < 20)
            {
                power *= endurancePercent;
            }
            if(sprintAttacking)
            {
                power *= 1.5f;
            }

            Debug.Log("Strike: " + power);
            float staminaReduction = currentWeapon.GetCurrentMode().GetRealStaminaUse();
            if (sprintAttacking)
            {
                staminaReduction *= 1.5f;
            }
            stats.values.stamina.currentValue -= staminaReduction;

            if(currentWeapon.endurance > 0)
            {
                currentWeapon.endurance -= currentWeapon.GetCurrentMode().GetRealEndurance(plasmaMode);
            }
            else
            {
                currentWeapon.endurance = 0;
            }
        }
	}

	void handleMomentum()
	{
		sprinting = false;

		if(attackCooldown <= 0)
        {
            if (Input.GetKey(Controls.runButton) && !staminaRecovery && Controls.isMoving())
            {
				sprinting = true;
                if (momentum < maxMomentum)
                {
                    momentum += Time.deltaTime;
                }
            }
            else if(momentum > 1)
            {
                momentum -= Time.deltaTime;
            }
            else
            {
                momentum = 1;
            }
		}
		else if (oldMomentum > 1)
        {
            transform.Translate(directionToVector() * Time.deltaTime * momentum * movementSpeed);

            if(oldMomentum > 0)
            {
                oldMomentum -= Time.deltaTime;
            }
        }
	}

	void handleStamina()
	{
		if(stats.values.stamina.currentValue > 0 && !staminaRecovery)
        {
            if (momentum > 1)
            {
                stats.values.stamina.currentValue -= Time.deltaTime * momentum * staminaReduction;
            }
        }
        else
        {
            staminaRecovery = true;
        }

		if(momentum <= 1)
        {
            if (stats.values.stamina.currentValue < stats.values.stamina.maxValue)
            {
                if (staminaRecovery)
                {
                    stats.values.stamina.currentValue += stats.values.staminaRecovery.value * 2 * Time.deltaTime;
                    float prcnt = (stats.values.stamina.currentValue / stats.values.stamina.maxValue) * 100;
                    if (prcnt > 50)
                    {
                        staminaRecovery = false;
                    }
                }
                else
                {
                    stats.values.stamina.currentValue += stats.values.staminaRecovery.value * Time.deltaTime;
                }
            }
            else
            {
                staminaRecovery = false;
                stats.values.stamina.currentValue = stats.values.stamina.maxValue;
            }
        }
        if(stats.values.stamina.currentValue < 0)
        {
            stats.values.stamina.currentValue = 0;
        }
	}

	void handleControls()
	{
		if(attackCooldown <= 0)
        {
			float horizontal = Controls.getHorizontal();
			float vertical = Controls.getVertical();

           	Vector2 vDir = Vector2.zero;
            if (vertical > 0)
            {
                vDir = Vector2.up;
                direction = "Up";
            }
            if (vertical < 0)
            {
                vDir = Vector2.down;
                direction = "Down";
            }

            Vector2 hDir = Vector2.zero;
            if (horizontal < 0)
            {
                hDir = Vector2.left;
                direction = "Left";
            }
            if (horizontal > 0)
            {
                hDir = Vector2.right;
                direction = "Right";
            }

			if(vertical != 0 || horizontal != 0)
			{
                transform.Translate(vDir * Time.deltaTime * movementSpeed * momentum);
                transform.Translate(hDir * Time.deltaTime * movementSpeed * momentum);
                characterAnimation.play("Walk");
			}
			else
			{
                characterAnimation.play("Idle");
			}

            if (Input.GetKeyDown(Controls.attackModeButton))
            {
				attackMode = !attackMode;
            }
        }

		if (Input.GetKeyDown(Controls.weaponModeButton) && attackMode)
        {
            if(modeCooldown <= 0)
            {
                modeCooldown = currentWeapon.GetCurrentMode().modeChangeCooldown;
                if (currentWeapon.currentMode < currentWeapon.modes.Count - 1)
                {
                    currentWeapon.currentMode += 1;
                }
                else
                {
                    currentWeapon.currentMode = 0;
                }
            }
        }
	}

	void plasmaAnimationUpdate()
	{
        if(plasmaMode)
        {
            string prefix = "Normal";
            if (currentWeapon.currentMode > 0)
            {
                prefix = "Alt" + currentWeapon.currentMode.ToString();
            }
            if (attackCooldown > 0)
            {
                prefix = prefix + "Attack";
                if(sprintAttacking)
                {
                    prefix = prefix + "Run";
                }
            }
            currentWeapon.plasmaAnimation.play(prefix);
        }
        else
        {
            currentWeapon.plasmaAnimation.stop();
        }

        currentWeapon.plasmaAnimation.run(direction);
    }

    void handlePlasma()
    {
		plasmaAnimationUpdate();

        if(currentWeapon.hasPlasma && attackMode)
        {
            if (plasmaCount > 0 && !plasmaRecovery)
            {
                if (Input.GetKey(Controls.plasmaModeButton))
                {
                    plasmaMode = true;
                }
                else
                {
                    plasmaMode = false;
                }
            }
            else
            {
                plasmaRecovery = true;
                plasmaMode = false;
            }

            if(plasmaRecovery)
            {
                if(plasmaCount >= currentWeapon.GetPlasmaCharge())
                {
                    plasmaRecovery = false;
                }
            }

            if(plasmaMode && plasmaCount > 0)
            {
                plasmaCount -= currentWeapon.GetCurrentMode().GetRealPlasmaDrain() * Time.deltaTime;
            }
            else if(plasmaCount < currentWeapon.GetPlasmaCharge())
            {
                plasmaCount += currentWeapon.GetPlasmaRecoverySpeed() * Time.deltaTime;
            }
        }
        else
        {
            plasmaMode = false;
        }
    }

    void OnGUI()
    {
        string txt = "";

        txt += "Plasma status: " + plasmaCount + " / " + currentWeapon.GetPlasmaCharge() + "\n";
        txt += "Recovery: " + plasmaRecovery + "\n\n";

        txt += "Stamina status: " + stats.values.stamina.currentValue + " / " + stats.values.stamina.maxValue + "\n";
        txt += "Recovery: " + staminaRecovery + "\n";
        txt += "Attack cooldown: " + attackCooldown + "\n";
        txt += "Momentum: " + momentum + "\n\n";

        txt += currentWeapon.weaponName + "\n";
        txt += "Modes: " + currentWeapon.modes.Count + "\n";
        txt += "Current mode: " + currentWeapon.currentMode + "\n";
        txt += "Mode change cooldown: " + modeCooldown + "\n\n";
        txt += "Plasma enabled: " + currentWeapon.hasPlasma + "\n";
        txt += "Using plasma: " + plasmaMode + "\n";
        txt += "Plasma drain: " + currentWeapon.GetCurrentMode().GetRealPlasmaDrain() + "\n";
        txt += "Endurance: " + currentWeapon.endurance + " / " + currentWeapon.maxEndurance + "\n";
        txt += "Endurance reduction: " + currentWeapon.GetCurrentMode().GetRealEndurance(plasmaMode) + "\n";
        txt += "Power: " + currentWeapon.GetCurrentMode().GetRealPower(plasmaMode) + "\n";
        txt += "Stamina: " + currentWeapon.GetCurrentMode().GetRealStaminaUse() + "\n";
        txt += "Attack cooldown: " + currentWeapon.GetCurrentMode().GetRealRecoveryTime() + "\n";

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), txt, style);
    }
}
