using System;
using Data = NaoBlocks.Core.Models;

namespace NaoBlocks.Web.Dtos
{
    public class User
    {
        public Data.UserSettings? Settings { get; set; }

        public string? Name { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }

        public DateTime? WhenAdded { get; set; }

        public string? Token { get; set; }

        public static User FromModel(Data.User value, bool includeDetails = false)
        {
            if (value == null) return null;

            var user = new User
            {
                Name = value.Name,
                Role = value.Role.ToString(),
                WhenAdded = value.WhenAdded
            };

            if (includeDetails)
            {
                user.Settings = value.Settings;
            }

            return user;
        }
    }
}