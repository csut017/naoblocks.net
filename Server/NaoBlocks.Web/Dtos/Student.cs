using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a student.
    /// </summary>
    public class Student : User
    {
        /// <summary>
        /// Gets or sets the student's age.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the student's gender.
        /// </summary>
        public string Gender { get; set; } = "Unknown";

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <returns>A new <see cref="Student"/> instance containing the required properties.</returns>
        public new static Student FromModel(Data.User value, bool includeDetails = false)
        {
            var student = new Student
            {
                Name = value.Name,
                WhenAdded = value.WhenAdded,
                Age = value.StudentDetails?.Age,
                Gender = value.StudentDetails?.Gender ?? "Unknown"
            };
            if (includeDetails)
            {
                student.Settings = value.Settings;
            }
            return student;
        }
    }
}
