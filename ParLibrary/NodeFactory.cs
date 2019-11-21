namespace ParLibrary
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Node factory.
    /// </summary>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownserhip dispose transferred")]
    public static class NodeFactory
    {
        /// <summary>
        /// Creates a new <see cref="Node"/> with a new NodeContainer format.
        /// </summary>
        /// <returns>The new node.</returns>
        /// <param name="name">Node name.</param>
        public static Node CreateContainer(string name)
        {
            return new Node(name, new ParFolder());
        }

        /// <summary>
        /// Creates the missing parent nodes to contain the child and add it.
        /// </summary>
        /// <param name="root">The root node that will contain the nodes.</param>
        /// <param name="path">
        /// The path for the child. It doesn't contain the root or child names.</param>
        /// <param name="child">The child to add to root with the path.</param>
        public static void CreateContainersForChild(Node root, string path, Node child)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // Replace wrong slashes to support native Windows paths
            path = path.Replace("\\", NodeSystem.PathSeparator);

            string[] parentNames = path.Split(
                new[] { NodeSystem.PathSeparator[0] },
                StringSplitOptions.RemoveEmptyEntries);

            Node currentNode = root;
            foreach (string name in parentNames)
            {
                Node subParent = currentNode.Children[name];
                if (subParent == null)
                {
                    subParent = CreateContainer(name);
                    currentNode.Add(subParent);
                }

                currentNode = subParent;
            }

            currentNode.Add(child);
        }

        /// <summary>
        /// Creates a Node from a file.
        /// </summary>
        /// <returns>The node.</returns>
        /// <param name="filePath">File path.</param>
        public static Node FromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string filename = Path.GetFileName(filePath);
            return FromFile(filePath, filename);
        }

        /// <summary>
        /// Creates a Node from a file.
        /// </summary>
        /// <returns>The node.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="nodeName">Node name.</param>
        public static Node FromFile(string filePath, string nodeName)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            // We need to catch if the node creation fails
            // for instance for null names, to dispose the stream.
            var format = new ParFile(DataStreamFactory.FromFile(filePath, FileOpenMode.ReadWrite))
            {
                CanBeCompressed = !filePath.EndsWith(".PAR", StringComparison.InvariantCultureIgnoreCase),
                FileDate = new FileInfo(filePath).CreationTime,
            };
            Node node;
            try
            {
                node = new Node(nodeName, format);
            }
            catch
            {
                format.Dispose();
                throw;
            }

            return node;
        }

        /// <summary>
        /// Creates a Node containing all the files from the directory.
        /// </summary>
        /// <returns>The container node.</returns>
        /// <param name="dirPath">Directory path.</param>
        /// <param name="filter">Filter for files in directory.</param>
        /// <param name="nodeName">Node name.</param>
        /// <param name="subDirectories">
        /// If <see langword="true" /> it searchs recursively in subdirectories.
        /// </param>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Ownserhip dispose transferred")]
        public static Node FromDirectory(
            string dirPath,
            string filter,
            string nodeName,
            bool subDirectories = false)
        {
            SearchOption options = subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            Node folder = CreateContainer(nodeName);
            foreach (string filePath in Directory.GetFiles(dirPath, filter, options))
            {
                string relParent = Path.GetDirectoryName(filePath)
                    .Replace(dirPath, string.Empty);
                CreateContainersForChild(folder, relParent, FromFile(filePath));
            }

            return folder;
        }
    }
}
