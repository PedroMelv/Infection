using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviourPun
{
    [Header("Health Basics")]
    protected float health;
    public float GetCurrentHealth { get { return health; } set { } }

    [SerializeField] protected float maxHealth;
    public float GetMaxHealth { get { return maxHealth; } set { } }

    public bool isDead = false;
    public Action<Vector3> OnTakeDamage;
    public Action OnDie;
    public Action OnFullHealth;
    
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
        if (!photonView.IsMine)
            return;
        if (canRegen && health < maxHealth && !isDead)
        {
            if(regenTimer <= 0f)
            {
                health += regenAmountPerSecond * Time.deltaTime;
                if (health > maxHealth)
                {
                    health = maxHealth;
                    OnFullHealth?.Invoke();
                }
            }
            else
            {
                regenTimer -= Time.deltaTime;
            }
        }
    }

    protected virtual void TakeDamage(float damage, Vector3 damageCameFrom)
    {
        health -= damage;

        health = Mathf.Clamp(health, 0, maxHealth);

        if(health <= 0f)
        {
            OnDie?.Invoke();
            isDead = true;
            return;
        }

        regenTimer = timeToStartRegen;

        OnTakeDamage?.Invoke(damageCameFrom);
    }

    [PunRPC]
    protected void RPC_TakeDamage(float damage, Vector3 damageCameFrom)
    {
        TakeDamage(damage, damageCameFrom);
    }

    public void CallTakeDamage(float damage, Vector3 damageCameFrom)
    {
        if (PhotonNetwork.InRoom) this.photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage, damageCameFrom); else TakeDamage(damage, damageCameFrom);
    }

    protected void ForceHeal(float percentage = 1f)
    {
        isDead = false;
        health += maxHealth * percentage;

        health = Mathf.Clamp(health, 0, maxHealth);
    }

    [PunRPC]
    protected void RPC_ForceHeal(float percentage = 1f)
    {
        ForceHeal(percentage);
    }

    public void CallForceHeal(float percentage = 1f)
    {
        if (PhotonNetwork.InRoom) this.photonView.RPC(nameof(RPC_ForceHeal), RpcTarget.All, percentage); else ForceHeal(percentage);
    }
}
