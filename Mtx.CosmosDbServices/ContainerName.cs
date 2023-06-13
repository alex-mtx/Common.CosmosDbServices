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
	/// <summary>
	/// Creates a new instance and enforces the rules from  <see href="https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-dotnet-create-container#name-a-container"/>
	/// </summary>
	/// <remarks>
	/// <para>In short:</para>
	/// <list type="bullet">
	/// <item>Keep container names between 3 and 63 characters long.</item>
	/// <item>Container names can only contain lowercase letters, numbers, or the dash(-) character.</item>
	/// <item>Container names must start with a lowercase letter or number.</item>
	/// </list>
	/// <para>Upper case is automatically converted to lower case.</para>
	/// 
	/// </remarks>
	/// <param name="name"></param>
	/// <returns></returns>
	public static ContainerName From(string name) => new ContainerName(name);
}
