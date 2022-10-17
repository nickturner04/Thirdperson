using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : Node
{
    public delegate Status Tick();
    public Tick ProcessMethod;
    public bool executing = false;

    public Leaf() { }
    public Leaf(string name, Tick pm) 
    {
        this.name = name;
        ProcessMethod = pm;
    }

    public override Status Process()
    {
        //Debug.Log(name);
        if (ProcessMethod != null)
        {
            return ProcessMethod();
        }
        return Status.Failure;
    }
}
