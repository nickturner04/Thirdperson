using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallel : Node
{
    public Parallel(string name)
    {
        this.name = name;
    }

    public override Status Process()
    {
        Status status = Status.Success;
        foreach (var child in children)
        {
            var s = child.Process();
            if (child.status != Status.Success)
            {
                status = Status.Failure;
            }
        }
        return status;

    }
}
