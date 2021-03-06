﻿using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Factory
{
    public class DependencyAnalyzerFactoryTests : FileBasedTestsBase
    {
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();

        [Fact]
        public void CreateInProcess_DefaultInfoImportance()
        {
            var configFilePath = GetFilePathInTestClassFolder("");

            var dependencyAnalyzer = CreateFactory()
                .CreateInProcess(configFilePath, _typeDependencyEnumeratorMock.Object);

            dependencyAnalyzer.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [Fact]
        public void CreateInProcess_InfoImportanceAppliedToFactory()
        {
            var configFilePath = GetFilePathInTestClassFolder("");

            var dependencyAnalyzer = CreateFactory()
                .SetDefaultInfoImportance(Importance.High)
                .CreateInProcess(configFilePath, _typeDependencyEnumeratorMock.Object);

            dependencyAnalyzer.InfoImportance.Should().Be(Importance.High);
        }

        private static DependencyAnalyzerFactory CreateFactory() 
            => new DependencyAnalyzerFactory(traceMessageHandler: null);
    }
}
