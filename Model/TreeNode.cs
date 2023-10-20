public class TreeNode<T>
{
    // Property to hold the value of the node
    public T Value { get; set; }

    // List to hold the child nodes
    public List<TreeNode<T>> Children { get; private set; }

    // Constructor initializes the value and creates a new list for children
    public TreeNode(T value)
    {
        Value = value;
        Children = new List<TreeNode<T>>();
    }

    // Method to add a child node
    public void AddChild(TreeNode<T> child)
    {
        Children.Add(child);
    }

    // Method to remove a child node
    public void RemoveChild(TreeNode<T> child)
    {
        Children.Remove(child);
    }

    public bool HasChild()
    {
        return Children.Count > 0;
    }
}