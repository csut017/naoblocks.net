using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a user.
    /// </summary>
    public class Teacher : User
    {
        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <returns>A new <see cref="Teacher"/> instance containing the required properties.</returns>
        public static Teacher FromModel(Data.User value)
        {
            return new Teacher
            {
                Name = value.Name
            };
        }
    }
}
