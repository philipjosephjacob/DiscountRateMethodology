using System;

namespace KrollDiscounting.Entities
{
    public class SpreadComponentDiagnostic
    {
        public SpreadComponentDiagnostic(string componentName, decimal val)
        {
            ComponentName = componentName;
            Val = val;
        }

        public string ComponentName { get; set; }
        public decimal Val { get; set; }

        public override string ToString()
        {
            return $"{ComponentName}: {Val}";
        }
    }
}