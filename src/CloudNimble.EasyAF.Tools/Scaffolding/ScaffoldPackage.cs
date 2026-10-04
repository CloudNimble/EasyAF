namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// A NuGet package reference for a scaffolded project.
    /// </summary>
    /// <param name="Id">The package id.</param>
    /// <param name="Version">The floating version, for example <c>4.*</c>.</param>
    public sealed record ScaffoldPackage(string Id, string Version);

}
