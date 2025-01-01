using SQLite;
using DreamCatcher.Models;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DreamCatcher{
    public class DreamDataRepo{
        SQLiteAsyncConnection Database;

        public const string DatabaseFilename = "DreamSQLite.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(DatabasePath, Flags);
            var result = await Database.CreateTableAsync<Dream>();
        }


        public async Task AddNewDream(Dream dream){
            int result = 0;
            string StatusMessage = string.Empty;
            try
            {
      
                await Init();

                // basic validation to ensure a name was entered
                if (dream.DreamType == 0)
                    throw new Exception("Valid dream required");

                result = await Database.InsertAsync(dream);

                StatusMessage = string.Format("{0} record(s) added ", result);
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Failed to add. Error: {0}", ex.Message);
                Debug.WriteLine(StatusMessage);
            }
        }

        public async Task ModifyDream(Dream dream)
        {
            try
            {
                await Init();
                if (dream.Id != 0)
                {
                    await Database.UpdateAsync(dream);
                }
                else
                    throw new Exception("Object doesn't exist");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task DeleteDream(Dream dream){
            try
            {
                await Init();
                if (dream.Id != 0)
                {
                    await Database.DeleteAsync(dream);
                }
                else
                    throw new Exception("Object doesn't exist");
            }catch(Exception ex){
                Debug.WriteLine(ex.Message);
            }
        }
        public async Task<List<Dream>> GetAllDreams()
        {
            await Init();
            return await Database.Table<Dream>().ToListAsync();
        }
        public async Task<List<Dream>> GetDreamsBetweenDate(DateTime from_date,DateTime to_date){
            to_date = to_date.AddDays(1);
            var dreamsInRange = (await GetAllDreams())
                                .Where(d => d.Time >= from_date && d.Time <= to_date)
                                .ToList();

            return dreamsInRange;
        }
    }
}