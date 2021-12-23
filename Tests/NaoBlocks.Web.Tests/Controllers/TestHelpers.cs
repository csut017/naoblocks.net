﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NaoBlocks.Web.Tests.Controllers
{
    public static class TestHelpers
    {
        internal static IEnumerable<string> ExtractValidationErrors(ExecutionResult result)
        {
            return result.ValidationErrors
                .Select(err => err.Error);
        }

        internal static void SetRequestBody(this ControllerBase controller, string? body)
        {
            var httpContext = new DefaultHttpContext();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body ?? string.Empty));
            httpContext.Request.Body = stream;
            httpContext.Request.ContentLength = stream.Length;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}