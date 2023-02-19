using ReactNative;
using ReactNative.Modules.Core;
using ReactNative.Shell;
using System.Collections.Generic;
using ReactNative.Modules.SQLite;

namespace TestRunner
{
    class MainPage : ReactPage
    {
        public override string MainComponentName
        {
            get
            {
                return "TestRunner";
            }
        }

#if BUNDLE
        public override string JavaScriptBundleFile
        {
            get
            {
                return "ms-appx:///ReactAssets/index.windows.bundle";
            }
        }
#endif

        public override List<IReactPackage> Packages
        {
            get
            {
                return new List<IReactPackage>
                {
                    new MainReactPackage(),
                    new SQLiteReactPackage()
                };
            }
        }

        public override bool UseDeveloperSupport
        {
            get
            {
                return true;
            }
        }
    }

}
