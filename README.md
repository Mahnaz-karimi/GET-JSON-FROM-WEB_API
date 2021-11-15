
## for creating table in microsoft sql database

CREATE TABLE ValutaKurser (
    id int IDENTITY(1,1) PRIMARY KEY,
    FromCurrency varchar(255) NOT NULL,
    ToCurrency varchar(255) NOT NULL,
    Rate numeric(25, 15) NOT NULL,
    UpdatedAt Datetime2 NOT NULL
);
