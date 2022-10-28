//This node will execute all of it's children until one of them fails, it is functionally the same as the selector node,
//but may improve readability of trees by making success/failure, line up with what the programmer percieves to be a success.
public class Sequence : Node
{
    public Sequence(string name)
    {
        this.name = name;
    }

    public override Status Process()
    {
        
        Status childStatus = children[currentChild].Process();
        if (childStatus == Status.Running) return Status.Running;
        if (childStatus == Status.Failure)
        {
            currentChild = 0;
            return Status.Failure;
        }
        
        currentChild++;
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.Success;
        }

        return Status.Running;
    }
}
