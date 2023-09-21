using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [Header("Health Basics")]
    protected float health;
    public float GetCurrentHealth { get { return health; } set { } }

    [SerializeField] protected float maxHealth;
    public float GetMaxHealth { get { return maxHealth; } set { } }

    public bool isDead = false;
    public Action OnTakeDamage;
    public Action OnDie;
    
    [Header("Regen")]
    [SerializeField] protected bool canRegen;
    [SerializeField] protected float regenAmountPerSecond;
    [SerializeField] protected float timeToStartRegen;
    private float regenTimer;

    public virtual void Start()
    {
        health = maxHealth;
    }

    public virtual void Update()
    {
        if(canRegen && health < maxHealth)
        {
            if(regenTimer <= 0f)
            {
                health += regenAmountPerSecond * Time.deltaTime;
                if (health > maxHealth) health = maxHealth;
            }
            else
            {
                regenTimer -= Time.deltaTime;
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if(health <= 0f)
        {
            OnDie?.Invoke();
            isDead = true;
            return;
        }

        regenTimer = timeToStartRegen;

        OnTakeDamage?.Invoke();
    }
}
