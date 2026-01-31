USE RentWisePro;
GO

INSERT INTO dbo.RentalListings
(
  
	Zpid,
    StreetAddress,
    City,
    State,
    ZipCode,
    County,
    PropertyType,
    Bedrooms,
    Bathrooms,
    Price,
    EstimatedRent,
    ImgSrc,
    SourceSystem,
    IngestedAtUtc
)
SELECT
    CAST(A.Zpid AS varchar(64)) as Zpid,
    A.StreetAddress,
    A.City,
    A.State,
    A.ZipCode,
    A.County,
    A.PropertyType,
    CAST(A.Bedrooms as int),
    CAST(A.Bathrooms as decimal(5,2)),
    CAST(A.Price as decimal(18,2)),
    CAST(A.EstimatedRent as decimal(18,2)),
    A.ImgSrc,
    'Zillow' as SourceSystem,
    SYSUTCDATETIME() as IngestedAtUtc
FROM RentalAnalyzerDB.dbo.RentalListings A
WHERE A.Zpid IS NOT NULL;
