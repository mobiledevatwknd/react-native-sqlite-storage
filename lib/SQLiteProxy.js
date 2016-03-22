
var dbmap = {};

var nextTick = window.setImmediate || function(fun) {
    window.setTimeout(fun, 0);
};


var plugin = {
	open: function(args, win, fail) {
	    var options = args;
	    var res;

			//var dbname = options.name;
			// from @EionRobb / phonegap-win8-sqlite:
			var opendbname = Windows.Storage.ApplicationData.current.localFolder.path + "\\" + dbname;
			console.log("open db name: " + dbname + " at full path: " + opendbname);

      SQLite3JS.openAsync(opendbname, options).then(
          db => {
            	dbmap[dbname] = db;
              win();
          },
          error => {
            fail(error);
          }
        );
      }

		}
	},
	close: function(args, win, fail) {
	  var options = args;
		try {
			var dbname = options.path;
			nextTick(function() {
				if (!!dbmap[dbname] && dbmap[dbname].close() == 0) {
					delete dbmap[dbname];
					win();
				} else {
					fail(); // XXX TODO REPORT ERROR
				}
			});
		}
	},
	backgroundExecuteSqlBatch: function(args, win, fail) {
	    var options = args;
	    var dbname = options.dbargs.dbname;
		var executes = options.executes;
	    //var executes = options.executes.map(function (e) { return [String(e.qid), e.sql, e.params]; });
		var db = dbmap[dbname];
		var results = [];
		var i, count=executes.length;
		//console.log("executes: " + JSON.stringify(executes));
		//console.log("execute sql count: " + count);

    var oldTotalChanges = db.totalChanges();

    var promise = Promise.resolve(null);
    executes.forEach( (e) => {
      promise = promise.then( () => {
        return db.allAsync(e.sql, e.params)
      }).then
      ( rows => {
        var newTotalChanges = db.totalChanges();
        var rowsAffected = newTotalChanges - oldTotalChanges;
        oldTotalChanges = newTotalChanges; // Not in original code
        var result = { rows : rows, rowsAffected : rowsAffected };
        if (rowsAffected > 0)
        {
          var lastInsertRowId = db.lastInsertRowId();
          if (lastInsertRowId !== 0) result.insertId = lastInsertRowId;
          results.push( {
            type: "success",
            qid: e.qid,
            result: result
          })
        }
      },
        err => {
          console.log("sql exception error: " + err);
  				results.push({
  					type: "error",
  					qid: e.qid,
  					result: { code: -1, message: err }
  				});
        });

    });
    promise.then(
      () => { win(results);}
    );
	},
	"delete": function(args, win, fail) {
	    var options = args;
	    var res;
		try {
		    //res = SQLitePluginRT.SQLitePlugin.deleteAsync(JSON.stringify(options));
			var dbname = options.path;

			WinJS.Application.local.exists(dbname).then(function(isExisting) {
				if (!isExisting) {
					// XXX FUTURE TBD consistent for all platforms:
					fail("file does not exist");
					return;
				}

				if (!!dbmap[dbname]) {
					dbmap[dbname].close_v2();

					delete dbmap[dbname];
				}

				//console.log('test db name: ' + dbname);
				Windows.Storage.ApplicationData.current.localFolder.getFileAsync(dbname)
					.then(function (dbfile) {
						//console.log('get db file to delete ok');
						return dbfile.deleteAsync(Windows.Storage.StorageDeleteOption.permanentDelete);
					}, function (e) {
						console.log('get file failure: ' + JSON.stringify(e));
						// XXX FUTURE TBD consistent for all platforms:
						fail(e);
					}).then(function () {
						//console.log('delete ok');
						win();
					}, function (e) {
						console.log('delete failure: ' + JSON.stringify(e));
						// XXX FUTURE TBD consistent for all platforms:
						fail(e);
					});

			});

		} catch(ex) {
			fail(ex);
		}
		//handle(res, win, fail);
	}
};



export default plugin;
