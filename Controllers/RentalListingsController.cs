using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models;
using RentWisePro.Web.Services;
using RentWisePro.Web.Services.MarketData;

namespace RentWisePro.Web.Controllers
{
    [Authorize]
    public class RentalListingsController : Controller
    {
        private const int DefaultPageSize = 24;
        private const int MaxPageSize = 96;
        private readonly RentWiseProDbContext _dbContext;
        private readonly CompositeScoreCalculator _scoreCalculator;
        private readonly IGeoMarketDataLookup _marketDataLookup;

        public RentalListingsController(
            RentWiseProDbContext dbContext,
            CompositeScoreCalculator scoreCalculator,
            IGeoMarketDataLookup marketDataLookup)
        {
            _dbContext = dbContext;
            _scoreCalculator = scoreCalculator;
            _marketDataLookup = marketDataLookup;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? city,
            string? state,
            decimal? minPrice,
            decimal? maxPrice,
            int? minBedrooms,
            decimal? minBathrooms,
            decimal? minRpr,
            string? propertyType,
            string? vacancyBucket,
            string? affordabilityBucket,
            string? sortBy,
            int page = 1,
            int? pageSize = null)
        {
            var listingsQuery = _dbContext.RentalListings.AsNoTracking();
            var normalizedPageSize = NormalizePageSize(pageSize);
            var normalizedMinPrice = NormalizeNonNegative(minPrice);
            var normalizedMaxPrice = NormalizeNonNegative(maxPrice);
            var normalizedMinBedrooms = NormalizeNonNegative(minBedrooms);
            var normalizedMinBathrooms = NormalizeNonNegative(minBathrooms);
            var normalizedMinRpr = NormalizeNonNegative(minRpr);
            var normalizedVacancyBucket = NormalizeBucket(vacancyBucket);
            var normalizedAffordabilityBucket = NormalizeBucket(affordabilityBucket);

            if (normalizedMinPrice.HasValue && normalizedMaxPrice.HasValue
                && normalizedMinPrice.Value > normalizedMaxPrice.Value)
            {
                (normalizedMinPrice, normalizedMaxPrice) = (normalizedMaxPrice, normalizedMinPrice);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                listingsQuery = listingsQuery.Where(listing =>
                    (listing.StreetAddress != null && listing.StreetAddress.Contains(term)) ||
                    (listing.City != null && listing.City.Contains(term)) ||
                    (listing.State != null && listing.State.Contains(term)) ||
                    (listing.ZipCode != null && listing.ZipCode.Contains(term)) ||
                    (listing.PropertyType != null && listing.PropertyType.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityTerm = city.Trim();
                listingsQuery = listingsQuery.Where(listing =>
                    listing.City != null && listing.City.Contains(cityTerm));
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                var stateTerm = state.Trim().ToUpperInvariant();
                listingsQuery = listingsQuery.Where(listing =>
                    listing.State != null && listing.State.ToUpper() == stateTerm);
            }

            if (!string.IsNullOrWhiteSpace(propertyType))
            {
                var typeTerm = propertyType.Trim();
                listingsQuery = listingsQuery.Where(listing =>
                    listing.PropertyType != null && listing.PropertyType == typeTerm);
            }

            if (normalizedMinPrice.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Price >= normalizedMinPrice);
            }

            if (normalizedMaxPrice.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Price <= normalizedMaxPrice);
            }

            if (normalizedMinBedrooms.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Bedrooms >= normalizedMinBedrooms);
            }

            if (normalizedMinBathrooms.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Bathrooms >= normalizedMinBathrooms);
            }

            if (normalizedMinRpr.HasValue)
            {
                var minRprRatio = normalizedMinRpr.Value / 100m;
                listingsQuery = listingsQuery.Where(listing =>
                    (listing.Rpr.HasValue && listing.Rpr.Value >= minRprRatio) ||
                    (!listing.Rpr.HasValue
                     && listing.EstimatedRent.HasValue
                     && listing.Price.HasValue
                     && listing.Price.Value > 0
                     && listing.EstimatedRent.Value / listing.Price.Value >= minRprRatio));
            }

            var normalizedSortBy = NormalizeSortBy(sortBy);
            var requiresMarketFilter = !string.IsNullOrEmpty(normalizedVacancyBucket)
                || !string.IsNullOrEmpty(normalizedAffordabilityBucket)
                || normalizedSortBy == "score";

