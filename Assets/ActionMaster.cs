using UnityEngine;
using System.Collections;

public class ActionMaster : MonoBehaviour
{
	void Start ()
    {
        DoDuringStart();
        StartCoroutine(GridPosition());
	}
	
	void Update ()
    {
        DoDuringUpdate();
	}

    IEnumerator GridPosition()
    {
        while (!TileKeeper.levelSet)
        {
            yield return null;
        }

        Vector2 gridPos = TileKeeper.TranslateToPoint(transform.position);
        TileKeeper.SetAvailability(gridPos, false);
    }

    public virtual void DoDuringStart()
    {

    }

    public virtual void DoDuringUpdate()
    {

    }

    public virtual void DoOnTrigger()
    {
        Debug.Log("Response from ActionMaster");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Bullet")
        {
            Debug.Log("I'm hit by an attack!");
            //Destroy(col.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Bullet")
        {
            Debug.Log("I'm hit by a bullet!");
            //Destroy(col.gameObject);
        }
    }
}
