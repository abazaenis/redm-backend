namespace Redm_backend.Dtos
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;

	using Redm_backend.Dtos.HomePage;

	public class StatusConverter : JsonConverter<Status>
	{
		public override Status Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Read the string value from the JSON and convert it to the enum
			string stringValue = reader.GetString()!;

			// Convert the string to the corresponding enum value
			return stringValue!.ToLower() switch
			{
				"normal" => Status.Normal,
				"warning" => Status.Warning,
				"abnormal" => Status.Abnormal,
				_ => throw new JsonException($"Unexpected value '{stringValue}' for Status")
			};
		}

		public override void Write(Utf8JsonWriter writer, Status value, JsonSerializerOptions options)
		{
			// Convert the enum value back to a string when writing JSON
			string stringValue = value switch
			{
				Status.Normal => "Normal",
				Status.Warning => "Warning",
				Status.Abnormal => "Abnormal",
				_ => throw new JsonException($"Unexpected Status value '{value}'")
			};

			writer.WriteStringValue(stringValue);
		}
	}
}
