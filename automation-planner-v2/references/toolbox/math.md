# Toolbox: Math and Financial

---

## Math — ParentObject: "System.Math"

BCL static math class. Use for arithmetic, rounding, logarithms, and trigonometry.

| MethodName | Intent |
|---|---|
| `Abs` | get the absolute value of a number |
| `Sign` | get an integer indicating the sign of a number (-1, 0, 1) |
| `Ceiling` | get the smallest integer greater than or equal to a number |
| `Floor` | get the largest integer less than or equal to a number |
| `Round` | round a number to the nearest integer or specified decimal places |
| `Truncate` | get the integral part of a number, discarding the fraction |
| `Pow` | raise a number to a specified power |
| `Sqrt` | get the square root of a number |
| `Exp` | get e raised to a specified power |
| `Log` | get the natural or specified-base logarithm of a number |
| `Log10` | get the base-10 logarithm of a number |
| `Max` | get the larger of two numbers |
| `Min` | get the smaller of two numbers |
| `Sin` | get the sine of an angle in radians |
| `Cos` | get the cosine of an angle in radians |
| `Tan` | get the tangent of an angle in radians |
| `Asin` | get the angle whose sine is a given value |
| `Acos` | get the angle whose cosine is a given value |
| `Atan` | get the angle whose tangent is a given value |
| `Atan2` | get the angle whose tangent is the quotient of two numbers |
| `Sinh` | get the hyperbolic sine of an angle |
| `Cosh` | get the hyperbolic cosine of an angle |
| `Tanh` | get the hyperbolic tangent of an angle |
| `BigMul` | get the full 64-bit product of two 32-bit integers |
| `DivRem` | divide two integers and return quotient; remainder via outputs |
| `IEEERemainder` | get the IEEE 754 remainder of division of two numbers |

---

## Random — ParentObject: "Microsoft.VisualBasic.VBMath"

| MethodName | Intent |
|---|---|
| `Randomize` | seed the random number generator |
| `Rnd` | get a random single-precision number between 0 and 1 |

---

## Partition — ParentObject: "Microsoft.VisualBasic.Interaction"

| MethodName | Intent |
|---|---|
| `Partition` | get a string representing the range interval a number falls within |

---

## Financial

`ParentObject: "Microsoft.VisualBasic.Financial"` — static service.

Use for depreciation, annuity calculations, and investment analysis.

| MethodName | Display name | Intent |
|---|---|---|
| `DDB` | DoubleDecliningBalance | depreciation via double-declining balance method |
| `SLN` | StraightLineDepreciation | straight-line depreciation for a single period |
| `SYD` | SumOfYearsDigits | sum-of-years digits depreciation |
| `Pmt` | Payment | periodic payment for an annuity |
| `PPmt` | PrincipalPayment | principal payment for a given period |
| `IPmt` | InterestPayment | interest payment for a given period |
| `NPer` | NumberOfPeriods | number of periods for an annuity |
| `Rate` | InterestRatePerPeriod | interest rate per period for an annuity |
| `PV` | PresentValue | present value of an annuity |
| `FV` | FutureValue | future value of an annuity |
| `NPV` | NetPresentValue | net present value from a discount rate and cash flow array |
| `IRR` | InternalRateOfReturn | internal rate of return for periodic cash flows |
| `MIRR` | ModifiedInternalRateOfReturn | modified internal rate of return for periodic cash flows |

Use `MethodName` (left column) in `MethodStep.MethodName` — not the display name.
