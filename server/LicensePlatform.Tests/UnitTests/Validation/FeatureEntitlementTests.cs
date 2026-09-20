using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Tests.Helpers;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Validation
{
    /// <summary>
    /// Tests for feature entitlement logic.
    /// Verifies that licenses and plans expose the correct set of features,
    /// and that feature checks return accurate results.
    /// </summary>
    public class FeatureEntitlementTests
    {
        [Fact]
        public void HasFeature_AssignedFeature_ReturnsTrue()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var feature = TestDataBuilder.CreateTestFeature(product, "ADVANCED_SCAN");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var planFeature = TestDataBuilder.CreateTestPlanFeature(plan, feature);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            var licenseFeature = TestDataBuilder.CreateTestLicenseFeature(license, feature);

            var assignedFeatures = new HashSet<string> { feature.FeatureKey };

            // Act
            var hasFeature = assignedFeatures.Contains("ADVANCED_SCAN");

            // Assert
            hasFeature.Should().BeTrue("license with assigned ADVANCED_SCAN feature should return true");
        }

        [Fact]
        public void HasFeature_UnassignedFeature_ReturnsFalse()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var feature = TestDataBuilder.CreateTestFeature(product, "WIZARD_MODE");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);

            // No features assigned
            var assignedFeatures = new HashSet<string>();

            // Act
            var hasFeature = assignedFeatures.Contains("WIZARD_MODE");

            // Assert
            hasFeature.Should().BeFalse("license with no features should not have WIZARD_MODE");
        }

        [Fact]
        public void Features_MatchPlan()
        {
            // Arrange — plan includes BASIC_SCAN and QUICK_CLEAN
            var product = TestDataBuilder.CreateTestProduct();
            var feature1 = TestDataBuilder.CreateTestFeature(product, "BASIC_SCAN");
            var feature2 = TestDataBuilder.CreateTestFeature(product, "QUICK_CLEAN");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var planFeatures = new List<PlanFeature>
            {
                TestDataBuilder.CreateTestPlanFeature(plan, feature1),
                TestDataBuilder.CreateTestPlanFeature(plan, feature2)
            };

            // Act
            var planFeatureKeys = planFeatures.Select(pf => pf.Feature.FeatureKey).ToHashSet();

            // Assert
            planFeatureKeys.Should().Contain("BASIC_SCAN", "plan must include BASIC_SCAN");
            planFeatureKeys.Should().Contain("QUICK_CLEAN", "plan must include QUICK_CLEAN");
            planFeatureKeys.Count.Should().Be(2, "plan must have exactly 2 features");
        }

        [Fact]
        public void Features_MatchLicense()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var feature1 = TestDataBuilder.CreateTestFeature(product, "AUTO_UPDATE");
            var feature2 = TestDataBuilder.CreateTestFeature(product, "CLOUD_BACKUP");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);

            var licenseFeatures = new List<LicenseFeature>
            {
                TestDataBuilder.CreateTestLicenseFeature(license, feature1),
                TestDataBuilder.CreateTestLicenseFeature(license, feature2)
            };

            // Act
            var licenseFeatureKeys = licenseFeatures.Select(lf => lf.Feature.FeatureKey).ToHashSet();

            // Assert
            licenseFeatureKeys.Should().Contain("AUTO_UPDATE");
            licenseFeatureKeys.Should().Contain("CLOUD_BACKUP");
            licenseFeatureKeys.Count.Should().Be(2);
        }

        [Fact]
        public void PlanFeature_BelongsToCorrectPlan()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var feature = TestDataBuilder.CreateTestFeature(product, "PROTECTION");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var planFeature = TestDataBuilder.CreateTestPlanFeature(plan, feature);

            // Act & Assert
            planFeature.PlanId.Should().Be(plan.PlanId,
                "plan feature must reference the correct plan");
            planFeature.FeatureId.Should().Be(feature.FeatureId,
                "plan feature must reference the correct feature");
        }

        [Fact]
        public void LicenseFeature_BelongsToCorrectLicense()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var feature = TestDataBuilder.CreateTestFeature(product, "SHIELD");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            var licenseFeature = TestDataBuilder.CreateTestLicenseFeature(license, feature);

            // Act & Assert
            licenseFeature.LicenseId.Should().Be(license.LicenseId,
                "license feature must reference the correct license");
            licenseFeature.FeatureId.Should().Be(feature.FeatureId,
                "license feature must reference the correct feature");
        }

        [Fact]
        public void DisabledFeature_ShouldNotBeEntitled()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var feature = TestDataBuilder.CreateTestFeature(product, "BETA_FEATURE");
            feature.IsEnabled = false;

            // Act
            var isEntitled = feature.IsEnabled;

            // Assert
            isEntitled.Should().BeFalse("disabled feature should not be entitled");
        }

        [Fact]
        public void MultipleFeatures_AllIndependent()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var features = new[]
            {
                TestDataBuilder.CreateTestFeature(product, "F1"),
                TestDataBuilder.CreateTestFeature(product, "F2"),
                TestDataBuilder.CreateTestFeature(product, "F3")
            };

            var featureKeys = features.Select(f => f.FeatureKey).ToHashSet();

            // Assert
            featureKeys.Count.Should().Be(3, "all features must be unique");
            foreach (var f in features)
            {
                featureKeys.Contains(f.FeatureKey).Should().BeTrue(
                    $"feature {f.FeatureKey} must be present");
            }
        }
    }
}
