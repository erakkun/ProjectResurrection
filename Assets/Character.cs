using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    [HideInInspector]
    public CharacterValues stats;
    [HideInInspector]
    public AnimationSystem anim;

    void Start()
    {
        stats = GetComponent<CharacterValues>();
        anim = GetComponent<AnimationSystem>();
    }

    void Update()
    {
        
    }

    void OnGUI()
    {
        
    }
}
