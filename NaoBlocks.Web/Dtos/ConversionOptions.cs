using System;

namespace NaoBlocks.Web.Dtos
{
    [Flags]
    public enum ConversionOptions
    {
        SummaryOnly = 1,

        IncludeDetails = 2
    }
}