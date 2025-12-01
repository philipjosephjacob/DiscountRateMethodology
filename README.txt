Description of solution

There are two sets of primary entities:

The abstract  DiscountMethodologyAbstract base class is the primary entity of interest in the solution.
This entity sets up a methodology by which one may estimate the discount yield of a bond. The yield may be drived from two primary sources:
- from a price
- from a set of sub-component yields as outlined in the equation in the exercise description.

An abstract method specifies that a quote is produced by applying the methodology to a Bond.


A Bond is created purely as a vehicle to which to apply the main entity in the solution.

A base abstract class Bond is defined with a few elementary properties such as identifiers and an IssueDate.
From that we derive the notion of a bond with credit, namely a an abstract CorporateBond (ignore philosophical questions such as the existence of credit risk in sovereign issuance).

From CorporateBond two concrete classes are derived which model the cases of a bond with coupons and a Zero.

There are two forms of Quote:
- A yield based quote contains a price, and is calculated from the parts of a components of a yield as specified in the Example Discount Rate Methodology in the requirements.
- A price based quote, contains a yield, and is calculated from a bond price.

Use of EntityFramework

EntityFramework is used to persist Bonds and DiscountMethodologies. Certain other entities such as those used to represent diagnostic information are not persisted since their creation is idempotent and they are relatively cheap to create.

To run the solution, run the unit test Kroll.Discounting.Tests

The solution was build using Visual Studio 2026 Community Edition. Certain comments were generated using Co-pilot.
