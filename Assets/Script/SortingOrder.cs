using UnityEngine;
using System.Collections;

public class SortingOrder : MonoBehaviour
{
    public bool update = false;
    public float yOffset = 0;
    SpriteRenderer rend;

	void Start ()
    {
        rend = GetComponent<SpriteRenderer>();
        Set();
	}
	
	void Update ()
    {
        if(update)
        {
            Set();
        }
    }

    void Set()
    {
        //rend.sortingOrder = -(int)(transform.position.y + yOffset);
        transform.position = new Vector3(transform.position.x, transform.position.y, (transform.position.y+yOffset));
    }
}
