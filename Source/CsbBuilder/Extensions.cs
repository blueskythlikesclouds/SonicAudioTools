using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsbBuilder
{
    public static class Extensions
    {
        public static TreeNode CreateNodesByFullPath(this TreeView treeView, string path)
        {
            string fullPath = string.Empty;
            TreeNode currentNode = null;

            foreach (string node in path.Split('/'))
            {
                fullPath += node;

                TreeNode treeNode = treeView.FindNodeByFullPath(fullPath);
                bool notFound = treeNode == null;

                if (notFound)
                {
                    treeNode = new TreeNode(node);
                    treeNode.Name = treeNode.Text;
                }

                if (currentNode != null && notFound)
                {
                    currentNode.Nodes.Add(treeNode);
                }

                else if (currentNode == null && notFound)
                {
                    treeView.Nodes.Add(treeNode);
                }

                currentNode = treeNode;
                fullPath += '/';
            }

            return currentNode;
        }


        public static TreeNode FindNodeByFullPath(this TreeNode treeNode, string path)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                if (node.FullPath == path)
                {
                    return node;
                }

                TreeNode childNode = node.FindNodeByFullPath(path);
                if (childNode != null)
                {
                    return childNode;
                }
            }

            return null;
        }

        public static TreeNode FindNodeByFullPath(this TreeView treeView, string path)
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                if (node.FullPath == path)
                {
                    return node;
                }

                TreeNode childNode = node.FindNodeByFullPath(path);
                if (childNode != null)
                {
                    return childNode;
                }
            }

            return null;
        }
    }
}
