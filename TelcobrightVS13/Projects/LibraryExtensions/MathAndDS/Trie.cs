using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryExtensions
{
    public class Trie
    {
        private char RootChar { get; }
        private char Path { get; }
        public string FullPath { get; }
        private Trie root { get; }
        private Dictionary<char, Trie> Children { get; } = new Dictionary<char, Trie>();

       
        public Trie findBestMatch(string query)
        {
            char[] queryAsArr= new char[] {this.RootChar};
            queryAsArr=queryAsArr.Concat(query.ToCharArray()).ToArray();
            Trie t = Trie.findBestMatch(this.root, queryAsArr);
            return t;
        }
        public Trie(IEnumerable<string> data, char rootChar)//constructor
        {
            this.root = new Trie(rootChar, null);
            this.RootChar = rootChar;
            //data = data.OrderByDescending(s => s.Length).ToList();
            foreach (string s in data)
            {
                char[] chars = (rootChar + s).ToCharArray();
                char[] fullPath = new char[chars.Length];

                for (var index = 0; index < chars.Length; index++)
                {
                    char path = chars[index];
                    fullPath[index]=path;
                    Trie bestMatch = findBestMatch(root, fullPath);
                    string fullQueryForDebug = new string(fullPath.TakeWhile(c => c > 0).ToArray()); //for debug only, remove later
                    string fullBestMatchForDebug = bestMatch.FullPath;//for debug only, remove later
                    if (bestMatch.FullPath == new string(fullPath.TakeWhile(c => c > 0).ToArray())) continue;
                    Trie child = new Trie(path, bestMatch);
                }

                /*foreach (char path in chars)
                {
                    //fullPath .Add(path);
                    string fullNameForDebug = new string(fullPath) ;//for debug only, remove later
                    Trie bestMatch = findBestMatch(root, fullPath);
                    if (bestMatch.FullPath == new string(fullPath)) continue;
                    Trie child = new Trie(path, bestMatch);
                }*/
            }
        }

        void printRecursive(Trie trie)
        {
            Console.WriteLine(trie.FullPath);
            if (trie.Children.Count == 0) return;

            foreach (var child in trie.Children.Values)
            {
                printRecursive(child);
            }
        }




        public void print()
        {
            Trie trie = this.root;
            if (trie == null) return;
            printRecursive(trie);
        }



        private Trie(char path, Trie parent)
        {
            Path = path;
            if (parent == null)
            {
                this.FullPath = path.ToString();
            }
            else //parent not null
            {
                parent.Children.Add(this.Path, this);
                this.FullPath = parent.FullPath + path;
            }
        }

        static Trie findBestMatch(Trie trie, char[] query)
        {
            char head = query[0];
            char[] tail = query.Skip(1).ToArray();
            //013
            //0->0
            //1->01
            //3 -> null

            Trie bestMatch = null;
            if (head == trie.Path)
            {
                bestMatch = trie;
                var children = bestMatch.Children;
                if (children == null || tail.Length == 0)
                {
                    return bestMatch;
                }
                Trie nextChild = null;
                children.TryGetValue(tail[0], out nextChild); //tail[0]=1

                if (nextChild == null)
                {
                    return bestMatch;
                }
                bestMatch = findBestMatch(nextChild, tail);
                return bestMatch;
            }
            return null;
        }

       

    }
}