            var totalCount = 0;
            var totalPages = 1;
            var normalizedPage = 1;
            List<RentalListingCardVm> listings;

            if (requiresMarketFilter)
            {
                var listingCards = await listingsQuery
                    .Select(listing => new RentalListingCardVm
                    {
                        RentalListingId = listing.RentalListingId,
                        Zpid = listing.Zpid,
                        StreetAddress = listing.StreetAddress,
                        City = listing.City,
                        State = listing.State,
                        ZipCode = listing.ZipCode,
                        PropertyType = listing.PropertyType,
                        Price = listing.Price,
                        EstimatedMonthlyRent = listing.EstimatedRent,
                        Rpr = listing.Rpr,
                        Grm = listing.Grm,
                        CashFlow = listing.CashFlow,
                        PricePerSqft = listing.PricePerSqft,
                        Bedrooms = listing.Bedrooms,
                        Bathrooms = listing.Bathrooms,
                        ImgSrc = listing.ImgSrc,
                        IngestedAtUtc = listing.IngestedAtUtc
                    })
                    .ToListAsync();

                await EnrichListingMetricsAsync(listingCards, HttpContext.RequestAborted);

                if (!string.IsNullOrEmpty(normalizedVacancyBucket))
                {
                    listingCards = listingCards
                        .Where(listing => string.Equals(listing.VacancyBucket, normalizedVacancyBucket, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(normalizedAffordabilityBucket))
                {
                    listingCards = listingCards
                        .Where(listing => string.Equals(listing.AffordabilityBucket, normalizedAffordabilityBucket, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                listingCards = ApplyInMemorySort(listingCards, normalizedSortBy);

                totalCount = listingCards.Count;
                totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)normalizedPageSize));
                normalizedPage = Math.Clamp(page, 1, totalPages);
                listings = listingCards
                    .Skip((normalizedPage - 1) * normalizedPageSize)
                    .Take(normalizedPageSize)
                    .ToList();
            }
            else
            {
                listingsQuery = ApplySorting(listingsQuery, normalizedSortBy);
                totalCount = await listingsQuery.CountAsync();
                totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)normalizedPageSize));
                normalizedPage = Math.Clamp(page, 1, totalPages);

                listings = await listingsQuery
                    .Skip((normalizedPage - 1) * normalizedPageSize)
                    .Take(normalizedPageSize)
                    .Select(listing => new RentalListingCardVm
                    {
                        RentalListingId = listing.RentalListingId,
                        Zpid = listing.Zpid,
                        StreetAddress = listing.StreetAddress,
                        City = listing.City,
                        State = listing.State,
                        ZipCode = listing.ZipCode,
                        PropertyType = listing.PropertyType,
                        Price = listing.Price,
                        EstimatedMonthlyRent = listing.EstimatedRent,
                        Rpr = listing.Rpr,
                        Grm = listing.Grm,
                        CashFlow = listing.CashFlow,
                        PricePerSqft = listing.PricePerSqft,
                        Bedrooms = listing.Bedrooms,
                        Bathrooms = listing.Bathrooms,
                        ImgSrc = listing.ImgSrc,
                        IngestedAtUtc = listing.IngestedAtUtc
                    })
                    .ToListAsync();

