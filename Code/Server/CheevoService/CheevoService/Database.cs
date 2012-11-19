using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CheevoService
{
    class Database
    {
        private static SQLiteConnection sqliteCon = new SQLiteConnection(Properties.Settings.Default.ConnectionString);

        public static void Setup()
        { 
            /*
            CREATE TABLE available_cheevos
            (
              ID int NOT NULL,
              Title varchar(255) NOT NULL,
              Description varchar(1024) NOT NULL,
              Category varchar(255) NOT NULL,
              Points int NOT NULL,
              CreatedTime datetime NOT NULL,
              PRIMARY KEY(Cheevo_Id)
            )
             */
            /*
            CREATE TABLE popped_cheevos
            (
              Pop_Id int NOT NULL,
              ProposedTime datetime NOT NULL,
              AwardedTime datetime,
              User varchar(255) NOT NULL,
              Cheevo_Id int NOT NULL,
              FirstModerator varchar(255),
              SecondModerator varchar(255),
              PRIMARY KEY(Pop_Id),
              FOREIGN KEY(Cheevo_Id) REFERENCES available_cheevos(Cheevo_Id)
            )
             */

            // Define the SQL Create table statement
            const string createACTable = "CREATE TABLE [available_cheevos] (ID INTEGER primary key, Title TEXT NOT NULL, Description TEXT NOT NULL, Category TEXT NOT NULL, Points int NOT NULL, CreatedTime INTEGER  default current_timestamp, UNIQUE (Title))";
            const string createPCTable = "CREATE TABLE [popped_cheevos] (ID INTEGER primary key, CheevoID int NOT NULL, User TEXT NOT NULL, ProposedTime INTEGER NOT NULL, AwardedTime INTEGER , FirstMod TEXT NOT NULL, SecondMod TEXT, FOREIGN KEY(CheevoID) REFERENCES available_cheevos(ID) )";

            lock (sqliteCon)
            {
                sqliteCon.Open();

                using (SQLiteTransaction sqlTransaction = sqliteCon.BeginTransaction())
                using (SQLiteCommand createACCommand = new SQLiteCommand(createACTable, sqliteCon, sqlTransaction))
                using (SQLiteCommand createPCCommand = new SQLiteCommand(createPCTable, sqliteCon, sqlTransaction))
                {
                    createACCommand.ExecuteNonQuery();
                    createPCCommand.ExecuteNonQuery();
                    sqlTransaction.Commit();
                }
                sqliteCon.Close();
            }
        }

        public static void AddCheevo(string title, string description, string category, int points)
        {
            var addCmd = "INSERT INTO available_cheevos(ID, Title, Description, Category, Points) VALUES(NULL, @title, @description, @category, @points)";

            bool dbOpened = false;

            lock (Database.sqliteCon)
            {
                try
                {
                    Database.sqliteCon.Open();
                    dbOpened = true;

                    using (SQLiteTransaction sqlTransaction = sqliteCon.BeginTransaction())
                    using (SQLiteCommand addCommand = new SQLiteCommand(addCmd, sqliteCon, sqlTransaction))
                    {
                        addCommand.Parameters.AddWithValue("@title", title );
                        addCommand.Parameters.AddWithValue("@description", description );
                        addCommand.Parameters.AddWithValue("@category", category );
                        addCommand.Parameters.AddWithValue("@points", points );

                        try
                        {
                            addCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();
                        }
                        catch
                        {
                            // do nothing, cheevo already added
                        }
                    }     
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                }
                finally
                {
                    if (dbOpened)
                    {
                        Database.sqliteCon.Close();
                    }
                }
            }
        }

        public static IEnumerable<Cheevo> GetCheevos()
        {
            List<Cheevo> cheevos = new List<Cheevo>();

            /*
            CREATE TABLE available_cheevos
            (
              ID int NOT NULL,
              Title varchar(255) NOT NULL,
              Description varchar(1024) NOT NULL,
              Category varchar(255) NOT NULL,
              Points int NOT NULL,
              CreatedTime datetime NOT NULL,
              PRIMARY KEY(Cheevo_Id)
            )
             */

            const string loadCheevos = "select Title,Description,Category,Points,ID from available_cheevos";
            bool dbOpened = false;

            lock (Database.sqliteCon)
            {
                try
                {
                    Database.sqliteCon.Open();
                    dbOpened = true;

                    using (SQLiteCommand selectCommand = new SQLiteCommand(loadCheevos, Database.sqliteCon))
                    using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                    {
                        while (dataReader.HasRows && dataReader.Read())
                        {
                            cheevos.Add(new Cheevo(dataReader.GetString(0), dataReader.GetString(1), dataReader.GetString(2), dataReader.GetInt32(3), DateTime.MinValue, dataReader.GetInt32(4)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                }
                finally
                {
                    if (dbOpened)
                    {
                        Database.sqliteCon.Close();
                    }
                }
            }
            return cheevos;
        }

        public static Dictionary<string, CheevoUser> LoadPoppedCheevos()
        {
            var dbCheevos = new Dictionary<string, CheevoUser>();

            const string loadCheevos = "select Title,Description,Category,Points,AwardedTime,User,available_cheevos.ID " +
                                       "from popped_cheevos inner join available_cheevos on available_cheevos.ID = popped_cheevos.ID where AwardedTime is not null";

            bool dbOpened = false;

            lock (Database.sqliteCon)
            {
                try
                {
                    Database.sqliteCon.Open();
                    dbOpened = true;

                    using (SQLiteCommand selectCommand = new SQLiteCommand(loadCheevos, Database.sqliteCon))
                    using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                    {
                        while (dataReader.HasRows && dataReader.Read())
                        {
                            var myDate = new DateTime(dataReader.GetInt64(4));
                            var user = new CheevoUser(dataReader.GetString(5));

                            if (!dbCheevos.ContainsKey(user.Username.ToLower()))
                            {
                                dbCheevos.Add(user.Username.ToLower(), user);
                            }

                            dbCheevos[user.Username.ToLower()].Add(
                                new Cheevo(
                                    dataReader.GetString(0), 
                                    dataReader.GetString(1), 
                                    dataReader.GetString(2), 
                                    dataReader.GetInt32(3), 
                                    myDate, 
                                    dataReader.GetInt32(6)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                }
                finally
                {
                    if (dbOpened)
                    {
                        Database.sqliteCon.Close();
                    }
                }
            }
            return dbCheevos;
        }

        static bool CheevoAlreadyProposed(string user, int id)
        {
            bool ret = false;
            string loadCheevos = "select ID from popped_cheevos where CheevoID = @id and User = @user and SecondMod is null";

            bool dbOpened = false;

            lock (Database.sqliteCon)
            {
                try
                {
                    Database.sqliteCon.Open();
                    dbOpened = true;

                    using (SQLiteCommand selectCommand = new SQLiteCommand(loadCheevos, Database.sqliteCon))
                    {
                        selectCommand.Parameters.AddWithValue("@id", id);
                        selectCommand.Parameters.AddWithValue("@user", user);

                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            ret = dataReader.HasRows;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                }
                finally
                {
                    if (dbOpened)
                    {
                        Database.sqliteCon.Close();
                    }
                }
            }
            return ret;
        }

        public static bool ProposeCheevo(string user, string proposes, int id)
        {
            bool ret = false;
            bool dbOpened = false;

            // complete it
            const string completeIt = "update popped_cheevos set SecondMod = @user, AwardedTime = @awarded where CheevoID = @cheevoID and User = @proposes";

            // add it
            const string addIt = "insert into popped_cheevos (ID, CheevoID, User, ProposedTime, FirstMod) VALUES (NULL, @id, @proposes, @time, @user)";

            lock (sqliteCon)
            {
                try
                {
                    string cmd;
                    if (CheevoAlreadyProposed(proposes, id))
                    {
                        cmd = completeIt;
                    }
                    else
                    {
                        cmd = addIt;
                    }

                    sqliteCon.Open();
                    dbOpened = true;

                    using (SQLiteTransaction sqlTransaction = sqliteCon.BeginTransaction())
                    {
                        using (SQLiteCommand command = new SQLiteCommand(cmd, sqliteCon, sqlTransaction))
                        {
                            if (cmd == completeIt)
                            {
                                command.Parameters.AddWithValue("@user", user);
                                command.Parameters.AddWithValue("@awarded", DateTime.Now.Ticks);
                                command.Parameters.AddWithValue("@cheevoID", id);
                                command.Parameters.AddWithValue("@proposes", proposes);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@id", id);
                                command.Parameters.AddWithValue("@proposes", proposes);
                                command.Parameters.AddWithValue("@time", DateTime.Now.Ticks);
                                command.Parameters.AddWithValue("@user", user);
                            }

                            try
                            {
                                command.ExecuteNonQuery();
                                sqlTransaction.Commit();
                                ret = true;
                            }
                            catch
                            {
                                // do nothing, cheevo already added
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + " " + ex.StackTrace);
                }
                finally
                {
                    if (dbOpened)
                    {
                        Database.sqliteCon.Close();
                    }
                }
            }

            return ret;
        }
    }
}
