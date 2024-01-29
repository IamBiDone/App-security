using Newtonsoft.Json;

public class RecaptchaResponse
{
	[JsonProperty("success")]
	public bool Success { get; set; }
}