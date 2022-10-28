//This node will process all of it's child children until one of them succeeds, at that point it will reset and go back to the first one
//If the child returns running it will wait for it to finish before processing the next one
public class Selector : Node
{
    public Selector() { }

    public Selector(string name)
    {
        this.name = name;
    }

    public override Status Process()
    {
        
        Status childStatus = children[currentChild].Process();
        if (childStatus == Status.Running) return Status.Running;
        if (childStatus == Status.Success)
        {
            currentChild = 0;
            return Status.Success;
        }
        currentChild++;
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.Failure;
        }

        return Status.Running;
    }
}
