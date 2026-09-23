UML: діаграма компонентів та архітектури

Застосунок побудований за архітектурним патерном MVVM (Model-View-ViewModel), 
що розділяє інтерфейс, логіку взаємодії та дані на окремі шари.

```mermaid
graph TD
    View["View<br/>(.axaml екрани)"] -->|data binding| ViewModel["ViewModel<br/>(логіка екрану)"]
    ViewModel -->|викликає| Services["Services<br/>(логування, розрахунки)"]
    ViewModel -->|читає/пише| Data["Data (DbContext)<br/>(Entity Framework Core)"]
    Data -->|зберігає в| DB[("SQLite<br/>(файл бази даних)")]
```

Опис компонентів

View — відповідає лише за відображення інтерфейсу (кнопки, таблиці, 
форми). Не містить бізнес-логіки.

ViewModel — містить логіку конкретного екрана: обробляє дії користувача, 
готує дані для відображення, звертається до Services та Data.

Services — допоміжна логіка, що не належить конкретному екрану: 
журналювання дій, розрахунки (наприклад, витрата пального л/100км).

Data (DbContext) — шар доступу до бази даних через Entity Framework 
Core: збереження, читання, оновлення та видалення записів.

SQLite — файлова база даних, у якій фізично зберігаються всі дані 
застосунку локально на пристрої користувача.

Схема основних потоків взаємодії

Сценарій: додавання нового запису (наприклад, заправки)

```mermaid
sequenceDiagram
    actor User as Користувач
    participant V as View
    participant VM as ViewModel
    participant S as Services
    participant D as Data (DbContext)
    participant DB as SQLite

    User->>V: Натискає "Додати заправку"
    V->>VM: Відкрити форму додавання
    User->>V: Вводить дані та натискає "Зберегти"
    V->>VM: Передає введені дані
    VM->>VM: Перевіряє коректність (валідація)
    VM->>D: Зберегти новий запис
    D->>DB: INSERT запису
    DB-->>D: Підтвердження збереження
    D-->>VM: Успіх
    VM->>S: Записати дію в лог
    VM-->>V: Оновити список заправок
    V-->>User: Показати оновлений список
```

Сценарій: перегляд графіка витрат

```mermaid
sequenceDiagram
    actor User as Користувач
    participant V as View
    participant VM as ViewModel
    participant D as Data (DbContext)
    participant DB as SQLite

    User->>V: Відкриває екран "Звіти"
    V->>VM: Запит на завантаження даних
    VM->>D: Отримати всі записи за період
    D->>DB: SELECT записів
    DB-->>D: Повертає дані
    D-->>VM: Список записів
    VM->>VM: Групує дані по категоріях
    VM-->>V: Передає дані для графіка
    V-->>User: Відображає графік
```