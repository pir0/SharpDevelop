﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.IO;

using ICSharpCode.PackageManagement;
using ICSharpCode.PackageManagement.Design;
using NuGet;
using NUnit.Framework;
using PackageManagement.Tests.Helpers;

namespace PackageManagement.Tests
{
	[TestFixture]
	public class PackageFromRepositoryTests
	{
		FakePackage fakePackage;
		TestablePackageFromRepository package;
		FakePackageRepository fakeRepository;
		
		void CreatePackage()
		{
			package = new TestablePackageFromRepository();
			fakePackage = package.FakePackagePassedToConstructor;
			fakeRepository = package.FakePackageRepositoryPassedToConstructor;
		}
		
		[Test]
		public void Repository_PackageCreatedWithSourceRepository_ReturnsSourceRepository()
		{
			CreatePackage();
			IPackageRepository repository = package.Repository;
			
			Assert.AreEqual(fakeRepository, repository);
		}
		
		[Test]
		public void AssemblyReferences_WrappedPackageHasOneAssemblyReference_ReturnsOneAssemblyReference()
		{
			CreatePackage();
			fakePackage.AssemblyReferenceList.Add(new FakePackageAssemblyReference());
			
			IEnumerable<IPackageAssemblyReference> assemblyReferences = package.AssemblyReferences;
			List<IPackageAssemblyReference> expectedAssemblyReferences = fakePackage.AssemblyReferenceList;
			
			CollectionAssert.AreEqual(expectedAssemblyReferences, assemblyReferences);
		}
		
		[Test]
		public void Id_WrappedPackageIdIsTest_ReturnsTest()
		{
			CreatePackage();
			fakePackage.Id = "Test";
			string id = package.Id;
			
			Assert.AreEqual("Test", id);
		}
		
		[Test]
		public void Version_WrappedPackageVersionIsOnePointOne_ReturnsOnePointOne()
		{
			CreatePackage();
			var expectedVersion = new Version("1.1");
			fakePackage.Version = expectedVersion;
			Version version = package.Version;
			
			Assert.AreEqual(expectedVersion, version);
		}
		
		[Test]
		public void Title_WrappedPackageTitleIsTest_ReturnsTest()
		{
			CreatePackage();
			fakePackage.Title = "Test";
			string title = package.Title;
			
			Assert.AreEqual("Test", title);
		}
		
		[Test]
		public void Authors_WrappedPackageHasOneAuthor_ReturnsOneAuthor()
		{
			CreatePackage();
			fakePackage.AuthorsList.Add("Author1");
			
			IEnumerable<string> authors = package.Authors;
			List<string> expectedAuthors = fakePackage.AuthorsList;
			
			CollectionAssert.AreEqual(expectedAuthors, authors);
		}
		
		[Test]
		public void Owners_WrappedPackageHasOneOwner_ReturnsOneOwner()
		{
			CreatePackage();
			fakePackage.OwnersList.Add("Owner1");
			
			IEnumerable<string> owners = package.Owners;
			List<string> expectedOwners = fakePackage.OwnersList;
			
			CollectionAssert.AreEqual(expectedOwners, owners);
		}
		
		[Test]
		public void IconUrl_WrappedPackageIconUrlIsHttpSharpDevelopNet_ReturnsHttpSharpDevelopNet()
		{
			CreatePackage();
			var expectedUrl = new Uri("http://sharpdevelop.net");
			fakePackage.IconUrl = expectedUrl;
			Uri url = package.IconUrl;
			
			Assert.AreEqual(expectedUrl, url);
		}
		
		[Test]
		public void LicenseUrl_WrappedPackageLicenseUrlIsHttpSharpDevelopNet_ReturnsHttpSharpDevelopNet()
		{
			CreatePackage();
			var expectedUrl = new Uri("http://sharpdevelop.net");
			fakePackage.LicenseUrl = expectedUrl;
			Uri url = package.LicenseUrl;
			
			Assert.AreEqual(expectedUrl, url);
		}
		
		[Test]
		public void ProjectUrl_WrappedPackageProjectUrlIsHttpSharpDevelopNet_ReturnsHttpSharpDevelopNet()
		{
			CreatePackage();
			var expectedUrl = new Uri("http://sharpdevelop.net");
			fakePackage.ProjectUrl = expectedUrl;
			Uri url = package.ProjectUrl;
			
			Assert.AreEqual(expectedUrl, url);
		}
		
		[Test]
		public void ReportAbuseUrl_WrappedPackageReportAbuseUrlIsHttpSharpDevelopNet_ReturnsHttpSharpDevelopNet()
		{
			CreatePackage();
			var expectedUrl = new Uri("http://sharpdevelop.net");
			fakePackage.ReportAbuseUrl = expectedUrl;
			Uri url = package.ReportAbuseUrl;
			
			Assert.AreEqual(expectedUrl, url);
		}
		
		[Test]
		public void RequiresLicenseAcceptance_WrappedPackageRequiresLicenseAcceptanceIsTrue_ReturnsTrue()
		{
			CreatePackage();
			fakePackage.RequireLicenseAcceptance = true;
			
			Assert.IsTrue(package.RequireLicenseAcceptance);
		}
		
