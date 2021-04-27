using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VariableGroupUsage.Models
{
    public class HomeViewModel
    {
        public Dictionary<int, string> AllVariableGroups { get; set; }
        public int SelectedVariableGroupId { get; set; }

        public List<String> VariableGroupAppearsInBuildPipelines { get; set; }
        public List<String> VariableGroupAppearsInReleasePipelines { get; set; }

    }
}
