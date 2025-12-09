# Db migrations

### Usefull commands

1. Make sure you have the EF CLI tools:
    ```cmd
   dotnet tool install --global dotnet-ef
   ```
   
2. Create new migrations using command:
    ```cmd
   dotnet ef migrations add YourMigrationName
   ```
   
3. Apply the migrations using:
    ```cmd
   dotnet ef database update
   ```

### Git rules
- **DO** commit the Migrations/ folder.
- **DO** keep migration history – it’s the evolution of the DB schema.
- **DON’T** let each dev generate their own migrations for the same change.
- **DON’T** delete old migrations unless you know exactly what you’re doing and the team agrees.