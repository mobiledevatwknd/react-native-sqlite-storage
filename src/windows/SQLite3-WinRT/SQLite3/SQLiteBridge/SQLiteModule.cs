using System;
using System.Collections.Generic;
using System.Linq;
using ReactNative.Bridge;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using SQLite3;
using ReactNative.Modules.Core;

namespace ReactNative.Modules.SQLite
{
    public class SQLiteModule : ReactContextNativeModuleBase
    {
        static string version;
        static Dictionary<string, Database> databases = new Dictionary<string, Database>();

        public SQLiteModule(ReactContext reactContext) : base(reactContext)
        {

        }

        public override string Name
        {
            get
            {
                return "SQLite";
            }

        }

        [ReactMethod]
        public async void open(
            JObject config,
            ICallback doneCallback,
            ICallback errorCallback
            )
        {
            try
            {
                string dbname = config.Value<string>("name") ?? "";
                string opendbname = ApplicationData.Current.LocalFolder.Path + "\\" + dbname;
                string key = config.Value<string>("key");
                Database db = await (key != null ? Database.OpenAsyncWithKey(opendbname, key) : Database.OpenAsync(opendbname));
                if (version == null)
                {
                    JObject result = JObject.Parse( await db.OneAsyncVector("SELECT sqlite_version() || ' (' || sqlite_source_id() || ')' as version", new List<string>()));
                    version = result.Value<string>("version");
                }
                databases[dbname] = db;
                doneCallback.Invoke();
            }
            catch(Exception e)
            {
                errorCallback.Invoke(e.Message);
            }

        }

        [ReactMethod]
        public void close(
            JObject config,
            ICallback doneCallback,
            ICallback errorCallback
        )
        {
            try
            {
                string dbname = config.Value<string>("name") ?? "";
                Database db = databases[dbname];
                db.closedb();
                databases.Remove(dbname);
            }
            catch(Exception e)
            {
                errorCallback.Invoke(e.Message);
            }
        }

        [ReactMethod]
        public async void backgroundExecuteSqlBatch(
            JObject config,
            ICallback doneCallback,
            ICallback errorCallback
        )
        {
            try
            {
                string dbname = config.Value<JObject>("dbargs").Value<string>("name") ?? "";
                JArray executes = config.Value<JArray>("executes");
                Database db = databases[dbname];
                JArray results = new JArray();
                long totalChanges = db.TotalChanges;
                string q = "";
                foreach( JObject e in executes)
                {
                    try
                    {
                        q = e.Value<string>("qid");
                        string s = e.Value<string>("sql");
                        IReadOnlyList<Object> p = e.ToObject<IReadOnlyList<Object>>();

                        JArray rows = JArray.Parse(await db.AllAsyncVector(s, p));
                        long rowsAffected = db.TotalChanges - totalChanges;
                        totalChanges = db.TotalChanges;
                        JObject result = new JObject();
                        result["rowsAffected"] = rowsAffected;
                        result["rows"] = rows;
                        JObject resultInfo = new JObject();
                        resultInfo["type"] = "success";
                        resultInfo["qid"] = q;
                        resultInfo["result"] = result;
                        results.Add(resultInfo);
                    }
                    catch( Exception err)
                    {
                        JObject resultInfo = new JObject();
                        JObject result = new JObject();
                        result["code"] = -1;
                        result["message"] = err.Message;
                        resultInfo["type"] = "error";
                        resultInfo["qid"] = q;
                        resultInfo["result"] = result;
                        results.Add(result);
                    }


                }
                doneCallback.Invoke(results);
            }
            catch(Exception e)
            {
                errorCallback.Invoke(e.Message);
            }
        }

        [ReactMethod]
        public async void delete(
            JObject config,
            ICallback doneCallback,
            ICallback errorCallback)
        {
            try
            {
                string dbname = config.Value<string>("name") ?? "";
                if (databases.Keys.Contains(dbname))
                {
                    Database db = databases[dbname];
                    db.closedb();
                    databases.Remove(dbname);
                }
                StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(dbname);
                await file.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
                doneCallback.Invoke();
            }
            catch (Exception err)
            {
                errorCallback.Invoke(err.Message);
            }
        }


    }

    public class SQLiteReactPackage : IReactPackage
    {
        public IReadOnlyList<INativeModule> CreateNativeModules(ReactContext reactContext)
        {
            return new List<INativeModule>
        {
            new SQLiteModule(reactContext)
        };
        }

        public IReadOnlyList<Type> CreateJavaScriptModulesConfig()
        {
            return new List<Type>(0);
        }

        public IReadOnlyList<UIManager.IViewManager> CreateViewManagers(
            ReactContext reactContext)
        {
            return new List<UIManager.IViewManager>(0);
        }

    }

}
