using NaoBlocks.Web.Dtos;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NaoBlocks.Web.Helpers
{
    internal class TutorialExerciseLineConverter
        : JsonConverter<TutorialExerciseLine>
    {
        public override TutorialExerciseLine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var line = new TutorialExerciseLine();
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.EndObject:
                                return line;

                            case JsonTokenType.PropertyName:
                                var propertyName = reader.GetString();
                                reader.Read();
                                switch (propertyName)
                                {
                                    case "message":
                                        line.Message = reader.GetString();
                                        break;

                                    case "order":
                                        if (reader.TryGetInt32(out int order))
                                        {
                                            line.Order = order;
                                        }
                                        else
                                        {
                                            throw new JsonException("order is not a valid number");
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                    break;

                case JsonTokenType.String:
                    line.Message = reader.GetString();
                    break;
            }
            return line;
        }

        public override void Write(Utf8JsonWriter writer, TutorialExerciseLine value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value.Message != null) writer.WriteString("message", value.Message);
            if (value.Order != null) writer.WriteNumber("order", value.Order.Value);
            writer.WriteEndObject();
        }
    }
}