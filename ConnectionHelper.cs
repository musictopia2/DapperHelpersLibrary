using System;
using System.Text;
using CommonBasicStandardLibraries.Exceptions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System.Linq;
using CommonBasicStandardLibraries.BasicDataSettingsAndProcesses;
using static CommonBasicStandardLibraries.BasicDataSettingsAndProcesses.BasicDataFunctions;
using CommonBasicStandardLibraries.CollectionClasses;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using fs = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.FileHelpers;
using js = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.NewtonJsonStrings; //just in case i need those 2.
//i think this is the most common things i like to do
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using DapperHelpersLibrary.ConditionClasses;
using DapperHelpersLibrary.EntityInterfaces;
using DapperHelpersLibrary.Extensions;
using static DapperHelpersLibrary.SQLHelpers.StatementFactoryUpdates;
using DapperHelpersLibrary.SQLHelpers;
using static DapperHelpersLibrary.SQLHelpers.SortInfo;
using cs = DapperHelpersLibrary.ConditionClasses.ConditionOperators;

namespace DapperHelpersLibrary
{
    public class ConnectionHelper
    {

        #region Static Functions
        public static CustomBasicList<ICondition> StartConditionWithID(int ID)
        {
            CustomBasicList<ICondition> ThisList = new CustomBasicList<ICondition>();
            AndCondition ThisCon = new AndCondition();
            ThisCon.Property = nameof(ISimpleDapperEntity.ID);
            ThisCon.Value = ID;
            ThisList.Add(ThisCon);
            return ThisList;
        }

        public static CustomBasicList<ICondition> StartWithOneCondition(string Property, object Value)
        {
            CustomBasicList<ICondition> ThisList = new CustomBasicList<ICondition>();
            AndCondition ThisCon = new AndCondition();
            ThisCon.Property = Property;
            ThisCon.Value = Value;
            ThisList.Add(ThisCon);
            return ThisList;
        }
        public static CustomBasicList<ICondition> StartWithNullCondition(string Property, string Operator )
        {
            CustomBasicList<ICondition> ThisList = new CustomBasicList<ICondition>();
            AndCondition ThisCon = new AndCondition();
            ThisCon.Property = Property;
            if (Operator != cs.IsNotNull && Operator != cs.IsNull)
                throw new BasicBlankException("Only null or is not null is allowed when starting with null conditions");
            //this was needed for the tv shows.
            ThisCon.Operator = Operator;
            ThisList.Add(ThisCon);
            return ThisList;
        }

        public static CustomBasicList<ICondition> StartWithOneCondition(string Property, string Operator, object Value)
        {
            CustomBasicList<ICondition> ThisList = new CustomBasicList<ICondition>();
            AndCondition ThisCon = new AndCondition();
            ThisCon.Property = Property;
            ThisCon.Value = Value;
            ThisCon.Operator = Operator;
            ThisList.Add(ThisCon);
            return ThisList;
        }

        public static CustomBasicList<SortInfo> StartSorting(string Property)
        {
            SortInfo ThisSort = new SortInfo();
            ThisSort.Property = Property;
            return new CustomBasicList<SortInfo> { ThisSort };
        }
        public static CustomBasicList<SortInfo> StartSorting(string Property, EnumOrderBy Order)
        {
            SortInfo ThisSort = new SortInfo();
            ThisSort.Property = Property;
            ThisSort.OrderBy = Order;
            return new CustomBasicList<SortInfo> { ThisSort };
        }
        public static CustomBasicList<UpdateEntity> StartUpdate(string Property, object value)
        {
            CustomBasicList<UpdateEntity> output = new CustomBasicList<UpdateEntity>
            {
                new UpdateEntity(Property, value)
            };
            return output;
        }
        #endregion

        #region Main Functions
        public enum EnumDatabaseCategory
        {
            SQLServer = 0,
            SQLite = 1
        };
        //this only supports sql server and sqlite for now.
        private readonly EnumDatabaseCategory Category;
        private readonly string ConnectionString;

