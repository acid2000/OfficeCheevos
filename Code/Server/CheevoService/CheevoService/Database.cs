using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

//todo all this sql needs to be either parameterised or escapes otherwise we will be pwned

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
            var addCmd = string.Format("INSERT INTO available_cheevos(ID, Title, Description, Category, Points) VALUES(NULL, '{0}', '{1}', '{2}', '{3}')", title, description, category, points);

            lock (sqliteCon)
            {
                sqliteCon.Open();

                using (SQLiteTransaction sqlTransaction = sqliteCon.BeginTransaction())
                using (SQLiteCommand addCommand = new SQLiteCommand(addCmd, sqliteCon, sqlTransaction))
                {
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

                sqliteCon.Close();
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

            lock (Database.sqliteCon)
            {
                Database.sqliteCon.Open();

                using (SQLiteCommand selectCommand = new SQLiteCommand(loadCheevos, Database.sqliteCon))
                using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                {
                    while (dataReader.HasRows && dataReader.Read())
                    {
                        cheevos.Add(new Cheevo(dataReader.GetString(0), dataReader.GetString(1), dataReader.GetString(2), dataReader.GetInt32(3), DateTime.MinValue, dataReader.GetInt32(4)));
                    }
                }
                Database.sqliteCon.Close();
            }
            return cheevos;
        }

        public static Dictionary<string, CheevoUser> LoadPoppedCheevos()
        {
            var dbCheevos = new Dictionary<string, CheevoUser>();

            const string loadCheevos = "select Title,Description,Category,Points,AwardedTime,User " +
                                       "from popped_cheevos inner join available_cheevos on available_cheevos.ID = popped_cheevos.ID where AwardedTime is not null";

            lock (Database.sqliteCon)
            {
                Database.sqliteCon.Open();

                using (SQLiteCommand selectCommand = new SQLiteCommand(loadCheevos, Database.sqliteCon))
                using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                {
                    while (dataReader.HasRows && dataReader.Read())
                    {
                        var myDate = new DateTime(dataReader.GetInt64(4));

                        var awardedCheevo = new Cheevo(dataReader.GetString(0), dataReader.GetString(1), dataReader.GetString(2), dataReader.GetInt32(3), myDate, -1);
                        var user = new CheevoUser(dataReader.GetString(5));
                        user.Add(awardedCheevo);

                        dbCheevos.Add(user.Username.ToLower(), user);
                    }
                }
                Database.sqliteCon.Close();
            }
            return dbCheevos;
        }

        static bool CheevoAlreadyProposed(string user, int id)
        {
            bool ret = false;
            string loadCheevos = string.Format("select ID from popped_cheevos where CheevoID = {0} and User = '{1}' and SecondMod is null", id, user);

            lock (Database.sqliteCon)
            {
                sqliteCon.Open();

                using (SQLiteCommand selectCommand = new SQLiteCommand(loadCheevos, Database.sqliteCon))
                using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                {          
                    ret = dataReader.HasRows;
                }
                sqliteCon.Close();
            }
            return ret;
        }

        public static bool ProposeCheevo(string user, string proposes, int id)
        {
            bool ret = false;

            string cmd;

            if (CheevoAlreadyProposed(proposes, id))
            {
                // complete it
                cmd = string.Format("update popped_cheevos set SecondMod = '{0}', AwardedTime = '{1}' where CheevoID = {2} and User = '{3}'", user, DateTime.Now.Ticks, id, proposes);
            }
            else
            {
                // add it
                cmd = string.Format("insert into popped_cheevos (ID, CheevoID, User, ProposedTime, FirstMod) VALUES (NULL, {0}, '{1}', '{2}', '{3}')", id, proposes, DateTime.Now.Ticks, user);
            }

            lock (sqliteCon)
            {
                sqliteCon.Open();

                using (SQLiteTransaction sqlTransaction = sqliteCon.BeginTransaction())
                using (SQLiteCommand addCommand = new SQLiteCommand(cmd, sqliteCon, sqlTransaction))
                {
                    try
                    {
                        addCommand.ExecuteNonQuery();
                        sqlTransaction.Commit();
                        ret = true;
                    }
                    catch
                    {
                        // do nothing, cheevo already added
                    }
                }

                sqliteCon.Close();
            }

            return ret;
        }
    }
}
