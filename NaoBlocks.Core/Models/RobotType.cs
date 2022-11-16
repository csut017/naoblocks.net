﻿using System;
using System.Collections.Generic;

namespace NaoBlocks.Core.Models
{
    public class RobotType
    {
        public static RobotType Unknown
        {
            get { return new RobotType { Name = "Unknown" }; }
        }

        public string Id { get; set; } = string.Empty;

        public bool IsDefault { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime WhenAdded { get; set; }

        public IList<ToolboxCategory> Toolbox { get; private set; } = new List<ToolboxCategory>();

        public IList<BlockSet> BlockSets { get; private set; } = new List<BlockSet>();
    }
}