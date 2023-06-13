using System.Text.RegularExpressions;

namespace Mtx.CosmosDbServices;

public record ContainerName
{

	private ContainerName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
		}
		if (name.Length < 3 || name.Length > 63)
			throw new ArgumentOutOfRangeException(nameof(name), "must be between 3 and 63 characters long.");

		name = name.ToLower();

		string regexPattern = @"^[a-z0-9][a-z0-9\-]{1,61}[a-z0-9]$";
		Regex regex = new Regex(regexPattern);

		if (!regex.IsMatch(name))
		{
			throw new ArgumentException("Container names can only contain lowercase letters, numbers, or the dash(-) character and must start with a lowercase letter or number.");
		}

		Name = name;
	}

	public string Name { get; }
	public static implicit operator string(ContainerName containerName) => containerName.Name;
	public static ContainerName From(string name) => new ContainerName(name);
}
