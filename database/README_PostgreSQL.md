# Настройка PostgreSQL для MathieuShop

## Основной способ

1. Откройте файл `MathieuShop.Avalonia/appsettings.json`.
2. Укажите свои параметры PostgreSQL:
   - `Host`
   - `Port`
   - `Database`
   - `Username`
   - `Password`
3. Запустите файл `database/Setup-PostgreSQL.cmd`.

## Быстрый запуск через PowerShell

```powershell
.\database\Setup-PostgreSQL.ps1 -Host localhost -Port 5432 -Database mathieu_shop -Username postgres -Password ваш_пароль
```

## Что делает настройка

- создаёт базу данных PostgreSQL, если её ещё нет;
- применяет миграции `Entity Framework Core`;
- добавляет демонстрационные данные.

## После настройки

После завершения настройки можно запускать приложение обычным способом.

## Файлы в папке `database`

- `MathieuShop_PostgreSQL_Schema.sql` — SQL-скрипт схемы для `pgAdmin` или `Query Tool`;
- `MathieuShop_PostgreSQL_Full.sql` — полный SQL-скрипт структуры и демонстрационных данных;
- `Setup-PostgreSQL.ps1` — настройка базы через PowerShell;
- `Setup-PostgreSQL.cmd` — запуск настройки в один клик для Windows.
