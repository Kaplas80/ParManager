namespace ParLib.UnitTests
{
    using NUnit.Framework;
    using ParLibrary.Converter;
    using System;
    using System.IO;
    using Yarhl.FileSystem;

    public class ZeroLengthPar
    {
        /// <summary>
        /// Test that reading a 0B PAR file returns an empty node when using AllowZeroLengthPars = true.
        /// </summary>
        [Test]
        public void ZeroLengthParIsEmptyNodeWhenAllowed()
        {
            var readerParameters = new ParArchiveReaderParameters
            {
                Recursive = true,

                // If we encounter a zero-length PAR at any point, we treat it as an empty directory.
                AllowZeroLengthPars = true,
            };

            // This creates an empty BinaryStream for the Node, so it's a 0b file
            Node test_0b_par = NodeFactory.FromMemory("test_0b_par.par");
            test_0b_par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters);

            Assert.AreEqual(0, test_0b_par.Children.Count);
        }

        /// <summary>
        /// Test that reading a 0B PAR file throws an exception when using AllowZeroLengthPars = false.
        /// </summary>
        [Test]
        public void ZeroLengthParThrowsWhenNotAllowed()
        {
            var readerParameters = new ParArchiveReaderParameters
            {
                Recursive = true,

                AllowZeroLengthPars = false,
            };

            // This creates an empty BinaryStream for the Node, so it's a 0b file
            Node test_0b_par = NodeFactory.FromMemory("test_0b_par.par");

            Assert.Throws<InvalidDataException>(() => test_0b_par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters));
        }

        /// <summary>
        /// Test that when recursively reading a PAR that *contains* a 0B PAR file, that the 0B par is treated as an empty directory with the correct name.
        /// </summary>
        [Test]
        public void ParContainingZeroLengthParHasNodeWhenAllowed()
        {
            var readerParameters = new ParArchiveReaderParameters
            {
                Recursive = true,

                AllowZeroLengthPars = true,
            };

            // This loads the file stored in Tests/ParLib.UnitTests
            Node test_par_containing_0b_par = NodeFactory.FromFile("test_par_containing_0b_par.par", Yarhl.IO.FileOpenMode.Read);
            test_par_containing_0b_par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters);

            // The toplevel par should have one child (it's a directory)
            Assert.AreEqual(1, test_par_containing_0b_par.Children.Count);

            // That child should be '.', which should *also* have one child.
            // (I exported this test file with IncludeDots = true).
            Assert.AreEqual(".", test_par_containing_0b_par.Children[0].Name);
            Assert.AreEqual(1, test_par_containing_0b_par.Children[0].Children.Count);

            // That child should be test_0kb_par.par
            var test_0kb_par = test_par_containing_0b_par.Children[0].Children[0];
            Assert.AreEqual("test_0kb_par.par", test_0kb_par.Name);

            // test_0kb_par.par should have no children
            Assert.AreEqual(0, test_0kb_par.Children.Count);

            // Overall the listing is
            // /test_par_containing_0b_par.par
            // /test_par_containing_0b_par.par/./
            // /test_par_containing_0b_par.par/./test_0kb_par/
        }

        /// <summary>
        /// Test that reading a PAR *containing* 0B PAR file throws an exception when using AllowZeroLengthPars = false.
        ///
        /// This is not necessarily desirable behaviour, and at time of writing there isn't a nice way to surface the error to the user.
        /// "test_par_containing_0b_par.par is fine, but it contains test_0b_par.par and that is bad!"
        ///
        /// This test is meant to document the current behaviour, and if the behaviour is improved then it should be changed or removed.
        /// </summary>
        [Test]
        public void ParContainingZeroLengthParThrowsWhenNotAllowed()
        {
            var readerParameters = new ParArchiveReaderParameters
            {
                Recursive = true,

                AllowZeroLengthPars = false,
            };

            // This loads the file stored in Tests/ParLib.UnitTests
            Node test_par_containing_0b_par = NodeFactory.FromFile("test_par_containing_0b_par.par", Yarhl.IO.FileOpenMode.Read);

            Assert.Throws<InvalidDataException>(() => test_par_containing_0b_par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters));
        }
    }
}
