namespace Redm_backend.Dtos.PeriodHistory
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;

	public class ActionConverter : JsonConverter<ActionType>
	{
		public override ActionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Read the string value from the JSON and convert it to the enum
			string stringValue = reader.GetString();

			// Convert "Add" or "Delete" to the corresponding enum value
			return stringValue.ToLower() switch
			{
				"add" => ActionType.Add,
				"delete" => ActionType.Delete,
				_ => throw new JsonException($"Unexpected value '{stringValue}' for ActionType")
			};
		}

		public override void Write(Utf8JsonWriter writer, ActionType value, JsonSerializerOptions options)
		{
			// Convert the enum value back to a string when writing JSON
			string stringValue = value switch
			{
				ActionType.Add => "Add",
				ActionType.Delete => "Delete",
				_ => throw new JsonException($"Unexpected ActionType value '{value}'")
			};

			writer.WriteStringValue(stringValue);
		}
	}
}
