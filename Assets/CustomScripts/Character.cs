using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [Header("Health/Armor Stats")]
    public float health = 100;
    public float maxHealth = 100;
    public float armor = 10;
    public float maxArmor = 10;
    //By frame, 60 Seconds/Frame
    public float timeUntilArmorRecovery = 300;
    public float timeSinceLastHit = 0;

    [Header("Effects Stats")]
    public bool isOnFire = false;
    public float depleteHealthByFire = 1;
    public bool isPoisoned = false;
    public float depleteHealthByPoison = 1;
    public bool isInvincible = false;

    [Header("Stanima")]
    public float stanima = 100;
    public float maxStanima = 100;
    public float timeUntilStanimaRecovery = 120;
    public float timeSinceLastUsedStanima = 0;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        manageBars();
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
        timeSinceLastHit = 0;
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
        timeSinceLastHit = 0;
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
        timeSinceLastUsedStanima = 0;
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

    public void manageBars()
    {
        if (healthBar != null && armorBar != null && staminaBar != null && currencyText != null)
        {
            Color goodHealth = Color.green;
            Color badHealth = Color.red;

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
            staminaBar.fillAmount = stanima / maxStanima;

            currencyText.text = "NY$" + currency + ".00";
        }
    }
}
