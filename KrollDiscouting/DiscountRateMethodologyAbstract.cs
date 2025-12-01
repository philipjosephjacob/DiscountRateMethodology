using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static KrollDiscounting.Entities.YieldQuote;

namespace KrollDiscounting.Entities
{


    public abstract class QuoteAbstract : IEquatable<QuoteAbstract>
    {

        /// <summary>
        /// prcision governed by DiscountRateMethodology
        /// </summary>
        public decimal Value { get; init; }

        public QuoteAbstract(decimal value)
        {
            Value = value;
        }

       
        public virtual bool Equals(QuoteAbstract other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return Value == other.Value ;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as QuoteAbstract);
        }

        public override int GetHashCode()
        {

            return HashCode.Combine(Value);
        }
    }

    public class YieldQuote : QuoteAbstract, IEquatable<YieldQuote>
    {
        public YieldQuote(YieldTypeEnum yieldTypeEnum, decimal value):base(value)
        {
            YieldType = yieldTypeEnum;
   
        }
        public enum YieldTypeEnum
        {
            YTM,
            YTC,
            YTW
        }

        public YieldTypeEnum YieldType { get; init; }

        public bool Equals(YieldQuote other)
        {
            if (!base.Equals(other))
                return false;


            return YieldType == other.YieldType;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as YieldQuote);
        }

        public override int GetHashCode()
        {

            return HashCode.Combine(base.GetHashCode(), YieldType);
        }

    }

    public class PriceQuote : QuoteAbstract, IEquatable<PriceQuote>
    {
        public PriceQuote(string currency, decimal value) : base(value)
        {
            Currency= currency;

        }
        public string Currency { get; init; }

        public bool Equals(PriceQuote other)
        {
            if (!base.Equals(other))
                return false;


            return Currency == other.Currency;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PriceQuote);
        }

        public override int GetHashCode()
        {

            return HashCode.Combine(base.GetHashCode(), Currency);
        }
    }

    [Table("DiscountRateMethodologies")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(PriceBasedDiscountRateMethodology), typeDiscriminator: "priceBasedDiscountRateMethodology")]
    [JsonDerivedType(typeof(YieldBasedDiscountRateMethodology), typeDiscriminator: "yieldBasedDiscountRateMethodology")]
    public abstract class DiscountRateMethdologyAbstract: IEquatable<DiscountRateMethdologyAbstract>
    {
        /// <summary>
        /// ID, not exposed for export
        /// </summary>
        [Key,JsonIgnore]
        public Guid Id { get; set; }

        /// <summary>
        /// External Identifier
        /// </summary>
        [MaxLength(200), Required]
        public required string Name { get; set; }

        public int? Precision { get; set; }


        public int GetApplicablePrecision()
        {
            return Precision==null ? 2 : (int)Precision;
        }

        /// <summary>
        /// Abstract method to get bond price quote based on the discount rate methodology
        /// </summary>
        /// <param name="bond"></param>
        /// <returns></returns>
        public abstract QuoteAbstract GetBondPriceQuote(Bond bond);

        public virtual List<SpreadComponentDiagnostic> GetSpreadComponentsDiagnostics()
        {
            var diagnostics = new List<SpreadComponentDiagnostic>();
            return diagnostics;
        }

        /// <summary>
        /// Export the entire object graph to JSON string as required in task (I)
        /// </summary>
        /// <returns>string with Json object</returns>
        public string ExportObjectGraph()
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve, // Handles cycles and duplicate references
                WriteIndented = true // Makes the JSON human-readable
            };

            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }

        public static DiscountRateMethdologyAbstract? ImportObjectGraph(string jsonString)

        {

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve, // Handles cycles and duplicate references
                WriteIndented = true // Makes the JSON human-readable
            };

            return JsonSerializer.Deserialize<DiscountRateMethdologyAbstract>(jsonString, options); 
        }

        /// Implementation of equality

        /// <summary>
        /// Do not check ID since DB specific
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(DiscountRateMethdologyAbstract other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return Precision == other.Precision && Name == other.Name;
        }


        public override bool Equals(object obj)
        {

            return Equals(obj as DiscountRateMethdologyAbstract);
        }


        public override int GetHashCode()
        {

            return HashCode.Combine(Precision, Name);
        }


    }

    /// <summary>
    /// Concrete implementation of price based discount rate methodology
    /// </summary>
    public class PriceBasedDiscountRateMethodology : DiscountRateMethdologyAbstract, IEquatable<PriceBasedDiscountRateMethodology>
    {
        /// <summary>
        /// Field conveying price quote as fraction of par (eg. 0.95 )
        /// </summary>
        public double PriceAsFracOfPar { get; set; }

        public override List<SpreadComponentDiagnostic> GetSpreadComponentsDiagnostics()
        {

            var diags = base.GetSpreadComponentsDiagnostics();

            /// todo: add price based diagnostics
            return diags;

        }
        public override QuoteAbstract GetBondPriceQuote(Bond bond)
        {
            var ytmFromPrice=bond.GetYTMFromPrice(PriceAsFracOfPar, bond.IssueDate);
            

            decimal ytmdec= Math.Round((decimal)ytmFromPrice, GetApplicablePrecision());
            return new YieldQuote(YieldQuote.YieldTypeEnum.YTM, ytmdec);
        }

        public bool Equals(PriceBasedDiscountRateMethodology other)
        {
            if (other is null) return false;

            if (!base.Equals(other))
            {
                return false;
            }


            return PriceAsFracOfPar == other.PriceAsFracOfPar;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PriceBasedDiscountRateMethodology);
        }

        public override int GetHashCode()
        {
           
            return HashCode.Combine(base.GetHashCode(), PriceAsFracOfPar);
        }
    }
    /// <summary>
    /// Concrete implementation of yield based discount rate methodology
    /// </summary>
    public class YieldBasedDiscountRateMethodology : DiscountRateMethdologyAbstract, IEquatable<YieldBasedDiscountRateMethodology>
    {
        /// <summary>
        /// Risk free rate percentage. Uses decimal(5,2) to match real-world conventions. Internal calculations convert to fraction.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal RiskFreeRatePct { get; set; }

        public required BenchmarkSpreadAbstract BenchmarkSpread { get; set; }

        /// <summary>
        /// Credit Spread percentage. Uses decimal(5,2) to match real-world conventions. Internal calculations convert to fraction.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? CreditSpreadPct { get; set; }

        /// <summary>
        /// Asset-type Spread percentage. Uses decimal(5,2) to match real-world conventions.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AssetTypeSpreadPct { get; set; }

        public override List<SpreadComponentDiagnostic> GetSpreadComponentsDiagnostics()
        {

            var diagnostics = base.GetSpreadComponentsDiagnostics();

       
            diagnostics.Add(BenchmarkSpread.GetSpreadComponent());

            if (CreditSpreadPct.HasValue)
            {
                diagnostics.Add(new SpreadComponentDiagnostic("CreditSpread", CreditSpreadPct.Value));
            }
            if (AssetTypeSpreadPct.HasValue)
            {
                diagnostics.Add(new SpreadComponentDiagnostic("AssetTypeSpread", AssetTypeSpreadPct.Value));
            }
            return diagnostics;
        }

        /// <summary>
        /// Computed effective total yield as fraction given the risk-free rate and spreads
        /// </summary>
        public double EffectiveTotalYieldFrac
        {
            get
            {
                double totalSpread = (double)RiskFreeRatePct;

                totalSpread += (double)BenchmarkSpread.GetSpread();

                if (CreditSpreadPct.HasValue)
                {
                    totalSpread += (double)CreditSpreadPct.Value;
                }
                if (AssetTypeSpreadPct.HasValue)
                {
                    totalSpread += (double)AssetTypeSpreadPct.Value;
                }
                return totalSpread/100.0;
            }
        }
        public override QuoteAbstract GetBondPriceQuote(Bond bond)
        {
            var price=bond.PriceBondFromYTM(EffectiveTotalYieldFrac, bond.IssueDate);
            
            decimal roundedPrice = Math.Round((decimal)price, GetApplicablePrecision());
            
            return new PriceQuote(bond.Currency ?? "USD", roundedPrice);
        }

        /// Equality implementations


        public virtual bool Equals(YieldBasedDiscountRateMethodology other)
        {
            if (other is null) return false;

            if (!base.Equals(other))
            {
                return false;
            }


            return RiskFreeRatePct == other.RiskFreeRatePct && 
                   BenchmarkSpread== other.BenchmarkSpread &&
                   CreditSpreadPct == other.CreditSpreadPct && 
                   AssetTypeSpreadPct == other.AssetTypeSpreadPct &&
                   Precision == other.Precision;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as YieldBasedDiscountRateMethodology);
        }

        public override int GetHashCode()
        {

            return HashCode.Combine(base.GetHashCode(), RiskFreeRatePct, BenchmarkSpread, CreditSpreadPct, AssetTypeSpreadPct);
        }
    }

   
}