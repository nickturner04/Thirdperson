public class Leaf : Node
{
    //This class is a node which calls a delagate function when processed and returns it's output.
    public delegate Status Tick();
    public Tick ProcessMethod;
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
