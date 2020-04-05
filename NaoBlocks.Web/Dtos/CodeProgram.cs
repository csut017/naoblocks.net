using System;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class CodeProgram
    {
        public string? Code { get; set; }

        public long? Id { get; set; }

        public string? Name { get; set; }

        public bool? Store { get; set; }

        public DateTime WhenAdded { get; set; }

        public static CodeProgram? FromModel(Data.CodeProgram? value)
        {
            return value == null ? null : new CodeProgram
            {
                Id = value.Number,
                Name = value.Name,
                WhenAdded = value.WhenAdded
            };
        }

        public static CodeProgram? FromModelWithDetails(Data.CodeProgram? value)
        {
            return value == null ? null : new CodeProgram
            {
                Id = value.Number,
                Name = value.Name,
                WhenAdded = value.WhenAdded,
                Code = value.Code
            };
        }
    }
}