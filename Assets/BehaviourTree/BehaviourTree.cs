using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : Node
{
    public BehaviourTree()
    {
        name = "Tree";
    }
    public BehaviourTree(string name)
    {
        this.name = name;
    }

    public override Status Process()
    {
        return children[currentChild].Process();
    }

    struct NodeLevel
    {
        public NodeLevel(int level, Node node)
        {
            this.level = level;
            this.node = node;
        }
        public int level;
        public Node node;
    }

    public void PrintTree()
    {
        string treePrintout = "";
        Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();
        Node currentNode = this;
        nodeStack.Push(new NodeLevel(0,currentNode));

        while (nodeStack.Count > 0)
        {
            NodeLevel nextNode = nodeStack.Pop();
            treePrintout += new string('-',nextNode.level) + nextNode.node.name + "\n";
            for (int i = nextNode.node.children.Count - 1 ; i >= 0; i--)
            {
                nodeStack.Push(new NodeLevel(nextNode.level + 1, nextNode.node.children[i]));
            }
        }
        Debug.Log(treePrintout);
    }
}
