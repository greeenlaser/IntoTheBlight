using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_DamageType : MonoBehaviour
{
    [SerializeField] private string damageTypeName;
    [SerializeField] private float maxDamage;
    [Tooltip("How many times a second does this damageType deal damage?")]
    [Range(1, 10)]
    [SerializeField] private int damageTimer = 1;
    [Range(0f, 2f)]
    [SerializeField] private float innerRange;
    [Range(0.1f, 4f)]
    [SerializeField] private float middleRange;
    [Range(0.5f, 10f)]
    [SerializeField] private float outerRange;
    public DamageType damageType;
    public enum DamageType
    {
        fire,
        electricity,
        gas,
        radiation,
        psy
    }
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public string damageDealerSeverity;

    //private variables
    private bool dealDamage;
    private float timeUntilDamageDealt;
    private string theDamageType;

    private void Start()
    {
        if (damageType == DamageType.fire)
        {
            theDamageType = "fire";
        }
        else if (damageType == DamageType.electricity)
        {
            theDamageType = "electricity";
        }
        else if (damageType == DamageType.gas)
        {
            theDamageType = "gas";
        }
        else if (damageType == DamageType.radiation)
        {
            theDamageType = "radiation";
        }
        else if (damageType == DamageType.psy)
        {
            theDamageType = "psy";
        }
    }

    private void OnDrawGizmosSelected()
    {
        //red sphere for inner damage range
        Gizmos.color = new Color32(255, 0, 0, 255);
        Gizmos.DrawWireSphere(transform.position, innerRange);

        //orange sphere for middle damage range
        Gizmos.color = new Color32(255, 145, 100, 255);
        Gizmos.DrawWireSphere(transform.position, middleRange);

        //yellow sphere for outer damage range
        Gizmos.color = new Color32(255, 255, 0, 255);
        Gizmos.DrawWireSphere(transform.position, outerRange);
    }

    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, outerRange);

        if (colliders.Length > 0)
        {
            foreach (var target in colliders)
            {
                if ((target.transform.GetComponent<Player_Health>() != null
                    && target.GetComponent<Player_Health>().isPlayerAlive
                    && target.GetComponent<Player_Health>().canTakeDamage)
                    || (target.GetComponent<AI_Health>() != null
                    && target.GetComponent<AI_Health>().isAlive
                    && target.GetComponent<AI_Health>().isKillable)
                    || (target.GetComponent<Env_DestroyableCrate>() != null
                    && target.GetComponent<Env_DestroyableCrate>().crateHealth > 0))
                {
                    timeUntilDamageDealt += Time.deltaTime;
                    if (timeUntilDamageDealt > damageTimer)
                    {
                        dealDamage = true;
                        timeUntilDamageDealt = 0;
                    }

                    if (dealDamage)
                    {
                        float distance = Vector3.Distance(target.transform.position, transform.position);
                        float finalDamage = 0;

                        //damaging killable player
                        if (target.transform.GetComponent<Player_Health>() != null)
                        {
                            if (distance <= outerRange
                                && distance > middleRange)
                            {
                                //33% damage to target
                                finalDamage = Mathf.Floor(maxDamage / 3 * 10) / 10;
                                damageDealerSeverity = "minor";
                            }
                            else if (distance <= middleRange
                                     && distance > innerRange)
                            {
                                //66% damage to target
                                finalDamage = Mathf.Floor(maxDamage / 3 * 2 * 10) / 10;
                                damageDealerSeverity = "moderate";
                            }
                            else if (distance <= innerRange)
                            {
                                //100% damage to target
                                finalDamage = maxDamage;
                                damageDealerSeverity = "severe";
                            }
                            
                            target.transform.GetComponent<Player_Health>().DealDamage(damageTypeName, theDamageType, finalDamage);

                            //Debug.Log("Dealt " + damageDealt + " " + damageType + " damage to " + target.name + "!");
                        }
                        //damaging killable AI
                        else if (target.transform.GetComponent<AI_Health>() != null)
                        {
                            target.transform.GetComponent<AI_Health>().DealDamage(theDamageType, finalDamage);
                            //Debug.Log("Dealt " + damageDealt + " " + damageType + " damage to " + target.name + "!");
                        }
                        //damaging destroyable crate
                        else if (target.transform.GetComponent<Env_DestroyableCrate>() != null)
                        {
                            target.transform.GetComponent<Env_DestroyableCrate>().DealDamage(finalDamage);
                            //Debug.Log("Dealt " + damageDealt + " " + damageType + " damage to " + target.name + "!");
                        }

                        dealDamage = false;
                    }
                }
            }
        }
        else
        {
            if (timeUntilDamageDealt > 0)
            {
                timeUntilDamageDealt = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player_Health>() != null
            && !other.gameObject.GetComponent<Player_Health>().elementalDamageDealers.Contains(gameObject)
            && other.gameObject.GetComponent<Player_Health>().canTakeDamage
            && other.gameObject.GetComponent<Player_Health>().isPlayerAlive)
        {
            other.gameObject.GetComponent<Player_Health>().elementalDamageDealers.Add(gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player_Health>() != null
            && other.GetComponent<Player_Health>().elementalDamageDealers.Contains(gameObject))
        {
            other.GetComponent<Player_Health>().elementalDamageDealers.Remove(gameObject);
        }
    }
}