using KrollDiscounting.Data;
using KrollDiscounting.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

// Adjust the namespace to match project test namespace.
namespace KrollDiscounting.Tests
{
    [TestClass]
    public class BondContextTests
    {
        const string exportedJason = "{\r\n  \"$id\": \"1\",\r\n  \"$type\": \"yieldBasedDiscountRateMethodology\",\r\n  \"RiskFreeRatePct\": 2.5,\r\n  \"BenchmarkSpread\": {\r\n    \"$id\": \"2\",\r\n    \"$type\": \"avgBenchmarkSpread\",\r\n    \"SpreadPct\": 10.0,\r\n    \"DiscountRateMethodology\": {\r\n      \"$ref\": \"1\"\r\n    }\r\n  },\r\n  \"CreditSpreadPct\": 1.0,\r\n  \"AssetTypeSpreadPct\": 0.5,\r\n  \"EffectiveTotalYieldFrac\": 0.14,\r\n  \"Name\": \"YieldMethod\",\r\n  \"Precision\": 4\r\n}";

        private (BondContext, SqliteConnection)  DBContextInitialize()
        {
            // Perform setup tasks for the entire class
            // This method runs once before any tests in MyTestClass
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<BondContext>()
                .UseSqlite(connection)
                .Options;
            var ctx = new BondContext(options);

            ctx.Database.EnsureCreated();


            return (ctx, connection);
        }


        private void DBContextCleanup(DbContext? ctx, SqliteConnection connection)
        {
            ctx?.Dispose();
            connection?.Dispose();
        }
        private (AvgBenchmarkSpread, MedianBenchmarkSpread) CreateSpreads()
        {
            // Benchmark spreads
            var avgSpread = new AvgBenchmarkSpread { SpreadPct = 10.0m };
            var medianSpread = new MedianBenchmarkSpread { SpreadPct = 20.0m };
            return (avgSpread, medianSpread);
        }
        private (PriceBasedDiscountRateMethodology, YieldBasedDiscountRateMethodology) CreateDRMObjects(AvgBenchmarkSpread avgSpread)
        {
 

            // Discount rate methodologies
            var priceMethod = new PriceBasedDiscountRateMethodology
            {
                Name = "PriceMethod",
                PriceAsFracOfPar = 1.05,
                Precision = 5            };

            var yieldMethod = new YieldBasedDiscountRateMethodology
            {
                Name = "YieldMethod",
                RiskFreeRatePct = 2.5m,
                BenchmarkSpread = avgSpread,
                CreditSpreadPct = 1.0m,
                AssetTypeSpreadPct = 0.5m,
                Precision = 4
            };
            return (priceMethod, yieldMethod);
           
        }

