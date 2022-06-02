using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : BaseUnit
{
    public ClassType classType = ClassType.Rogue;
    //[SerializeField] public TriggerRogueAnimations AnimatorPrefab;
    //private TriggerRogueAnimations animator;
    //private Animator animator;
    //[SerializeField] private AnimationClip attackAnimation;

    public override void Init()
    {
        base.Init();
        //animator = GetComponent<Animator>();
        //animator = Instantiate(AnimatorPrefab);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
