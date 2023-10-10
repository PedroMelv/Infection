using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviourPun
{
    [Header("Health Basics")]
    protected float health;
    public float SetCurrentHealth { get { return GetCurrentHealth; } set
        {
            CallUpdateHealth(value);
        } }
    public float GetCurrentHealth { get { return health; } set { } }

    [SerializeField] protected bool playerDriven = false;

    [SerializeField] protected float maxHealth;
    public float GetMaxHealth { get { return maxHealth; } set { } }

    public bool isDead = false;
    public Action<Vector3> OnTakeDamage;
    public Action OnDie;
    public Action OnHeal;
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
        if (!photonView.IsMine && playerDriven)
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

                SetCurrentHealth = health;
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

    protected virtual void ForceHeal(float percentage = 1f)
    {
        isDead = false;
        health += maxHealth * percentage;

        health = Mathf.Clamp(health, 0, maxHealth);

        OnHeal?.Invoke();
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

    protected void CallUpdateHealth(float to)
    {
        if (PhotonNetwork.InRoom) photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, to); else health = to;
    }

    [PunRPC]
    public void RPC_UpdateHealth(float to)
    {
        health = to;
    }
}
