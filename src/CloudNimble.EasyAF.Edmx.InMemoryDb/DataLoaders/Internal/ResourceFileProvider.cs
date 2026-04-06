// --------------------------------------------------------------------------------------------
// <copyright file="ResourceFileProvider.cs" company="Effort Team">
//     Copyright (C) Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.DataLoaders.Internal
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.DataLoaders;

    internal class ResourceFileProvider : IFileProvider
    {
        private readonly bool valid;
        private readonly Assembly assembly;
        private readonly string resourcePath;

        public ResourceFileProvider(Uri path)
        {
            if (path is null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Scheme != "res")
            {
                throw new ArgumentException("Invalid path", "path");
            }

            valid = false;

            var asmName = path.Host;

            assembly = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(x =>
                    x.GetName().Name.StartsWith(
                        asmName,
                        StringComparison.InvariantCultureIgnoreCase));

            if (assembly is null)
            {
                return;
            }

            var parts = path.Segments
                .Select(x => x.TrimEnd('/'))
                .Where(x => !string.IsNullOrEmpty(x));

            resourcePath = string.Format("{0}.{1}",
                assembly.GetName().Name,
                string.Join(".", parts));

            var resoures = assembly.GetManifestResourceNames();

            valid = resoures.Any(x =>
                x.StartsWith(
                    resourcePath,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        public IFileReference GetFile(string name)
        {
            if (!IsValid)
            {
                return null;
            }

            var path = string.Format("{0}.{1}", resourcePath, name);

            return new ResourceFileReference(assembly, path);

        }

        public bool IsValid
        {
            get
            {
                return valid;
            }
        }
    }
}
