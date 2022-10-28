using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decorator : Node
{
    public delegate bool Filter();
    public Filter filter;

    public Decorator() { }

    public Decorator(string name, Filter filter)
    {
        this.name = name;
        this.filter = filter;
    }

    //If this node's delegate returns true, process the child node, else return failure
    public override Status Process()
    {
        if (filter())
        {
            return children[0].Process();
        }
        else
        {
            return Status.Failure;
        }
    }
}
