using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPlayer : Character
{
    public Weapon currentWeapon;
    bool activatedPlasma = false;
    float modeCooldown = 0;
    float plasmaCount = 0;
    bool plasmaRecovery = false;
    bool staminaRecovery = false;
    float attackCooldown = 0;
    float momentum = 1;
    float oldMomentum = 0;

    public float movementSpeed;
    public float maxMomentum = 2;
    public float staminaReduction = 2;
    string direction = "Right";
    bool attackMode = false;
    bool sprintAttack = false;

    public GUIStyle style;

    void Update()
    {
        if(currentWeapon == null || anim == null)
        {
            return;
        }

        string r = direction;
        if(attackMode)
        {
            r = r + "Attack";
            if(currentWeapon.currentMode > 0)
            {
                r = r + "Alt" + currentWeapon.currentMode.ToString();
            }
        }
        anim.Run(r);
        anim.Speed(momentum);

        r = direction;
        if(activatedPlasma)
        {
            string prefix = "Normal";
            if (currentWeapon.currentMode > 0)
            {
                prefix = "Alt" + currentWeapon.currentMode.ToString();
            }
            if (attackCooldown > 0)
            {
                prefix = prefix + "Attack";
                if(sprintAttack)
                {
                    prefix = prefix + "Run";
                }
            }
            currentWeapon.plasmaAnimation.Play(prefix);
        }
        else
        {
            currentWeapon.plasmaAnimation.Stop();
        }
        currentWeapon.plasmaAnimation.Run(r);

        if(attackCooldown <= 0)
        {
            if (Input.GetKey(KeyCode.L) && !staminaRecovery && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)))
            {
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

            string newDir = "";
            if (Input.GetKey(KeyCode.W))
            {
                newDir = "Up";
                direction = "Up";
                transform.Translate(Vector2.up * Time.deltaTime * movementSpeed * momentum);
                anim.Play("Walk");
            }
            if (Input.GetKey(KeyCode.S))
            {
                newDir = "Down";
                direction = "Down";
                transform.Translate(Vector2.down * Time.deltaTime * movementSpeed * momentum);
                anim.Play("Walk");
            }
            if (Input.GetKey(KeyCode.A))
            {
                newDir = "Left";
                direction = "Left";
                transform.Translate(Vector2.left * Time.deltaTime * movementSpeed * momentum);
                anim.Play("Walk");
            }
            if (Input.GetKey(KeyCode.D))
            {
                newDir = "Right";
                direction = "Right";
                transform.Translate(Vector2.right * Time.deltaTime * movementSpeed * momentum);
                anim.Play("Walk");
            }
            if (newDir == "")
            {
                anim.Play("Idle");
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                attackMode = !attackMode;
            }
        }
        else if (oldMomentum > 1)
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

            transform.Translate(v * Time.deltaTime * momentum * movementSpeed);

            if(oldMomentum > 0)
            {
                oldMomentum -= Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && attackMode)
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

        if(stats.values.stamina.currentValue > 0 && !staminaRecovery)
        {
            if (momentum > 1)
            {
                stats.values.stamina.currentValue -= Time.deltaTime * momentum * staminaReduction;
            }

            if (Input.GetKeyDown(KeyCode.Space) && attackCooldown <= 0 && attackMode)
            {
                if(momentum >= maxMomentum)
                {
                    sprintAttack = true;
                    anim.Play("AttackRun");
                }
                else
                {
                    anim.Play("Attack");
                }
                oldMomentum = momentum;
                momentum = 1;
                attackCooldown = currentWeapon.GetCurrentMode().GetRealRecoveryTime();

                float power = currentWeapon.GetCurrentMode().GetRealPower(activatedPlasma) * stats.points.strength.GetRealValue();
                float endurancePercent = ((float)currentWeapon.endurance / (float)currentWeapon.maxEndurance);
                if(endurancePercent * 100 < 20)
                {
                    power *= endurancePercent;
                }
                if(sprintAttack)
                {
                    power *= 1.5f;
                }

                Debug.Log("Strike: " + power);
                float staminaReduction = currentWeapon.GetCurrentMode().GetRealStaminaUse();
                if (sprintAttack)
                {
                    staminaReduction *= 1.5f;
                }
                stats.values.stamina.currentValue -= staminaReduction;

                if(currentWeapon.endurance > 0)
                {
                    currentWeapon.endurance -= currentWeapon.GetCurrentMode().GetRealEndurance(activatedPlasma);
                }
                else
                {
                    currentWeapon.endurance = 0;
                }
            }
        }
        else
        {
            staminaRecovery = true;
        }

        if(attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        else
        {
            if(sprintAttack)
            {
                sprintAttack = false;
            }
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

        if(modeCooldown > 0)
        {
            modeCooldown -= Time.deltaTime;
        }

        if(currentWeapon.hasPlasma && attackMode)
        {
            if (plasmaCount > 0 && !plasmaRecovery)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    activatedPlasma = true;
                }
                else
                {
                    activatedPlasma = false;
                }
            }
            else
            {
                plasmaRecovery = true;
                activatedPlasma = false;
            }

            if(plasmaRecovery)
            {
                if(plasmaCount >= currentWeapon.GetPlasmaCharge())
                {
                    plasmaRecovery = false;
                }
            }

            if(activatedPlasma && plasmaCount > 0)
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
            activatedPlasma = false;
        }
    }

    void OnGUI()
    {
        if (currentWeapon == null)
        {
            return;
        }

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
        txt += "Using plasma: " + activatedPlasma + "\n";
        txt += "Plasma drain: " + currentWeapon.GetCurrentMode().GetRealPlasmaDrain() + "\n";
        txt += "Endurance: " + currentWeapon.endurance + " / " + currentWeapon.maxEndurance + "\n";
        txt += "Endurance reduction: " + currentWeapon.GetCurrentMode().GetRealEndurance(activatedPlasma) + "\n";
        txt += "Power: " + currentWeapon.GetCurrentMode().GetRealPower(activatedPlasma) + "\n";
        txt += "Stamina: " + currentWeapon.GetCurrentMode().GetRealStaminaUse() + "\n";
        txt += "Attack cooldown: " + currentWeapon.GetCurrentMode().GetRealRecoveryTime() + "\n";

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), txt, style);
    }
}
