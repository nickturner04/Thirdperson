using System.Collections.Generic;

public abstract class Node
{
    //This class contains the functions and data needed for a node on the behaviour tree but is never instantiated itself.
    public enum Status { Success,Running,Failure}
    public Status status;
    public List<Node> children = new List<Node>();
    public int currentChild = 0;
    public string name;

    public Node() { }
    public Node(string name)
    {
        this.name = name;
    }

    public virtual Status Process()
    {
        return children[currentChild].Process();
    }

    public void AddChild(Node n)
    {
        children.Add(n);
    }
}
