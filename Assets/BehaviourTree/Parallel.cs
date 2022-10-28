//This node is able to process multiple children similarly to sequence and selector, however it will execute all of them regardless of their status,
//This can be used to make an AI agent perform multiple tasks at the same time without waiting for one to finish running
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