		[Test]
		public void Description_WrappedPackageDescriptionIsTest_ReturnsTest()
		{
			CreatePackage();
			fakePackage.Description = "Test";
			string description = package.Description;
			
			Assert.AreEqual("Test", description);
		}
		
		[Test]
		public void Summary_WrappedPackageSummaryIsTest_ReturnsTest()
		{
			CreatePackage();
			fakePackage.Summary = "Test";
			string summary = package.Summary;
			
			Assert.AreEqual("Test", summary);
		}
		
		[Test]
		public void Language_WrappedPackageLanguageIsTest_ReturnsTest()
		{
			CreatePackage();
			fakePackage.Language = "Test";
			string language = package.Language;
			
			Assert.AreEqual("Test", language);
		}
		
		[Test]
		public void Tags_WrappedPackageTagsIsTest_ReturnsTest()
		{
			CreatePackage();
			fakePackage.Tags = "Test";
			string tags = package.Tags;
			
			Assert.AreEqual("Test", tags);
		}
		
		[Test]
		public void FrameworkAssemblies_WrappedPackageHasOneFrameworkAssembly_ReturnsOneFrameworkAssembly()
		{
			CreatePackage();
			fakePackage.FrameworkAssembliesList.Add(new FrameworkAssemblyReference("System.Xml"));
			
			IEnumerable<FrameworkAssemblyReference> assemblies = package.FrameworkAssemblies;
			IEnumerable<FrameworkAssemblyReference> expectedAssemblies = fakePackage.FrameworkAssemblies;
			
			CollectionAssert.AreEqual(expectedAssemblies, assemblies);
		}
		
		[Test]
		public void Dependencies_WrappedPackageHasOneDependency_ReturnsOneDependency()
		{
			CreatePackage();
			fakePackage.DependenciesList.Add(new PackageDependency("Test"));
			
			IEnumerable<PackageDependency> dependencies = package.Dependencies;
			IEnumerable<PackageDependency> expectedDependencies = fakePackage.Dependencies;
			
			CollectionAssert.AreEqual(expectedDependencies, dependencies);
		}
		
		[Test]
		public void GetFiles_WrappedPackageHasOneFile_ReturnsOneFile()
		{
			CreatePackage();
			fakePackage.FilesList.Add(new PhysicalPackageFile());
			
			IEnumerable<IPackageFile> files = package.GetFiles();
			IEnumerable<IPackageFile> expectedFiles = fakePackage.FilesList;
			
			CollectionAssert.AreEqual(expectedFiles, files);
		}
		
		[Test]
		public void DownloadCount_WrappedPackageDownloadCountIsTen_ReturnsTen()
		{
			CreatePackage();
			fakePackage.DownloadCount = 10;
			int count = package.DownloadCount;
			
			Assert.AreEqual(10, count);
		}
		
		[Test]
		public void RatingsCount_WrappedPackageRatingsCountIsTen_ReturnsTen()
		{
			CreatePackage();
			fakePackage.RatingsCount = 10;
			int count = package.RatingsCount;
			
			Assert.AreEqual(10, count);
		}
		
		[Test]
		public void Rating_WrappedPackageRatingIsFive_ReturnsFive()
		{
			CreatePackage();
			fakePackage.Rating = 5.0;
			double rating = package.Rating;
			
			Assert.AreEqual(5.0, rating);
		}
		
		[Test]
		public void GetStream_WrappedPackageHasStream_ReturnsWrappedPackageStream()
		{
			CreatePackage();
			var expectedStream = new MemoryStream();
			fakePackage.Stream = expectedStream;
			
			Stream stream = package.GetStream();
			
			Assert.AreEqual(expectedStream, stream);
		}
		
		[Test]
		public void HasDependencies_WrappedPackageHasNoDependencies_ReturnsFalse()
		{
			CreatePackage();
			fakePackage.DependenciesList.Clear();
			bool result = package.HasDependencies;
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void HasDependencies_WrappedPackageHasOneDependency_ReturnsTrue()
		{
			CreatePackage();
			fakePackage.DependenciesList.Add(new PackageDependency("Test"));
			bool result = package.HasDependencies;
			
			Assert.IsTrue(result);
		}
		
		[Test]
		public void LastUpdated_PackageWrapsDataServicePackageThatHasLastUpdatedDateOffset_ReturnsDateFromDataServicePackage()
		{
			CreatePackage();
			var expectedDateTime = new DateTime(2011, 1, 2);
			package.DateTimeOffsetToReturnFromGetDataServicePackageLastUpdated = new DateTimeOffset(expectedDateTime);
			
			DateTime? lastUpdated = package.LastUpdated;
			
			Assert.AreEqual(expectedDateTime, lastUpdated.Value);
		}
		
		[Test]
		public void LastUpdated_PackageWrapsPackageThatDoesNotHaveLastUpdatedDateOffset_ReturnsNullDate()
		{
			CreatePackage();
			package.DateTimeOffsetToReturnFromGetDataServicePackageLastUpdated = null;
			
			DateTime? lastUpdated = package.LastUpdated;
			
			Assert.IsFalse(lastUpdated.HasValue);
		}
	}
}