                await EnrichListingMetricsAsync(listings, HttpContext.RequestAborted);
            }

            var propertyTypes = await _dbContext.RentalListings
                .AsNoTracking()
                .Where(listing => listing.PropertyType != null && listing.PropertyType != string.Empty)
                .Select(listing => listing.PropertyType!)
                .Distinct()
                .OrderBy(type => type)
                .ToListAsync();

            var viewModel = new RentalListingsIndexVm
            {
                Listings = listings,
                PropertyTypes = propertyTypes.Select(type => new SelectListItem(type, type)).ToList(),
                Search = search,
                City = city,
                State = state,
                MinPrice = normalizedMinPrice,
                MaxPrice = normalizedMaxPrice,
                MinBedrooms = normalizedMinBedrooms,
                MinBathrooms = normalizedMinBathrooms,
                MinRpr = normalizedMinRpr,
                PropertyType = propertyType,
                VacancyBucket = normalizedVacancyBucket,
                AffordabilityBucket = normalizedAffordabilityBucket,
                SortBy = normalizedSortBy,
                Page = normalizedPage,
                PageSize = normalizedPageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        private static int NormalizePageSize(int? pageSize)
        {
            if (!pageSize.HasValue || pageSize <= 0)
            {
                return DefaultPageSize;
            }

            return Math.Min(pageSize.Value, MaxPageSize);
        }

        private static decimal? NormalizeNonNegative(decimal? value)
        {
            return value.HasValue && value.Value >= 0 ? value : null;
        }

        private static int? NormalizeNonNegative(int? value)
        {
            return value.HasValue && value.Value >= 0 ? value : null;
        }

        private static string NormalizeSortBy(string? sortBy)
        {
            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "price" => "price",
                "rpr" => "rpr",
                "cashflow" => "cashflow",
                "score" => "score",
                "lastseen" => "lastseen",
                _ => "recent"
            };
        }

        private static IQueryable<Domain.Entities.RentalListing> ApplySorting(
            IQueryable<Domain.Entities.RentalListing> listingsQuery,
            string? sortBy)
        {
            return NormalizeSortBy(sortBy) switch
            {
                "price" => listingsQuery
                    .OrderByDescending(listing => listing.Price)
                    .ThenByDescending(listing => listing.IngestedAtUtc)
                    .ThenBy(listing => listing.RentalListingId),
                "rpr" => listingsQuery
                    .OrderByDescending(listing =>
                        listing.Rpr.HasValue
                            ? listing.Rpr
                            : listing.EstimatedRent.HasValue
                              && listing.Price.HasValue
                              && listing.Price.Value > 0
                                ? listing.EstimatedRent.Value / listing.Price.Value
                                : (decimal?)null)
                    .ThenByDescending(listing => listing.EstimatedRent)
                    .ThenByDescending(listing => listing.Price)
                    .ThenByDescending(listing => listing.IngestedAtUtc)
                    .ThenBy(listing => listing.RentalListingId),
                "cashflow" => listingsQuery
                    .OrderByDescending(listing => listing.CashFlow)
                    .ThenByDescending(listing => listing.EstimatedRent)
                    .ThenByDescending(listing => listing.Price)
                    .ThenByDescending(listing => listing.IngestedAtUtc)
                    .ThenBy(listing => listing.RentalListingId),
                "lastseen" => listingsQuery
                    .OrderByDescending(listing => listing.IngestedAtUtc)
                    .ThenByDescending(listing => listing.Price)
                    .ThenBy(listing => listing.RentalListingId),
                _ => listingsQuery
                    .OrderByDescending(listing => listing.IngestedAtUtc)
                    .ThenByDescending(listing => listing.Price)
                    .ThenBy(listing => listing.RentalListingId)
            };
        }

        private static List<RentalListingCardVm> ApplyInMemorySort(
            List<RentalListingCardVm> listings,
            string sortBy)
        {
            return sortBy switch
            {
                "score" => listings
                    .OrderByDescending(listing => listing.CompositeScore)
                    .ThenByDescending(listing => listing.Rpr)
                    .ThenByDescending(listing => listing.IngestedAtUtc)
                    .ToList(),
                "cashflow" => listings
                    .OrderByDescending(listing => listing.CashFlow)
                    .ThenByDescending(listing => listing.EstimatedMonthlyRent)
                    .ThenByDescending(listing => listing.IngestedAtUtc)
                    .ToList(),
                "rpr" => listings
                    .OrderByDescending(listing => listing.Rpr)
                    .ThenByDescending(listing => listing.EstimatedMonthlyRent)
                    .ThenByDescending(listing => listing.IngestedAtUtc)
                    .ToList(),
                "price" => listings
                    .OrderByDescending(listing => listing.Price)
                    .ThenByDescending(listing => listing.IngestedAtUtc)
                    .ToList(),
                "lastseen" => listings
                    .OrderByDescending(listing => listing.IngestedAtUtc)
                    .ThenByDescending(listing => listing.Price)
                    .ToList(),
                _ => listings
                    .OrderByDescending(listing => listing.IngestedAtUtc)
                    .ThenByDescending(listing => listing.Price)
                    .ToList()
            };
        }

        private async Task EnrichListingMetricsAsync(
            List<RentalListingCardVm> listingCards,
            CancellationToken cancellationToken)
        {
            if (listingCards.Count == 0)
            {
                return;
            }

            var listingKeys = listingCards
                .Select(listing => new RentalListingMarketKey(listing.RentalListingId, listing.City, listing.State, listing.ZipCode))
                .ToList();

            var marketMetrics = await _marketDataLookup.GetMetricsAsync(listingKeys, cancellationToken);

            foreach (var listing in listingCards)
            {
                GeoMarketMetrics? metrics = null;
                if (marketMetrics.TryGetValue(listing.RentalListingId, out var foundMetrics))
                {
                    metrics = foundMetrics;
                    listing.VacancyRate = metrics.VacancyRate;
                    listing.AffordabilityIndex = metrics.AffordabilityIndex;
                    listing.FairMarketRent = metrics.FairMarketRent;
                }

                if (!listing.Rpr.HasValue
                    && listing.Price.HasValue
                    && listing.Price.Value > 0
                    && listing.EstimatedMonthlyRent.HasValue)
                {
                    listing.Rpr = listing.EstimatedMonthlyRent.Value / listing.Price.Value;
                }

                if (!listing.Grm.HasValue
                    && listing.Price.HasValue
                    && listing.EstimatedMonthlyRent.HasValue
                    && listing.EstimatedMonthlyRent.Value > 0)
                {
                    listing.Grm = listing.Price.Value / (listing.EstimatedMonthlyRent.Value * 12m);
                }

                if (listing.Rpr.HasValue)
                {
                    listing.RentToPriceRatioMonthly = listing.Rpr.Value;
                    listing.RentToPriceRatioMonthlyPct = listing.Rpr.Value.ToString("0.00%");
                }

                listing.VacancyBucket = GetVacancyBucket(listing.VacancyRate);
                listing.VacancyBadgeClass = GetVacancyBadgeClass(listing.VacancyBucket);
                listing.VacancyBucketLabel = GetBucketLabel(listing.VacancyBucket);
                listing.AffordabilityBucket = GetAffordabilityBucket(listing.AffordabilityIndex);
                listing.AffordabilityBucketLabel = GetBucketLabel(listing.AffordabilityBucket);

                if (listing.EstimatedMonthlyRent.HasValue
                    && listing.FairMarketRent.HasValue
                    && listing.FairMarketRent.Value > 0)
                {
                    listing.RentVsFmrDeltaPct =
                        (listing.EstimatedMonthlyRent.Value - listing.FairMarketRent.Value)
                        / listing.FairMarketRent.Value;
                }

                var scoreResult = _scoreCalculator.Calculate(new CompositeScoreInputs(
                    listing.Rpr,
                    listing.EstimatedMonthlyRent,
                    listing.FairMarketRent,
                    listing.VacancyRate,
                    listing.AffordabilityIndex,
                    listing.PricePerSqft,
                    metrics?.MedianPricePerSqft,
                    listing.PropertyType));

                listing.CompositeScore = scoreResult.Score;
                listing.CompositeScoreVersion = scoreResult.Version;
                listing.CompositeScoreBreakdown = scoreResult.Breakdown;
                listing.CompositeScoreTooltip = CompositeScoreCalculator.BuildTooltip(scoreResult);
            }
        }

        private static string? NormalizeBucket(string? bucket)
        {
            return bucket?.Trim().ToLowerInvariant() switch
            {
                "low" => "low",
                "medium" => "medium",
                "high" => "high",
                _ => null
            };
        }

        private static string? GetVacancyBucket(decimal? vacancyRate)
        {
            if (!vacancyRate.HasValue)
            {
                return null;
            }

            if (vacancyRate.Value <= 5m)
            {
                return "low";
            }

            if (vacancyRate.Value <= 9m)
            {
                return "medium";
            }

            return "high";
        }

        private static string? GetAffordabilityBucket(decimal? affordabilityIndex)
        {
            if (!affordabilityIndex.HasValue)
            {
                return null;
            }

            if (affordabilityIndex.Value >= 110m)
            {
                return "high";
            }

            if (affordabilityIndex.Value >= 90m)
            {
                return "medium";
            }

            return "low";
        }

        private static string? GetVacancyBadgeClass(string? bucket)
        {
            return bucket switch
            {
                "low" => "bg-success",
                "medium" => "bg-warning text-dark",
                "high" => "bg-danger",
                _ => null
            };
        }

        private static string? GetBucketLabel(string? bucket)
        {
            return bucket switch
            {
                "low" => "Low",
                "medium" => "Medium",
                "high" => "High",
                _ => null
            };
        }
    }
}
