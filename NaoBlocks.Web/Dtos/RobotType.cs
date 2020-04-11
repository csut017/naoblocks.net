using System.Collections.Generic;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class RobotType
    {
        public string Name { get; set; } = string.Empty;

        public bool? IsDefault { get; set; }

        public IList<Data.ToolboxCategory>? Toolbox { get; private set; }

        public static RobotType? FromModel(Data.RobotType value)
        {
            return FromModel(value, ConversionOptions.SummaryOnly);
        }

        public static RobotType? FromModel(Data.RobotType value, ConversionOptions opts)
        {
            if (value == null) return null;
            
            var output = new RobotType
            {
                Name = value.Name,
                IsDefault = value.IsDefault
            };

            if (opts.HasFlag(ConversionOptions.IncludeDetails))
            {
                output.Toolbox = value.Toolbox;
            }

            return output;
        }
    }
}