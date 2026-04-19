# MathieuShop

Приложение на `Avalonia` для лавки «Матье» с ролями клиента, администратора, модератора и мастера.

## Что реализовано

- авторизация и регистрация клиента;
- каталог услуг с поиском, фильтрацией, пагинацией и сортировкой;
- пополнение баланса;
- запись на выбранную услугу к выбранному мастеру с получением номера в очереди;
- отзывы по услугам и мастерам;
- редактирование услуг модератором;
- привязка и отвязка мастеров к услугам;
- повышение квалификации мастеров;
- редактирование пользователей и добавление сотрудников администратором;
- просмотр записей мастером и отправка заявки на повышение квалификации;
- фиксация даты и времени последнего изменения услуги;
- unit-тесты для логики последнего изменения.

## Технологии

- `.NET 8`
- `Avalonia 11.3.6`
- `CommunityToolkit.Mvvm`
- `Entity Framework Core 8`
- `PostgreSQL / Npgsql`

## Запуск приложения

1. При необходимости скорректируйте строку подключения в `MathieuShop.Avalonia/appsettings.json`.
2. Соберите решение:

```powershell
dotnet build .\MathieuShop.sln
```

3. Запустите приложение:

```powershell
dotnet run --project .\MathieuShop.Avalonia\MathieuShop.Avalonia.csproj
```

## Подготовка PostgreSQL

В архив уже добавлены готовые файлы для базы данных:

- `database/Setup-PostgreSQL.cmd` — запуск настройки в один клик;
- `database/Setup-PostgreSQL.ps1` — настройка через PowerShell;
- `database/MathieuShop_PostgreSQL_Schema.sql` — SQL-скрипт схемы для `pgAdmin`;
- `database/README_PostgreSQL.md` — отдельная инструкция по PostgreSQL.

Быстрый вариант:

```powershell
.\database\Setup-PostgreSQL.ps1 -Host localhost -Port 5432 -Database mathieu_shop -Username postgres -Password ваш_пароль
```

После этого база будет создана, миграции применены, а демонстрационные данные добавлены автоматически.

## Демо-аккаунты

- `client / demo123`
- `moderator / demo123`
- `admin / demo123`
- `master1 / demo123`

## Дополнительные материалы

- пользовательская инструкция: `docs/UserGuide.md`
- спецификация: `docs/Specification.md`
- тест-кейсы: `docs/TestCases.md`
- диаграммы в текстовом виде: `docs/Diagrams.md`
