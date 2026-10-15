# Модель даних (Data Model)

## ER-діаграма

```mermaid
erDiagram
    FuelRecord {
        int Id PK
        dateTime Date
        decimal Liters
        decimal PricePerLiter
        decimal TotalCost
        int Odometer
        string Notes
    }

    ServiceRecord {
        int Id PK
        dateTime Date
        string ServiceType
        decimal TotalCost
        int Odometer
        string Description
    }

    OtherExpense {
        int Id PK
        dateTime Date
        string Category
        decimal TotalCost
        string Notes
    }
