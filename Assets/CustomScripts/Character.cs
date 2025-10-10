using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    public static List<Character> charList = new List<Character>();
    public static List<Character> bluForList = new List<Character>();
    public static int numberOfCharacters = 0;
    public static int numberOfBluFor = 0;

    [Header("Health/Armor Stats")]
    public float health = 100;
    public float maxHealth = 100;
    public float armor = 10;
    public float maxArmor = 10;
    //By frame, 60 Seconds/Frame
    public float timeUntilArmorRecovery;

    [Header("Effects Stats")]
    public bool isOnFire = false;
    public float depleteHealthByFire = 1;
    public bool isPoisoned = false;
    public float depleteHealthByPoison = 1;
    public bool isInvincible = false;

    [Header("Stanima")]
    public float stanima = 100;
    public float maxStanima = 100;
    public float timeUntilStanimaRecovery;

    [Header("Currency Stats")]
    public float currency = 0;

    [Header("Teams")]
    [Tooltip("Current viable factions: BluFor, OpFor, Neutral")]
    public string faction;

    [Header("Images")]
    public Image staminaBar;
    public Image armorBar;
    public Image healthBar;
    public TextMeshProUGUI currencyText;

    public GameObject hitParticle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        charList.Add(this);
        numberOfCharacters++;

        if (faction == "BluFor")
        {
            bluForList.Add(this);
            numberOfBluFor++;
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        manageBars();

        if(!Pause.isGamePaused)
        {
            while (isOnFire)
            {
                decreaseTrueHealth(depleteHealthByFire);
            }
            while (isPoisoned)
            {
                decreaseTrueHealth(depleteHealthByPoison);
            }

            if (timeUntilArmorRecovery<=0 && health > 0)
            {
                increaseArmor(1);
            }
            else if(timeUntilArmorRecovery>-1)
            {
                timeUntilArmorRecovery -= Time.deltaTime;
            }

            if (timeUntilStanimaRecovery<=0 && stanima<maxStanima)
            {
                increaseStanima(1);
            }
            else if(timeUntilStanimaRecovery>-1)
            {
                timeUntilStanimaRecovery -= Time.deltaTime;
            }
        }

        if (attacker != null)
        {
            if (attacker.health <= 0|| attackedCooldown<=0)
            {
                ResetAttacker();
            }
            else
            {
                attackerDistance=Vector3.Distance(attacker.transform.position, transform.position);
            }
        }

        if (attackedCooldown > 0)
        {
            attackedCooldown -= Time.deltaTime;
        }
    }

    public Character attacker;
    public float attackerDistance=Mathf.Infinity;
    public float attackedCooldown;

    public void RegisterAttacker(Character attacker)
    {
        attackedCooldown = 5;
        this.attacker = attacker;
    }

    public void ResetAttacker()
    {
        attacker = null;
        attackerDistance = Mathf.Infinity;
    }

    public void increaseHealth(float health = 1)
    {
        if (this.health + health > maxHealth)
        {
            this.health = maxHealth;
        }
        else
        {
            this.health += health;
        }
    }

    public void decreaseTrueHealth(float health = 1)
    {
        timeUntilArmorRecovery = 5;
        if (!isInvincible)
        {
            if (this.health - health < 0)
            {
                this.health = 0;
            }
            else
            {
                this.health -= health;
            }
        }
    }

    public void increaseArmor(float armor = 1)
    {
        if (this.armor + armor > maxArmor)
        {
            this.armor = maxArmor;
        }
        else
        {
            this.armor += armor;
        }
    }

    public void decreaseHealthAndArmor(float armor = 1, float health = 1)
    {
        timeUntilArmorRecovery = 5;
        if (!isInvincible)
        {
            if (this.armor > 0)
            {
                this.armor -= armor;
                if (this.armor < 0)
                {
                    this.armor = 0;
                }
            }
            else
            {
                decreaseTrueHealth(health);
            }
        }
    }

    public void increaseStanima(float stanima = 1)
    {
        if (this.stanima + stanima > maxStanima)
        {
            this.stanima = maxStanima;
        }
        else
        {
            this.stanima += stanima;
        }
    }

    public void decreaseStanima(float stanima = 1)
    {
        timeUntilStanimaRecovery = 2;
        if (this.stanima - stanima <= 0)
        {
            this.stanima = 0;
        }
        else
        {
            this.stanima -= stanima;
        }
    }

    public void setMaxHealth(float maxHealth = 100)
    {
        this.maxHealth = maxHealth;
    }

    public void setMaxArmor(float maxArmor = 10)
    {
        this.maxArmor = maxArmor;
    }

    public void setMaxStanima(float maxStanima = 100)
    {
        this.maxStanima = maxStanima;
    }

    public void setCurrency(float currency)
    {
        this.currency = currency;
    }

    public void increaseCurrency(float currency = 1)
    {
        this.currency += currency;
    }

    public void pay(Character c, float currency = 1)
    {
        if (this.currency - currency >= 0)
        {
            c.currency += currency;
            this.currency -= currency;
        }
    }

    public bool transaction(ref float currency)
    {
        if (this.currency - currency < 0)
        {
            return false;
        }
        else
        {
            this.currency -= currency;
            return true;
        }
    }

    public string goodHealth;

    public void manageBars()
    {
        if (healthBar != null && armorBar != null)
        {
            ColorUtility.TryParseHtmlString(this.goodHealth, out Color goodHealth);
            ColorUtility.TryParseHtmlString("#ff4d4d", out Color badHealth);

            if (health / maxHealth <= 0.2)
            {
                healthBar.color = badHealth;
            }
            else if (health / maxHealth > 0.2)
            {
                healthBar.color = goodHealth;
            }

            healthBar.fillAmount = health / maxHealth;
            armorBar.fillAmount = armor / maxArmor;
        }

        if (staminaBar != null){
            staminaBar.fillAmount = stanima / maxStanima;
        }

        if (currencyText != null)
        {
            currencyText.text = "NY$" + currency + ".00";
        }
    }
}
