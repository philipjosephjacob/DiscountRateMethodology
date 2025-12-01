using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace KrollDiscounting.Entities
{

    [Table("BenchmarkSpreads")]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(AvgBenchmarkSpread), typeDiscriminator: "avgBenchmarkSpread")]
    [JsonDerivedType(typeof(MedianBenchmarkSpread), typeDiscriminator: "medianBenchmarkSpread")]
    public abstract class BenchmarkSpreadAbstract : IEquatable<BenchmarkSpreadAbstract>
    {
        [Key, JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public Guid? DiscountRateMethdologyAbstractId { get; set; }

        public YieldBasedDiscountRateMethodology? DiscountRateMethodology { get; set; } = null!;
        public abstract decimal GetSpread();

        public SpreadComponentDiagnostic GetSpreadComponent()
        {
            string typenname = this.GetType().Name;
            var spreadComponentDiagnostic = new SpreadComponentDiagnostic(typenname, GetSpread());
            return spreadComponentDiagnostic;
        }

        public virtual bool Equals(BenchmarkSpreadAbstract other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return DiscountRateMethdologyAbstractId == other.DiscountRateMethdologyAbstractId ;
        }

        public override bool Equals(object obj)
        {
             return Equals(obj as BenchmarkSpreadAbstract);
        }


        public override int GetHashCode()
        {

            return HashCode.Combine(DiscountRateMethdologyAbstractId);
        }
        public static bool operator ==(BenchmarkSpreadAbstract left, BenchmarkSpreadAbstract right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(BenchmarkSpreadAbstract left, BenchmarkSpreadAbstract right)
        {
            return !(left == right);
        }
    }

    public abstract class NonZeroBenchmarkSpread : BenchmarkSpreadAbstract, IEquatable<NonZeroBenchmarkSpread>
    {
        public override decimal GetSpread()
        {
            return SpreadPct;
        }
        public decimal SpreadPct { get; set; }

        public virtual bool Equals(NonZeroBenchmarkSpread other)
        {
            if (other is null) return false;

            if (!base.Equals(other))
            {
                return false;
            }
            if (this.GetType() != other.GetType())
            {
                return false;
            }
            return GetSpread() == other.GetSpread();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NonZeroBenchmarkSpread);
        }


        public override int GetHashCode()
        {

            return HashCode.Combine(base.GetHashCode(), SpreadPct);
        }
    }

    public class ZeroBenchmarkSpread : BenchmarkSpreadAbstract   
    {
        public override decimal GetSpread()
        {
            return 0.0m;
        }



    }



    public class AvgBenchmarkSpread : NonZeroBenchmarkSpread
    {
        // Intentionally empty — extend with domain-specific properties/behavior if needed
    }

    public class MedianBenchmarkSpread : NonZeroBenchmarkSpread
    {
        // Intentionally empty — extend with domain-specific properties/behavior if needed
    }
}