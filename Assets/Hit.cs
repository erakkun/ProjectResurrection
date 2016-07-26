using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hit : MonoBehaviour
{
    [Header("Information")]
    public GameObject sender;
    public float power;

    [Header("Behaviour")]
    public float destroyIn = 0.5f;
    public Vector2 movement = Vector2.zero;
    public bool destroyOnContact = false;
    public bool useLifeTime = true;
    public bool damaging = true;

    [System.Serializable]
    public class HitFlare
    {
        public GameObject onDestroy;
        public Vector2 point;
        public float destroyIn;
    }

    public List<HitFlare> flares = new List<HitFlare>();

    void Start()
    {
        if(destroyOnContact)
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if(!col)
            {
                Debug.Log("There was no collider attached.");
                return;
            }
            col.isTrigger = false;
        }
        if(useLifeTime)
        {
            StartCoroutine(WaitForDestroy());
        }
    }

    void Update()
    {
        transform.Translate(movement * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(destroyOnContact)
        {
            CheckEm(col.gameObject);
            Destroy(gameObject);
        }
    }

    void CheckEm(GameObject obj)
    {
        CharacterNPC npc = obj.GetComponent<CharacterNPC>();
        if(npc)
        {
            npc.characterStats.health.currentValue -= power;
        }
    }

    IEnumerator WaitForDestroy()
    {
        yield return new WaitForSeconds(destroyIn);

        foreach(HitFlare hit in flares)
        {
            GameObject obj = null;
            if (hit.onDestroy)
            {
                obj = Instantiate(hit.onDestroy, transform.position + (Vector3)hit.point, transform.rotation) as GameObject;

                Hit well = obj.GetComponent<Hit>();
                if(well)
                {
                    well.destroyIn = hit.destroyIn;
                    well.useLifeTime = true;
                    well.destroyOnContact = false;
                    well.power = power;
                }
            }
        }

        Destroy(gameObject);
    }
}