        public static ConnectionHelper GetSQLiteTestHelper()
        {
            return new ConnectionHelper();
        }

        private ConnectionHelper()
        {
            IsTesting = true;
            ConnectionString = GetInMemorySQLiteString();
            Category = EnumDatabaseCategory.SQLite; //only sqlite can be used for testing
        }

        private readonly bool IsTesting;
        private string GetInMemorySQLiteString()
        {
            return "Data Source=:memory:";
        }

        


        public ConnectionHelper(EnumDatabaseCategory category, string PathOrDatabaseName)
        {
            if (IsTesting == true)
                throw new BasicBlankException("You already decided to test this");
            if (category == EnumDatabaseCategory.SQLServer)
            {
                ISQLServer sqls = cons.Resolve<ISQLServer>();
                ConnectionString = sqls.GetConnectionString(PathOrDatabaseName);
            }
            else
                ConnectionString = $@"Data Source = {PathOrDatabaseName}";
            Category = category;
        }

        
        public IDbConnection GetConnection() //if you want the most flexibility
        {
            if (Category == EnumDatabaseCategory.SQLite)
            {
                IDbConnection output = new SQLiteConnection(ConnectionString);
                if (IsTesting == true)
                    output.Open(); //for testing, has to open connection.
                output.Dispose(); //i think.
                return new SQLiteConnection(ConnectionString);
            }
            else if (Category == EnumDatabaseCategory.SQLServer)
            {
                if (IsTesting == true)
                    throw new BasicBlankException("You can't be testing on a sql server database");
                return new SqlConnection(ConnectionString);
            }
            else
                throw new BasicBlankException("Only SQL Server And SQLite Databases Are Currently Supported");
        }



        #endregion

        #region Work Functions


