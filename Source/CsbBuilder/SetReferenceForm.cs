using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CsbBuilder.BuilderNodes;

namespace CsbBuilder
{
    public partial class SetReferenceForm : Form
    {
        public TreeNode SelectedNode
        {
            get
            {
                return nodeTree.SelectedNode;
            }
        }

        public SetReferenceForm(TreeView treeView)
        {
            InitializeComponent();

            nodeTree.ImageList = treeView.ImageList;

            foreach (TreeNode treeNode in treeView.Nodes)
            {
                AddNode(treeNode, null);
            }

            nodeTree.ExpandAll();
        }

        private void AddNode(TreeNode treeNode, TreeNode parentNode)
        {
            bool cancel = false;

            if (treeNode.Tag is BuilderSynthNode)
            {
                BuilderSynthNode synthNode = (BuilderSynthNode)treeNode.Tag;
                if (treeNode.Parent != null && treeNode.Parent.Tag != null && synthNode.Type == 0)
                {
                    cancel = true;
                }
            }

            TreeNode newNode = null;
            if (!cancel)
            {
                newNode = new TreeNode();
                newNode.Name = treeNode.Name;
                newNode.Text = treeNode.Text;
                newNode.ImageIndex = treeNode.ImageIndex;
                newNode.SelectedImageIndex = treeNode.SelectedImageIndex;

                if (parentNode != null)
                {
                    parentNode.Nodes.Add(newNode);
                }

                else
                {
                    nodeTree.Nodes.Add(newNode);
                }
            }

            foreach (TreeNode childNode in treeNode.Nodes)
            {
                AddNode(childNode, newNode);
            }
        }

        private void SetReferenceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (SelectedNode != null && SelectedNode.ImageIndex == 3)
                {
                    MessageBox.Show("You can't set a folder as a reference!", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
        }

        private void nodeTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Close();
            DialogResult = DialogResult.OK;
        }
    }
}
