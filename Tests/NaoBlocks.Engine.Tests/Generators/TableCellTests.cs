using NaoBlocks.Engine.Generators;
using System;
using System.Collections.Generic;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class TableCellTests
    {
        public static IEnumerable<object?[]> ToStringData =>
            new List<object?[]>
            {
                new object?[] {null, null, ""},
                new object?[] {"Testing", null, "Testing"},
                new object?[] {"Testing", "0", "Testing"},
                new object?[] {1234, "#,##0", "1,234"},
                new object?[] {1234, null, "1234"},
                new object?[] {1234L, "#,##0", "1,234"},
                new object?[] {1234L, null, "1234"},
                new object?[] {12.34, "0.000", "12.340"},
                new object?[] {12.34, null, "12.34"},
                new object?[] {12.34, null, "12.34" },
                new object?[] {true, null, "Yes" },
                new object?[] {false, null, "No" },
                new object?[] {new DateTime(2022, 1, 2), "yyyy-MM-dd", "2022-01-02"},
                new object?[] {new DateTime(2022, 1, 2), null, "2/01/2022 12:00:00 am"},
            };

        [Fact]
        public void ImplicitlyConvertsFromBoolean()
        {
            TableCell cell = true;
            Assert.Equal(true, cell.Value);
            Assert.Null(cell.Format);
        }

        [Fact]
        public void ImplicitlyConvertsFromDateTime()
        {
            var now = DateTime.Now;
            TableCell cell = now;
            Assert.Equal(now, cell.Value);
            Assert.Equal("yyyy-MM-dd", cell.Format);
        }

        [Fact]
        public void ImplicitlyConvertsFromDouble()
        {
            TableCell cell = 12.34;
            Assert.Equal(12.34, cell.Value);
            Assert.Equal("0.00", cell.Format);
        }

        [Fact]
        public void ImplicitlyConvertsFromInt()
        {
            TableCell cell = 1234;
            Assert.Equal(1234, cell.Value);
            Assert.Equal("0", cell.Format);
        }

        [Fact]
        public void ImplicitlyConvertsFromLong()
        {
            TableCell cell = 123456L;
            Assert.Equal(123456L, cell.Value);
            Assert.Equal("0", cell.Format);
        }

        [Fact]
        public void ImplicitlyConvertsFromString()
        {
            TableCell cell = "testing";
            Assert.Equal("testing", cell.Value);
            Assert.Null(cell.Format);
        }

        [Fact]
        public void TableCellCanBeInitialisedEmpty()
        {
            var cell = new TableCell();
            Assert.Null(cell.Value);
            Assert.Null(cell.Format);
        }

        [Fact]
        public void TableCellCanBeInitialisedWithData()
        {
            var cell = new TableCell("data", "format");
            Assert.Equal("data", cell.Value);
            Assert.Equal("format", cell.Format);
        }

        [Theory]
        [MemberData(nameof(ToStringData))]
        public void ToStringGeneratesRepresentation(object? data, string? format, string expected)
        {
            var cell = new TableCell(data, format);
            var actual = cell.ToString();
            Assert.Equal(expected, actual);
        }
    }
}