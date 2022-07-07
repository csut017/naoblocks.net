using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace NaoBlocks.Engine.Tests.Generators
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReportFormatDataAttribute
        : DataAttribute
    {
        private readonly ReportFormat[] allowedFormats;

        public ReportFormatDataAttribute(params ReportFormat[] allowedFormats)
        {
            this.allowedFormats = allowedFormats;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            foreach (ReportFormat format in Enum.GetValues(typeof(ReportFormat)))
            {
                yield return new object[] { format, this.allowedFormats.Contains(format) };
            }
        }
    }
}