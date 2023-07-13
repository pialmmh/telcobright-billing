using System.Collections.Generic;

public class Trie
{
    public char Path { get; }
    public string FullPath { get; }
    public Trie Parent { get; set; }
    public Dictionary<char, Trie> Children { get; set; } = new Dictionary<char, Trie>();
    public Trie(char path, Trie parent)
    {
        Path = path;
        Parent = parent;
        if (parent == null)
        {
            this.FullPath = path.ToString();
        }
        else//parent not null
        {
            parent.Children.Add(this.Path, this);
            this.FullPath = parent.FullPath + path;
        }
    }
}