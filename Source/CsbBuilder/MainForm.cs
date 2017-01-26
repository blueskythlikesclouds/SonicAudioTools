﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using CsbBuilder.Builder;
using CsbBuilder.BuilderNode;
using CsbBuilder.Audio;
using CsbBuilder.Importer;
using CsbBuilder.Project;
using CsbBuilder.Serialization;
using CsbBuilder.Properties;

using SonicAudioLib.IO;

using NAudio.Wave;

namespace CsbBuilder
{
    public partial class MainForm : Form
    {
        private bool enabled = false;
        private bool saved = true;
        private CsbProject project = null;

        private TreeNode copiedNode = null;
        private TreeView treeViewOfCopiedNode = null;

        private List<IWavePlayer> sounds = new List<IWavePlayer>();

        public MainForm()
        {
            InitializeComponent();

            imageList.Images.Add("Node", Resources.Node);
            imageList.Images.Add("Folder", Resources.Folder);
            imageList.Images.Add("FolderOpen", Resources.FolderOpen);
            imageList.Images.Add("Sound", Resources.Sound);
        }

        public void ClearTreeViews()
        {
            cueTree.Nodes.Clear();
            synthTree.Nodes.Clear();
            soundElementTree.Nodes.Clear();
            aisacTree.Nodes.Clear();
            voiceLimitGroupTree.Nodes.Clear();
        }

        public void ExpandAllTreeViews()
        {
            cueTree.ExpandAll();
            if (cueTree.Nodes.Count > 0)
            {
                cueTree.Nodes[0].EnsureVisible();
            }

            synthTree.ExpandAll();
            if (synthTree.Nodes.Count > 0)
            {
                synthTree.Nodes[0].EnsureVisible();
            }

            soundElementTree.ExpandAll();
            if (soundElementTree.Nodes.Count > 0)
            {
                soundElementTree.Nodes[0].EnsureVisible();
            }

            aisacTree.ExpandAll();
            if (aisacTree.Nodes.Count > 0)
            {
                aisacTree.Nodes[0].EnsureVisible();
            }

            voiceLimitGroupTree.ExpandAll();
            if (voiceLimitGroupTree.Nodes.Count > 0)
            {
                voiceLimitGroupTree.Nodes[0].EnsureVisible();
            }
        }

        public void ClearProject()
        {
            project = null;
            ClearTreeViews();
            Text = "CSB Builder";
            saved = true;
        }

        public void DisplayOnTreeView()
        {
            foreach (BuilderCueNode cueNode in project.CueNodes)
            {
                TreeNode treeNode = new TreeNode();
                treeNode.ContextMenuStrip = cueReferenceMenu;
                treeNode.Name = cueNode.Name;
                treeNode.Text = treeNode.Name;
                treeNode.Tag = cueNode;
                cueTree.Nodes.Add(treeNode);
            }

            foreach (BuilderSynthNode synthNode in project.SynthNodes)
            {
                TreeNode treeNode = synthTree.CreateNodesByFullPath(synthNode.Name);
                treeNode.ContextMenuStrip = trackMenu;
                treeNode.Tag = synthNode;

                if (synthNode.Type == BuilderSynthType.Single)
                {
                    treeNode.ContextMenuStrip = trackItemMenu;
                    treeNode.ImageIndex = 3;
                    treeNode.SelectedImageIndex = 3;
                }

                while ((treeNode = treeNode.Parent) != null)
                {
                    if (treeNode.Tag == null)
                    {
                        treeNode.ContextMenuStrip = synthFolderMenu;
                    }
                }
            }

            foreach (BuilderSoundElementNode soundElementNode in project.SoundElementNodes)
            {
                TreeNode treeNode = soundElementTree.CreateNodesByFullPath(soundElementNode.Name);
                treeNode.ContextMenuStrip = soundElementMenu;
                treeNode.Tag = soundElementNode;
                treeNode.ImageIndex = 3;
                treeNode.SelectedImageIndex = 3;

                while ((treeNode = treeNode.Parent) != null)
                {
                    if (treeNode.Tag == null)
                    {
                        treeNode.ContextMenuStrip = folderMenu;
                    }
                }
            }

            foreach (BuilderAisacNode aisacNode in project.AisacNodes)
            {
                TreeNode treeNode = aisacTree.CreateNodesByFullPath(aisacNode.Name);
                treeNode.ContextMenuStrip = aisacNodeMenu;
                treeNode.Tag = aisacNode;

                while ((treeNode = treeNode.Parent) != null)
                {
                    if (treeNode.Tag == null)
                    {
                        treeNode.ContextMenuStrip = aisacFolderMenu;
                    }
                }
            }

            foreach (BuilderVoiceLimitGroupNode voiceLimitGroupNode in project.VoiceLimitGroupNodes)
            {
                TreeNode treeNode = new TreeNode();
                treeNode.ContextMenuStrip = nodeMenu;
                treeNode.Name = voiceLimitGroupNode.Name;
                treeNode.Text = treeNode.Name;
                treeNode.Tag = voiceLimitGroupNode;
                voiceLimitGroupTree.Nodes.Add(treeNode);
            }
        }

        public void SetProject(CsbProject project)
        {
            StopSound(null, null);

            if (!enabled)
            {
                splitContainer1.Enabled = true;
                splitContainer2.Enabled = true;
                propertyGrid.Enabled = true;
                saveProjectAsToolStripMenuItem.Enabled = true;
                saveProjectToolStripMenuItem.Enabled = true;
                buildCurrentProjectAsToolStripMenuItem.Enabled = true;
                buildCurrentProjectToolStripMenuItem.Enabled = true;
                mergeProjectToolStripMenuItem.Enabled = true;
                ımportAndMergeToolStripMenuItem.Enabled = true;
                enabled = true;
            }

            ClearProject();
            this.project = project;
            project.Create();

            DisplayOnTreeView();
            UpdateAllNodes();
            ExpandAllTreeViews();

            Text += $" - {project.Name}";
        }

        private void CreateNewProject(object sender, EventArgs e)
        {
            if (AskForSave() == DialogResult.Cancel)
            {
                return;
            }

            using (CreateNewProjectForm newProject = new CreateNewProjectForm())
            {
                if (newProject.ShowDialog(this) == DialogResult.OK)
                {
                    SetProject(newProject.Project);
                    project.Save();
                }
            }
        }

        private void ImportCsbFile(object sender, EventArgs e)
        {
            if (AskForSave() == DialogResult.Cancel)
            {
                return;
            }

            using (OpenFileDialog openCsbFile = new OpenFileDialog
            {
                Title = "Import CSB File",
                DefaultExt = "csb",
                Filter = "CSB Files|*.csb",
            })
            {
                if (openCsbFile.ShowDialog() == DialogResult.OK)
                {
                    using (CreateNewProjectForm createProject = new CreateNewProjectForm(openCsbFile.FileName))
                    {
                        if (createProject.ShowDialog(this) == DialogResult.OK)
                        {
                            createProject.Project.Create();
                            CsbImporter.Import(openCsbFile.FileName, createProject.Project);
                            SetProject(createProject.Project);
                            project.Save();
                        }
                    }
                }
            }
        }

        private void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            e.Node.TreeView.SelectedNode = e.Node;
        }

