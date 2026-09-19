using System.Collections;
using System.Collections.Generic;
using GHPC.State;
using MelonLoader;

namespace ModUtil
{
    internal class ModuleManager
    {
        internal static Dictionary<string, Module> modules = new Dictionary<string, Module>();
        private string mod_id;

        public ModuleManager(string mod_id)
        {
            this.mod_id = mod_id;
        }

        public void Add(string id, Module module)
        {
            module.Id = id;
            module.mod_id = mod_id;
            modules.Add(id, module);
        }

        public void UnloadAllDynamicAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool dynamic_unloaded = module.TryUnloadDynamicAssets();

                if (dynamic_unloaded)
                {
                    MelonLogger.Msg(mod_id + " dynamic assets unloaded from module: " + id);
                }
            }
        }

        public void LoadAllDynamicAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool started = module.TryLoadDynamicAssets();

                if (started)
                {
                    MelonLogger.Msg(mod_id + " deferred dynamic assets loading from module: " + id);
                }
            }
        }

        public void LoadAllStaticAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool static_loaded = module.TryLoadStaticAssets();

                if (static_loaded)
                {
                    MelonLogger.Msg(mod_id + " static assets loaded from module: " + id);
                }
            }
        }
    }
}
