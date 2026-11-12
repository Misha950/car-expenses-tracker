# Модель даних (Data Model)

## ER-діаграма

```mermaid
erDiagram
    FuelRecord {
        int Id PK
        dateTime Date
        decimal Liters
        decimal PricePerLiter
        int Mileage
    }

    ServiceRecord {
        int Id PK
        dateTime Date
        string Type
        string Description
        decimal Cost
        int Mileage
        dateTime NextServiceDate "необов'язково"
    }

    OtherExpense {
        int Id PK
        dateTime Date
        string Type
        decimal Amount
        string Comment
    }
```

Примітка: загальна сума заправки (`TotalCost = Liters × PricePerLiter`) не зберігається в базі, а обчислюється в коді. Таблиці між собою не повʼязані: кожна зберігає окремий вид витрат.