using nexRemoteFree.Server.Components.TreeView;
using nexRemoteFree.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nexRemoteFree.Server.Components.Scripts
{
    public class ScriptTreeNode
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public TreeItemType ItemType { get; set; }
        public string Name { get; init; }
        public List<ScriptTreeNode> ChildItems { get; } = new();
        public SavedScript Script { get; init; }
    }
}
