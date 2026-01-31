using System;

namespace RentWisePro.Web.Services.Finance
{
    public static class FinanceMath
    {
        public static decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termYears)
        {
            if (principal <= 0 || termYears <= 0)
            {
                return 0m;
            }

            var totalPayments = termYears * 12;
            if (annualInterestRate <= 0)
            {
                return principal / totalPayments;
            }

            var monthlyRate = (double)(annualInterestRate / 100m / 12m);
            var factor = Math.Pow(1 + monthlyRate, totalPayments);
            var payment = (double)principal * (monthlyRate * factor) / (factor - 1);

            return (decimal)payment;
        }
    }
}