        public (ZeroCouponCorporateBond, CouponCorporateBond) CreateBonds()
        {
            // Bonds
            var zeroCoupon = new ZeroCouponCorporateBond
            {
                Name = "ZeroCorpBond",
                FaceValue = 1000m,
                Currency = "USD",
                IssueDate=new DateOnly(2025,1,1),
                MaturityDate=new DateOnly(2025, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var couponBond = new CouponCorporateBond
            {
                Name = "CouponCorpBond",
                FaceValue = 1000m,
                Currency = "USD",
                CouponRatePct = 5.25m,
                CouponFrequencyPerYear = 2,
                IssueDate = new DateOnly(2025, 1, 1),
                MaturityDate = new DateOnly(2027, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };
            return (zeroCoupon, couponBond);


        }

        [TestMethod]
        public void CheckBondEquality()
        {
            var couponBondA = new CouponCorporateBond
            {
                Name = "CouponCorpBondA",
                FaceValue = 1000m,
                Currency = "USD",
                CouponRatePct = 5.25m,
                CouponFrequencyPerYear = 2,
                IssueDate = new DateOnly(2025, 1, 1),
                MaturityDate = new DateOnly(2027, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var couponBondB = new CouponCorporateBond
            {
                Name = "CouponCorpBondA",
                FaceValue = 1000m,
                Currency = "USD",
                CouponRatePct = 5.25m,
                CouponFrequencyPerYear = 2,
                IssueDate = new DateOnly(2025, 1, 1),
                MaturityDate = new DateOnly(2027, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var couponBondC = new CouponCorporateBond
            {
                Name = "CouponCorpBond",
                FaceValue = 1000m,
                Currency = "USD",
                CouponRatePct = 6.25m,
                CouponFrequencyPerYear = 2,
                IssueDate = new DateOnly(2025, 1, 1),
                MaturityDate = new DateOnly(2027, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var zeroCouponA = new ZeroCouponCorporateBond
            {
                Name = "ZeroCorpBond",
                FaceValue = 1000m,
                Currency = "USD",
                IssueDate = new DateOnly(2025, 1, 1),
                MaturityDate = new DateOnly(2025, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var zeroCouponB = new ZeroCouponCorporateBond
            {
                Name = "ZeroCorpBond",
                FaceValue = 1000m,
                Currency = "USD",
                IssueDate = new DateOnly(2025, 1, 1),
                MaturityDate = new DateOnly(2025, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var zeroCouponC = new ZeroCouponCorporateBond
            {
                Name = "ZeroCorpBond",
                FaceValue = 1000m,
                Currency = "USD",
                IssueDate = new DateOnly(2025, 1, 1),
                MaturityDate = new DateOnly(2028, 12, 1),
                CreatedAt = DateTimeOffset.UtcNow
            };

            Assert.AreEqual<CouponCorporateBond>(couponBondA, couponBondB);
            Assert.AreNotEqual<CouponCorporateBond>(couponBondA, couponBondC);
            Assert.IsFalse(couponBondA.Equals( zeroCouponA));
            Assert.IsFalse(zeroCouponA.Equals(couponBondA));

            Assert.AreEqual<ZeroCouponCorporateBond>(zeroCouponA, zeroCouponB);
            Assert.AreNotEqual<ZeroCouponCorporateBond>(zeroCouponA, zeroCouponC);


        }

        [TestMethod]
        public void CheckDRMEquality()
        {
            // Discount rate methodologies
            var priceMethod = new PriceBasedDiscountRateMethodology
            {
                Name = "PriceMethod",
                PriceAsFracOfPar = 101.5,
                Precision = 5
            };

            var benchspreadA = new AvgBenchmarkSpread { SpreadPct = 10.0m };
            var benchspreadB = new AvgBenchmarkSpread { SpreadPct = 10.0m };
            var benchspreadC = new AvgBenchmarkSpread { SpreadPct = 11.0m };

            Assert.AreEqual<AvgBenchmarkSpread>(benchspreadA, benchspreadB);
            Assert.AreNotEqual<AvgBenchmarkSpread>(benchspreadA, benchspreadC);

            var yieldMethodA = new YieldBasedDiscountRateMethodology
            {
                Name = "YieldMethod",
                RiskFreeRatePct = 2.5m,
                BenchmarkSpread = benchspreadA,
                CreditSpreadPct = 1.0m,
                AssetTypeSpreadPct = 0.5m,
                Precision = 4
            };

            var yieldMethodB = new YieldBasedDiscountRateMethodology
            {
                Name = "YieldMethod",
                RiskFreeRatePct = 2.5m,
                BenchmarkSpread = new AvgBenchmarkSpread { SpreadPct = 10.0m },
                CreditSpreadPct = 1.0m,
                AssetTypeSpreadPct = 0.5m,
                Precision = 4
            };
            var yieldMethodC = new YieldBasedDiscountRateMethodology
            {
                Name = "YieldMethod",
                RiskFreeRatePct = 2.5m,
                BenchmarkSpread = new AvgBenchmarkSpread { SpreadPct = 10.0m },
                CreditSpreadPct = 1.0m,
                AssetTypeSpreadPct = 0.5m,
                Precision = 5
            };
            Assert.AreEqual<YieldBasedDiscountRateMethodology>(yieldMethodA, yieldMethodB);
            Assert.AreNotEqual<YieldBasedDiscountRateMethodology>(yieldMethodA, yieldMethodC);

            Assert.IsFalse(yieldMethodA.Equals(priceMethod));
        }

        [TestMethod]
        public void CanCreateConcreteEntities_WithSqliteInMemory()
        {
            BondContext ctx;
            SqliteConnection connection;
            (ctx, connection) = DBContextInitialize();

            AvgBenchmarkSpread avgBenchmarkSpread;
            MedianBenchmarkSpread medianBenchmarkSpread;
            (avgBenchmarkSpread, medianBenchmarkSpread) = CreateSpreads();

            ctx?.BenchmarkSpreads.AddRange(avgBenchmarkSpread, medianBenchmarkSpread);

            PriceBasedDiscountRateMethodology priceMethod;
            YieldBasedDiscountRateMethodology yieldMethod;
            (priceMethod,yieldMethod)=  CreateDRMObjects(avgBenchmarkSpread);
            ctx?.DiscountRateMethodologies.AddRange(priceMethod, yieldMethod);

            ZeroCouponCorporateBond zeroCoupon;
            CouponCorporateBond couponBond;
            (zeroCoupon, couponBond) = CreateBonds();

            ctx?.Bonds.AddRange(zeroCoupon, couponBond);
            ctx?.SaveChanges();

            var bondCount = ctx?.Bonds.Count();
            var drmCount = ctx?.DiscountRateMethodologies.Count();
            var spreadCount = ctx?.BenchmarkSpreads.Count();

            Assert.AreEqual(2, bondCount);
            Assert.AreEqual(2, drmCount);
            Assert.AreEqual(2, spreadCount);

            // Verify relationships persisted
            var loadedCouponBond = ctx?.Bonds
                .OfType<CouponCorporateBond>()
                .SingleOrDefault(b => b.Name == "CouponCorpBond");

            Assert.IsNotNull(loadedCouponBond);

            DBContextCleanup(ctx, connection);
        }

        [TestMethod]
        public void ExportObject()
        {
            BondContext ctx;
            SqliteConnection connection;
            (ctx, connection) = DBContextInitialize();

            AvgBenchmarkSpread avgBenchmarkSpread;
            MedianBenchmarkSpread medianBenchmarkSpread;
            (avgBenchmarkSpread, medianBenchmarkSpread) = CreateSpreads();

            ctx?.BenchmarkSpreads.AddRange(avgBenchmarkSpread, medianBenchmarkSpread);

            PriceBasedDiscountRateMethodology priceMethod;
            YieldBasedDiscountRateMethodology yieldMethod;
            (priceMethod, yieldMethod) = CreateDRMObjects(avgBenchmarkSpread);
            ctx?.DiscountRateMethodologies.AddRange(priceMethod, yieldMethod);

            ctx?.SaveChanges();

            var loadedDiscountRateMethodologies = ctx?.DiscountRateMethodologies.ToList();
            Assert.AreEqual(2, loadedDiscountRateMethodologies?.Count);

            var loadedDiscountRateMethodology= ctx?.DiscountRateMethodologies
                .SingleOrDefault(b => b.Name == "YieldMethod");

            Assert.IsNotNull(loadedDiscountRateMethodology);

            var jsonString = loadedDiscountRateMethodology.ExportObjectGraph();
            Assert.IsNotNull(jsonString);
            Assert.IsPositive(jsonString.Length);

            Assert.AreEqual( exportedJason, jsonString);

           
            DBContextCleanup(ctx, connection);

        }

        [TestMethod]
        public void ImportObject()
        {
            BondContext ctx;
            SqliteConnection connection;
            (ctx, connection) = DBContextInitialize();
            var importedDiscountRateMethodology = DiscountRateMethdologyAbstract.ImportObjectGraph(exportedJason);
            Assert.IsNotNull(importedDiscountRateMethodology?.Name, "YieldMethod");
            
            DBContextCleanup(ctx, connection);
        }

        const string validresultString = "AvgBenchmarkSpread: 10.0\r\nCreditSpread: 1.0\r\nAssetTypeSpread: 0.5\r\n";
        [TestMethod]
        public void PriceCorpBonds()
        {
            BondContext ctx;
            SqliteConnection connection;
            (ctx, connection) = DBContextInitialize();

            AvgBenchmarkSpread avgBenchmarkSpread;
            MedianBenchmarkSpread medianBenchmarkSpread;
            (avgBenchmarkSpread, medianBenchmarkSpread) = CreateSpreads();

            ctx?.BenchmarkSpreads.AddRange(avgBenchmarkSpread, medianBenchmarkSpread);

            PriceBasedDiscountRateMethodology priceMethod;
            YieldBasedDiscountRateMethodology yieldMethod;
            (priceMethod, yieldMethod) = CreateDRMObjects(avgBenchmarkSpread);
            ctx?.DiscountRateMethodologies.AddRange(priceMethod, yieldMethod);

            ZeroCouponCorporateBond zeroCoupon;
            CouponCorporateBond couponBond;
            (zeroCoupon, couponBond) = CreateBonds();   

            ctx?.SaveChanges();
            // a bond in which discounting methodology is based on a price, returns a yield
            var pricebasedQuoteOfYield=priceMethod.GetBondPriceQuote(couponBond);

            // a bond in which discounting methodology is based on a yield, returns a price
            var yieldbasedQuoteOfPrice = yieldMethod.GetBondPriceQuote(couponBond);


            Assert.AreEqual<decimal>( 0.02666m, pricebasedQuoteOfYield.Value);
            Assert.AreEqual<decimal>( 9654.3248m,yieldbasedQuoteOfPrice.Value);

            // For required diagnostics in step (III)
            var spreadDiagnostics=yieldMethod.GetSpreadComponentsDiagnostics();
            var diagString=spreadDiagnostics.Select(sd => sd.ToString()).Aggregate(String.Empty, (total, nextValue) => total + nextValue + "\r\n");

            Assert.AreEqual(validresultString, diagString);


            DBContextCleanup(ctx, connection);


        }
    }

}