        public void DoWork(Action<IDbConnection> action)
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Open(); //i think we should be in a habit of opening/closing transactions.
                action.Invoke(cons);
                cons.Close();
            }
        }

        public async Task DoWorkAsync(Func<IDbConnection, Task> action)
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Open(); //i think we should be in a habit of opening/closing transactions.
                await action.Invoke(cons);
                cons.Close();
            }
        }

        public async Task DoBulkWorkAsync(Func<IDbConnection, IDbTransaction, Task> action,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            using (IDbConnection cons = GetConnection()) //you are responsible for committing transaction
            {
                cons.Open();
                if (isolationLevel == IsolationLevel.Unspecified)
                {
                    using (IDbTransaction tran = cons.BeginTransaction())

                        await action.Invoke(cons, tran); //the client is responsible for committing or rolling back transaction.
                }
                else
                {
                    using (IDbTransaction tran = cons.BeginTransaction(isolationLevel))

                        await action.Invoke(cons, tran);  //clients are responsible for commiting the transaction here too.
                }
                cons.Close();
            }
        }


        public void DoBulkWork<E>(Action<IDbConnection, IDbTransaction, E> action,
            ICustomBasicList<E> ThisList, IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            Action<IDbConnection> BeforeWork = null, Action<IDbConnection> AfterWork = null)
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Open();
                if (BeforeWork != null)
                    BeforeWork.Invoke(cons);
                ThisList.ForEach(Items =>
                {
                    if (isolationLevel == IsolationLevel.Unspecified)
                    {
                        using (IDbTransaction tran = cons.BeginTransaction())

                            action.Invoke(cons, tran, Items); //the client is responsible for committing or rolling back transaction.
                    }
                    else
                    {
                        using (IDbTransaction tran = cons.BeginTransaction(isolationLevel))

                            action.Invoke(cons, tran, Items);  //clients are responsible for commiting the transaction here too.
                    }
                });
                if (AfterWork != null)
                    AfterWork.Invoke(cons);
                cons.Close();
            }
        }
        public void DoWork(Action<IDbConnection, IDbTransaction> action, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Open();
                if (isolationLevel == IsolationLevel.Unspecified)
                {
                    using (IDbTransaction tran = cons.BeginTransaction())
                    {
                        action.Invoke(cons, tran); //the client is responsible for committing or rolling back transaction.
                    }


                }
                else
                {
                    using (IDbTransaction tran = cons.BeginTransaction(isolationLevel))
                    {
                        action.Invoke(cons, tran);
                    }
                }
                cons.Close();
            }
        }

        public async Task DoBulkWorkAsync<E>(Func<IDbConnection, IDbTransaction, E, Task> action, ICustomBasicList<E> ThisList, IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            Action<IDbConnection> BeforeWork = null, Func<IDbConnection, Task> AfterWork = null)
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Open();
                if (BeforeWork != null)
                    BeforeWork.Invoke(cons);
                await ThisList.ForEachAsync(async Items =>
                {
                    if (isolationLevel == IsolationLevel.Unspecified)
                    {
                        using (IDbTransaction tran = cons.BeginTransaction())

                            await action.Invoke(cons, tran, Items); //the client is responsible for committing or rolling back transaction.
                    }
                    else
                    {
                        using (IDbTransaction tran = cons.BeginTransaction(isolationLevel))

                            await action.Invoke(cons, tran, Items);
                    }
                });
                if (AfterWork != null)
                {
                    await AfterWork.Invoke(cons);
                }
                cons.Close();
            }
        }

        public async Task DoWorkAsync(Func<IDbConnection, IDbTransaction, Task> action, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Open();
                if (isolationLevel == IsolationLevel.Unspecified)
                {
                    using (IDbTransaction tran = cons.BeginTransaction())
                    {
                        await action.Invoke(cons, tran); //the client is responsible for committing or rolling back transaction.
                    }


                }
                else
                {
                    using (IDbTransaction tran = cons.BeginTransaction(isolationLevel))
                    {
                        await action.Invoke(cons, tran);
                    }
                }
                cons.Close();
            }
        }
        #endregion

        #region Unique Functions

        public async Task UpdateListOnlyAsync<E>(ICustomBasicList<E> UpdateList, EnumUpdateCategory Category = EnumUpdateCategory.Common, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E: class, ISimpleDapperEntity
        {
            await DoBulkWorkAsync<E>(async (cons, tran, ThisEntity) =>
            {
                await cons.UpdateEntityAsync(ThisEntity, Category: Category, ThisTran: tran);
                tran.Commit();
            }, UpdateList, isolationLevel);
                
        }

        public async Task UpdateListAutoOnlyAsync<E>(CustomBasicList<E> UpdateList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity, IUpdatableEntity
        {
            await UpdateListOnlyAsync(UpdateList, Category: EnumUpdateCategory.Auto, isolationLevel);
        }

        //this may require rethinking.

        public async Task UpdateListOnlyAsync<E>(CustomBasicList<E> UpdateList, CustomBasicList<UpdateFieldInfo> ManuelList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
        {
            await DoBulkWorkAsync(async (cons, tran, ThisEntity) =>
            {
                await cons.UpdateEntityAsync(ThisEntity, ManuelList, ThisTran: tran);
                tran.Commit(); //i think i forgot this.
            }, UpdateList, isolationLevel);

        }
        public void UpdateCommonListOnly<E>(CustomBasicList<E> UpdateList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E : class, ISimpleDapperEntity
        {
            DoBulkWork((cons, tran, ThisEntity) =>
            {
                cons.UpdateEntity(ThisEntity, EnumUpdateCategory.Common, ThisTran: tran);
                tran.Commit(); //i think i forgot this.
            }, UpdateList, isolationLevel);

        }
        public async Task UpdateCommonOnlyAsync<E>(E ThisEntity) where E : class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                await cons.UpdateEntityAsync(ThisEntity, EnumUpdateCategory.Common);
            }
        }

        public void UpdateCommonOnly<E>(E ThisEntity) where E : class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.UpdateEntity(ThisEntity, EnumUpdateCategory.Common);
            }
        }

        #endregion

        #region Direct To Extensions Except Get

        public async Task AddListOnlyAsync<E>(CustomBasicList<E> AddList, IsolationLevel isolationLevel = IsolationLevel.Unspecified) where E: class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Open();
                if (isolationLevel == IsolationLevel.Unspecified)
                {
                    using (IDbTransaction tran = cons.BeginTransaction())
                    {
                        await cons.InsertRangeAsync(AddList, tran);
                        tran.Commit();
                    }
                }
                else
                {
                    using (IDbTransaction tran = cons.BeginTransaction(isolationLevel))

                    {
                        await cons.InsertRangeAsync(AddList, tran);
                        tran.Commit();
                    }
                }
                cons.Close();
            }
        }
        public async Task AddEntityAsync<E>(E ThisEntity) where E:class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                ThisEntity.ID = await cons.InsertSingleAsync(ThisEntity); //i think if doing it this way, let this give the id.
            }
        }
        public void AddEntity<E>(E thisEntity) where E: class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                thisEntity.ID = cons.InsertSingle(thisEntity); //i think if doing it this way, let this give the id.
            }
        }
        public void DeleteOnly<E>(E ThisEntity) where E: class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Delete(ThisEntity);
            }
        }

        public async Task DeleteOnlyAsync<E>(E ThisEntity) where E: class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                await cons.DeleteAsync(ThisEntity);
            }
        }

        public void DeleteOnly<E>(int ID) where E : class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                cons.Delete<E>(ID);
            }
        }

        public async Task DeleteOnlyAsync<E>(int ID) where E : class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                await cons.DeleteAsync<E>(ID);
            }
        }

        public async Task ExecuteAsync(string sqls) //in this case, can't be in transaction obviously
        {
            await DoWorkAsync(async cons =>
            {
                await cons.ExecuteAsync(sqls);
            });
        }

        public async Task ExecuteAsync(CustomBasicList<string> SQLList, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            await DoWorkAsync(async (cons, trans) =>
            {
                await SQLList.ForEachAsync(async Items =>
                {
                    await cons.ExecuteAsync(Items, null, trans);
                });
                trans.Commit();
            }, isolationLevel);
        }

        public bool Exists<E>(CustomBasicList<ICondition> Conditions) where E: class
        {
            bool rets = false;
            DoWork(cons =>
            {
                rets = cons.Exists<E>(Conditions);
            });
            return rets;
        }




        #endregion

        #region Direct To Extensions For Getting
        //this is when you only need to get something and do nothing else.

        public R GetSingleObject<E, R>(string Property, CustomBasicList<SortInfo> SortList, CustomBasicList<ICondition> Conditions = null) where E:class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.GetSingleObject<E, R>(Property, SortList, Conditions);
            }
        }

        public async Task<R> GetSingleObjectAsync<E, R>(string Property, CustomBasicList<SortInfo> SortList, CustomBasicList<ICondition> Conditions = null) where E : class, ISimpleDapperEntity
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetSingleObjectAsync<E, R>(Property, SortList, Conditions);
            }
        }


        public CustomBasicList<R> GetObjectList<E, R>(string Property, CustomBasicList<ICondition> Conditions = null, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.GetObjectList<E, R>(Property, Conditions, SortList, HowMany);
            }
        }

        public async Task<CustomBasicList<R>> GetObjectListAsync<E, R>(string Property, CustomBasicList<ICondition> Conditions = null, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetObjectListAsync<E, R>(Property, Conditions, SortList, HowMany);
            }
        }

        public E Get<E>(int ID) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E>(ID);
            }
                
        }

        public IEnumerable<E> Get<E>(CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E>(SortList, HowMany);
            }
        }

        public async Task<E> GetAsync<E>(int ID) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E>(ID);
            }
        }

        //public async Task<I> GetAsync<I, E>(int ID) where E : class, I
        //{
        //    using (IDbConnection cons = GetConnection())
        //    {
        //        return await cons.GetAsync<I, E>(ID);
        //    }
        //}

        public async Task<IEnumerable<E>> GetAsync<E>(CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E>(SortList, HowMany);
            }
        }

        public E Get<E, D1>(int ID) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E, D1>(ID);
            }
        }

        public IEnumerable<E> Get<E, D1>(CustomBasicList<SortInfo> SortList, int HowMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E, D1>(SortList, HowMany);
            }
        }

        public async Task<E> GetAsync<E, D1>(int ID) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E, D1>(ID);
            }
        }

        public async Task<IEnumerable<E>> GetAsync<E, D1>(CustomBasicList<SortInfo> SortList, int HowMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E, D1>(SortList, HowMany);
            }
        }

        public E Get<E, D1, D2>(int ID) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E, D1, D2>(ID);
            }
        }

        public IEnumerable<E> Get<E, D1, D2>(CustomBasicList<SortInfo> SortList, int HowMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E, D1, D2>(SortList, HowMany);
            }
        }

        public async Task<E> GetAsync<E, D1, D2>(int ID) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E, D1, D2>(ID);
            }
        }



        public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(CustomBasicList<SortInfo> SortList, int HowMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E, D1, D2>(SortList, HowMany);
            }
        }

        public E GetOneToMany<E, D1>(int ID) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.GetOneToMany<E, D1>(ID);
            }
        }

        public IEnumerable<E> GetOneToMany<E, D1>(CustomBasicList<SortInfo> SortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.GetOneToMany<E, D1>(SortList);
            }
        }

        public async Task<E> GetOneToManyAsync<E, D1>(int ID) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetOneToManyAsync<E, D1>(ID);
            }
        }



        public async Task<IEnumerable<E>> GetOneToManyAsync<E, D1>(CustomBasicList<SortInfo> SortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetOneToManyAsync<E, D1>(SortList);
            }
        }


        public CustomBasicList<E> Get<E>(CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E>(Conditions, SortList, HowMany);
            }
        }
        //public CustomBasicList<I> Get<I, E>(CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class, I
        //{
        //    using (IDbConnection cons = GetConnection())
        //    {
        //        return cons.Get<E>
        //    }
        //}




        public async Task<CustomBasicList<E>> GetAsync<E>(CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E>(Conditions, SortList, HowMany);
            }
        }
        //public async Task<CustomBasicList<I>> GetAsync<I, E>(CustomBasicList<ICondition> Conditions, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class, I
        //{
        //    using (IDbConnection cons = GetConnection())
        //    {
        //        return await cons.GetAsync<
        //    }
        //}

        public CustomBasicList<E> GetOneToMany<E, D1>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.GetOneToMany<E, D1>(ConditionList, SortList);
            }
        }

        public CustomBasicList<E> GetOneToMany<E, D1, D2>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.GetOneToMany<E, D1, D2>(ConditionList, SortList);
            }
        }

        public async Task<CustomBasicList<E>> GetOneToManyAsync<E, D1>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetOneToManyAsync<E, D1>(ConditionList, SortList);
            }
        }


        public async Task<CustomBasicList<E>> GetOneToManyAsync<E, D1, D2>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await GetOneToManyAsync<E, D1, D2>(ConditionList, SortList);
            }
        }

        public IEnumerable<E> Get<E, D1>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E, D1>(ConditionList, SortList, HowMany);
            }
        }

        //i think the best bet instead of creating another method is just sending in 1.


        public async Task<IEnumerable<E>> GetAsync<E, D1>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class, IJoinedEntity where D1 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E, D1>(ConditionList, SortList, HowMany);
            }
        }

        public IEnumerable<E> Get<E, D1, D2>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return cons.Get<E, D1, D2>(ConditionList, SortList, HowMany);
            }
        }

        public async Task<IEnumerable<E>> GetAsync<E, D1, D2>(CustomBasicList<ICondition> ConditionList, CustomBasicList<SortInfo> SortList = null, int HowMany = 0) where E : class, IJoin3Entity<D1, D2> where D1 : class where D2 : class
        {
            using (IDbConnection cons = GetConnection())
            {
                return await cons.GetAsync<E, D1, D2>(ConditionList, SortList, HowMany);
            }
        }

        #endregion

    }
}