        private TreeNode CreateCueNode(TreeNodeCollection collection, TreeNode parent, int nodeIndex = -1)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"Cue_{index}"))
            {
                index++;
            }

            TreeNode treeNode = nodeIndex == -1 ? collection.Add($"Cue_{index}") : collection.Insert(nodeIndex, $"Cue_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = cueReferenceMenu;
            BuilderCueNode cueNode = new BuilderCueNode();
            cueNode.Name = treeNode.FullPath;

            if (project.CueNodes.Count > 0)
            {
                cueNode.Identifier = project.CueNodes.Max(cue => cue.Identifier) + 1;
            }

            treeNode.Tag = cueNode;

            project.CueNodes.Add(cueNode);

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private void RemoveNode(BuilderBaseNode node)
        {
            if (node is BuilderCueNode)
            {
                project.CueNodes.Remove((BuilderCueNode)node);
            }

            else if (node is BuilderSynthNode)
            {
                BuilderSynthNode synthNode = (BuilderSynthNode)node;
                project.SynthNodes.Remove(synthNode);

                project.CueNodes.Where(cue => cue.SynthReference == synthNode.Name).ToList().ForEach(cue => cue.SynthReference = string.Empty);
                project.SynthNodes.Where(synth => synth.Children.Contains(synthNode.Name)).ToList().ForEach(synth => synth.Children.Remove(synthNode.Name));
            }

            else if (node is BuilderSoundElementNode)
            {
                BuilderSoundElementNode soundElementNode = (BuilderSoundElementNode)node;
                project.SoundElementNodes.Remove(soundElementNode);

                project.SynthNodes.Where(soundElement => soundElement.SoundElementReference == soundElementNode.Name).ToList().ForEach(synth => synth.SoundElementReference = string.Empty);
            }

            else if (node is BuilderAisacNode)
            {
                BuilderAisacNode aisacNode = (BuilderAisacNode)node;
                project.AisacNodes.Remove(aisacNode);

                project.SynthNodes.Where(synth => synth.AisacReference == aisacNode.Name).ToList().ForEach(synth => synth.AisacReference = string.Empty);
            }

            else if (node is BuilderVoiceLimitGroupNode)
            {
                BuilderVoiceLimitGroupNode voiceLimitGroupNode = (BuilderVoiceLimitGroupNode)node;
                project.VoiceLimitGroupNodes.Remove(voiceLimitGroupNode);

                project.SynthNodes.Where(synth => synth.VoiceLimitGroupReference == voiceLimitGroupNode.Name).ToList().ForEach(voiceLimitGroup => voiceLimitGroup.VoiceLimitGroupReference = string.Empty);
            }
        }

        private TreeNode CreateSynthNode(TreeNodeCollection collection, TreeNode parent, int nodeIndex = -1)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"Synth_{index}"))
            {
                index++;
            }

            TreeNode treeNode = nodeIndex == -1 ? collection.Add($"Synth_{index}") : collection.Insert(nodeIndex, $"Synth_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = trackMenu;
            BuilderSynthNode synthNode = new BuilderSynthNode();
            synthNode.Type = BuilderSynthType.WithChildren;
            synthNode.Name = treeNode.FullPath;
            treeNode.Tag = synthNode;
            project.SynthNodes.Add(synthNode);

            if (parent != null && parent.Tag is BuilderSynthNode)
            {
                BuilderSynthNode parentSynthNode = (BuilderSynthNode)parent.Tag;

                if (parentSynthNode.Type == BuilderSynthType.WithChildren)
                {
                    parentSynthNode.Children.Add(synthNode.Name);
                }
            }

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private TreeNode CreateSoundNode(TreeNodeCollection collection, TreeNode parent, int nodeIndex = -1)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"Sound_{index}"))
            {
                index++;
            }

            TreeNode treeNode = nodeIndex == -1 ? collection.Add($"Sound_{index}") : collection.Insert(nodeIndex, $"Sound_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = trackItemMenu;
            treeNode.ImageIndex = 3;
            treeNode.SelectedImageIndex = 3;
            BuilderSynthNode synthNode = new BuilderSynthNode();
            synthNode.Name = treeNode.FullPath;
            treeNode.Tag = synthNode;
            project.SynthNodes.Add(synthNode);

            if (parent != null && parent.Tag is BuilderSynthNode)
            {
                BuilderSynthNode parentSynthNode = (BuilderSynthNode)parent.Tag;

                if (parentSynthNode.Type == BuilderSynthType.WithChildren)
                {
                    parentSynthNode.Children.Add(synthNode.Name);
                }
            }

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private TreeNode CreateSoundElementNode(TreeNodeCollection collection, TreeNode parent, int nodeIndex = -1)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"SoundElement_{index}"))
            {
                index++;
            }

            TreeNode treeNode = nodeIndex == -1 ? collection.Add($"SoundElement_{index}") : collection.Insert(nodeIndex, $"SoundElement_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = soundElementMenu;
            BuilderSoundElementNode soundElementNode = new BuilderSoundElementNode();
            soundElementNode.Name = treeNode.FullPath;
            treeNode.Tag = soundElementNode;
            treeNode.ImageIndex = 3;
            treeNode.SelectedImageIndex = 3;
            project.SoundElementNodes.Add(soundElementNode);

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private TreeNode CreateAisacNode(TreeNodeCollection collection, TreeNode parent, int nodeIndex = -1, BuilderAisacNode aisacNodeToImport = null)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"AISAC_{index}"))
            {
                index++;
            }

            TreeNode treeNode = nodeIndex == -1 ? collection.Add($"AISAC_{index}") : collection.Insert(nodeIndex, $"AISAC_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = aisacNodeMenu;
            BuilderAisacNode aisacNode = aisacNodeToImport != null ? aisacNodeToImport : new BuilderAisacNode();
            aisacNode.Name = treeNode.FullPath;
            treeNode.Tag = aisacNode;
            project.AisacNodes.Add(aisacNode);

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private TreeNode CreateVoiceLimitGroupNode(TreeNodeCollection collection, TreeNode parent, int nodeIndex = -1)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"VoiceLimitGroup_{index}"))
            {
                index++;
            }

            TreeNode treeNode = nodeIndex == -1 ? collection.Add($"VoiceLimitGroup_{index}") : collection.Insert(nodeIndex, $"VoiceLimitGroup_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = nodeMenu;
            BuilderVoiceLimitGroupNode voiceLimitGroup = new BuilderVoiceLimitGroupNode();
            voiceLimitGroup.Name = treeNode.FullPath;
            treeNode.Tag = voiceLimitGroup;
            project.VoiceLimitGroupNodes.Add(voiceLimitGroup);

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private TreeNode CreateFolder(TreeNodeCollection collection, TreeNode parent)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"Folder_{index}"))
            {
                index++;
            }

            TreeNode treeNode = collection.Add($"Folder_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = folderMenu;
            treeNode.ImageIndex = 1;
            treeNode.SelectedImageIndex = 2;

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private TreeNode CreateSynthFolder(TreeNodeCollection collection)
        {
            saved = false;

            int index = 0;

            while (collection.ContainsKey($"Folder_{index}"))
            {
                index++;
            }

            TreeNode treeNode = collection.Add($"Folder_{index}");
            treeNode.Name = treeNode.Text;
            treeNode.ContextMenuStrip = synthFolderMenu;
            treeNode.ImageIndex = 1;
            treeNode.SelectedImageIndex = 2;

            treeNode.TreeView.SelectedNode = treeNode;
            treeNode.EnsureVisible();
            return treeNode;
        }

        private void CreateChildNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == cueTree)
                {
                    CreateCueNode(selectedTree.Nodes, null);
                }

                else if (selectedTree == synthTree)
                {
                    if (selectedNode.Tag is BuilderSynthNode && ((BuilderSynthNode)selectedNode.Tag).Type == BuilderSynthType.WithChildren)
                    {
                        CreateSoundNode(selectedNode.Nodes, selectedNode);
                    }

                    else
                    {
                        CreateSynthNode(selectedNode.Nodes, selectedNode);
                    }
                }

                else if (selectedTree == soundElementTree)
                {
                    CreateSoundElementNode(selectedNode.Nodes, selectedNode);
                }

                else if (selectedTree == aisacTree)
                {
                    CreateAisacNode(selectedNode.Nodes, selectedNode);
                }

                else if (selectedTree == voiceLimitGroupTree)
                {
                    CreateVoiceLimitGroupNode(selectedTree.Nodes, null);
                }
            }
        }

        private void CreateNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;

                if (selectedTree == cueTree)
                {
                    CreateCueNode(selectedTree.Nodes, null);
                }

                else if (selectedTree == synthTree)
                {
                    CreateSynthNode(selectedTree.Nodes, null);
                }

                else if (selectedTree == soundElementTree)
                {
                    CreateSoundElementNode(selectedTree.Nodes, null);
                }

                else if (selectedTree == aisacTree)
                {
                    CreateAisacNode(selectedTree.Nodes, null);
                }

                else if (selectedTree == voiceLimitGroupTree)
                {
                    CreateVoiceLimitGroupNode(selectedTree.Nodes, null);
                }
            }
        }

        private void CreateFolder(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;

                if (selectedTree == synthTree)
                {
                    CreateSynthFolder(selectedTree.Nodes);
                }

                else
                {
                    CreateFolder(selectedTree.Nodes, null);
                }
            }
        }

        private void RemoveNodes(TreeNodeCollection collection)
        {
            foreach (TreeNode node in collection)
            {
                if (node.Tag is BuilderBaseNode)
                {
                    RemoveNode((BuilderBaseNode)node.Tag);
                }

                RemoveNodes(node.Nodes);
            }
        }

        private void RemoveNode(TreeNode treeNode)
        {
            saved = false;

            if (treeNode.Tag is BuilderBaseNode)
            {
                RemoveNode((BuilderBaseNode)treeNode.Tag);
            }

            RemoveNodes(treeNode.Nodes);
            treeNode.Remove();
        }

        private void RemoveNode(object sender, EventArgs e)
        {
            saved = false;

            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                RemoveNode(selectedNode);
            }
        }

        private void BuildProject(object sender, EventArgs e)
        {
            project.BinaryDirectory.Create();
            project.Order(cueTree, synthTree, soundElementTree, aisacTree, voiceLimitGroupTree);
            Builder.CsbBuilder.Build(project, Path.Combine(project.BinaryDirectory.FullName, project.Name + ".csb"));

            Process.Start(project.BinaryDirectory.FullName);
        }

        private void CopyFullPath(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                Clipboard.SetText(selectedNode.FullPath);
            }
        }

        private void CreateChildFolder(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == synthTree)
                {
                    CreateSynthFolder(selectedNode.Nodes);
                }

                else
                {
                    CreateFolder(selectedNode.Nodes, selectedNode);
                }
            }
        }

        private void RenameNode(object sender, TreeNodeMouseClickEventArgs e)
        {
            e.Node.BeginEdit();
        }

        private void UpdateNodeNoRename(TreeNode treeNode)
        {
            if (treeNode.Tag is BuilderBaseNode)
            {
                BuilderBaseNode baseNode = (BuilderBaseNode)treeNode.Tag;
                baseNode.Name = treeNode.FullPath;
            }

            foreach (TreeNode childNode in treeNode.Nodes)
            {
                UpdateNodeNoRename(childNode);
            }
        }

        private void UpdateNodes(TreeNodeCollection collection)
        {
            foreach (TreeNode treeNode in collection)
            {
                if (treeNode.Tag is BuilderBaseNode)
                {
                    BuilderBaseNode baseNode = (BuilderBaseNode)treeNode.Tag;

                    string previousName = baseNode.Name;
                    baseNode.Name = treeNode.FullPath;

                    // Do it only if it's different, don't waste time
                    if (previousName != baseNode.Name)
                    {
                        string fullPath = baseNode.Name;
                        if (baseNode is BuilderSynthNode)
                        {
                            project.CueNodes.Where(cue => cue.SynthReference == previousName).ToList().ForEach(cue => cue.SynthReference = fullPath);
                            project.SynthNodes.Where(synth => synth.Children.Contains(previousName)).ToList().ForEach(synth => synth.Children[synth.Children.IndexOf(previousName)] = fullPath);
                        }

                        else if (baseNode is BuilderSoundElementNode)
                        {
                            project.SynthNodes.Where(synth => synth.SoundElementReference == previousName).ToList().ForEach(synth => synth.SoundElementReference = fullPath);
                        }

                        else if (baseNode is BuilderAisacNode)
                        {
                            project.SynthNodes.Where(synth => synth.AisacReference == previousName).ToList().ForEach(synth => synth.AisacReference = fullPath);
                        }

                        else if (baseNode is BuilderVoiceLimitGroupNode)
                        {
                            project.SynthNodes.Where(synth => synth.VoiceLimitGroupReference == previousName).ToList().ForEach(synth => synth.VoiceLimitGroupReference = fullPath);
                        }
                    }
                }

                else
                {
                    treeNode.ImageIndex = 1;
                    treeNode.SelectedImageIndex = 2;
                }

                UpdateNodes(treeNode.Nodes);
            }
        }

        private void UpdateAllNodes()
        {
            UpdateNodes(cueTree.Nodes);
            UpdateNodes(synthTree.Nodes);
            UpdateNodes(soundElementTree.Nodes);
            UpdateNodes(aisacTree.Nodes);
            UpdateNodes(voiceLimitGroupTree.Nodes);
        }

        private void OnRenameEnd(object sender, NodeLabelEditEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Label))
            {
                e.CancelEdit = true;
                return;
            }

            saved = false;
            string previousName = e.Node.FullPath;

            e.Node.Name = e.Label;
            e.Node.Text = e.Label;

            string fullPath = e.Node.FullPath;

            UpdateAllNodes();

            if (e.Node.Tag is BuilderSynthNode)
            {
                project.CueNodes.Where(cue => cue.SynthReference == previousName).ToList().ForEach(cue => cue.SynthReference = fullPath);
                project.SynthNodes.Where(synth => synth.Children.Contains(previousName)).ToList().ForEach(synth => synth.Children[synth.Children.IndexOf(previousName)] = fullPath);
            }

            else if (e.Node.Tag is BuilderSoundElementNode)
            {
                project.SynthNodes.Where(synth => synth.SoundElementReference == previousName).ToList().ForEach(synth => synth.SoundElementReference = fullPath);
            }

            else if (e.Node.Tag is BuilderAisacNode)
            {
                project.SynthNodes.Where(synth => synth.AisacReference == previousName).ToList().ForEach(synth => synth.AisacReference = fullPath);
            }

            else if (e.Node.Tag is BuilderVoiceLimitGroupNode)
            {
                project.SynthNodes.Where(synth => synth.VoiceLimitGroupReference == previousName).ToList().ForEach(synth => synth.VoiceLimitGroupReference = fullPath);
            }

            propertyGrid.Refresh();
        }

        private void OnPropertyChange(object s, PropertyValueChangedEventArgs e)
        {
            saved = false;
            propertyGrid.Refresh();
        }

        private DialogResult AskForSave()
        {
            DialogResult dialogResult = DialogResult.No;

            if (!saved)
            {
                dialogResult = MessageBox.Show("Do you want to save your changes?", "CSB Builder", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    SaveProject(null, null);
                }
            }

            return dialogResult;
        }

        private void LoadProject(object sender, EventArgs e)
        {
            if (AskForSave() == DialogResult.Cancel)
            {
                return;
            }

            using (OpenFileDialog openProject = new OpenFileDialog
            {
                Title = "Open CSB Project File",
                Filter = "CSB Project files|*.csbproject",
                DefaultExt = "csbproject",
            })
            {
                if (openProject.ShowDialog() == DialogResult.OK)
                {
                    CsbProject csbProject = CsbProject.Load(openProject.FileName);

                    if (!csbProject.AudioDirectory.Exists)
                    {
                        MessageBox.Show("Audio directory does not seem to be present for this project. All the audio references will be removed.");
                        csbProject.SoundElementNodes.ForEach(soundElementNode => soundElementNode.Intro = string.Empty);
                        csbProject.SoundElementNodes.ForEach(soundElementNode => soundElementNode.Loop = string.Empty);
                    }

                    SetProject(csbProject);
                }
            }
        }

        private void SaveProject(object sender, EventArgs e)
        {
            project.Order(cueTree, synthTree, soundElementTree, aisacTree, voiceLimitGroupTree);
            project.Save();
            saved = true;
        }

        private void SaveProjectAs(object sender, EventArgs e)
        {
            using (SaveFileDialog saveProject = new SaveFileDialog
            {
                Title = "Save CSB Project File As",
                Filter = "CSB Project files|*.csbproject",
                DefaultExt = "csbproject",
            })
            {
                if (saveProject.ShowDialog() == DialogResult.OK)
                {
                    project.Order(cueTree, synthTree, soundElementTree, aisacTree, voiceLimitGroupTree);
                    project.SaveAs(saveProject.FileName);
                    saved = true;

                    // Reload the project
                    SetProject(project);
                }
            }
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            if (AskForSave() == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            Close();
        }

        private void BuildProjectAs(object sender, EventArgs e)
        {
            using (SaveFileDialog buildCsb = new SaveFileDialog()
            {
                Title = "Build Current Project As",
                DefaultExt = "csb",
                Filter = "CSB Files|*.csb",
                FileName = project.Name,
            })
            {
                if (buildCsb.ShowDialog() == DialogResult.OK)
                {
                    project.Order(cueTree, synthTree, soundElementTree, aisacTree, voiceLimitGroupTree);
                    Builder.CsbBuilder.Build(project, buildCsb.FileName);
                }
            }
        }

        private void AfterNodeSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid.SelectedObject = e.Node.Tag;
        }

        private void MergeProject(object sender, EventArgs e)
        {
            using (OpenFileDialog openProject = new OpenFileDialog
            {
                Title = "Merge Project With",
                Filter = "CSB Project files|*.csbproject",
                DefaultExt = "csbproject",
            })
            {
                if (openProject.ShowDialog() == DialogResult.OK)
                {
                    CsbProject csbProject = CsbProject.Load(openProject.FileName);

                    if (!csbProject.AudioDirectory.Exists)
                    {
                        MessageBox.Show("Audio directory does not seem to be present for this project. All the audio references will be removed.");
                        csbProject.SoundElementNodes.ForEach(soundElementNode => soundElementNode.Intro = string.Empty);
                        csbProject.SoundElementNodes.ForEach(soundElementNode => soundElementNode.Loop = string.Empty);
                    }

                    else
                    {
                        // Copy all to this project's audio folder
                        foreach (FileInfo audioFile in csbProject.AudioDirectory.EnumerateFiles())
                        {
                            audioFile.CopyTo(Path.Combine(project.AudioDirectory.FullName, audioFile.Name), true);
                        }
                    }

                    project.CueNodes.AddRange(csbProject.CueNodes);
                    project.SynthNodes.AddRange(csbProject.SynthNodes);
                    project.SoundElementNodes.AddRange(csbProject.SoundElementNodes);
                    project.AisacNodes.AddRange(csbProject.AisacNodes);
                    project.VoiceLimitGroupNodes.AddRange(csbProject.VoiceLimitGroupNodes);

                    // Reload
                    SetProject(project);
                }
            }
        }

        private void ImportAndMergeProject(object sender, EventArgs e)
        {
            using (OpenFileDialog openCsbFile = new OpenFileDialog
            {
                Title = "Import And Merge CSB File With",
                DefaultExt = "csb",
                Filter = "CSB Files|*.csb",
            })
            {
                if (openCsbFile.ShowDialog() == DialogResult.OK)
                {
                    CsbProject csbProject = new CsbProject();
                    csbProject.Directory = project.Directory;
                    CsbImporter.Import(openCsbFile.FileName, csbProject);

                    project.CueNodes.AddRange(csbProject.CueNodes);
                    project.SynthNodes.AddRange(csbProject.SynthNodes);
                    project.SoundElementNodes.AddRange(csbProject.SoundElementNodes);
                    project.AisacNodes.AddRange(csbProject.AisacNodes);
                    project.VoiceLimitGroupNodes.AddRange(csbProject.VoiceLimitGroupNodes);

                    // Reload
                    SetProject(project);
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            TreeView treeView = (TreeView)sender;

            if (e.KeyCode == Keys.Delete && treeView.SelectedNode != null)
            {
                RemoveNode(treeView.SelectedNode);
            }

            else if (e.Control && e.KeyCode == Keys.C && treeView.SelectedNode != null)
            {
                CopyNode(treeView.SelectedNode);
            }

            else if (e.Control && e.KeyCode == Keys.V && treeView == treeViewOfCopiedNode)
            {
                if (treeView.SelectedNode == null)
                {
                    PasteNode();
                }

                else if (treeView.SelectedNode != null && (treeView.SelectedNode.Tag == null || (treeView.SelectedNode.Tag is BuilderSynthNode && ((BuilderSynthNode)treeView.SelectedNode.Tag).Type == BuilderSynthType.WithChildren)))
                {
                    PasteAsChildNode(treeView.SelectedNode);
                }

                else if (treeView.SelectedNode != null && treeView.SelectedNode.Tag != null)
                {
                    PasteNode(treeView.SelectedNode.Index + 1);
                }
            }

            else if (e.KeyCode == Keys.Enter && treeView.SelectedNode != null)
            {
                treeView.SelectedNode.BeginEdit();
            }
        }

        private bool CheckIfAny(TreeView treeView)
        {
            foreach (TreeNode treeNode in treeView.Nodes)
            {
                if (CheckIfAny(treeNode))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckIfAny(TreeNode treeNode)
        {
            if (treeNode.Tag != null)
            {
                return true;
            }

            foreach (TreeNode childNode in treeNode.Nodes)
            {
                if (CheckIfAny(childNode))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetAisacReference(object sender, EventArgs e)
        {
            if (!CheckIfAny(aisacTree))
            {
                MessageBox.Show("This project does not contain any Aisac nodes.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SetReferenceForm setReferenceForm = new SetReferenceForm(aisacTree))
            {
                if (setReferenceForm.ShowDialog(this) == DialogResult.OK)
                {
                    ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

                    if (menuStrip.SourceControl is TreeView)
                    {
                        TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                        TreeNode selectedNode = selectedTree.SelectedNode;

                        if (selectedNode.Tag is BuilderSynthNode)
                        {
                            BuilderSynthNode synthNode = (BuilderSynthNode)selectedNode.Tag;

                            if (setReferenceForm.SelectedNode == null)
                            {
                                synthNode.AisacReference = string.Empty;
                            }

                            else
                            {
                                synthNode.AisacReference = setReferenceForm.SelectedNode.FullPath;
                            }
                        }

                        saved = false;
                        propertyGrid.Refresh();
                    }
                }
            }
        }

        private void SetVoiceLimitGroupReference(object sender, EventArgs e)
        {
            if (voiceLimitGroupTree.Nodes.Count == 0)
            {
                MessageBox.Show("This project does not contain any Voice Limit Group nodes.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SetReferenceForm setReferenceForm = new SetReferenceForm(voiceLimitGroupTree))
            {
                if (setReferenceForm.ShowDialog(this) == DialogResult.OK)
                {
                    ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

                    if (menuStrip.SourceControl is TreeView)
                    {
                        TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                        TreeNode selectedNode = selectedTree.SelectedNode;

                        if (selectedNode.Tag is BuilderSynthNode)
                        {
                            BuilderSynthNode synthNode = (BuilderSynthNode)selectedNode.Tag;

                            if (setReferenceForm.SelectedNode == null)
                            {
                                synthNode.VoiceLimitGroupReference = string.Empty;
                            }

                            else
                            {
                                synthNode.VoiceLimitGroupReference = setReferenceForm.SelectedNode.FullPath;
                            }
                        }

                        saved = false;
                        propertyGrid.Refresh();
                    }
                }
            }
        }

        private void SetSynthReference(object sender, EventArgs e)
        {
            if (!CheckIfAny(synthTree))
            {
                MessageBox.Show("This project does not contain any Synth nodes.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SetReferenceForm setReferenceForm = new SetReferenceForm(synthTree))
            {
                if (setReferenceForm.ShowDialog(this) == DialogResult.OK)
                {
                    ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

                    if (menuStrip.SourceControl is TreeView)
                    {
                        TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                        TreeNode selectedNode = selectedTree.SelectedNode;

                        if (selectedNode.Tag is BuilderCueNode)
                        {
                            BuilderCueNode cueNode = (BuilderCueNode)selectedNode.Tag;

                            if (setReferenceForm.SelectedNode == null)
                            {
                                cueNode.SynthReference = string.Empty;
                            }

                            else
                            {
                                cueNode.SynthReference = setReferenceForm.SelectedNode.FullPath;
                            }
                        }

                        saved = false;
                        propertyGrid.Refresh();
                    }
                }
            }
        }

        private void SelectSynthReference(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedNode.Tag is BuilderCueNode)
                {
                    BuilderCueNode cueNode = (BuilderCueNode)selectedNode.Tag;
                    if (!string.IsNullOrEmpty(cueNode.SynthReference))
                    {
                        TreeNode treeNode = synthTree.FindNodeByFullPath(cueNode.SynthReference);

                        synthTree.SelectedNode = treeNode;
                        treeNode.EnsureVisible();
                        synthTree.Focus();
                    }
                }
            }
        }

        private void SelectAisacReference(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedNode.Tag is BuilderSynthNode)
                {
                    BuilderSynthNode synthNode = (BuilderSynthNode)selectedNode.Tag;
                    if (!string.IsNullOrEmpty(synthNode.AisacReference))
                    {
                        TreeNode treeNode = aisacTree.FindNodeByFullPath(synthNode.AisacReference);

                        tabControl1.SelectTab(tabPage1);
                        aisacTree.SelectedNode = treeNode;
                        treeNode.EnsureVisible();
                        aisacTree.Focus();
                    }
                }
            }
        }

        private void SelectVoiceLimitGroupReference(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedNode.Tag is BuilderSynthNode)
                {
                    BuilderSynthNode synthNode = (BuilderSynthNode)selectedNode.Tag;
                    if (!string.IsNullOrEmpty(synthNode.VoiceLimitGroupReference))
                    {
                        TreeNode treeNode = voiceLimitGroupTree.FindNodeByFullPath(synthNode.VoiceLimitGroupReference);

                        tabControl1.SelectTab(tabPage2);
                        voiceLimitGroupTree.SelectedNode = treeNode;
                        treeNode.EnsureVisible();
                        voiceLimitGroupTree.Focus();
                    }
                }
            }
        }

        private void SelectSoundElementReference(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedNode.Tag is BuilderSynthNode)
                {
                    BuilderSynthNode synthNode = (BuilderSynthNode)selectedNode.Tag;
                    if (!string.IsNullOrEmpty(synthNode.SoundElementReference))
                    {
                        TreeNode treeNode = soundElementTree.FindNodeByFullPath(synthNode.SoundElementReference);

                        soundElementTree.SelectedNode = treeNode;
                        treeNode.EnsureVisible();
                        soundElementTree.Focus();
                    }
                }
            }
        }

        private void SetSoundElementReference(object sender, EventArgs e)
        {
            if (!CheckIfAny(soundElementTree))
            {
                MessageBox.Show("This project does not contain any Sound nodes.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SetReferenceForm setReferenceForm = new SetReferenceForm(soundElementTree))
            {
                if (setReferenceForm.ShowDialog(this) == DialogResult.OK)
                {
                    ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

                    if (menuStrip.SourceControl is TreeView)
                    {
                        TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                        TreeNode selectedNode = selectedTree.SelectedNode;

                        if (selectedNode.Tag is BuilderSynthNode)
                        {
                            BuilderSynthNode synthNode = (BuilderSynthNode)selectedNode.Tag;

                            if (setReferenceForm.SelectedNode == null)
                            {
                                synthNode.SoundElementReference = string.Empty;
                            }

                            else
                            {
                                synthNode.SoundElementReference = setReferenceForm.SelectedNode.FullPath;
                            }
                        }

                        saved = false;
                        propertyGrid.Refresh();
                    }
                }
            }
        }

        private void SetAudio(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                BuilderSoundElementNode soundElementNode = (BuilderSoundElementNode)selectedNode.Tag;

                if (selectedNode.Tag is BuilderSoundElementNode)
                {
                    string intro = soundElementNode.Intro;
                    if (!string.IsNullOrEmpty(intro))
                    {
                        intro = Path.Combine(project.AudioDirectory.FullName, intro);
                    }

                    string loop = soundElementNode.Loop;
                    if (!string.IsNullOrEmpty(loop))
                    {
                        loop = Path.Combine(project.AudioDirectory.FullName, loop);
                    }

                    using (SetAudioForm setAudioForm = new SetAudioForm(intro, loop))
                    {
                        if (setAudioForm.ShowDialog(this) == DialogResult.OK)
                        {
                            uint sampleRate = soundElementNode.SampleRate;
                            byte channelCount = soundElementNode.ChannelCount;

                            uint introSampleCount = 0;
                            soundElementNode.Intro = project.AddAudio(setAudioForm.Intro);
                            if (!string.IsNullOrEmpty(setAudioForm.Intro))
                            {
                                ReadAdx(setAudioForm.Intro, out sampleRate, out channelCount, out introSampleCount);
                            }

                            uint loopSampleCount = 0;
                            soundElementNode.Loop = project.AddAudio(setAudioForm.Loop);
                            if (!string.IsNullOrEmpty(setAudioForm.Loop))
                            {
                                ReadAdx(setAudioForm.Loop, out sampleRate, out channelCount, out loopSampleCount);
                            }

                            soundElementNode.SampleRate = sampleRate;
                            soundElementNode.ChannelCount = channelCount;
                            soundElementNode.SampleCount = introSampleCount + loopSampleCount;
                            propertyGrid.Refresh();
                        }
                    }
                }
            }
        }

        public void ReadAdx(string path, out uint sampleRate, out byte channelCount, out uint sampleCount)
        {
            AdxHeader header = AdxConverter.LoadHeader(path);
            sampleRate = header.SampleRate;
            channelCount = header.ChannelCount;
            sampleCount = header.SampleCount;
        }

        private void CreateChildSoundNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedNode.Tag == null)
                {
                    CreateSoundNode(selectedNode.Nodes, selectedNode);
                }
            }
        }

        private void CreateSound(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;

                CreateSoundNode(selectedTree.Nodes, null);
            }
        }

        private int GetByteCountFromMiliseconds(int miliseconds, int sampleRate, int channelCount, int bitsPerSample)
        {
            return (int)(miliseconds * sampleRate / 1000.0) * channelCount * bitsPerSample / 8;
        }

        private void AddSoundElementSound(BuilderSoundElementNode soundElementNode, double volume, double pitch, int sampleCount, int delayTime)
        {
            BufferedWaveProvider provider = null;
            int delayInBytes = 0;

            if (!string.IsNullOrEmpty(soundElementNode.Intro))
            {
                provider = AdxConverter.Decode(project.GetFullAudioPath(soundElementNode.Intro), volume, pitch);

                if (delayTime != 0)
                {
                    delayInBytes = GetByteCountFromMiliseconds(delayTime, provider.WaveFormat.SampleRate, provider.WaveFormat.Channels, provider.WaveFormat.BitsPerSample);

                    byte[] buffer = new byte[provider.BufferLength];
                    provider.Read(buffer, 0, buffer.Length);

                    provider = new BufferedWaveProvider(provider.WaveFormat);
                    provider.BufferLength = delayInBytes + buffer.Length;
                    provider.ReadFully = false;

                    provider.AddSamples(new byte[delayInBytes], 0, delayInBytes);
                    provider.AddSamples(buffer, 0, buffer.Length);
                }
            }

            // If there's ANYTHING that can loop audio files in NAudio... please, tell me!
            if (!string.IsNullOrEmpty(soundElementNode.Loop))
            {
                BufferedWaveProvider loopProvider = AdxConverter.Decode(project.GetFullAudioPath(soundElementNode.Loop), volume, pitch);

                if (provider == null && delayTime != 0)
                {
                    delayInBytes = GetByteCountFromMiliseconds(delayTime, loopProvider.WaveFormat.SampleRate, loopProvider.WaveFormat.Channels, loopProvider.WaveFormat.BitsPerSample);
                }

                int writtenByteCount = 0;

                byte[] intro = null;

                byte[] loop = new byte[loopProvider.BufferLength];
                loopProvider.Read(loop, 0, loop.Length);

                if (provider != null)
                {
                    intro = new byte[provider.BufferLength];
                    provider.Read(intro, 0, intro.Length);
                }

                provider = new BufferedWaveProvider(loopProvider.WaveFormat);
                provider.BufferLength = 0;

                if (sampleCount != -1)
                {
                    provider.BufferLength = delayInBytes + (sampleCount * provider.WaveFormat.Channels * provider.WaveFormat.BitsPerSample / 8);
                }

                else
                {
                    if (intro != null)
                    {
                        provider.BufferLength += intro.Length;
                    }

                    provider.BufferLength += loop.Length;
                }

                provider.ReadFully = false;

                if (intro != null)
                {
                    provider.AddSamples(intro, 0, intro.Length);
                    writtenByteCount += intro.Length;
                }

                else if (intro == null && delayTime != 0)
                {
                    provider.BufferLength += delayInBytes;
                    provider.AddSamples(new byte[delayInBytes], 0, delayInBytes);
                    writtenByteCount += delayInBytes;
                }

                if (sampleCount != -1)
                {
                    while (writtenByteCount < provider.BufferLength)
                    {
                        if ((writtenByteCount + loop.Length) > provider.BufferLength)
                        {
                            provider.AddSamples(loop, 0, provider.BufferLength - writtenByteCount);
                            break;
                        }

                        writtenByteCount += loop.Length;
                        provider.AddSamples(loop, 0, loop.Length);
                    }
                }

                else
                {
                    provider.AddSamples(loop, 0, loop.Length);
                }
            }

            if (provider != null)
            {
                WaveOut sound = new WaveOut();
                sound.Init(provider);
                sounds.Add(sound);
            }
        }

        private void AddSoundElementSound(TreeNode soundElementTreeNode, double volume, double pitch, int sampleCount, int delayTime)
        {
            BuilderSoundElementNode soundElementNode = (BuilderSoundElementNode)soundElementTreeNode.Tag;
            AddSoundElementSound(soundElementNode, volume, pitch, sampleCount, delayTime);
        }

        private double GetSecondsFromSampleCount(int sampleCount, int sampleRate)
        {
            return sampleCount / (double)sampleRate;
        }

        private void GetTheBiggestSampleCount(TreeNode synthTreeNode, ref int sampleCount)
        {
            if (synthTreeNode.Tag is BuilderSynthNode)
            {
                BuilderSynthNode synthNode = (BuilderSynthNode)synthTreeNode.Tag;

                if (synthNode.Type == BuilderSynthType.Single && !string.IsNullOrEmpty(synthNode.SoundElementReference))
                {
                    BuilderSoundElementNode soundElementNode = (BuilderSoundElementNode)soundElementTree.FindNodeByFullPath(synthNode.SoundElementReference).Tag;

                    // We want the loop to be at least 10 seconds
                    int soundElementSampleCount = (int)soundElementNode.SampleCount;

                    while (GetSecondsFromSampleCount(soundElementSampleCount, (int)soundElementNode.SampleRate) < 10.0)
                    {
                        soundElementSampleCount *= 2;
                    }

                    if (soundElementSampleCount > sampleCount)
                    {
                        sampleCount = soundElementSampleCount;
                    }
                }
            }

            foreach (TreeNode childNode in synthTreeNode.Nodes)
            {
                GetTheBiggestSampleCount(childNode, ref sampleCount);
            }
        }

        private double GetAbsolutePitch(TreeNode synthTreeNode)
        {
            double pitch = 0;

            while (synthTreeNode != null)
            {
                if (synthTreeNode.Tag is BuilderSynthNode)
                {
                    BuilderSynthNode synthNode = (BuilderSynthNode)synthTreeNode.Tag;
                    pitch += synthNode.Pitch;
                }

                synthTreeNode = synthTreeNode.Parent;
            }

            return pitch / 1000.0;
        }

        private double GetAbsoluteVolume(TreeNode synthTreeNode)
        {
            double volume = 1000;

            while (synthTreeNode != null)
            {
                if (synthTreeNode.Tag is BuilderSynthNode)
                {
                    BuilderSynthNode synthNode = (BuilderSynthNode)synthTreeNode.Tag;
                    volume = (volume * synthNode.Volume) / 1000;
                }

                synthTreeNode = synthTreeNode.Parent;
            }

            return volume / 1000.0;
        }

        private int GetAbsoluteDelayTime(TreeNode synthTreeNode)
        {
            int delayTime = 0;

            while (synthTreeNode != null)
            {
                if (synthTreeNode.Tag is BuilderSynthNode)
                {
                    BuilderSynthNode synthNode = (BuilderSynthNode)synthTreeNode.Tag;
                    delayTime += (int)synthNode.DelayTime;
                }

                synthTreeNode = synthTreeNode.Parent;
            }

            return delayTime;
        }

        private void AddSynthSound(TreeNode synthTreeNode, int sampleCount)
        {
            BuilderSynthNode synthNode = (BuilderSynthNode)synthTreeNode.Tag;

            if (synthNode.Type == BuilderSynthType.WithChildren)
            {
                if (synthNode.PlaybackType == BuilderSynthPlaybackType.RandomNoRepeat || synthNode.PlaybackType == BuilderSynthPlaybackType.Random)
                {
                    TreeNode childNode = synthTreeNode.Nodes[synthNode.RandomChildNode];
                    BuilderSynthNode childSynthNode = (BuilderSynthNode)childNode.Tag;

                    if (childSynthNode.Type == BuilderSynthType.Single && !string.IsNullOrEmpty(childSynthNode.SoundElementReference) && childSynthNode.PlayThisTurn)
                    {
                        AddSoundElementSound(soundElementTree.FindNodeByFullPath(childSynthNode.SoundElementReference), GetAbsoluteVolume(childNode), GetAbsolutePitch(childNode), sampleCount, GetAbsoluteDelayTime(childNode));
                    }

                    else if (childSynthNode.Type == BuilderSynthType.WithChildren)
                    {
                        AddSynthSound(childNode, sampleCount);
                    }
                }

                else if (synthNode.PlaybackType == BuilderSynthPlaybackType.Sequential)
                {
                    TreeNode childNode = synthTreeNode.Nodes[synthNode.NextChildNode];
                    BuilderSynthNode childSynthNode = (BuilderSynthNode)childNode.Tag;

                    if (childSynthNode.Type == BuilderSynthType.Single && !string.IsNullOrEmpty(childSynthNode.SoundElementReference) && childSynthNode.PlayThisTurn)
                    {
                        AddSoundElementSound(soundElementTree.FindNodeByFullPath(childSynthNode.SoundElementReference), GetAbsoluteVolume(childNode), GetAbsolutePitch(childNode), sampleCount, GetAbsoluteDelayTime(childNode));
                    }

                    else if (childSynthNode.Type == BuilderSynthType.WithChildren)
                    {
                        AddSynthSound(childNode, sampleCount);
                    }
                }

                else
                {
                    foreach (TreeNode childNode in synthTreeNode.Nodes)
                    {
                        BuilderSynthNode childSynthNode = (BuilderSynthNode)childNode.Tag;

                        if (childSynthNode.Type == BuilderSynthType.Single && !string.IsNullOrEmpty(childSynthNode.SoundElementReference) && childSynthNode.PlayThisTurn)
                        {
                            AddSoundElementSound(soundElementTree.FindNodeByFullPath(childSynthNode.SoundElementReference), GetAbsoluteVolume(childNode), GetAbsolutePitch(childNode), sampleCount, GetAbsoluteDelayTime(childNode));
                        }

                        else if (childSynthNode.Type == BuilderSynthType.WithChildren)
                        {
                            AddSynthSound(childNode, sampleCount);
                        }
                    }
                }
            }

            else if (synthNode.Type == BuilderSynthType.Single)
            {
                if (!string.IsNullOrEmpty(synthNode.SoundElementReference) && synthNode.PlayThisTurn)
                {
                    AddSoundElementSound(soundElementTree.FindNodeByFullPath(synthNode.SoundElementReference), synthNode.Volume / 1000.0, synthNode.Pitch / 1000.0, GetTheBiggestSampleCount(synthTreeNode), (int)synthNode.DelayTime);
                }
            }
        }

        private int GetTheBiggestSampleCount(TreeNode synthNode)
        {
            int sampleCount = -1;
            GetTheBiggestSampleCount(synthNode, ref sampleCount);

            return sampleCount;
        }

        private void PlaySound(object sender, EventArgs e)
        {
            StopSound(sender, e);

            if (cueTree.Focused && cueTree.SelectedNode != null && cueTree.SelectedNode.Tag is BuilderCueNode)
            {
                BuilderCueNode cueNode = (BuilderCueNode)cueTree.SelectedNode.Tag;

                if (!string.IsNullOrEmpty(cueNode.SynthReference))
                {
                    TreeNode synthNode = synthTree.FindNodeByFullPath(cueNode.SynthReference);
                    AddSynthSound(synthNode, GetTheBiggestSampleCount(synthNode));
                }
            }

            else if (soundElementTree.Focused && soundElementTree.SelectedNode != null && soundElementTree.SelectedNode.Tag is BuilderSoundElementNode)
            {
                AddSoundElementSound(soundElementTree.SelectedNode, 1, 0, -1, 0);
            }

            else if (synthTree.Focused && synthTree.SelectedNode != null && synthTree.SelectedNode.Tag is BuilderSynthNode)
            {
                AddSynthSound(synthTree.SelectedNode, GetTheBiggestSampleCount(synthTree.SelectedNode));
            }

            sounds.ForEach(sound => sound.Play());
        }

        private void StopSound(object sender, EventArgs e)
        {
            sounds.ForEach(sound =>
            {
                sound.Stop();
                sound.Dispose();
            });

            sounds.Clear();
        }

        private TreeNode CloneNode(TreeNode treeNode)
        {
            TreeNode clonedNode = (TreeNode)treeNode.Clone();
            CloneTag(clonedNode);

            return clonedNode;
        }

        private void CloneTag(TreeNode treeNode)
        {
            if (treeNode.Tag != null && treeNode.Tag is ICloneable)
            {
                treeNode.Tag = ((ICloneable)treeNode.Tag).Clone();
            }

            foreach (TreeNode childNode in treeNode.Nodes)
            {
                CloneTag(childNode);
            }
        }

        private void CopyNode(TreeNode treeNode)
        {
            treeViewOfCopiedNode = treeNode.TreeView;
            copiedNode = CloneNode(treeNode);
        }

        private void CopyNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                CopyNode(selectedNode);
            }
        }

        private void AddToProject(TreeNode treeNode)
        {
            if (treeNode.Tag is BuilderCueNode)
            {
                BuilderCueNode cueNode = (BuilderCueNode)treeNode.Tag;

                // Do checks
                if (!string.IsNullOrEmpty(cueNode.SynthReference) && synthTree.FindNodeByFullPath(cueNode.SynthReference) == null)
                {
                    cueNode.SynthReference = string.Empty;
                }

                project.CueNodes.Add(cueNode);
            }

            else if (treeNode.Tag is BuilderSynthNode)
            {
                BuilderSynthNode synthNode = (BuilderSynthNode)treeNode.Tag;

                if (!string.IsNullOrEmpty(synthNode.SoundElementReference) && soundElementTree.FindNodeByFullPath(synthNode.SoundElementReference) == null)
                {
                    synthNode.SoundElementReference = string.Empty;
                }

                if (!string.IsNullOrEmpty(synthNode.AisacReference) && aisacTree.FindNodeByFullPath(synthNode.AisacReference) == null)
                {
                    synthNode.AisacReference = string.Empty;
                }

                if (!string.IsNullOrEmpty(synthNode.VoiceLimitGroupReference) && !voiceLimitGroupTree.Nodes.ContainsKey(synthNode.VoiceLimitGroupReference))
                {
                    synthNode.VoiceLimitGroupReference = string.Empty;
                }

                project.SynthNodes.Add(synthNode);
            }

            if (treeNode.Tag is BuilderSoundElementNode)
            {
                BuilderSoundElementNode soundElementNode = (BuilderSoundElementNode)treeNode.Tag;

                if (!string.IsNullOrEmpty(soundElementNode.Intro) && !File.Exists(project.GetFullAudioPath(soundElementNode.Intro)))
                {
                    soundElementNode.Intro = string.Empty;
                }

                if (!string.IsNullOrEmpty(soundElementNode.Loop) && !File.Exists(project.GetFullAudioPath(soundElementNode.Loop)))
                {
                    soundElementNode.Loop = string.Empty;
                }

                project.SoundElementNodes.Add(soundElementNode);
            }

            if (treeNode.Tag is BuilderAisacNode)
            {
                project.AisacNodes.Add((BuilderAisacNode)treeNode.Tag);
            }

            if (treeNode.Tag is BuilderVoiceLimitGroupNode)
            {
                project.VoiceLimitGroupNodes.Add((BuilderVoiceLimitGroupNode)treeNode.Tag);
            }
        }

        private void PasteAsChildNode(TreeNode parent, int nodeIndex = -1)
        {
            saved = false;

            TreeNode nodeToPaste = CloneNode(copiedNode);

            // Look for any duplicates
            int index = -1;
            string name = nodeToPaste.Name;

            while (parent.Nodes.ContainsKey(nodeToPaste.Name))
            {
                nodeToPaste.Name = $"{name}_Copy{++index}";
            }

            nodeToPaste.Text = nodeToPaste.Name;

            // paste it now
            if (nodeIndex == -1)
            {
                parent.Nodes.Add(nodeToPaste);
            }

            else
            {
                parent.Nodes.Insert(nodeIndex, nodeToPaste);
            }

            // Update the CSB node and its children (will prevent some issues)
            UpdateNodeNoRename(nodeToPaste);

            // fix node if the tree is synthTree, and the nodes are synth nodes
            if (parent.TreeView == synthTree && parent.Tag is BuilderSynthNode && nodeToPaste.Tag is BuilderSynthNode)
            {
                BuilderSynthNode synthNode = (BuilderSynthNode)parent.Tag;
                BuilderSynthNode copiedSynthNode = (BuilderSynthNode)nodeToPaste.Tag;

                if (synthNode.Type == BuilderSynthType.WithChildren)
                {
                    // add the new node to track's children
                    synthNode.Children.Add(copiedSynthNode.Name);
                }
            }

            AddToProject(nodeToPaste);

            nodeToPaste.TreeView.SelectedNode = nodeToPaste;
            nodeToPaste.EnsureVisible();
        }

        private void PasteNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == treeViewOfCopiedNode)
                {
                    // Parent is a CSB node but the copied object is a folder? abort.
                    if (selectedNode.Tag != null && copiedNode.Tag == null)
                    {
                        return;
                    }

                    PasteAsChildNode(selectedNode);
                }
            }
        }

        private void PasteNode(int nodeIndex = -1)
        {
            saved = false;

            TreeNode nodeToPaste = CloneNode(copiedNode);

            // check if it's cue and fix duplicate identifier if needed
            if (nodeToPaste.Tag is BuilderCueNode)
            {
                BuilderCueNode cueNode = (BuilderCueNode)nodeToPaste.Tag;

                while (project.CueNodes.Exists(cue => cue.Identifier == cueNode.Identifier))
                {
                    cueNode.Identifier++;
                }
            }

            // paste
            if (treeViewOfCopiedNode.SelectedNode != null && treeViewOfCopiedNode.SelectedNode.Parent != null)
            {
                // Look for any duplicates
                int index = -1;
                string name = nodeToPaste.Name;

                while (treeViewOfCopiedNode.SelectedNode.Parent.Nodes.ContainsKey(nodeToPaste.Name))
                {
                    nodeToPaste.Name = $"{name}_Copy{++index}";
                }

                nodeToPaste.Text = nodeToPaste.Name;

                if (nodeIndex == -1)
                {
                    treeViewOfCopiedNode.SelectedNode.Parent.Nodes.Add(nodeToPaste);
                }

                else
                {
                    treeViewOfCopiedNode.SelectedNode.Parent.Nodes.Insert(nodeIndex, nodeToPaste);
                }
            }

            else
            {
                // Look for any duplicates
                int index = -1;
                string name = nodeToPaste.Name;

                while (treeViewOfCopiedNode.Nodes.ContainsKey(nodeToPaste.Name))
                {
                    nodeToPaste.Name = $"{name}_Copy{++index}";
                }

                nodeToPaste.Text = nodeToPaste.Name;

                if (nodeIndex == -1)
                {
                    treeViewOfCopiedNode.Nodes.Add(nodeToPaste);
                }

                else
                {
                    treeViewOfCopiedNode.Nodes.Insert(nodeIndex, nodeToPaste);
                }
            }

            // update the CSB node and its children
            UpdateNodeNoRename(nodeToPaste);
            AddToProject(nodeToPaste);

            nodeToPaste.TreeView.SelectedNode = nodeToPaste;
            nodeToPaste.EnsureVisible();
        }

        private void PasteNodeOnTree(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;

                if (selectedTree == treeViewOfCopiedNode)
                {
                    PasteNode();
                }
            }
        }

        private void PasteAndInsertNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == treeViewOfCopiedNode)
                {
                    PasteNode(selectedNode.Index + 1);
                }
            }
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            MessageBox.Show($"{Program.ApplicationVersion} by Skyth (blueskythlikesclouds)", "CSB Builder");
        }

        private void CreateChildTrackNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == synthTree)
                {
                    CreateSynthNode(selectedNode.Nodes, selectedNode);
                }
            }
        }

        private void CreateAndInsertNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == cueTree)
                {
                    CreateCueNode(selectedTree.Nodes, null, selectedNode.Index + 1);
                }

                else if (selectedTree == synthTree)
                {
                    if (selectedNode.Tag is BuilderSynthNode && ((BuilderSynthNode)selectedNode.Tag).Type == BuilderSynthType.WithChildren)
                    {
                        CreateSoundNode(selectedNode.Nodes, selectedNode, selectedNode.Index + 1);
                    }

                    else
                    {
                        CreateSynthNode(selectedNode.Nodes, selectedNode, selectedNode.Index + 1);
                    }
                }

                else if (selectedTree == soundElementTree)
                {
                    CreateSoundElementNode(selectedNode.Parent != null ? selectedNode.Parent.Nodes : selectedTree.Nodes, selectedNode, selectedNode.Index + 1);
                }

                else if (selectedTree == aisacTree)
                {
                    CreateAisacNode(selectedNode.Nodes, selectedNode, selectedNode.Index + 1);
                }

                else if (selectedTree == voiceLimitGroupTree)
                {
                    CreateVoiceLimitGroupNode(selectedTree.Nodes, null, selectedNode.Index + 1);
                }
            }
        }

        private void CreateAndInsertSoundNode(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedNode.Tag is BuilderSynthNode)
                {
                    if (selectedNode.Parent == null)
                    {
                        CreateSoundNode(synthTree.Nodes, null, selectedNode.Index + 1);
                    }

                    else
                    {
                        CreateSoundNode(selectedNode.Parent.Nodes, selectedNode.Parent, selectedNode.Index + 1);
                    }
                }
            }
        }

        private void LoadTemplate(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == aisacTree)
                {
                    using (OpenFileDialog openAisacTemplate = new OpenFileDialog()
                    {
                        Title = "Load AISAC Template",
                        Filter = "XML Files|*.xml",
                        DefaultExt = "xml",
                        FileName = selectedNode != null ? selectedNode.Name : string.Empty,
                    })
                    {
                        if (openAisacTemplate.ShowDialog() == DialogResult.OK)
                        {
                            XmlSerializer aisacSerializer = new XmlSerializer(typeof(BuilderAisacNode));

                            try
                            {
                                using (Stream source = File.OpenRead(openAisacTemplate.FileName))
                                {
                                    BuilderAisacNode aisacNode = (BuilderAisacNode)aisacSerializer.Deserialize(source);
                                    TreeNode aisacTreeNode = null;

                                    // folder
                                    if (selectedNode != null && selectedNode.Tag == null)
                                    {
                                        aisacTreeNode = CreateAisacNode(selectedNode.Nodes, selectedNode, -1, aisacNode);
                                    }

                                    // aisac
                                    else if (selectedNode != null && selectedNode.Tag is BuilderAisacNode)
                                    {
                                        project.AisacNodes[project.AisacNodes.IndexOf((BuilderAisacNode)selectedNode.Tag)] = aisacNode;

                                        aisacNode.Name = selectedNode.FullPath;
                                        selectedNode.Tag = aisacNode;
                                        selectedTree.SelectedNode = selectedNode;
                                    }

                                    // nothing, create a new aisac
                                    else if (selectedNode == null)
                                    {
                                        aisacTreeNode = CreateAisacNode(aisacTree.Nodes, null, -1, aisacNode);
                                    }

                                    // fix the current aisac if it was imported
                                    if (aisacTreeNode != null)
                                    {
                                        if (!string.IsNullOrEmpty(aisacNode.Name))
                                        {
                                            // duplicate? fix it really quick
                                            string name = Path.GetFileName(aisacNode.Name);
                                            string uniqueName = name;

                                            TreeNodeCollection collection = aisacTreeNode.Parent != null ? aisacTreeNode.Parent.Nodes : aisacTree.Nodes;

                                            int index = -1;
                                            while (collection.ContainsKey(uniqueName))
                                            {
                                                uniqueName = $"{name}_{++index}";
                                            }

                                            aisacTreeNode.Name = uniqueName;
                                            aisacTreeNode.Text = aisacTreeNode.Name;
                                        }

                                        aisacNode.Name = aisacTreeNode.FullPath;
                                        aisacTreeNode.Tag = aisacNode;

                                        selectedTree.SelectedNode = aisacTreeNode;
                                        aisacTreeNode.EnsureVisible();
                                    }
                                }
                            }

                            catch
                            {
                                MessageBox.Show("The template file is invalid.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void SaveTemplate(object sender, EventArgs e)
        {
            ContextMenuStrip menuStrip = (ContextMenuStrip)((ToolStripItem)sender).Owner;

            if (menuStrip.SourceControl is TreeView)
            {
                TreeView selectedTree = (TreeView)menuStrip.SourceControl;
                TreeNode selectedNode = selectedTree.SelectedNode;

                if (selectedTree == aisacTree)
                {
                    using (SaveFileDialog saveAisacTemplate = new SaveFileDialog()
                    {
                        Title = "Save AISAC Template",
                        Filter = "XML Files|*.xml",
                        DefaultExt = "xml",
                        FileName = selectedNode.Name,
                    })
                    {
                        if (saveAisacTemplate.ShowDialog() == DialogResult.OK)
                        {
                            if (selectedNode.Tag is BuilderAisacNode)
                            {
                                XmlSerializer aisacSerializer = new XmlSerializer(typeof(BuilderAisacNode));

                                using (Stream destination = File.Create(saveAisacTemplate.FileName))
                                {
                                    aisacSerializer.Serialize(destination, selectedNode.Tag);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
